<!-- Context: project-intelligence/navigation | Priority: high | Version: 3.3 | Updated: 2026-11-18 -->

# Project Intelligence — Navigation

> Start here for quick project understanding. These files bridge business and technical domains.

## What is BlocoNaRua?

A **.NET 8 RESTful API** that manages Brazilian carnival blocks (street groups), their members, and meetings. Consumed by a Flutter mobile client (separate repo). Self-hosted Postgres (Dokploy) is the data store. Five entities, three roles, **API-issued JWT (Google OAuth + HS256)** for identity.

## Folder map

```
.opencode/context/
├── paths.json                      # Path config for context loading
├── project-intelligence/           # THIS folder — domain-specific knowledge
│   ├── navigation.md               # ← you are here
│   ├── concepts/                   # Core domain understanding
│   │   ├── business-domain.md      # What the project is for
│   │   ├── technical-domain.md     # Stack, architecture, patterns (.NET)
│   │   ├── identity-auth.md        # Login → JWT → memberId → admin policy
│   │   └── business-tech-bridge.md # Feature → code mapping
│   ├── examples/                  # Working code patterns
│   │   └── component-patterns.md  # Entity, repo, service, controller
│   ├── guides/                    # Step-by-step workflows
│   │   └── api-patterns.md       # API conventions (Flutter consumer)
│   └── lookup/                   # Quick reference
│       ├── naming.md              # Naming conventions
│       ├── decisions.md          # Architectural decisions + rationale
│       └── living-notes.md       # Active issues, debt, open questions
├── development/                   # Generic development standards
│   └── principles/
│       ├── api-design.md          # REST/GraphQL design principles
│       └── clean-code.md          # Universal clean-code guidance
└── (mcp-tools-figma.md was removed — not applicable to a backend-only repo)
```

## Quick routes

| What you need | File | Description |
|---------------|------|-------------|
| Understand the "why" | `concepts/business-domain.md` | Brazilian carnival context, users, value |
| Stack & patterns | `concepts/technical-domain.md` | .NET 8, EF Core, layered architecture, Docker |
| Auth & identity | `concepts/identity-auth.md` | Login → JWT → memberId → admin policy |
| Feature → code map | `concepts/business-tech-bridge.md` | How each business need is implemented |
| Why a choice was made | `lookup/decisions.md` | Identity layer, caching, layout, etc. |
| Active issues | `lookup/living-notes.md` | Image upload, v1 freeze, etc. |
| API conventions (consumer side) | `guides/api-patterns.md` | Flutter client reference |
| Component patterns | `examples/component-patterns.md` | Repository / service / state |
| Naming | `lookup/naming.md` | Naming reference for code generation |
| Generic REST design | `../../development/principles/api-design.md` | REST theory |
| Generic clean code | `../../development/principles/clean-code.md` | Universal standards |

## Endpoints

All routes live under `api/v1/`. Every route requires `Authorization: Bearer <JWT>` except the anonymous auth routes (`POST /api/v1/auth/login`, `POST /api/v1/auth/signup`, `GET /auth/google/challenge`, `GET /auth/google/complete`, `POST /auth/refresh`).

| Area | Routes | Auth |
|------|--------|------|
| Auth | `GET /api/v1/auth/google/challenge` → `GET /api/v1/auth/google/complete` (returns token pair), `POST /api/v1/auth/refresh` (rotation), `POST /api/v1/auth/logout` (Bearer) | **none** for challenge/complete/refresh; Bearer for logout — Google OAuth + API-issued HS256 tokens |
| Media | `POST /api/v1/media/image` (multipart upload, 413/415 validation) | Bearer JWT |
| Admin | `DELETE /api/v1/admin/signup-cleanup/{uuid}` | Bearer JWT **+ `role: admin`** (`AdminOnly` policy) |
| Members | `GET/POST /api/v1/Members`, `GET/PUT/DELETE /api/v1/Members/{id}`, `GET /api/v1/Members/uuid/{uuid}`, `GET /api/v1/Members/{id}/blocks`, `GET /api/v1/Members/{id}/meetings` | Bearer JWT |
| Carnival blocks | `GET/POST /api/v1/CarnivalBlocks`, `GET/PUT/DELETE /api/v1/CarnivalBlocks/{id}` | Bearer JWT |
| Block members | `GET/POST /api/v1/CarnivalBlockMembers`, `GET /api/v1/CarnivalBlockMembers/block/{blockId}`, `POST /api/v1/CarnivalBlockMembers/join/{inviteCode}`, `PUT/DELETE /api/v1/CarnivalBlockMembers/{id}` | Bearer JWT |
| Meetings | `GET/POST /api/v1/Meetings`, `GET/PUT/DELETE /api/v1/Meetings/{id}`, `GET /api/v1/Meetings/block/{blockId}` | Bearer JWT |
| Meeting presences | `GET/POST /api/v1/MeetingPresences`, `GET/PUT/DELETE /api/v1/MeetingPresences/{id}` | Bearer JWT |

Full per-endpoint map (params, status codes, examples): `guides/api-patterns.md`. Auth chain: `concepts/identity-auth.md`.

## Onboarding order (new dev or agent)

1. **`navigation.md`** (this file) — orientation.
2. **`business-domain.md`** — what we're building and for whom.
3. **`technical-domain.md`** — how it's built.
4. **`identity-auth.md`** — how a caller is authenticated and resolved to a member.
5. **`lookup/decisions.md`** — why the shape is what it is.
6. **`lookup/living-notes.md`** — what's broken and what's owed.
7. Skim `business-tech-bridge.md` when working on a specific feature.
8. Cross-reference `lookup/naming.md` and `examples/component-patterns.md` when generating new code.

## What's NOT in this repo

- **No Flutter code.** The mobile client is a separate repository. References to Flutter/Dart in older versions of these files were incorrect for this repo and have been removed.
- **No background jobs, no queues.** Single-process API.

## Maintenance

- **Update `business-domain.md`** when target users or value props change.
- **Update `technical-domain.md`** when a new project is added to the solution or a major pattern shifts.
- **Update `identity-auth.md`** when the auth provider, token shape, claim mapping, or a policy changes.
- **Append `lookup/decisions.md`** whenever a meaningful architectural choice is made.
- **Update `lookup/living-notes.md`** weekly or whenever a ticket/issue status changes.

## Integration

These files are loaded by OpenCode agents via `.opencode/context/paths.json` (`local: ".opencode/context"`). See `~/.config/opencode/context/` for global patterns and standards.