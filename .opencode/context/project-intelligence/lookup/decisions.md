<!-- Context: project-intelligence/decisions | Priority: high | Version: 2.2 | Updated: 2026-11-18 -->

# Decisions Log

> Architectural and business decisions for BlocoNaRua, with rationale and trade-offs.

## Quick Reference

- **Purpose**: Prevent "why was this done?" debates by recording intent at the time.
- **Format**: One decision per section, oldest first.
- **Status values**: Decided | Pending | Deprecated

---

## Decision: Layered Clean-Architecture Project Layout (Core → Domain → Data → Services → Restful)

**Date**: 2025 (initial)
**Status**: Decided
**Owner**: @rafaelsnv

### Context
The codebase needed a clear separation between entity definitions, persistence, business rules, and HTTP transport so that adding new entities (or swapping the data layer) would not ripple through the whole solution.

### Decision
Adopt a five-project .NET solution:
- `BlocoNaRua.Core` — abstractions only (`EntityBase`, `IRepositoryBase<T>`)
- `BlocoNaRua.Domain` — entity classes + enums
- `BlocoNaRua.Data` — EF Core `DbContext`, configurations, repositories
- `BlocoNaRua.Services` — business rules, caching, authorization
- `BlocoNaRua.Restful` — controllers, mappers, request/response DTOs, `Program.cs`

### Rationale
- The Domain has no dependency on EF Core — keeps it POCO and testable.
- Repositories inherit from a generic `RepositoryBase<TEntity>` so 80% of CRUD is free.
- Controllers never touch repositories directly — they go through services.
- The Flutter client (separate repo) consumes only the `Restful` project; the rest is backend-only.

### Alternatives Considered
| Alternative | Pros | Cons | Why Rejected |
|-------------|------|------|--------------|
| Single Web project | Simpler solution file | No separation of concerns, hard to test | Rejected for maintainability |
| Vertical Slice (Feature folders) | Easy to find everything for one feature | Doesn't compose well with EF Core migrations | Rejected — overkill for 5 entities |

### Impact
- **Positive**: Adding a new entity is a clear recipe (entity → config → repo → service → controller + 3 DTOs).
- **Negative**: Five projects means five `csproj` to keep in sync; build is slightly slower.
- **Risk**: Junior devs sometimes put logic in controllers — guarded by code review.

---

## Decision: PostgreSQL on Supabase as the Primary Datastore

**Date**: 2025 (initial)
**Status**: Decided
**Owner**: @rafaelsnv

### Context
A managed Postgres offering was needed to avoid running a database server; the project has zero DBA time and a tiny budget.

### Decision
Use Supabase Postgres with `Npgsql.EntityFrameworkCore.PostgreSQL` 9.0.4. Connection string is resolved from `appsettings.Development.json` (`ConnectionStrings:DefaultConnection`) or, in deployed environments, from the `POSTGRESQLCONNSTR_SupabaseDB` environment variable (see `BlocoNaRua.Data/Extensions/DataExtensions.cs`).

### Rationale
- Free tier covers carnival traffic shape (idle most of year, spikes in Feb–Mar).
- Schema is the standard `public` schema, with snake_case columns (`HasColumnName("invite_code")`).
- Postgres enum support (`HasPostgresEnum<RolesEnum>()`) lets the role enum map to a real DB enum, not a string column.

### Alternatives Considered
| Alternative | Pros | Cons | Why Rejected |
|-------------|------|------|--------------|
| Self-hosted Postgres | Full control | Needs a server, backups, monitoring | Rejected — no ops time |
| Cosmos / Firestore | Hosted, scales | Different data model, query language | Rejected — relational model fits the data |

### Impact
- **Positive**: Zero ops overhead, free hosting for low traffic.
- **Negative**: Vendor coupling — but Postgres is portable, the EF Core mappings are portable, and Supabase exports SQL dumps.
- **Risk**: Free tier may throttle at peak carnival. Mitigation: 5-min `IMemoryCache` on every read.

---

## Decision: C# 12 Primary Constructors + Records for DTOs

