<!-- Context: project-intelligence/identity | Priority: high | Version: 1.1 | Updated: 2026-11-18 -->

# Identity & Authentication

> Canonical reference for how BlocoNaRua authenticates a caller and resolves that caller to a `Member`.
> Auth provider: **API-issued JWT (HS256) + Google OAuth 2.0**. Token format: **JWT (Bearer)**. Verified against the code on 2026-11-18.

## Quick Reference

- **Purpose**: Single source of truth for the login → token → claims → memberId chain and the admin policy.
- **Update When**: auth provider change, token/claim shape change, new policy, new identity lookup path.
- **Scope**: Backend only. The Flutter client lives in a separate repo.

---

## The Chain (end to end)

```
① GOOGLE LOGIN   GET /api/v1/auth/google/challenge
                      │  [AllowAnonymous] → Challenge("Google") → 302 to accounts.google.com
                      ▼
                  Google OAuth 2.0  →  302 back to /api/v1/auth/google/callback
                      │  (the "GoogleState" cookie carries the OAuth state)
                      ▼
                  GET /api/v1/auth/google/complete
                      │  AuthenticateAsync("GoogleState") → GooglePrincipal (sub, email, name, picture)
                      ▼
                  AuthService.ProcessGoogleCallbackAsync
                      │  upsert members by (provider='google', provider_id=sub)
                      ▼
                  TokenService.IssueTokens → { access_token, refresh_token, expires_in, token_type }

② BEARER TOKEN   Authorization: Bearer <access_token>   on every subsequent request

③ VALIDATION     JwtBearer handler — BlocoNaRua.Restful/Program.cs:85-109
                      │  HS256, self-issued (no JWKS discovery, no outbound call)
                      │  issuer/audience = Auth:Issuer / Auth:Audience (default "bloconarua-api")
                      │  IssuerSigningKey = Auth:SigningKey (base64 or raw UTF8, ≥ 32 bytes)
                      ▼
                  HttpContext.User  (ClaimsPrincipal, MapInboundClaims = false)

④ RESOLUTION     "sub" claim (member UUID)
                      │
                      ▼
                  IMemberIdentityService.GetMemberIdAsync()
                      │  Members table lookup by UUID
                      ▼
                  int memberId  →  passed to services as the acting member

⑤ AUTHORIZATION  [Authorize] on member-facing controllers
                  [Authorize(Policy = "AdminOnly")] on AdminController (role: admin claim)
```

---

## 1. Google OAuth — the only working login path

**Controller** — `BlocoNaRua.Restful/Controllers/AuthController.cs` (class-level `[AllowAnonymous]`)

### 1.1 `GET /api/v1/auth/google/challenge`

- Issues `Challenge(new AuthenticationProperties { RedirectUri = "/api/v1/auth/google/complete" }, "Google")` → **302** to `accounts.google.com`.
- The Google handler (`Program.cs:118-124`) reads `Authentication:Google:ClientId` / `ClientSecret` and has `CallbackPath = "/api/v1/auth/google/callback"` — **that** is the URI registered in Google Cloud Console. The handler's own callback path is not an API endpoint; it replays the state into the `GoogleState` cookie and redirects to `RedirectUri`.

### 1.2 `GET /api/v1/auth/google/complete`

- `AuthenticateAsync("GoogleState")` → the Google principal. The state cookie scheme (`Program.cs:110-117`, cookie `bnr_google_state`, HttpOnly, SameSite=Lax, 10 min, no sliding expiration) is a **short-lived OAuth state carrier only** — the API itself authenticates exclusively with the Bearer JWT.
- Reads claims: `sub` (Google subject), `email`, `name`, `urn:google:picture` (avatar URL). Missing `sub` or `email` → `400`.
- The state cookie is single use → `SignOutAsync("GoogleState")` before tokens are issued.
- Success → **200** with the token pair: `{ access_token, refresh_token, expires_in, token_type }` (`token_type = "bearer"`).

**Service** — `BlocoNaRua.Services/Implementations/AuthService.cs` (`ProcessGoogleCallbackAsync`)

- Upserts the member by `(provider, provider_id)` via `IMembersRepository.GetByProviderAsync`:
  - **New** → `MemberEntity` with `Provider = "google"`, `ProviderId = <sub>`, `Uuid = Guid.NewGuid()`, `IsActive = true`, `EmailConfirmed = true`, `ProfileImage = <picture>` (empty string when Google sends none).
  - **Existing** → reused; an inactive member is rejected with `UnauthorizedAccessException` → `401`. The avatar is only filled in when the member has none, so a Google account without a picture never wipes an avatar uploaded earlier.
