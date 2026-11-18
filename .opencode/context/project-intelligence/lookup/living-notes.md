<!-- Context: project-intelligence/notes | Priority: high | Version: 3.1 | Updated: 2026-11-18 -->

# Living Notes

> Active issues, technical debt, open questions for BlocoNaRua.

## Quick Reference

- **Purpose**: Single place to find what's broken, what's owed, and what's still being decided.
- **Update**: When status changes; review before opening a PR.
- **Source of truth for cross-team items**: resolved items live in the Archive below.

---

## Technical Debt

| Item | Impact | Priority | Mitigation |
|------|--------|----------|------------|
| Spoofable client-supplied identity header | Privilege escalation — anyone could act as any member | ✅ Resolved 2026-07-24 | JWT Bearer auth landed (`Authorization: Bearer <JWT>` + `IMemberIdentityService`) — header removed from all code |
| `validateStatus` on Flutter Dio accepts 4xx | Errors swallowed inconsistently | 🔴 Critical | Tighten to throw on 4xx/5xx once backend stable |
| `service_role` key referenced in client-side code patterns (companion repo) | Account takeover if leaked | ✅ Resolved (verified 2026-11-17) | Key is backend-only — `BlocoNaRua.Services/Implementations/AuthService.cs` reads it from configuration; client only sees the JWT |
| Three FKs on `MeetingPresence` | Redundant data | 🟡 Medium | Acceptable for query speed; revisit if storage matters |
| `IRepositoryBase<>` has no `AsNoTracking` for `GetByIdAsync` | Possible stale-entity bugs on update | 🟡 Medium | Use `AsNoTracking` consistently; document the trade-off |
| Cache key strings are scattered | Easy to typo a key, ghost data persists | 🟢 Low | Centralize `CacheKeys` static class (future) |
| ~~No integration tests against real Postgres~~ | ✅ **Resolved 2026-07-24**: 33 integration tests now cover Members, CarnivalBlocks, Meetings, MeetingPresence APIs. InMemory FK gaps documented with `// ponytail:` comments. Testcontainers for real Postgres deferred to future CI work. | 🟢 Low | ✅ Resolved |

### Technical Debt Details

**EF Core `GetByIdAsync` Tracking**
- *Priority*: 🟡 Medium
- *Impact*: `DbSet.FindAsync` tracks the entity. A subsequent `UpdateAsync` then `AddAsync` of a new instance with the same key can throw.
- *Root Cause*: `IRepositoryBase.GetByIdAsync` uses `FindAsync`, not `AsNoTracking().FirstOrDefault`.
- *Proposed Solution*: Either always `AsNoTracking` and accept manual updates, or split read vs write repositories.
- *Effort*: Small.
- *Status*: Acknowledged.

---

## Live Tickets

> Previously mirrored from `BACKEND_TICKETS.md` (deleted 2026-11-17 — all tickets resolved).

| Ticket | Item | Impact | Priority | Mitigation |
|--------|------|--------|----------|------------|
| **TKT-002** | ~~Accept `memberId` on `POST /api/v1/MeetingPresences`~~ | ✅ Resolved 2026-11-17 — optional `memberId` ships on `MeetingPresenceCreate` (`MeetingPresenceCreate.cs:7`), controller falls back to the JWT member (`MeetingPresencesController.cs:47`) |
| **TKT-003** | ~~Authorize `PUT /api/v1/MeetingPresences/{id}` for managers~~ | ✅ Resolved 2026-11-17 — `MeetingPresenceService.AuthorizeForMemberAsync` ( lines 31-39 ) allows block owner/manager-role members; JWT identity flows through `UpdateAsync` |

### Ticket Details

**TKT-002 / TKT-003 — Manager presence authorization**
- *Context*: The meeting details screen lets block owners/managers set presence for any block member. The client sends `memberId` on create, but the API ignores it, and updates are restricted to the row owner.
- *Scope*: `MeetingPresenceCreate` schema is `{ meetingId, carnivalBlockId, isPresent }` with `additionalProperties: false`; `MeetingPresenceUpdate` accepts only `isPresent`.
- *Auth model*: Block-level roles live in `CarnivalBlockMembers` (`Owner` / role 1 Admin / role 2 Moderador) and are resolved inside the service layer — not via an ASP.NET policy.
- *Note*: Related to the old `memberId: 0` symptom below — that symptom predates JWT auth and needs re-verification under the current identity flow.
- *Status*: ~~Open~~ ✅ Resolved 2026-11-17 — shipped (`MeetingPresenceService.cs:31-39`); see Archive.

---

## Open Questions

