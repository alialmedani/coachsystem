# CLAUDE.md

Guidance for Claude Code when working in this repository. **The code is the spec** — when any
doc disagrees with the code, the code wins. The reference is the `Trainee` / `TrainingPlan`
vertical slices under `src/`.

## What CoachApp is

An **ABP Framework 10.4.0 / .NET 10** backend API, built as a **single application** (ABP `app`
template) in a pragmatic, **feature-folder** style (modeled on `../JasimExpress/express_api`).
SQL Server, OpenIddict auth, multi-tenant, Swagger. There is **no `modules/` folder** — everything
lives under `src/`.

## Solution layout (single app, `src/`)

| Project | Responsibility |
|---|---|
| `CoachApp.Domain.Shared` | Consts (`Entites/<Feature>/`), enums (`Enums/`), `CoachAppDomainErrorCodes`, localization (`Localization/CoachApp/*.json`) |
| `CoachApp.Domain` | Entities (`Entites/<Feature>/`); a `*Manager : DomainService` only for a genuine cross-aggregate rule |
| `CoachApp.Application.Contracts` | DTOs + `I<Entity>AppService` (`Entites/<Feature>/`), permissions (`Permissions/`) |
| `CoachApp.Application` | AppServices (`Apis/<Feature>/`), central `CoachAppMapsterConfig` + `MapsterObjectMapper` |
| `CoachApp.EntityFrameworkCore` | `CoachAppDbContext`, `EntityConfigurations/<Feature>/`, `Migrations/` |
| `CoachApp.HttpApi` / `.HttpApi.Host` | Conventional (auto) controllers, host wiring, Swagger, auth |
| `CoachApp.DbMigrator` | Console app: applies EF migrations + seeds admin/permissions |

## Conventions (mirror the `Trainee` slice)

- **Feature folders + feature-based namespaces** (the layer name is dropped): entity + DTOs +
  `I<Entity>AppService` → `CoachApp.Entites.<Feature>`; AppService → `CoachApp.Apis.<Feature>`;
  enums → `CoachApp.Enums`; EF config → `CoachApp.EntityConfigurations.<Feature>`; permissions →
  `CoachApp.Permissions`; error codes → `CoachApp` (`CoachAppDomainErrorCodes`). `<Feature>` is the
  pluralized entity (`Trainees`).
- **Pragmatic domain.** Aggregates are `FullAuditedAggregateRoot<Guid>` (+ `IMultiTenant`) with
  **public settable properties** and a `static Create(Guid id, …, Guid? tenantId)` factory method
  that validates required fields (`Check.*` vs `<Entity>Consts`). Real invariants are **behavior
  methods** on the entity that throw `BusinessException` (guarded fields kept `protected set`).
  **No separate `*Factory` class; no `*Manager` for simple CRUD.**
- **Fat AppServices.** Query inline (`GetQueryableAsync` + `WhereIf` + `System.Linq.Dynamic.Core`
  `.OrderBy(sorting)` + `.Skip().Take()` + `AsyncExecuter`); cross-aggregate uniqueness/existence
  checks live here (throwing `BusinessException(CoachAppDomainErrorCodes.X)`); use the **generic
  `IRepository<T, Guid>`** — no custom repository for plain CRUD. Map out with `ObjectMapper.Map`.
- **Mapping = Mapster.** One `TypeAdapterConfig<Entity, Dto>.NewConfig()` per mapping in
  `CoachAppMapsterConfig.Configure()`; entity → DTO only. Registered in `CoachAppApplicationModule`
  (`MapsterObjectMapper : IAutoObjectMappingProvider`). **No Mapperly / AutoMapper / MediatR / FluentValidation.**
- **EF Core.** One `CoachAppDbContext`; each entity has an `IEntityTypeConfiguration<T>` under
  `EntityConfigurations/<Feature>/`, auto-applied via `builder.ApplyConfigurationsFromAssembly(...)`.
  Table prefix `CoachAppConsts.DbTablePrefix` (`"App"`), null schema.
- **Permissions** in central `CoachAppPermissions` (group `"CoachApp"`) + `CoachAppPermissionDefinitionProvider`;
  `[Authorize(...)]` on the AppService class + write methods.
- **Localization** in the single `CoachAppResource`; add keys to `Localization/CoachApp/en.json`
  **and** `ar.json`. Error codes are `"CoachApp:0000N"`, mapped via `MapCodeNamespace("CoachApp", …)`.
- **HTTP.** AppServices are auto-exposed as REST via conventional controllers
  (`ConventionalControllers.Create(typeof(CoachAppApplicationModule).Assembly)` in the host). No manual controllers.

## Adding a feature (per-entity vertical slice)

Build bottom-up: `Domain.Shared` (Consts/Enum/error code) → `Domain/Entites/<Feature>` (entity with
`static Create` + behavior invariants) → `Application.Contracts/Entites/<Feature>` (DTOs + interface)
→ `Application.Contracts/Permissions` → `Application/Apis/<Feature>` (fat AppService) +
`CoachAppMapsterConfig` line → `EntityFrameworkCore/EntityConfigurations/<Feature>` + `DbSet` →
localization (en + ar) → migration → tests. See `.claude/agents/coachapp-backend.md` for the full checklist.

## Commands

```bash
dotnet build CoachApp.slnx                          # whole solution; must be clean
dotnet run --project src/CoachApp.HttpApi.Host      # start API + Swagger (https://localhost:44xxx/swagger)
dotnet test                                         # xUnit + Shouldly

# Migrations — schema is managed by EF migrations (NOT manual SQL).
dotnet ef migrations add <Name> --project src/CoachApp.EntityFrameworkCore --startup-project src/CoachApp.EntityFrameworkCore
dotnet run --project src/CoachApp.DbMigrator        # apply migrations + seed
```

⚠️ **Applying a migration / running DbMigrator touches the database — always ask the user before
doing so.** Writing/generating migration files is fine.

## Agents & rules

- Specialist subagents in `.claude/agents/`: `coachapp-architect` (plan/route), `coachapp-backend`
  (implement), `coachapp-db` (EF/migrations), `coachapp-tester`, `coachapp-reviewer`,
  `coachapp-security`, `coachapp-runner`. They all enforce the conventions above.
- Detailed rules in `.cursor/rules/**/*.mdc` and `.abpstudio/ai-rules/app.mdc`.