- Then `ITokenService.IssueTokens(member)` + `membersRepository.UpdateAsync`.

---

## 2. Password login & signup — `POST /api/v1/auth/login` / `POST /api/v1/auth/signup`

- Login route `api/v{version:apiVersion}/[controller]` → `POST /api/v1/auth/login`, `[Consumes("application/x-www-form-urlencoded")]`, body `LoginRequest` (email + password).
- Signup route → `POST /api/v1/auth/signup`, `[FromBody] SignupRequest` (email + password + name, JSON).
- `AuthService.LoginAsync` looks the member up by email (`IMembersRepository.GetByEmailAsync`, `IgnoreQueryFilters` so soft-deleted rows are found), then checks `provider = "password"` and verifies the password with `IPasswordHasher<MemberEntity>` (`Microsoft.AspNetCore.Identity.PasswordHasher`, ASP.NET Identity format, registered as a singleton in `ServicesExtensions.cs`).
- Every failure is neutral: unknown email, wrong password and a Google-provider account all raise `UnauthorizedAccessException("Invalid credentials")` → **401** (no account enumeration). An inactive member gets `"Member account is inactive."`.
- `AuthService.SignupAsync` normalizes the email to lower case, stores it as `provider = "password"` / `provider_id = <lowercased email>` (so it matches the unique `ux_members_provider` index), `email_confirmed = false`, `is_active = true` and a fresh `uuid`; it returns `null` when the email is already registered → the controller answers **409**.
- Both endpoints issue the same token pair shape as the Google callback: `{ access_token, refresh_token, expires_in, token_type }`.

---

## 3. JWT validation — `BlocoNaRua.Restful/Program.cs:85-109`

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidIssuer              = authIssuer,          // Auth:Issuer, default "bloconarua-api"
            ValidateAudience         = true,
            ValidAudience            = authAudience,       // Auth:Audience, default "bloconarua-api"
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = new SymmetricSecurityKey(ResolveSigningKeyBytes(configuration["Auth:SigningKey"])),
            ValidateLifetime         = true,
            ClockSkew                = TimeSpan.FromMinutes(5)
        };
        // OnAuthenticationFailed → logs "JWT auth failed: {Error}"
    })
