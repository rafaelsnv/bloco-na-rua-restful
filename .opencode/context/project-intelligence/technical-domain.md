<!-- Context: project-intelligence/technical | Priority: critical | Version: 1.0 | Updated: 2026-05-17 -->

# Technical Domain

**Purpose**: Tech stack, architecture, and development patterns for BlocoNaRua REST API.
**Audience**: Developers, AI agents
**Last Updated**: 2026-05-17

## Quick Reference

**Update Triggers**: Tech stack changes | New patterns | Architecture decisions
**Project**: Carnival block management system with meetings and member tracking

## Primary Stack

| Layer | Technology | Version | Rationale |
|-------|------------|---------|-----------|
| Framework | .NET 8.0 + ASP.NET Core | 8.0 | LTS, cross-platform, minimal APIs |
| Language | C# 12 | latest | Primary constructor support |
| Web | ASP.NET Core Web API | 8.0 | RESTful API with versioning |
| ORM | Entity Framework Core | 8.x | PostgreSQL via Npgsql |
| Database | PostgreSQL | via Supabase | Cloud-hosted |
| Architecture | Clean Architecture | N/A | Domain/Data/Services/Restful separation |

## Architecture Pattern

```
Type: Layered/Clean Architecture
Pattern: Domain → Data → Services → REST API
Diagram: N/A
```

### Why This Architecture?

- **Domain**: Entities and business rules (pure C#, no dependencies)
- **Data**: Repositories and EF Core mappings (data access)
- **Services**: Business logic and authorization (depends on Domain + Data)
- **Restful**: Controllers, DTOs, Mappers (HTTP interface, depends on Services)

### Project Structure

```
BlocoNaRua.sln
├── BlocoNaRua.Domain/           # Entities, Enums (core business)
├── BlocoNaRua.Core/             # Shared models (EntityBase, IRepositoryBase)
├── BlocoNaRua.Data/             # Repositories, DbContext, Mappings
├── BlocoNaRua.Services/         # Business logic, Interfaces
├── BlocoNaRua.Restful/          # Controllers, DTOs, Mappers
└── BlocoNaRua.Tests/            # Unit tests
```

## Key Technical Decisions

| Decision | Rationale | Impact |
|----------|-----------|--------|
| Primary constructors (C# 12) | Reduce boilerplate, clear dependencies | Cleaner service/repository initialization |
| Record DTOs | Immutable, concise, built-in equality | Type-safe API contracts |
| Header-based auth | Stateless, simple token passing | X-Logged-Member header for write ops |
| Exception-based error handling | Uniform error propagation | KeyNotFoundException → 404, UnauthorizedAccessException → 401 |

## Integration Points

| System | Purpose | Protocol | Direction |
|--------|---------|----------|-----------|
| PostgreSQL | Data persistence | EF Core/Npgsql | Internal |
| Supabase | Cloud database hosting | PostgreSQL | Outbound |

## Development Environment

```
Setup: dotnet restore && dotnet build
Requirements: .NET 8.0 SDK, PostgreSQL (local or Supabase)
Local Dev: dotnet run --project BlocoNaRua.Restful
Testing: dotnet test
```

## Onboarding Checklist

- [ ] Understand Clean Architecture layers
- [ ] Know primary constructor injection pattern
- [ ] Follow naming conventions (technical-naming.md)
- [ ] Use record DTOs (Create/Update/Response)
- [ ] Implement X-Logged-Member header auth

## 📂 Codebase References

**Solution**: `BlocoNaRua.sln` (6 projects)
**Entry**: `BlocoNaRua.Restful/` - ASP.NET Core Web API
**Domain**: `BlocoNaRua.Domain/Entities/` - Business entities

## Related Files

- `technical-api-pattern.md` - Controller and route patterns
- `technical-component-pattern.md` - Service and repository patterns
- `technical-naming.md` - Naming conventions
- `navigation.md` - Quick overview