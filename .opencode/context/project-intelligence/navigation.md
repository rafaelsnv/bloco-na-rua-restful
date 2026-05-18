<!-- Context: project-intelligence/nav | Priority: critical | Version: 1.0 | Updated: 2026-05-17 -->

# Project Intelligence

> Quick overview of BlocoNaRua REST API patterns.

## Structure

```
.opencode/context/project-intelligence/
├── navigation.md                    # This file
├── technical-domain.md              # Tech stack & architecture
├── technical-api-pattern.md         # Controller & DTO patterns
├── technical-component-pattern.md   # Service & repository patterns
└── technical-naming.md              # Naming conventions
```

## Quick Routes

| What You Need | File | Description |
|---------------|------|-------------|
| Tech stack | `technical-domain.md` | .NET 8, C# 12, EF Core, PostgreSQL |
| API patterns | `technical-api-pattern.md` | Routes, controllers, DTOs, mappers |
| Component patterns | `technical-component-pattern.md` | Services, repositories, DI |
| Naming conventions | `technical-naming.md` | PascalCase, camelCase rules |
| All patterns | Read all files | Complete project intelligence |

## Tech Stack

- **Framework**: .NET 8.0 + ASP.NET Core Web API
- **Language**: C# 12 (primary constructors)
- **ORM**: Entity Framework Core 8.x
- **Database**: PostgreSQL (via Supabase)
- **Architecture**: Clean Architecture

## Key Patterns

| Pattern | Example |
|---------|---------|
| Primary constructor | `Service(ICrud repo) { _repo = repo; }` |
| Record DTOs | `record CarnivalBlockCreate(string Name, int OwnerId);` |
| Header auth | `[FromHeader(Name = "X-Logged-Member")] int loggedMember` |
| Exception handling | `KeyNotFoundException` → 404, `UnauthorizedAccessException` → 401 |

## Usage

**New to project**: Read files in order: navigation → domain → api → component → naming

**Quick reference**: Jump to specific pattern using table above

## Maintenance

Update when:
- Tech stack changes
- New patterns introduced
- Architecture decisions made

---
Generated from global .NET patterns (`~/.config/opencode/context/project-intelligence/`)