```

| Parameter | Value |
|-----------|-------|
| Algorithm | **HS256**, symmetric, self-issued — no JWKS / no OIDC discovery / no outbound call |
| ValidIssuer | `Auth:Issuer` (default `bloconarua-api`) |
| ValidAudience | `Auth:Audience` (default `bloconarua-api`) |
| IssuerSigningKey | `Auth:SigningKey` — base64 first, raw UTF-8 fallback; **≥ 32 bytes**, otherwise a loud `InvalidOperationException` at startup |
| `MapInboundClaims` | `false` → claim names stay as issued (`sub`, `email`, `provider`, `role` are **not** remapped to `ClaimTypes.*`) |
| Lifetime | validated (`exp`), `ClockSkew` 5 min |
| Missing key | throws at startup — there is **no baked-in default secret** |

`Program.ResolveSigningKeyBytes` mirrors `TokenService.DecodeSigningKey` byte-for-byte, so the issuer and the validator always agree on the key.

Pipeline order: `app.UseAuthentication()` → `app.UseAuthorization()` → `UseStaticFiles("/uploads")` → exception handler → `app.MapControllers()`.

Swagger declares the `Bearer` security scheme, so the token can be pasted into `/swagger`.

**Token shape** (issued by `TokenService.CreateAccessToken`):

| Claim | Value |
|-------|-------|
| `sub` | member `Uuid` (GUID) — the identity anchor |
| `email` | member email |
| `provider` | `google` (or `password` fallback when the member row has no provider yet) |
| `role` | `admin` or `member` — see §6 |

---

## 4. Refresh & logout

### 4.1 `POST /api/v1/auth/refresh` (anonymous)

- Body `{ "refreshToken": "..." }` (`RefreshRequest`).
- `AuthService.RotateRefreshTokenAsync` → `ITokenService.ValidateRefreshToken` (lookup by the stored value, check `refresh_token_expires_at`) → `RotateRefreshToken` → `IssueTokens` → `UpdateAsync`.
- **Rotation**: every call issues a brand-new random refresh token and overwrites the stored one, so the old token is dead the moment the new pair is returned. The new token never equals the old one.
- Unknown / revoked / expired token → `null` → **401** `{ message = "Invalid refresh token" }`.

### 4.2 `POST /api/v1/auth/logout` (`[Authorize]`)

- Reads the `sub` claim → member `Uuid` → `AuthService.RevokeRefreshTokenAsync` → `ITokenService.RevokeRefreshToken` clears `refresh_token` / `refresh_token_expires_at`.
- Access tokens are stateless and keep working until they expire (`Auth:AccessTokenExpirationSeconds`); only the refresh token is revoked.
- Success → **200** `{ revoked = true }`.

**Refresh token shape** — opaque, 64 random bytes encoded base64url, stored **plaintext** on `members.refresh_token` (no hashing) with `members.refresh_token_expires_at`; default lifetime 30 days (`Auth:RefreshTokenExpirationDays`).

---

## 5. Identity Resolution — `sub` claim → `memberId`

Unchanged from the JWT era: the `sub` claim is the **member's UUID** (`members.uuid`).

**Service** — `BlocoNaRua.Services/Implementations/MemberIdentityService.cs` (`IMemberIdentityService`, registered scoped in `ServicesExtensions.cs`)

```csharp
var subClaim = _httpContextAccessor.HttpContext?.User.FindFirst("sub");
if (subClaim is null) throw new UnauthorizedAccessException("Missing 'sub' claim in JWT.");
if (!Guid.TryParse(subClaim.Value, out var uuid)) throw new UnauthorizedAccessException("Invalid 'sub' claim format.");
return await _membersRepository.GetByUuidAsync(uuid, CancellationToken.None);   // → MemberEntity
```

`GetMemberIdAsync()` returns `member.Id` (int) or throws `UnauthorizedAccessException("Member not found from JWT claim.")` — which the controllers map to **401**, not 500.

**Controller entry point** — `BlocoNaRua.Restful/Extensions/ControllerBaseExtensions.cs`

```csharp
public static int GetMemberId(this ControllerBase controller, IMemberIdentityService memberIdentityService)
```

- Verifies the `sub` claim exists, then delegates to `IMemberIdentityService` and unwraps the `AggregateException` into the inner `UnauthorizedAccessException`.
- Controllers use it directly or call `_memberIdentityService.GetMemberIdAsync()` — pick one style and stay consistent; do not mix.

**Consumers** — every `[Authorize]` controller (`Members`, `CarnivalBlocks`, `CarnivalBlockMembers`, `Meetings`, `MeetingPresences`, `Media`) resolves the acting member this way and passes the resulting `int` down to the service layer. The service layer then applies **per-block roles** (`Owner` / `Manager` / `Member`) via a private `GetMemberRoleInline(carnivalBlockId, memberId)` helper. Block-level authorization is business logic, **not** an ASP.NET policy — it needs the block id, which policies do not have.

---

## 6. Authorization — the `AdminOnly` policy

**Policy registration** — `BlocoNaRua.Restful/Program.cs:125-129`

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireClaim("role", "admin"));
});
```

**Usage** — `BlocoNaRua.Restful/Controllers/AdminController.cs`

```csharp
[ApiController]
[Route("api/v{version:apiVersion}/admin")]
[Authorize(Policy = "AdminOnly")]
public class AdminController(IAdminService adminService) : ControllerBase
```

- Protects everything under **`/api/v1/admin/*`** (currently `DELETE /api/v1/admin/signup-cleanup/{uuid}`).
- The `role` claim must be `admin`. It is **derived at token-issue time** by `TokenService.ResolveRole` from the `Auth:AdminEmails` config (comma-separated list): a member whose email is listed gets `role=admin`, everyone else gets `role=member`. There is no admin flag on the member row.
- Requests without the claim are rejected with `401`/`403` by the authorization middleware, never reaching the controller.

Two distinct authorization layers — do not conflate them:

| Layer | Mechanism | Scope |
|-------|-----------|-------|
| Platform admin | `AdminOnly` policy (`role: admin` claim) | `/api/v1/admin/*` |
| Block role | `GetMemberRoleInline` in the service layer (`Owner`/`Manager`/`Member` row in `CarnivalBlockMembers`) | Per-block operations |

---

## 7. Media uploads — `POST /api/v1/media/image`

**Controller** — `BlocoNaRua.Restful/Controllers/MediaController.cs`

