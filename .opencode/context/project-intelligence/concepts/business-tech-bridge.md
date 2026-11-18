<!-- Context: project-intelligence/bridge | Priority: high | Version: 2.2 | Updated: 2026-11-18 -->

# Business ↔ Tech Bridge

> How business needs translate to technical choices in BlocoNaRua.

## Quick Reference

- **Purpose**: Show stakeholders how technical decisions serve business goals; show devs the "why" behind constraints.
- **Update When**: New entity / endpoint, role model change, infrastructure pivot.

---

## Core Mapping

| Business Need | Technical Solution | Why This Mapping | Business Value |
|---------------|-------------------|------------------|----------------|
| Anyone can register as a Member | `POST /api/v1/Members` with email + phone validation (Bearer JWT required) | Self-service onboarding is the foundation | Reduces friction to first use |
| Owner controls their block | `Owner` role + `CarnivalBlock.OwnerId` FK; only Owner can delete a block | Owner is a hard constraint, not a soft one | Prevents griefing |
| Owner can delegate to helpers | `Manager` role in `CarnivalBlockMembersEntity.Role` | Subset of Owner powers (edit, invite, schedule) | Owners don't burn out |
| Block is private — join by invite | Two codes per block: `InviteCode` (member) and `ManagersInviteCode` (manager) | Codes are short, shareable, role-scoped | Avoids leaking member list |
| Members RSVP to meetings | `MeetingPresenceEntity` with `IsPresent` boolean + `MemberId`, `MeetingId`, `CarnivalBlockId` | Triple FK keeps the relationship denormalized for fast queries | Single source of truth for attendance |
| Owners track who actually came | `MeetingPresence` is queryable per meeting; cached for 5 min | Reads are far more frequent than writes | Cheap dashboard at carnival time |
| Members can self-mark, but not fake others' | Service layer checks `MemberId == acting member` OR role is Owner/Manager | Auth lives in service, not controller | Server is the trust boundary |
| Mobile-first consumption | JSON over HTTP, paginated reads, versioned URLs | Flutter client is the only consumer today | Single integration shape |
| Brazilian community budget | Self-hosted PostgreSQL (Dokploy docker-compose), EF Core migrations | Avoid vendor lock-in for the data layer | Migration path is open |

## Feature Mapping Examples

### Feature: Role-Based Block Editing

**Business Context**:
- User need: Owner and Manager can update a block's name and image; regular Members cannot.
- Business goal: Distribute operational load without losing control.
- Priority: High — this is the difference between "use the app" and "burnout".

**Technical Implementation**:
- Solution: `CarnivalBlockService.UpdateAsync(id, memberId, model)` resolves the caller's role with the inline `GetMemberRoleInline(blockId, memberId)` helper and throws `UnauthorizedAccessException` unless the role is `Owner` or `Manager`. `memberId` comes from the verified JWT (`sub` claim) via `IMemberIdentityService`.
- Architecture: Service-layer authorization, not controller-level (controllers translate exceptions to HTTP status).
- Trade-offs: Two role systems coexist — per-block roles (`Owner` / `Manager` / `Member`, enforced in the service layer) and a platform `Admin` enforced by the `AdminOnly` policy (`role: admin` claim, `Program.cs:97-101`) on `/api/v1/admin/*`. Block roles need the block id, so they cannot be an ASP.NET policy.

**Connection**:
Without this check, anyone with the `blockId` could rewrite a block's name. The business needs a chain of trust (Owner → Manager → Member) and the role enum + service-layer guard is what enforces it.

### Feature: Invite Codes for Onboarding

**Business Context**:
- User need: Owners invite managers; everyone invites regular members.
- Business goal: Frictionless onboarding without exposing the member list publicly.
- Priority: High — first-day UX.

**Technical Implementation**:
- Solution: On `CarnivalBlockService.CreateAsync`, two 8-char codes are generated via `Random.Shared.Next` over `A-Z0-9`: `InviteCode` (member) and `ManagersInviteCode` (manager).
- Architecture: Codes live on the entity, returned in `CarnivalBlockResponse`. The Flutter app shows a "Share invite" button that picks the right code by intent.
- Trade-offs: Codes are not single-use or time-limited. Anyone who leaves the block keeps the code in their screenshots. Acceptable for v1.

**Connection**:
The codes are the business's compromise between "anyone can join" (unsafe) and "Owner manually adds everyone" (unscalable). Two distinct codes also encode role at the moment of join.