**Date**: 2025 (initial)
**Status**: Decided
**Owner**: @rafaelsnv

### Context
.NET 8 ships C# 12 with primary constructors; the team wanted concise DI without sacrificing explicitness in business types.

### Decision
- **Services / Repositories**: use primary constructor injection (e.g. `CarnivalBlockService(ICarnivalBlocksRepository repository, ...)`), then also assign to a private readonly field for readability inside the body.
- **DTOs**: use `record` (or `record class`) for `*Create` / `*Update` / `*Response`.
- **Entities**: plain classes inheriting `EntityBase(int id)`, with a second parameterized constructor that calls `: this(id)`.

### Rationale
- Primary constructors remove three lines of boilerplate per dependency.
- Records give value-equality and immutability for transport types — exactly what an API DTO should be.
- The `EntityBase(int id)` primary constructor locks down `Id` as a private setter; mutations go through the service layer.

### Alternatives Considered
| Alternative | Pros | Cons | Why Rejected |
|-------------|------|------|--------------|
| Field-injected deps | Slightly terser | Hides dependencies, breaks testability | Rejected |
| Mutable DTO classes | Familiar | Easy to misuse, no equality | Rejected |

### Impact
- **Positive**: Idiomatic .NET 8 code; new devs recognize the patterns immediately.
- **Negative**: Field + primary-ctor duplication (`_service = service`) is a small wart — accepted for readability.

---

## Decision: Legacy Logged-Member HTTP Header for Identity (v1)

**Date**: 2025 (initial)
**Status**: Deprecated
**Owner**: @rafaelsnv

### Context
There was no auth provider; the Flutter client knew which `Member` it was acting as, and the backend needed that identity to authorize writes.

### Decision
The Flutter client passed an integer `loggedMember` via a custom HTTP header. Service methods took `int loggedMember` as the last parameter and used it to call `IAuthorizationService.GetMemberRole(blockId, loggedMember)`. The header was removed when JWT Bearer auth landed (2026-07-24); identity now comes from the verified JWT `sub` claim via `IMemberIdentityService`.

### Rationale
- Zero auth infrastructure to set up for v1.
- Header is consumed only by services — controllers just translate exceptions.

### Alternatives Considered
| Alternative | Pros | Cons | Why Rejected |
|-------------|------|------|--------------|
| JWT bearer token | Industry standard, signed | Needs an auth provider and key management | Deferred — not available at v1 launch |
| `?memberId=` query string | Easy to debug | Insecure, logs everywhere | Rejected on security grounds |
| Body field | No header needed | Mixes identity with payload, easy to spoof | Rejected |

### Impact
- **Positive**: Shipped v1 quickly.
- **Negative**: **Spoofable** — any authenticated user could claim to be any other member by sending their `memberId` in the header. Critical vulnerability, resolved by the 2026-07-24 JWT migration.
- **Risk**: Resolved — identity now derives from the verified JWT `sub` claim.

---

## Decision: `IMemoryCache` 5-Minute TTL on All Read Services

**Date**: 2025 (initial)
**Status**: Decided
**Owner**: @rafaelsnv

### Context
Reads are far more frequent than writes; carnival-season traffic spikes warrant cheap latency wins.

### Decision
Every service caches its reads with `TimeSpan.FromMinutes(5)`. Writes (create / update / delete) explicitly remove the affected cache keys:
- `CarnivalBlocks_All`, `CarnivalBlock_{id}`
- `Meetings_All`, `Meeting_{id}`
- `MeetingPresences_All`, `MeetingPresence_{id}`
- `Member_{uuid}`
- `CarnivalBlockMembers_Member_{memberId}`

### Rationale
- Avoids hammering Postgres during carnival peaks.
- Cache lives in-process; restart wipes it (acceptable).
- Write paths invalidate only what they touched.

### Alternatives Considered
| Alternative | Pros | Cons | Why Rejected |
|-------------|------|------|--------------|
| `IDistributedCache` (Redis) | Shared across instances | Needs Redis infra | Rejected for v1 (single-instance deploy) |
| No cache | Always fresh | DB cost at peak | Rejected |