- Route `api/v{version:apiVersion}/[controller]` → `POST /api/v1/media/image`, `[Authorize]`, `[Consumes("multipart/form-data")]`, form field `file` (`IFormFile`).
- Resolves the acting member via `IMemberIdentityService` (401 on failure), loads the member, saves the file, then replaces `member.ProfileImage` with the stored path.
- Errors: empty file → `400`; unsupported extension → `415`; oversized → `413`.
- The previous `/uploads/...` file of that member is deleted best-effort before the new one is set; external/legacy paths are left untouched.

**Service** — `BlocoNaRua.Services/Implementations/ImageUrlService.cs`

- Only the **extension** of the client-supplied name is used; the stored name is always `{guid:N}{ext}`, so caller-supplied path segments can never reach the file system.
- Extension must be in `Uploads:AllowedExtensions` (default `.jpg,.jpeg,.png,.webp`); size limit `Uploads:MaxSizeBytes` (default 5 MB). Seekable streams are rejected **before** any disk write; non-seekable streams are cut off while copying.
- Writes into `Uploads:BasePath` (default `/app/uploads`, a Dokploy volume mount) and returns the public path `/uploads/{guid}{ext}`.

**Serving** — `Program.cs:165-178`: `UseStaticFiles` with `RequestPath = "/uploads"` and a `PhysicalFileProvider(uploadsBasePath)`. **Public read** (avatars are not sensitive). If the directory cannot be created/served, the API logs a warning and keeps running without static files.

---

## 8. Schema & migration

`members` gained the auth columns (`BlocoNaRua.Domain/Entities/MemberEntity.cs`, `BlocoNaRua.Data/Mapping/MemberConfiguration.cs`):

| Column | Type | Notes |
|--------|------|-------|
| `provider` | text? | `google` for OAuth members; `password` for API-owned password accounts |
| `provider_id` | text? | the provider's subject id (`sub` for Google, the lower cased email for password accounts) |
| `password_hash` | text? | ASP.NET Identity hash (`IPasswordHasher<MemberEntity>`), set on password signups |
| `refresh_token` | text? | opaque base64url token, stored plaintext |
| `refresh_token_expires_at` | timestamptz? | refresh-token expiry |
| `email_confirmed` | bool | defaults `false`; Google members are created with `true` |

Indexes (EF mapping + SQL):

- `ux_members_provider` — unique on `(provider, provider_id)`.
- `ux_members_refresh_token` — unique on `(refresh_token)` **filtered** (`WHERE refresh_token IS NOT NULL`).

**Migration** — `sql/migrations/001_auth_columns.sql` — idempotent (`ADD COLUMN IF NOT EXISTS` / `CREATE UNIQUE INDEX IF NOT EXISTS`), schema-only. It deliberately does **not** migrate data: existing Supabase users are dropped and re-register through Google.

> The database is **Postgres** — self-hosted on Dokploy (docker-compose `bloconarua-postgres-*`, postgres:18); only the **auth provider** moved in-house. The connection string is unchanged (`ConnectionStrings:DefaultConnection`, or the `POSTGRESQLCONNSTR_SupabaseDB` env var in `BlocoNaRua.Data/Extensions/DataExtensions.cs` — the legacy env-var name persists; it points at the self-hosted host now).

---

## 9. Configuration keys

| Key | Required | Default | Purpose |
|-----|----------|---------|---------|
| `Auth__SigningKey` | **yes** | — | HS256 secret, base64 or raw UTF-8, **≥ 32 bytes**. Env-only; a missing/short key is a loud startup failure |
| `Auth__Issuer` | no | `bloconarua-api` | JWT issuer (validated + issued) |
| `Auth__Audience` | no | `bloconarua-api` | JWT audience (validated + issued) |
| `Auth__AdminEmails` | no | empty | comma-separated emails that receive `role=admin` |
| `Auth__AccessTokenExpirationSeconds` | no | `3600` | access-token lifetime |
| `Auth__RefreshTokenExpirationDays` | no | `30` | refresh-token lifetime |
| `Authentication__Google__ClientId` | yes (Google login) | — | Google OAuth client id |
| `Authentication__Google__ClientSecret` | yes (Google login) | — | Google OAuth client secret |
| `Uploads__BasePath` | no | `/app/uploads` | upload directory (volume mount) |
| `Uploads__MaxSizeBytes` | no | `5242880` (5 MB) | max upload size |
| `Uploads__AllowedExtensions` | no | `.jpg,.jpeg,.png,.webp` | allowed upload extensions |

