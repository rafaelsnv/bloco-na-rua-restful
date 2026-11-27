<!-- Context: project-intelligence/api-patterns | Priority: high | Version: 3.1 | Updated: 2026-11-18 -->

# API Patterns

> REST API conventions used by BlocoNaRua. Documents the contract the **Flutter consumer** must respect, and the .NET implementation patterns that produce it.
> Verified against the code on 2026-11-17 (`swagger.json` at the repo root is the route ground truth).

## Authentication

Login is **Google OAuth 2.0**; the API then issues its own **HS256 JWT** (no external IdP). Every endpoint except the anonymous auth routes requires:

```http
Authorization: Bearer <JWT>
```

- The JWT is validated in `BlocoNaRua.Restful/Program.cs:85-109` — self-issued HS256; issuer/audience `bloconarua-api` (`Auth:Issuer` / `Auth:Audience`), signing key `Auth:SigningKey` (env-only, ≥ 32 bytes), lifetime validated.
- Login flow: `GET /api/v1/auth/google/challenge` (302 to Google) → `GET /api/v1/auth/google/complete` returns the token pair. `POST /api/v1/auth/login` and `POST /api/v1/auth/signup` (both form-urlencoded) are the password paths.
- The acting member is resolved from the `sub` claim (member UUID) via `IMemberIdentityService` → `Members.Uuid` → `int memberId`.
- Block-level roles (`Owner` / `Manager` / `Member`) are applied in the service layer, not by an ASP.NET policy.
- `/api/v1/admin/*` additionally requires the `role: admin` claim (policy `AdminOnly`, `Program.cs:125-129`).

Full chain: `../concepts/identity-auth.md`.

## Endpoint shape

| Aspect | Convention |
|--------|------------|
| Base path | `api/v1/` |
| Versioning | URL segment via `Asp.Versioning.Mvc`; default version `1.0` |
| Controller naming | Plural, matches the entity collection (`Members`, `Meetings`, `CarnivalBlocks`, `CarnivalBlockMembers`, `MeetingPresences`) |
| Auth header | `Authorization: Bearer <JWT>` (required unless noted) |
| Content-Type | `application/json` (request + response); login and signup are `application/x-www-form-urlencoded` |
| Date format | ISO-8601 (`DateTime.UtcNow`) |
| Casing | JSON responses are `camelCase` (ASP.NET Core web defaults) — `inviteCode`, not `InviteCode` |

## Endpoint map

All routes below require `Authorization: Bearer <JWT>` **except** the anonymous auth routes (`login`, `signup`, `google/challenge`, `google/complete`, `refresh`).