### Impact
- **Positive**: Cheap scaling.
- **Negative**: Stale-by-up-to-5-minutes — fine for v1, may bite real-time dashboards later.
- **Risk**: Forgetting to invalidate a key on a new write path → user sees ghost data. Mitigated by code review.

---

## Decision: `MeetingPresence` Triple Foreign Key (`MemberId` + `MeetingId` + `CarnivalBlockId`)

**Date**: 2025 (initial)
**Status**: Decided (with future simplification possible)
**Owner**: @rafaelsnv

### Context
Attendance records span three entities; queries like "attendance for block X" are common.

### Decision
Store `CarnivalBlockId` on `MeetingPresence` even though it is derivable from `MeetingId`. All three FKs are `IsRequired()` with `OnDelete(DeleteBehavior.Restrict)`.

### Rationale
- Denormalized FK makes per-block queries a single index lookup, no join.
- `Restrict` on deletes protects historical attendance data — you cannot accidentally wipe last year's parade RSVPs.

### Alternatives Considered
| Alternative | Pros | Cons | Why Rejected |
|-------------|------|------|--------------|
| Only `MeetingId`, derive block | Less storage, no redundancy | Every query needs a join | Rejected for query speed |
| Cascade on block delete | Less code | Loses historical attendance | Rejected — historical data is the value |