### Feature: Meeting Presence (Attendance)

**Business Context**:
- User need: Members self-mark present; Owners see who came.
- Business goal: Replace WhatsApp "+1" threads with structured data.
- Priority: Core — this is the only "transactional" feature.

**Technical Implementation**:
- Solution: `MeetingPresenceEntity` has `MemberId`, `MeetingId`, `CarnivalBlockId`, `IsPresent`. `MeetingPresenceService.CreateAsync` allows either self (`MemberId == acting member` resolved from the JWT `sub` claim) OR Owner/Manager.
- Architecture: Triple FK is redundant (`CarnivalBlockId` is derivable from `MeetingId`) but speeds up joins.
- Trade-offs: No "tentative" state — only `IsPresent`. No check-in window. Acceptable for v1; future work could add a state machine (Invited / Maybe / Present / Absent).

**Connection**:
Without structured attendance, the block can't answer "who should we expect at the next parade?" — the single most-asked question during carnival season.

## Trade-off Decisions

| Situation | Business Priority | Technical Priority | Decision Made | Rationale |
|-----------|-------------------|-------------------|---------------|-----------|
| Block deletion should be safe | Prevent accidental data loss | Simple cascade delete | **Restrict** on `Meeting` and `MeetingPresence`, **Cascade** on `CarnivalBlockMembers` only | Owner can still wipe a block but meetings don't auto-delete historical attendance |
| Identity layer | JWT for SSO / mobile | API-issued JWT (HS256) + Google OAuth | **JWT shipped** — Google challenge/complete issues self-issued HS256 tokens; validated in `Program.cs:85-109` | Client proves identity with a token; server resolves the acting member from the `sub` claim |
| Read latency vs freshness | Carnival time = peak reads | Cache every read 5 min | `IMemoryCache` everywhere | Acceptable staleness, but cache invalidation on writes |
| Admin signups | Need cleanup if signup fails | Frontend-only API call | Server endpoint required | `DELETE /api/v1/admin/signup-cleanup/{uuid}` under the `AdminOnly` policy; EF hard-deletes the member row (bypassing soft delete) |

## Common Misalignments

| Misalignment | Warning Signs | Resolution Approach |
|--------------|---------------|---------------------|
| Frontend expects `memberId` in body of presence | Flutter hardcodes `memberId: 0` waiting for server-side identity | Implemented — the backend resolves `memberId` from the verified JWT `sub` claim (`IMemberIdentityService`); managers can pass an explicit `memberId` on create (TKT-002/003 — ✅ shipped 2026-11-17) |
| DTO names drift from Flutter entities | Flutter team renames `MemberCreate` → backend still has old name | Keep a single generated mapping or contract test |
| Cached list returns stale data after delete | User deletes a block, list still shows it | Services already invalidate cache keys on writes — but verify per-endpoint |

## Stakeholder Communication

**For Business Stakeholders**:
- Each role maps to a real database column, not a magic string — explainable and auditable.
- The 5-minute cache means real-time dashboards are stale by up to 5 minutes during peaks.
- Carnival traffic shape (Feb–Mar spike) justifies the simple Postgres-only stack.

**For Technical Stakeholders**:
- The Brazilian context drives UI/UX choices in the Flutter app (Portuguese copy, BR date formats).
- The acting member is resolved from the verified JWT (`Authorization: Bearer <token>`) — the `sub` claim is the only trusted source of identity.
- Owner-vs-Manager-vs-Member is enforced in the service layer (inline `GetMemberRoleInline`); never duplicate role checks in controllers.

## Onboarding Checklist

- [ ] Read `business-domain.md` for the big picture
- [ ] Walk one feature end-to-end (request → controller → service → repo → DB)
- [ ] Read `identity-auth.md` for the login → JWT → memberId chain
- [ ] Read `../lookup/decisions.md` for the "why"
- [ ] Skim `../lookup/living-notes.md` for known issues before opening a PR
- [ ] Understand the role enum before touching any service that receives a `memberId`

## Related Files

- `business-domain.md` — Business needs in detail
- `technical-domain.md` — Technical implementation in detail
- `identity-auth.md` — Auth flow: login → JWT → memberId → admin policy
- `../lookup/decisions.md` — Decisions made with full context
- `../lookup/living-notes.md` — Current open questions and issues