| Question | Stakeholders | Status | Next Action |
|----------|--------------|--------|-------------|
| Should `MeetingPresence` ever transition out of bool? (tentative / no-show states) | Product, backend | Open | Defer until post-v1; current scope is binary |
| ~~Where does image upload live (block image, profile image)? S3? Supabase Storage?~~ | Backend, frontend | ✅ Resolved (2026-11-18) | `IImageUrlService` + `MediaController` ship local volume storage (`/app/uploads`, 5 MB, `.jpg/.jpeg/.png/.webp`); swap the interface for R2 later if needed |
| Should block deletion be soft-delete or hard-delete? | Product | ✅ Resolved (2026-11-17) | Soft-delete shipped — `IsActive` on all entities + cascade soft-delete in all 5 repos (commit `a5311c9`) |
| Do we need a `PlatformAdmin` role above `Owner`? | Product | Open | Currently no — Owner is the top |
| Should `InviteCode` / `ManagersInviteCode` be single-use? | Product, security | Open | v1 ships single-block-multi-use; revisit if abuse happens |

### Open Question Details

**Image Storage**
- *Context*: `CarnivalBlockImage` and `ProfileImage` are strings. Upload endpoint shipped 2026-11-18.
- *Resolution*: ✅ Resolved 2026-11-18 — `IImageUrlService` (`ImageUrlService.cs`) + `POST /api/v1/media/image` (`MediaController`) store files on a **local volume** (`Uploads:BasePath`, default `/app/uploads`, Dokploy mount), 5 MB max, whitelist `.jpg/.jpeg/.png/.webp`; the member's `ProfileImage` is replaced and the previous file deleted best-effort.
- *Upgrade path*: the `IImageUrlService` interface is the swap point — replace the implementation with R2 (or S3) later if local volume storage becomes a bottleneck.
- *Status*: ✅ Resolved.

---

## Known Issues

| Issue | Severity | Workaround | Status |
|-------|----------|------------|--------|
| `POST /api/v1/MeetingPresences` returns 500 | High | Likely a stale symptom from the pre-JWT identity flow — the backend now owns identity via JWT | ⚠️ Likely stale — needs re-verification under JWT auth (2026-11-17); see TKT-002/TKT-003 |
| `GET /api/v1/MeetingPresences/meeting/{id}` returns 404 for empty meetings | Info | Expected — no rows yet | Handled gracefully on frontend |
| `enableOnBackInvokedCallback` warning on Android 14+ | Low | Fixed in Flutter side via `AndroidManifest.xml` | ✅ Fixed (companion repo, 2026-07-19) |
| Startup skips ~248 frames on Flutter (companion) | Low | Needs profiling — likely heavy initial repo construction | Open |

### Issue Details

**Meeting Presence 500**
- *Severity*: High
- *Impact*: Members cannot mark attendance from the Flutter app.
- *Historical reproduction*: The Flutter client hit `POST /api/v1/MeetingPresences` with `memberId: 0` and put the real member id in a client-supplied header — the backend had no trusted identity, so the row could not be resolved.
- *Root Cause*: The service trying to resolve `memberId: 0` against `carnival_blocks` and getting nothing.
- *Current state*: Identity now comes from the validated JWT (`sub` → `IMemberIdentityService` → `memberId`), so the `memberId: 0` path should no longer be reachable from the backend.
- *Fix Plan*: Re-verify against the current JWT-authenticated build. If it still reproduces, capture the request/response and the error-log row (written to the local `api_errors` table via EF — `ApiErrorLoggerService`) before changing code. Managers setting presence for other members is tracked separately as TKT-002/TKT-003.
- *Status*: ⚠️ Likely stale — presumed resolved with the identity migration (2026-07-24); needs re-verification under JWT auth (2026-11-17).

---

## Insights & Lessons Learned

### What Works Well
- **Layered architecture**: adding a new entity is a clear 5-step recipe.
- **Service-level authorization**: block-role checks live in the service layer (per-block `Owner`/`Manager`/`Member` roles resolved inline, since policies have no access to the block id).
- **Records for DTOs**: zero ceremony for transport types.
- **EF Core InMemory tests**: fast, isolated, no infra.

### What Could Be Better
- **Identity** should never have been a custom header — defer auth provider setup, do not defer security.
- **Cache invalidation** relies on developers remembering the right keys. A `CacheKeys` constants class would prevent typos.
- **Repository base** mixes tracking semantics — better to have explicit read vs write paths.

### Lessons Learned
- **Don't ship identity as a header**, even for v1. The refactor cost grows with every endpoint that uses it.
- **Service tests with InMemory are great** but never replace a real-DB smoke test before release.
- **Invite codes** as bare random strings are fine for v1 but lack revocation; if abuse happens, single-use is the next step.

---

## Patterns & Conventions Worth Preserving

- **Service constructor takes repo(s) + identity/role resolution + `IMemoryCache`** — every service follows the same shape.
- **Identity is `Authorization: Bearer <JWT>` + `IMemberIdentityService`**, never a client-supplied header. Controllers resolve the acting member via `IMemberIdentityService.GetMemberIdAsync()` (or the `ControllerBaseExtensions.GetMemberId()` helper) and pass the resulting `int` down to the service layer.
- **Block-level authorization stays in the service layer** (`GetMemberRoleInline(carnivalBlockId, memberId)`) — the block id is not available to an ASP.NET policy.
- **Platform admin** is the `AdminOnly` policy (`role: admin` claim) on `/api/v1/admin/*`.
- **Controllers translate exceptions**: `KeyNotFoundException` → 404, `UnauthorizedAccessException` → 401, `InvalidOperationException` → 400.
- **DTOs are `record`**; entity mappers are static `ToDTO` methods in `Mappers/XxxMapper.cs`.