### Impact
- **Positive**: Fast queries, audit-friendly.
- **Negative**: Three FKs to keep in sync in service code.
- **Risk**: If `Meeting.CarnivalBlockId` ever changes (it shouldn't), the presence row goes stale. Acceptable for v1.

---

## Decision: Separate Repo for `BlocoNaRua.Tests` Using EF Core InMemory + xUnit + Moq

**Date**: 2025 (initial)
**Status**: Decided
**Owner**: @rafaelsnv

### Context
Tests must run without a real Postgres. Service-level tests need a real-ish `DbContext`; controller-level tests mock the service.

### Decision
- **Service tests** use `TestDbContextFactory.GetContext(dbName)` with `UseInMemoryDatabase` — fresh DB per test.
- **Controller tests** use `Mock<IXxxService>` and call `new XxxController(mock.Object)`.
- Test framework: xUnit 2.5.3, mocking with Moq 4.20.72.

### Rationale
- EF Core InMemory gives realistic service tests without a DB.
- Controller tests verify request/response shape and exception mapping only.
- `IDisposable` + `EnsureDeleted()` per test keeps tests isolated.

### Alternatives Considered
| Alternative | Pros | Cons | Why Rejected |
|-------------|------|------|--------------|
| Real Postgres via Testcontainers | Realistic | Slow, requires Docker in CI | Deferred |
| Pure mocks for services | Fast | Doesn't exercise EF mapping | Rejected |

### Impact
- **Positive**: Fast test runs, no infra needed.
- **Negative**: InMemory provider doesn't enforce all constraints (e.g. cascades); tests can pass against InMemory and fail against real Postgres.

---

## Decision: JWT Bearer Auth for Integration Tests (2026-07-24)

**Date**: 2026-07-24
**Status**: Decided
**Owner**: @rafaelsnv

### Context
Integration tests cannot use real JWTs issued by an auth provider. A test-specific auth bypass was needed to verify controller behavior end-to-end.

### Decision
- **`TestAuthHandler`** — custom `AuthenticationHandler` registered in DI; bypasses JWT validation and injects custom `ClaimsPrincipal` with configurable `sub` claim.
- **`TestCurrentMemberHolder`** — `IAsyncLocal`-backed singleton carrying the current test member's UUID; consumed by `IMemberIdentityService` during requests.
- JWT `sub` claim (UUID) → `Members` table → `int Id` is the verified production path; test harness mirrors this exactly.

### Implementation
```csharp
// BlocoNaRuaWebApplicationFactory<TProgram> — WebApplicationFactory fixture
builder.Environment.SetEnvironmentVariable("Environment", "Testing");
// DataExtensions.AddEntityFramework swaps Postgres for InMemory when env=="Testing"

// TestAuthHandler in TestAuthHandler.cs
protected override Task<AuthenticateResult> HandleAuthenticateAsync()
{
    var claims = new[] { new Claim(ClaimTypes.NameIdentifier, _memberUuid.ToString()) };
    var identity = new ClaimsIdentity(claims, "TestScheme");
    var principal = new ClaimsPrincipal(identity);
    return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, "TestScheme")));
}

// TestCurrentMemberHolder — singleton, AsyncLocal<Guid> CurrentMemberUuid
```

### Rationale
- Mirrors production flow: `sub` claim → lookup in `Members` table.
- No secret keys needed; `TestAuthHandler` short-circuits before JWT validation.
- Fresh factory per test (`IAsyncLifetime`) ensures cache isolation.

### Alternatives Considered
| Alternative | Why Rejected |
|-------------|--------------|
| Mock `IMemberIdentityService` directly | Doesn't exercise the full HTTP pipeline |
| Use real JWTs with a test secret | Requires `appsettings.Testing.json` with a signing key |

### Impact
- ✅ 33 integration tests now run against 4 API controllers (Members, CarnivalBlocks, Meetings, MeetingPresence).
- ⚠️ `TestAuthHandler` is test-only; not registered in production `Program.cs`.

---

## Decision: Supabase `service_role` Key Held Backend-Only (2026-07-24)

**Date**: 2026-07-24
**Status**: Decided
**Owner**: @rafaelsnv

### Context
Members needed a way to authenticate without ever shipping a privileged Supabase credential to the client. A `service_role` key referenced from client-side code was previously flagged as a critical risk (account takeover if leaked).

### Decision
- `POST /auth/login` (`BlocoNaRua.Restful/Controllers/AuthController.cs`) proxies Supabase Auth through `BlocoNaRua.Services/Implementations/AuthService.cs`.
- The `service_role` key is read server-side only and never returned to clients; clients receive the Supabase-issued JWT.

### Rationale
- The privileged key stays inside the backend boundary — a leaked client bundle cannot escalate.
- One endpoint centralizes the Auth call, so credential handling has a single place to audit.

### Alternatives Considered
| Alternative | Pros | Cons | Why Rejected |
|-------------|------|------|--------------|
| Ship the privileged key to clients | No backend proxy | Full account takeover if leaked | Rejected on security grounds |
| Let each client call Supabase Auth directly | No backend hop | Scatters privileged-call decisions across clients | Rejected |

### Impact
- ✅ Resolves the flagged risk of the privileged key being referenced client-side.
- ⚠ The backend now owns credential rotation and the Auth-call surface.

---

## Decision: Remove Custom `IAuthorizationService` — Inline Role Checks + `CancellationToken` (2026-07-25)

**Date**: 2026-07-25
**Status**: Decided (Removed — shipped in commit `0581d26`)
**Owner**: @rafaelsnv

### Context
The custom `IAuthorizationService` abstraction existed to answer "what role does member X have in block Y". It ended up with a single usage pattern, so the interface plus DI plumbing cost more than the code it wrapped.

### Decision
- The custom `IAuthorizationService` was removed (commit `0581d26`).
- Each service resolves the role inline via `GetMemberRole`, next to the business rule that needs it.
- `CancellationToken` was adopted throughout the service → repository call chain in the same change.

### Rationale
- One pattern of use does not justify an abstraction; the indirection hid the rule instead of sharing it.
- Role checks now sit next to the business rules they protect, so the two cannot drift apart.
- `CancellationToken` flows from controller to database instead of being dropped at a boundary.

### Alternatives Considered
| Alternative | Pros | Cons | Why Rejected |
|-------------|------|------|--------------|
| Keep the authorization service interface | Central place for role rules | Indirection for a single pattern; rules drift from callers | Rejected |
| Static role helper | No DI needed | Global state, harder to test | Rejected |

### Impact
- ✅ Less indirection; role checks are visible where the business rule lives.
- ⚠ Historical entries in this log refer to the removed interface — read them as deprecated history.
- `CancellationToken` is now part of every service and repository signature; new code must thread it.

---

## Decision: Soft Delete via `IsActive` Flag + Repository Cascade (2026-10-20)

**Date**: 2026-10-20
**Status**: Decided
**Owner**: @rafaelsnv

### Context
Hard deletes broke referential and historical integrity: deleting a parent row left attendance records and block history without anchors, and the old rows were gone for good.

### Decision
- Every entity carries an `IsActive` flag (soft delete).
- All 5 repositories in `BlocoNaRua.Data/Repositories/` cascade the soft delete to dependent rows instead of removing them.
- Shipped in commit `a5311c9` (Oct 2026).

### Rationale
- History stays queryable, and a deletion no longer breaks referential integrity.
- Cascade lives in the repositories, so every delete path behaves the same way.

### Alternatives Considered
| Alternative | Pros | Cons | Why Rejected |
|-------------|------|------|--------------|
| Hard delete with FK `Restrict` | Simplest | Breaks historical integrity, blocks deletes | Rejected |
| Hard delete with FK `Cascade` | No orphans | Still destroys history | Rejected |

### Impact
- ✅ Referential and historical integrity preserved on delete.
- ⚠ Every read query must filter `IsActive` — a missing filter resurrects deleted rows.
- Cascade is implemented per repository; new repositories must implement it too.

---

## Decision: Supabase Auth Removed — API-Owned JWT + Google OAuth (2026-11-18)

**Date**: 2026-11-18
**Status**: Accepted
**Owner**: @rafaelsnv

### Context
The project is self-hosting its infrastructure (Dokploy). Keeping Supabase solely as an identity provider added an external dependency for something the API can own entirely, so Supabase was dropped completely.

### Decision
- The API issues and validates its own **HS256 JWTs** (no JWKS / OIDC discovery / outbound call); `Auth:SigningKey` is env-only.
- Social login is **Google-only OAuth 2.0**: `GET /auth/google/challenge` → `GET /auth/google/complete` returns the token pair; plus `POST /auth/refresh` (rotation) and `POST /auth/logout`.
- The `members` table is the **single identity source** (provider/provider_id/refresh-token columns); `sub` claim = member UUID.
- Password login is API-owned too: `POST /auth/login` and `POST /auth/signup` (phase 2, shipped 2026-11-18).
- Image uploads go to a **local `/uploads` volume** via `IImageUrlService` / `MediaController` (no object storage provider).

### Rationale
- Removes the last external identity dependency; one codebase owns the whole login → token → identity chain.
- Google is the only credential flow that needed no password infrastructure to ship.

### Alternatives Considered
| Alternative | Pros | Cons | Why Rejected |
|-------------|------|------|--------------|
| Keep Supabase Auth | Managed sessions, password grant ready | External dependency contradicts the self-hosted constraint | Rejected |
| Another IdP (Auth0/Cognito) | Managed, more providers | New vendor + cost | Rejected |

### Impact
- ⚠ `Auth:SigningKey` rotation invalidates **all** issued sessions (access + refresh) — every user must log in again.
- ⚠ Existing Supabase users are dropped (the migration is schema-only) — they re-register through Google.
- ➖ Negative: no password login until phase 2 ships.

---

## Deprecated Decisions

| Decision | Date | Replaced By | Why |
|----------|------|-------------|-----|
| Legacy logged-member HTTP header as the long-term identity layer | 2025 | JWT Bearer auth (completed 2026-07-24) | Header is trivially spoofable; JWT migration completed |

---

## Onboarding Checklist

- [ ] Read all Decided entries
- [ ] Skim Deprecated Decisions for context on past mistakes
- [ ] Understand that role checks live in services, not controllers
- [ ] Note that every read is cached 5 min and writes must invalidate

## Related Files

- `business-domain.md` — Business context
- `technical-domain.md` — Implementation details
- `living-notes.md` — Active issues that may become new decisions