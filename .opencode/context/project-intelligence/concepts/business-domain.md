<!-- Context: project-intelligence/business | Priority: high | Version: 2.2 | Updated: 2026-11-18 -->

# Business Domain

> Brazilian community carnival-block management API. Understand why this project exists and who it serves.

## Quick Reference

- **Purpose**: Backend API for organizing carnival blocks (blocos), their members, and meetings.
- **Update When**: New entity scope, role changes, business pivot, new target user segment.
- **Audience**: Backend devs maintaining the API, frontend devs integrating against it, product team.

---

## Project Identity

```
Project Name: BlocoNaRua (RESTful API)
Tagline: Manage carnival blocks, members, and meetings.
Problem Statement: Brazilian carnival is organized into "blocos" — street groups
                   with members who gather for parades and planning meetings.
                   Organizers (owners) and helpers (managers) need a shared
                   backend to manage membership, schedule meetings, and track
                   attendance. Today this is fragmented across WhatsApp groups,
                   spreadsheets, and ad-hoc tools.
Solution: Centralized RESTful API exposing CRUD for blocks, members,
          meetings, and attendance with role-based access control.
```

## Target Users

| User Segment | Who They Are | What They Need | Pain Points |
|--------------|--------------|----------------|-------------|
| **Owner** | Carnival block organizer who created the block | Full control: edit block, invite managers, schedule meetings, mark attendance | Coordinating via WhatsApp poll / spreadsheet |
| **Manager** | Trusted helper invited by Owner | Edit block details, schedule meetings, manage members (except Owner) | No shared system for proposals / RSVPs |
| **Member** | Regular participant of a block | View meetings, mark own attendance, see member list | Lost track of when/where meetings happen |

A **Guest** is a person who has not yet joined any block — they need the public list of blocks and to register a Member profile first.

## Value Proposition

**For Users (Members)**:
- Single source of truth for upcoming meetings and attendance
- Easy block discovery via invite code (member code, manager code)
- Self-service attendance marking

**For Organizers (Owner / Manager)**:
- Role-based delegation without sharing credentials
- Automated invite codes for onboarding members and managers
- Audit trail via CreatedAt / UpdatedAt on every entity

**For Business / Community**:
- Open data model that can later power a public carnival event calendar
- Foundation for notifications, geolocation, photo galleries, payments
- Brazilian Portuguese audience (UI/UX context — name "bloco na rua" = "block on the street")

## Success Metrics (proposed — not yet measured)

| Metric | Definition | Target | Current |
|--------|------------|--------|---------|
| Blocks created per season | Carnival blocks registered before Feb | +20% YoY | n/a |
| Members per active block | Median members of a block with >1 meeting | ≥ 10 | n/a |
| Meeting attendance rate | `IsPresent = true` count / total RSVPs | ≥ 60% | n/a |
| API uptime during carnival (Feb–Mar) | Successful 2xx responses | ≥ 99% | n/a |

## Business Model

```
Revenue Model: Free / community-driven (no monetization yet)
Pricing Strategy: n/a
Market Position: Niche — Brazilian street-carnival organizer tooling
Geography: Brazil-first (Portuguese copy, BR-portuguese domain terms)
```

## Key Stakeholders

| Role | Name / Handle | Responsibility | Contact |
|------|---------------|----------------|---------|
| Project Owner / Dev | [@rafaelsnv](https://github.com/rafaelsnv) | Architecture, code, ops | GitHub |
| Frontend Consumer | Separate Flutter repo | Mobile client consuming this API | (companion repo) |
| End Users | Carnival organizers and participants | Use the Flutter app backed by this API | in-app feedback |

## Roadmap Context

**Current Focus** (2026 Q3):
- ✅ JWT identity — API-issued HS256 + Google OAuth (shipped 2026-11-18)
- ✅ Backend-only admin endpoint (`/api/v1/admin/*`, `AdminOnly` policy) — shipped
- ✅ Manager presence authorization — TKT-002 / TKT-003 shipped (verified 2026-11-17)
- Stabilize the v1 endpoints
- Tighten Dio `validateStatus` semantics (frontend-side contract)

**Next Milestone**:
- v1.0 API freeze: all endpoints stable, role-based auth locked in
- v1.1 candidates: notifications, image upload for blocks / members

**Long-term Vision**:
- Public read-only API for tourism / city-event calendars
- Optional payment integration for bloco membership fees
- Multi-language (Portuguese / English)

## Business Constraints

- **Brazilian Portuguese** — domain language is Portuguese (`bloco`, `encontro`, `presença`), even though code identifiers are English.
- **Carnival seasonality** — peak traffic Feb–Mar, mostly idle the rest of the year (plan caching accordingly).
- **Community budget** — self-hosted Postgres (Dokploy docker-compose, postgres:18) + a small VM; no enterprise DB budget.
- **Self-hosted identity** — the API issues its own HS256 tokens (Google OAuth is the only social login, no external IdP); the `Members` table is the single source of truth for identity, and the JWT `sub` claim resolves to its `Guid Uuid`.

## Onboarding Checklist

- [ ] Read the README
- [ ] Understand the three roles: Member / Manager / Owner
- [ ] Read `BlocoNaRua.Domain/Enums/RolesEnum.cs`
- [ ] Skim `BlocoNaRua.Domain/Entities/` — five entities, see relationships
- [ ] Read `lookup/decisions.md` to learn the architectural choices

## Related Files

- `technical-domain.md` — How this is implemented
- `identity-auth.md` — How login, tokens, and identity resolution work
- `business-tech-bridge.md` — Business ↔ technical mapping
- `lookup/decisions.md` — Why each major choice was made
- `lookup/living-notes.md` — Active issues and debt
- `../../../../BACKEND_TICKETS.md` — Resolved cross-team ticket record (repo root; retained for history)