| Method | Route | Auth | Purpose |
|--------|-------|------|---------|
| `POST` | `/api/v1/auth/login` | none | Password login (form-urlencoded) — **200** with the token pair; bad credentials → **401** `{"message": "Invalid credentials"}` |
| `POST` | `/api/v1/auth/signup` | none | Create a password account (form-urlencoded `email`, `password`, `name`) — **201** with the token pair; duplicate email → **409**; invalid payload → **400** |
| `GET` | `/api/v1/auth/google/challenge` | none | Start Google OAuth — 302 to accounts.google.com |
| `GET` | `/api/v1/auth/google/complete` | none | OAuth callback — upserts member, returns `{ accessToken, refreshToken, expiresIn, tokenType }` |
| `POST` | `/api/v1/auth/refresh` | none | Rotate refresh token — returns a fresh token pair; old refresh token is dead on return |
| `POST` | `/api/v1/auth/logout` | Bearer | Revoke the member's refresh token (access tokens stay valid until expiry) |
| `POST` | `/api/v1/media/image` | Bearer | Upload an image (multipart, field `file`) — replaces the member's profile image; 413 oversized, 415 unsupported extension |
| `GET` | `/api/v1/Members` | Bearer | List members (`?page`, `?pageSize`, `?ids=1,2,3`) |
| `GET` | `/api/v1/Members/{id}` | Bearer | Member by id |
| `GET` | `/api/v1/Members/uuid/{uuid}` | Bearer | Member by UUID |
| `GET` | `/api/v1/Members/{id}/blocks` | Bearer | Blocks the member belongs to |
| `GET` | `/api/v1/Members/{id}/meetings` | Bearer | Meetings of the member's blocks |
| `POST` | `/api/v1/Members` | Bearer | Create member |
| `PUT` | `/api/v1/Members/{id}` | Bearer | Update member (self only) |
| `DELETE` | `/api/v1/Members/{id}` | Bearer | Delete member (self only) |
| `GET` | `/api/v1/CarnivalBlocks` | Bearer | List blocks (`?page`, `?pageSize`) |
| `GET` | `/api/v1/CarnivalBlocks/{id}` | Bearer | Block by id |
| `POST` | `/api/v1/CarnivalBlocks` | Bearer | Create block |
| `PUT` | `/api/v1/CarnivalBlocks/{id}` | Bearer | Update block (owner/manager) |
| `DELETE` | `/api/v1/CarnivalBlocks/{id}` | Bearer | Delete block (owner) |
| `GET` | `/api/v1/CarnivalBlockMembers` | Bearer | All memberships |
| `GET` | `/api/v1/CarnivalBlockMembers/block/{blockId}` | Bearer | Members of a block |
| `POST` | `/api/v1/CarnivalBlockMembers` | Bearer | Add membership (role) |
| `POST` | `/api/v1/CarnivalBlockMembers/join/{inviteCode}` | Bearer | Join a block by invite code |
| `PUT` | `/api/v1/CarnivalBlockMembers/{id}` | Bearer | Change a member's role |
| `DELETE` | `/api/v1/CarnivalBlockMembers/{id}` | Bearer | Remove a membership |
| `GET` | `/api/v1/Meetings` | Bearer | List meetings (`?page`, `?pageSize`) |
| `GET` | `/api/v1/Meetings/{id}` | Bearer | Meeting by id |
| `GET` | `/api/v1/Meetings/block/{blockId}` | Bearer | Meetings of a block |
| `POST` | `/api/v1/Meetings` | Bearer | Create meeting |
| `PUT` | `/api/v1/Meetings/{id}` | Bearer | Update meeting (owner/manager) |
| `DELETE` | `/api/v1/Meetings/{id}` | Bearer | Delete meeting (owner/manager) |
| `GET` | `/api/v1/MeetingPresences` | Bearer | List presences (`?page`, `?pageSize`) |
| `GET` | `/api/v1/MeetingPresences/{id}` | Bearer | Presence by id |
| `POST` | `/api/v1/MeetingPresences` | Bearer | Create presence |
| `PUT` | `/api/v1/MeetingPresences/{id}` | Bearer | Toggle presence |
| `DELETE` | `/api/v1/MeetingPresences/{id}` | Bearer | Delete presence |
| `DELETE` | `/api/v1/admin/signup-cleanup/{uuid}` | Bearer + `role: admin` | Hard-delete the member row via EF (incl. soft-deleted rows, by UUID) |

Notes:
- Swagger renders the auth routes as `/api/v1/Auth/...` (the `[controller]` token); ASP.NET routing is case-insensitive, so `/api/v1/auth/login` is the same endpoint.
- There is no `GET /api/v1/CarnivalBlockMembers/{id}` — use `GET /api/v1/CarnivalBlockMembers/block/{blockId}`.
- `DELETE /api/v1/admin/signup-cleanup/{uuid}` → `200 {"deleted": true}`, `404` when the member is unknown.

## Login & tokens

```http
GET /api/v1/auth/google/challenge HTTP/1.1
Host: api.bloconarua.com
```
→ `302` to accounts.google.com → Google redirects back → the API completes the OAuth state:

```http
GET /api/v1/auth/google/complete HTTP/1.1
Host: api.bloconarua.com
```
→ `200 OK`
```json
{
  "accessToken": "eyJhbGciOi...",
  "refreshToken": "opaque-base64url...",
  "expiresIn": 3600,
  "tokenType": "bearer"
}
```

- `POST /api/v1/auth/login` (form-urlencoded `email` + `password`) → **200** with the token pair; unknown email, wrong password and a non-password account all answer **401** `{"message": "Invalid credentials"}` (neutral — no account enumeration).
- `POST /api/v1/auth/signup` (form-urlencoded `email`, `password`, `name`) → **201** `{ "access_token", "refresh_token", "expires_in", "token_type" }`; the email is normalized to lower case; a duplicate email answers **409** `{"message": "Email already registered"}`.
- Refresh: `POST /api/v1/auth/refresh` with `{ "refreshToken": "..." }` — **rotation**: each call returns a brand-new token pair and invalidates the stored refresh token immediately. Unknown/expired token → `401 {"message": "Invalid refresh token"}`.
- Logout: `POST /api/v1/auth/logout` (Bearer) → `200 { "revoked": true }` — revokes the refresh token only.
- Invalid input on `complete` (missing `sub`/`email`) → `400`; inactive member → `401`.

## Request/response example