---

## Active Projects

| Project | Goal | Owner | Timeline |
|---------|------|-------|----------|
| v1.0 API freeze | Lock endpoint surface, document breaking changes | @rafaelsnv | Q3 2026 |
| ~~Manager presence authorization~~ | TKT-002 + TKT-003 — ✅ shipped 2026-11-17; see Archive | @rafaelsnv | Done |
| ~~Image upload~~ | ✅ Resolved 2026-11-18 — upload endpoint + local volume storage shipped (`IImageUrlService`, `MediaController`); see Open Question Details | @rafaelsnv | Done |

---

### Resolved: 2026-11-17 — Manager presence authorization (TKT-002 / TKT-003)
- **Resolution**: Verified implemented in code — `MeetingPresenceService.AuthorizeForMemberAsync` ( MeetingPresenceService.cs:31-39 ) gates create/update/delete; `CreateAsync` honors an optional `memberId` on `MeetingPresenceCreate` (falls back to the JWT member), and `UpdateAsync`/`DeleteAsync` allow block owners/managers to act on any presence row of that block's meetings.
- **Learnings**: Block-level roles should live in the service layer (policies have no access to the block id); a shared `AuthorizeForMemberAsync` guard covered all three routes once placed in the service.

---

## Archive (Older Resolved Items)

### Resolved: 2026-11-17 — Manager presence authorization (TKT-002 / TKT-003) — ticket table closed
- Cross-refs: original ticket file `BACKEND_TICKETS.md` deleted 2026-11-17 (all tickets resolved); resolutions preserved in this Archive and `decisions.md`.

### Resolved: 2026-07-24 — Spoofable `X-Logged-Member` Header
- **Resolution**: Replaced by JWT Bearer auth — Supabase-issued JWT validated in `BlocoNaRua.Restful/Program.cs:69-96` (issuer, audience `authenticated`, signature); the header was removed from all controllers and the client API layer (zero occurrences in `.cs` files).
- **Replacement pattern**: `Authorization: Bearer <JWT>` → `sub` claim → `IMemberIdentityService.GetMemberIdAsync()` → `int memberId`.
- **Learnings**: Identity was stubbed for v1 to ship without an auth provider; deferring an auth provider is fine, deferring security is not.

### Resolved: 2026-07-24 — `service_role` Key in Client-Side Code Patterns
- **Resolution**: The key is backend-only. `BlocoNaRua.Services/Implementations/AuthService.cs` reads `Supabase:ServiceRoleKey` from configuration and calls Supabase through the `SupabaseAdmin` named `HttpClient`; `POST /api/v1/auth/login` proxies Supabase Auth and returns only the short-lived JWT + refresh token. Verified 2026-11-17.
- **Learnings**: A privileged key should never be reachable from a client pattern — even a "temporary" one; keep it behind the login proxy.

### Resolved: 2026-07-24 — Meeting Presence 500 (`memberId: 0`)
- **Resolution**: The symptom came from the untrusted, client-supplied identity path — removed together with the spoofable header. ⚠️ Assumed resolved with the JWT migration; needs re-verification under the current JWT-authenticated build (2026-11-17).
- **Learnings**: When a bug is fixed by removing the mechanism that caused it, record the re-verification explicitly — do not silently close it.

### Resolved: 2026-07-19 — Plaintext Token Storage (Flutter side)
- **Resolution**: Migrated to `flutter_secure_storage` (iOS Keychain / Android EncryptedSharedPreferences).
- **Learnings**: Token storage must be platform-native from day one; shared prefs were always wrong.

### Resolved: 2026-07-19 — Android `OnBackInvokedCallback` Warning
- **Resolution**: Added `android:enableOnBackInvokedCallback="true"` in Flutter `AndroidManifest.xml`.
- **Learnings**: Android 14+ expects explicit opt-in to the new back API.

---

## Onboarding Checklist

- [ ] Read `decisions.md` first — know the shape of the system
- [ ] Skim Technical Debt and Known Issues before touching a file
- [ ] Search this file for your feature before opening a PR — it may already have context
- [ ] If you fix something in this file, note the fix in the Archive below (resolved items too)

## Related Files

- `decisions.md` — Past decisions (`lookup/decisions.md`)
- `../concepts/business-domain.md` — Business context
- `../concepts/technical-domain.md` — Technical state
- `../concepts/business-tech-bridge.md` — Trade-off context
- `../concepts/identity-auth.md` — JWT / login / identity chain
- `../../../../BACKEND_TICKETS.md` — Resolved cross-team ticket record (Archive + repo-root copy)
