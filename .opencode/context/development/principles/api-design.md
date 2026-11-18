<!-- Context: development/api-design | Priority: low | Version: 3.2 | Updated: 2026-11-18 -->

# API Design Patterns

**Core Idea**: REST/GraphQL API design principles with actionable patterns for versioning, auth, and client integration. The companion `project-intelligence/guides/api-patterns.md` shows how BlocoNaRua concretely applies these.

---

## REST Essentials

**URLs**: Nouns not verbs → `/users`, `/users/123`  
**Methods**: GET (retrieve), POST (create), PUT/PATCH (update), DELETE (remove)  
**Status Codes**: 2xx success, 4xx client error, 5xx server error

### Standard Response Format
```json
{ "data": {...}, "meta": { "timestamp": "..." } }
{ "error": { "code": "...", "message": "..." } }
```

### Query Support
```
GET /users?status=active&sort=createdAt:desc&page=2&pageSize=20
```

---

## GraphQL

**Schema**: Types define shape; Query/Mutation expose operations  
**N+1 Fix**: Use DataLoader to batch requests

```graphql
type Query { user(id: ID!): User }
type Mutation { createUser(input: CreateUserInput!): User! }
```

---

## Versioning & Auth

**URL versioning**: `/v1/users`, `/v2/users` — **this is what BlocoNaRua uses** (`api/v1/<Controller>`).  
**JWT**: Token in `Authorization: Bearer <token>` header — this is what BlocoNaRua uses (API-issued HS256 JWT (Google OAuth), live since 2026-11-18).  
**RBAC**: Role checks via middleware → `authorize('admin')`. BlocoNaRua enforces block roles in the **service layer** (inline `GetMemberRole` per service, not a shared authorization service) and resolves the acting member via `IMemberIdentityService` — keep this in mind when comparing patterns.

---

## Quick Reference

| Pattern | Use |
|---------|-----|
| `/users?fields=id,name` | Field selection |
| `/users/123/posts` | Shallow nesting |
| `Deprecation: true` header | Deprecation signal |

---

## BlocoNaRua-Specific Notes

- Exception → status mapping is hand-coded in every controller (`KeyNotFoundException` → 404, `UnauthorizedAccessException` → 401, `InvalidOperationException` → 400). This is a deliberate deviation from "use Problem Details" — kept because the team preferred readable code over RFC 7807 boilerplate.
- Pagination is in-memory (`Skip().Take()`) over a 5-minute cached list, not via SQL `OFFSET`. Acceptable for v1 traffic.
- `DateTime` on the wire is `DateTime?` (UTC). Format: ISO-8601.

**Reference**: REST API Design Rulebook, GraphQL.org, OpenAPI Spec, `project-intelligence/guides/api-patterns.md`