```http
POST /api/v1/CarnivalBlocks HTTP/1.1
Host: api.bloconarua.com
Content-Type: application/json
Authorization: Bearer eyJhbGciOi...

{ "name": "Bloco da Esquina", "ownerId": 42, "carnivalBlockImage": "https://..." }
```
→ `201 Created`
```json
{
  "id": 7,
  "ownerId": 42,
  "name": "Bloco da Esquina",
  "inviteCode": "A8K2QZ9X",
  "managersInviteCode": "M3P7H4L1",
  "carnivalBlockImage": "https://...",
  "createdAt": "2026-11-17T18:23:01Z",
  "updatedAt": "2026-11-17T18:23:01Z"
}
```

## Pagination

Optional query string on list endpoints:
```
GET /api/v1/Members?page=2&pageSize=20
```
- Both params optional. If either is missing, returns the full list.
- Page is 1-indexed; skip/take is applied in memory after the full fetch.
- `GET /api/v1/Members` also accepts `?ids=1,2,3` (comma-separated) to filter by id.

## Error responses

```http
HTTP/1.1 404 Not Found
Content-Type: text/plain

Carnival block member does not exist.
```
- When the controller maps a `KeyNotFoundException` the body is a plain string (`ex.Message`); other paths return an empty 404 body.
- Missing or invalid token → `401`. Valid token without `role: admin` on `/api/v1/admin/*` → `403`.
- For unhandled exceptions, the global handler returns JSON:
  ```json
  { "StatusCode": 500, "DateTime": "...", "Message": "Internal Server Error.", "Detailed": "<ex.Message>" }
  ```
  (written directly by `JsonSerializer.Serialize`, hence `PascalCase` keys — unlike the camelCase response bodies.)

## Flutter consumer expectations (companion repo)

Login → store the token → send it on every subsequent call:

```dart
// 1. login — open GET /api/v1/auth/google/challenge (browser flow);
//    the completion call returns { accessToken, refreshToken, expiresIn, tokenType }
//    POST /api/v1/auth/signup then POST /api/v1/auth/login (both form-urlencoded)
//    also return a token pair
final token = tokenPair['accessToken'] as String;
await secureStorage.write(key: 'access_token', value: token); // flutter_secure_storage

// 2. send the bearer token
final response = await dio.get(
  '/api/v1/Members/$id',
  options: Options(headers: {'Authorization': 'Bearer $token'}),
);
if (response.statusCode == 200) return MemberResponse.fromJson(response.data);
if (response.statusCode == 404) return null;
throw ApiException(response.statusCode, response.data);
```

```dart
final response = await dio.put(
  '/api/v1/CarnivalBlocks/$id',
  data: CarnivalBlockUpdate(...).toJson(),
  options: Options(headers: {'Authorization': 'Bearer $token'}),
);
```

- Better: install a Dio interceptor that reads the stored token and sets the header once — every call is then authenticated.
- The token expires after `expiresIn` seconds. Refresh via `POST /api/v1/auth/refresh` with the stored refresh token — each call rotates it (the old refresh token dies when the new pair is returned).
- Response bodies are camelCase — map them accordingly.

## 📂 Codebase References

- **Controllers**: `BlocoNaRua.Restful/Controllers/`
- **Auth + JWT setup**: `BlocoNaRua.Restful/Program.cs` (85-129)
- **Google OAuth + refresh rotation + revocation**: `BlocoNaRua.Restful/Controllers/AuthController.cs`, `BlocoNaRua.Services/Implementations/AuthService.cs`, `BlocoNaRua.Services/Implementations/TokenService.cs`
- **Identity resolution**: `BlocoNaRua.Services/Implementations/MemberIdentityService.cs`, `BlocoNaRua.Restful/Extensions/ControllerBaseExtensions.cs`
- **DTOs**: `BlocoNaRua.Restful/Models/<Entity>/`
- **Mappers**: `BlocoNaRua.Restful/Mappers/`
- **Routing/versioning**: `BlocoNaRua.Restful/Program.cs`
- **OpenAPI dump**: `swagger.json` (repo root)
- **Generic API design**: `../../development/principles/api-design.md`

## Related Files

- `../concepts/identity-auth.md` — Login → token → claims → memberId chain, admin policy
- `../concepts/technical-domain.md` — Stack and architecture
- `../examples/component-patterns.md` — Controller / Service / Repository patterns
- `../lookup/decisions.md` — Architectural decisions and rationale
- `../lookup/naming.md` — Naming reference
- `../lookup/living-notes.md` — Open issues and debt