The `Supabase__*` keys are **gone** — no code reads them any more (the `Supabase` section was removed from `appsettings.Development.json` together with the `service_role` key it carried).

> 🔐 **`Auth:SigningKey` is env-only.** It is the one secret that lets anyone mint a valid admin token, so it must never be committed to the repo, echoed in a response, or shipped to the client. **Rotating it invalidates every issued session** (all access tokens and all refresh tokens) — every user has to log in again. That is the accepted trade-off at this stage (see the plan's risk section).

---

## 10. Error logging — EF, no Supabase REST

**Service** — `BlocoNaRua.Services/Implementations/ApiErrorLoggerService.cs` (implements `IErrorLogger`)

- Writes straight to the `api_errors` table through EF Core: `ApiErrorEntity` (`BlocoNaRua.Domain/Entities/ApiErrorEntity.cs`, mapping in `BlocoNaRua.Data/Mapping/ApiErrorConfiguration.cs`, exposed as `AppDbContext.ApiErrors`).
- Same `IErrorLogger` contract as before (`LogAsync`, `LogWarningAsync`, `LogErrorAsync`, `LogCriticalAsync`) → the exception handler and `ImageUrlService` keep their call sites unchanged.
- Logging never throws: a failed insert is caught and written to the console instead of breaking the request pipeline.

**Admin service** — `BlocoNaRua.Services/Implementations/AdminService.cs` no longer calls a Supabase admin API: it hard-deletes the member row through `IMembersRepository.HardDeleteAsync`, locating it with `GetByUuidIgnoreFiltersAsync` (soft-deleted members are hidden by the `IsActive` query filter, but cleanup must still find them).

---

## 11. Testing — `TestAuthHandler` bypass

Integration tests cannot mint real Google identities, so the test host swaps the auth scheme.

| File | Role |
|------|------|
| `BlocoNaRua.Tests/Helpers/BlocoNaRuaWebApplicationFactory.cs` | `WebApplicationFactory<Program>`; env `Testing`; swaps EF Core for InMemory; registers `TestCurrentMemberHolder` singleton; registers the `TestAuth` scheme; `UseSetting`s for the new auth/upload config |
| `BlocoNaRua.Tests/Helpers/TestAuthHandler.cs` | `AuthenticationHandler<TestAuthOptions>` — injects a `ClaimsPrincipal` carrying a **`sub` claim** and returns `AuthenticateResult.Success`; JWT validation is short-circuited |
| `BlocoNaRua.Tests/Infrastructure/IntegrationTestBase.cs` | Seeds a member into the InMemory DB, then calls `SetCurrentMember(uuid)` so the holder feeds the `sub` claim |
| `BlocoNaRua.Tests/Integration/*.cs` | The integration tests themselves |
| `BlocoNaRua.Tests/Services/TokenServiceTests.cs`, `BlocoNaRua.Tests/Controllers/MediaControllerTests.cs`, `BlocoNaRua.Tests/Integration/MediaApiIntegrationTests.cs` | Coverage for the new token / upload behaviour |

- `TestCurrentMemberHolder.CurrentMemberSub` (a GUID) is the single source of the test identity; `TestAuthHandler` reads it per request, falling back to `TestAuthOptions.Sub`.
- The seeded member's UUID **must** equal the injected `sub`, otherwise `MemberIdentityService` throws `UnauthorizedAccessException` ("Member not found from JWT claim.").
- `TestAuthHandler` is **test-only** — it is not registered in `Program.cs`.

**Config the factory must supply** (the host resolves these at startup — without them it fails before serving any request):

```
Auth:SigningKey              = "dGVzdC1zaWduaW5nLWtleS1mb3ItYm5yLXRlc3RzLTMyIQ=="   (32-byte base64 test key)
Auth:Issuer                  = "bloconarua-api"
Auth:Audience                = "bloconarua-api"
Authentication:Google:ClientId     = "test-client"
Authentication:Google:ClientSecret = "test-secret"
Uploads:BasePath             = <temp folder>          (tests never touch the real volume)
```

- The **real Google OAuth roundtrip is untested** — it needs real Google credentials, so it is not exercised by the suite. Verify it in staging following the smoke-test checklist of the migration plan (§6): challenge → 302, callback → token pair, refresh rotation, revoked-token → 401, upload → 200/413/415, static read of `/uploads/{file}`.

See the *Integration Test Pattern* section of `examples/component-patterns.md` for the copy-paste snippets.

---

## 12. Deprecated history

> **Historical note (removed code — do not reintroduce):**
> Before 2026-07-24 the API took the acting member from a client-supplied header, `X-Logged-Member: <int>`. Any caller could spoof it and act as any member (privilege escalation), so it was **replaced by JWT Bearer auth**. The header no longer exists anywhere in the codebase (zero occurrences in `.cs` files); it survives only in this note and in the deprecated-decision table of `lookup/decisions.md`.

> **Historical note (removed provider — do not reintroduce):**
> Until 2026-11-18 the API proxied **Supabase Auth** for password login and validated Supabase-issued JWTs through OIDC discovery. That provider is **gone**: no `SupabaseAuth` HTTP client, no `service_role` key, no `Authority`/`MetadataAddress` in the JWT options, no Supabase REST call for error logging. The API now issues and validates its own HS256 tokens. The `Supabase` section of `appsettings.Development.json` (which carried a committed `service_role` key) and the dead `BlocoNaRua.Tests/appsettings.SupabaseTesting.json` were removed. The only Supabase traces left are the **database host** (the connection string and the `POSTGRESQLCONNSTR_SupabaseDB` env var) — that is the datastore, not the identity provider.

---

## File Map

| Concern | File |
|---------|------|
| Auth endpoints (login, signup, google challenge/complete, refresh, logout) | `BlocoNaRua.Restful/Controllers/AuthController.cs` |
| Google callback upsert + refresh rotation + revocation | `BlocoNaRua.Services/Implementations/AuthService.cs` |
| Auth contracts (`LoginRequest`, `LoginResponse`) | `BlocoNaRua.Services/Interfaces/IAuthService.cs` |
| Access/refresh token issuing, `role` claim from `Auth:AdminEmails` | `BlocoNaRua.Services/Implementations/TokenService.cs`, `BlocoNaRua.Services/Interfaces/ITokenService.cs` |
| JWT validation, `GoogleState` cookie scheme, Google handler, `AdminOnly` policy, `/uploads` static files | `BlocoNaRua.Restful/Program.cs` (73-129, 165-178) |
| `sub` → memberId lookup | `BlocoNaRua.Services/Implementations/MemberIdentityService.cs`, `BlocoNaRua.Services/Interfaces/IMemberIdentityService.cs` |
| Controller helper | `BlocoNaRua.Restful/Extensions/ControllerBaseExtensions.cs` |
| Image upload endpoint | `BlocoNaRua.Restful/Controllers/MediaController.cs` |
| Image storage (validation, save, delete) | `BlocoNaRua.Services/Implementations/ImageUrlService.cs`, `BlocoNaRua.Services/Interfaces/IImageUrlService.cs` |
| EF-backed error logging (replaced the Supabase REST logger) | `BlocoNaRua.Services/Implementations/ApiErrorLoggerService.cs`, `BlocoNaRua.Domain/Entities/ApiErrorEntity.cs`, `BlocoNaRua.Data/Mapping/ApiErrorConfiguration.cs` |
| Admin routes + EF hard delete | `BlocoNaRua.Restful/Controllers/AdminController.cs`, `BlocoNaRua.Services/Implementations/AdminService.cs` |
| Members repository (provider lookup, hard delete, ignore filters) | `BlocoNaRua.Data/Repositories/MembersRepository.cs`, `BlocoNaRua.Data/Repositories/Interfaces/IMembersRepository.cs` |
| Member entity + auth columns and indexes | `BlocoNaRua.Domain/Entities/MemberEntity.cs`, `BlocoNaRua.Data/Mapping/MemberConfiguration.cs` |
| Service registrations | `BlocoNaRua.Services/Extensions/ServicesExtensions.cs` |
| Idempotent auth migration SQL | `sql/migrations/001_auth_columns.sql` |
| Test auth bypass + test config | `BlocoNaRua.Tests/Helpers/TestAuthHandler.cs`, `BlocoNaRua.Tests/Helpers/BlocoNaRuaWebApplicationFactory.cs`, `BlocoNaRua.Tests/Infrastructure/IntegrationTestBase.cs` |

## Related Context

- `concepts/technical-domain.md` — stack, layers, conventions.
- `guides/api-patterns.md` — consumer-facing API conventions (Bearer usage, `/auth/*`, `/admin`).
- `examples/component-patterns.md` — integration-test pattern.
- `lookup/decisions.md` — decision log (JWT test bypass 2026-07-24; deprecated header entry).
- `lookup/living-notes.md` — open identity issues.
