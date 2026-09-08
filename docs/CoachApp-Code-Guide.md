# CoachApp — Code & Architecture Guide

> **Audience:** a programmer who needs to understand, explain, or extend this codebase.
> **Golden rule of this repo:** *the code is the spec.* When any document disagrees with the
> code, the code wins. This guide was written directly from the source.

---

## 1. What CoachApp is

CoachApp is the **backend API** for a coach-managed fitness platform. A **coach** manages
**trainees**, builds libraries (exercises, foods) and prescribes **workout plans** and
**nutrition plans**. Each trainee logs what they actually did (workout logs, nutrition logs),
records body-metric **progress**, and reads **notes** the coach writes for them.

Technology:

| Concern | Choice |
|---|---|
| Framework | **ABP Framework 10.4.0** |
| Runtime | **.NET 10** |
| Application shape | **Single application** (ABP `app` template) — there is **no `modules/` folder**; everything lives under `src/` |
| Code style | **Feature folders** + feature-based namespaces (modeled on `../JasimExpress/express_api`) |
| Database | **SQL Server** via **EF Core**, schema managed by **EF migrations** (never manual SQL) |
| Auth | **OpenIddict** (OAuth2 / OpenID Connect), bearer tokens |
| Multi-tenancy | Enabled — **one tenant per coach** |
| Mapping | **Mapster** (entity → DTO only) |
| API surface | **Auto-generated REST controllers** from application services (ABP conventional controllers) |
| Tests | **xUnit + Shouldly** |

> **Naming quirk you must know:** the folder/namespace segment is spelled **`Entites`** (missing an
> "i") *everywhere and consistently* — `CoachApp.Entites.Trainees`, `CoachApp.Entites.Foods`, etc.
> This is intentional and uniform across all layers. Reference it exactly; do not "fix" it in
> isolation or you will break namespaces.

---

## 2. The one mental model that explains everything: **two personas**

Almost every design decision in this codebase falls out of a single idea — there are **two kinds
of authenticated user**, and most features exist in **two parallel versions**:

| Persona | Role | Permission prefix | What they do | Typical service name |
|---|---|---|---|---|
| **Coach** (management side) | `"Coach"` | `CoachApp.Coach.*` | Create/manage trainees, libraries, plans; read trainees' logs & progress; write notes | `XxxAppService` |
| **Trainee** (self-service side) | `"Trainee"` | `CoachApp.Trainee.*` | Read *their own* profile & active plans; log *their own* workouts/nutrition/progress; read notes | `MyXxxAppService` |

So a feature like "workout plans" appears twice:

- `WorkoutPlanAppService` — coach-facing, full CRUD, takes an explicit `TraineeId`, guarded by
  `CoachApp.Coach.WorkoutPlans.*`.
- `MyWorkoutPlanAppService` — trainee-facing, **read-only**, never trusts a client-supplied
  `TraineeId` (it derives the trainee from the signed-in user), guarded by
  `CoachApp.Trainee.MyWorkoutPlans`.

Keep this split in your head and the whole solution reads cleanly.

**Tenant-per-coach.** Multi-tenancy is on (`MultiTenancyConsts.IsEnabled = true`). Every business
entity is `IMultiTenant`, so ABP's global tenant filter automatically isolates one coach's data
from another's — you almost never write a tenant `WHERE` clause by hand.

---

## 3. Solution layout (the 9 projects under `src/`)

ABP layers the solution. Dependencies point **downward** (Host → Application → Domain → Domain.Shared).

```
CoachApp.Domain.Shared      ← consts, enums, error codes, localization  (no dependencies)
        ▲
CoachApp.Domain             ← entities/aggregates, data seeders, settings
        ▲
CoachApp.Application.Contracts ← DTOs, service interfaces, permission definitions
        ▲
CoachApp.Application        ← application services (the real logic), Mapster config
        ▲
CoachApp.EntityFrameworkCore ← DbContext, EF entity configs, migrations, custom repos
        ▲
CoachApp.HttpApi(.Host)     ← auto REST controllers, host wiring, Swagger, auth
CoachApp.DbMigrator         ← console app: apply migrations + seed
CoachApp.HttpApi.Client     ← generated C# proxies (for other .NET clients)
```

| Project | Responsibility | Key files |
|---|---|---|
| **`CoachApp.Domain.Shared`** | The vocabulary: field-length consts (`Entites/<Feature>/<X>Consts.cs`), enums (`Enums/`), `CoachAppRoles`, `CoachAppPermissionPrefixes`, `CoachAppDomainErrorCodes`, `MultiTenancyConsts`, localization JSON (`Localization/CoachApp/*.json`) | `CoachAppRoles.cs`, `Enums/*.cs` |
| **`CoachApp.Domain`** | Entities & aggregate roots (`Entites/<Feature>/`), data seed contributors, OpenIddict seeder, settings | `Entites/Trainees/Trainee.cs`, `Data/CoachAppDataSeedContributor.cs`, `OpenIddict/OpenIddictDataSeedContributor.cs` |
| **`CoachApp.Application.Contracts`** | DTOs + `I<Entity>AppService` interfaces (`Entites/<Feature>/`), the whole **permission tree** (`Permissions/`) | `Permissions/CoachAppPermissions.cs`, `Permissions/CoachAppPermissionDefinitionProvider.cs` |
| **`CoachApp.Application`** | The **fat application services** (`Apis/<Feature>/`), central Mapster config, the Mapster ⇆ ABP bridge | `Apis/**`, `CoachAppMapsterConfig.cs`, `MapsterObjectMapper.cs` |
| **`CoachApp.EntityFrameworkCore`** | The single `CoachAppDbContext`, one `IEntityTypeConfiguration` per entity (`EntityConfigurations/<Feature>/`), `Migrations/`, custom repositories | `EntityFrameworkCore/CoachAppDbContext.cs`, `Repositories/*` |
| **`CoachApp.HttpApi`** | (Mostly empty) place for manual controllers — currently only framework boilerplate; real controllers are auto-generated | `Controllers/CoachAppController.cs` |
| **`CoachApp.HttpApi.Host`** | The runnable web app: module wiring, OpenIddict validation, CORS, Swagger, health checks | `CoachAppHttpApiHostModule.cs`, `appsettings.json`, `Program.cs` |
| **`CoachApp.DbMigrator`** | Console app that applies EF migrations and runs the data seeders (admin, roles, permissions, demo tenant, OpenIddict clients) | `DbMigratorHostedService.cs` |
| **`CoachApp.HttpApi.Client`** | ABP-generated typed C# HTTP proxies for the API (used by the console test app) | `CoachAppHttpApiClientModule.cs` |

---

## 4. The vertical-slice pattern (how one feature is built)

Every feature is a **vertical slice** that touches each layer in the same shape. Using the
reference `Trainee` slice, a slice consists of:

```
Domain.Shared/Entites/Trainees/TraineeConsts.cs          field-length limits (single source of truth)
Domain.Shared/Enums/{Gender,TrainingGoal}.cs             enums used by the entity
Domain/Entites/Trainees/Trainee.cs                       the aggregate root
Application.Contracts/Entites/Trainees/                  DTOs + I…AppService interface(s)
   TraineeDto.cs, CreateTraineeDto.cs, UpdateTraineeDto.cs, GetTraineeListInput.cs,
   ITraineeAppService.cs, IMyProfileAppService.cs
Application.Contracts/Permissions/CoachAppPermissions.cs (add the permission node)
Application/Apis/Trainees/                                the app services (the logic)
   TraineeAppService.cs, MyProfileAppService.cs
Application/CoachAppMapsterConfig.cs                      one mapping line: Trainee → TraineeDto
EntityFrameworkCore/EntityConfigurations/Trainees/TraineeConfiguration.cs   table mapping
EntityFrameworkCore/CoachAppDbContext.cs                 the DbSet
Domain.Shared/Localization/CoachApp/{en,ar}.json         labels + error messages
EntityFrameworkCore/Migrations/…                          the schema migration
test/…                                                    domain + application tests
```

The build order is **bottom-up**: Domain.Shared → Domain → Application.Contracts → Permissions →
Application (+ Mapster line) → EF config + DbSet → localization → migration → tests. This exact
checklist lives in `.claude/agents/coachapp-backend.md`.

---

## 5. Cross-cutting building blocks (the conventions)

### 5.1 Pragmatic domain

Aggregates are **not** "rich anemic-avoidance" models with private everything. The house style is
**pragmatic**:

- An aggregate root is `FullAuditedAggregateRoot<Guid>` and (for business data) `IMultiTenant`.
- Properties have **public virtual setters** — application services can and do assign them directly
  on update.
- Object creation goes through a **`static Create(Guid id, …, Guid? tenantId = null)` factory** that
  validates *required* fields using ABP's `Check` helper (e.g.
  `Check.NotNullOrWhiteSpace(name, nameof(name), TraineeConsts.MaxNameLength)`,
  `Check.Length(email, …, MaxEmailLength)`).
- A **`protected` parameterless constructor** exists for the ORM only.
- **Real invariants** (rules that must always hold) are expressed as **behavior methods**
  (`Activate()`, `Deactivate()`, `AddDay(...)`, `AddEntry(...)`, `ClearDays()`). Child collections
  are exposed as `{ get; protected set; }` and are only mutated through these methods.
- There is **no separate `*Factory` class** and **no `*Manager`** for plain CRUD. A `*Manager :
  DomainService` would only be introduced for a genuine cross-aggregate rule (none exist yet).

> **Where invariants actually live — read this carefully.** `Check.*` only guards *field shape*
> (required / max length) and throws `ArgumentException`. Cross-aggregate rules and business
> invariants are enforced **in the application service**, not the entity. The clearest example is
> the **"a trainee may have at most one active plan"** rule: the entity comment says so, but it is
> physically enforced by `SetActiveInternalAsync` in `WorkoutPlanAppService` /
> `NutritionPlanAppService` (deactivate the others, then activate this one).

### 5.2 Aggregates & ownership

Which entity owns which children (an owned child is only ever created/edited through its root):

| Aggregate root | Owns (composition) | Loose references (GUID only, no navigation) |
|---|---|---|
| `Trainee` | — | `UserId` → ABP `IdentityUser` |
| `Exercise` | — | — |
| `Food` | — | — |
| `WorkoutPlan` | `WorkoutDay` → `WorkoutExercise` | `TraineeId`, each `WorkoutExercise.ExerciseId` → `Exercise` |
| `WorkoutLog` | `WorkoutLogEntry` | `TraineeId`, `WorkoutPlanId?`, `WorkoutDayId?`, each entry's `ExerciseId` |
| `NutritionPlan` | `Meal` → `MealItem` | `TraineeId`, each `MealItem.FoodId` → `Food` |
| `NutritionLog` | `NutritionLogEntry` | `TraineeId`, `NutritionPlanId?`, each entry's `FoodId` |
| `ProgressEntry` | — | `TraineeId` |
| `TraineeNote` | — | `TraineeId`; author = audit `CreatorId` |

Editing an owned graph (plans) is done by **full replace**: `ClearDays()` / `ClearMeals()` then
rebuild from the incoming DTO. Logs are **created whole and deleted whole** — there is no update
path for a log.

### 5.3 Fat application services

The application service is where the work happens. The house pattern (see any `Apis/**` file):

- Inject the **generic** `IRepository<TEntity, Guid>` — **no custom repository for plain CRUD**.
  (Custom repositories exist only to eager-load owned graphs, see §5.9.)
- Query **inline**: `GetQueryableAsync()` → `.WhereIf(condition, predicate)` (conditional filters) →
  `System.Linq.Dynamic.Core` `.OrderBy(sorting)` (string-based sort from the DTO) →
  `.Skip(input.SkipCount).Take(input.MaxResultCount)` → materialize with `AsyncExecuter.ToListAsync`
  and count with `AsyncExecuter.CountAsync`.
- A sensible **default sort** when the client sends none (e.g. `"Name asc"`, `"Date desc"`,
  `"FirstName asc"`).
- **Cross-aggregate existence checks live here**, not in the entity — e.g.
  `CheckTraineeExistsAsync`, `CheckExercisesExistAsync`, `CheckFoodsExistAsync`. They throw
  `UserFriendlyException(L["..."])`.
- Map out with `ObjectMapper.Map<Entity, Dto>` (which routes through Mapster).
- Return ABP DTOs: single item → `XxxDto`, list → `PagedResultDto<XxxDto>` (or a plain `List<>` for
  the small trainee-side "my plans" lists).

Everything derives from `CoachAppAppService`, which provides `ObjectMapper`, `GuidGenerator`,
`AsyncExecuter`, `CurrentTenant`, `CurrentUser`, and `L` (localizer).

### 5.4 The Coach-vs-"My" split, precisely

This is the single most important pattern to explain to another developer:

| Aspect | Coach service (`XxxAppService`) | Trainee service (`MyXxxAppService`) |
|---|---|---|
| Permission | `CoachApp.Coach.<Feature>.*` | `CoachApp.Trainee.<Feature>` |
| Who is the trainee? | An **explicit `TraineeId`** in the input DTO (validated to exist) | **Derived from the signed-in user** via `GetCurrentTraineeIdAsync()` — never taken from the client |
| Row-level safety | Relies on the ambient tenant filter (+ the `TraineeId` filter for lists) | **Explicit ownership guard**: if `row.TraineeId != currentTraineeId` it throws `EntityNotFoundException` (a 404 — it does not reveal that the row exists) |
| Capabilities | Full CRUD (+ `SetActive` on plans) | Restricted — see the matrix below |

`GetCurrentTraineeIdAsync()` is the linchpin: it calls `CurrentUser.GetId()` (the `IdentityUser`
id) and finds the `Trainee` whose `UserId` matches. If none exists it throws
`EntityNotFoundException(typeof(Trainee), userId)`.

**Trainee-side capability matrix (deliberately asymmetric):**

| Feature | Trainee can… |
|---|---|
| MyProfile | read only |
| MyWorkoutPlans | list / get / get-active (read-only) |
| WorkoutLogs | **create**, list, get, **delete** — *no update* |
| MyNutritionPlans | list / get / get-active (read-only) |
| NutritionLogs | **create**, list, get, **delete** — *no update* |
| MyProgress | **create**, list, get, **delete** — *no update* |
| MyNotes | list / get (read-only) — notes are authored by the coach |

### 5.5 Permissions (the two-persona tree)

Defined as nested `const string`s in **`CoachAppPermissions`** (group name `"CoachApp"`), and
declared to ABP in **`CoachAppPermissionDefinitionProvider`**. The full tree:

```
CoachApp
├─ Coach
│  ├─ Trainees        (+ .Create .Update .Delete)
│  ├─ Exercises       (+ .Create .Update .Delete)
│  ├─ WorkoutPlans    (+ .Create .Update .Delete)
│  ├─ Foods           (+ .Create .Update .Delete)
│  ├─ NutritionPlans  (+ .Create .Update .Delete)
│  ├─ Tracking        (leaf — "view a trainee's logs & progress"; shared by both log services)
│  ├─ Progress        (+ .Create .Update .Delete)
│  └─ Notes           (+ .Create .Update .Delete)
└─ Trainee
   ├─ MyProfile        (leaf)
   ├─ MyWorkoutPlans   (leaf)
   ├─ WorkoutLogs      (+ .Create)
   ├─ MyNutritionPlans (leaf)
   ├─ NutritionLogs    (+ .Create)
   ├─ MyProgress       (+ .Create)
   └─ MyNotes          (leaf)
```

- The string value of a node is its dotted path, e.g. `Coach.Trainees.Create` == the literal
  string `"CoachApp.Coach.Trainees.Create"`.
- `[Authorize(...)]` is applied at the **service class** level (usually the `.Default` node, so the
  whole service requires it) and again on each **write method** (`.Create` / `.Update` / `.Delete`).
- **Read-only coach log services reuse `Coach.Tracking.Default`** — i.e. `WorkoutLogAppService`
  and `NutritionLogAppService` are both `[Authorize("CoachApp.Coach.Tracking")]`. There is no
  per-feature "view logs" permission.
- No permission is host-only or tenant-only; every node is defined for both multi-tenancy sides.

### 5.6 How roles get their permissions (seeding)

`CoachAppDataSeedContributor` seeds the two personas **by prefix**, which is why
`CoachAppPermissionPrefixes` exists in Domain.Shared (the Domain layer can't reference the
Application.Contracts constants):

1. Ensure the `"Coach"` role exists, then grant it **every** permission whose name starts with
   `"CoachApp.Coach."`.
2. Ensure the `"Trainee"` role exists, then grant it every permission starting with
   `"CoachApp.Trainee."`.
3. From the host context (when multi-tenancy is on), ensure a **demo tenant named `"demo"`** exists
   so tenant-per-coach is testable out of the box — its auto-seeded admin acts as the demo coach.

The seeder runs for the host **and for every tenant**, so the roles + grants exist wherever coaches
and trainees live.

### 5.7 Identity link & multi-tenancy

- A `Trainee` is linked **1:1** to an ABP `IdentityUser` by the raw `Guid UserId` (there is **no**
  navigation property — just the id). The link is enforced structurally by a **unique index on
  `{ TenantId, UserId }`**.
- Creating a trainee (`TraineeAppService.CreateAsync`) is a **two-step provisioning** operation:
  1. Create the login: `new IdentityUser(GuidGenerator.Create(), userName, email, CurrentTenant.Id)`,
     set the password, and `AddToRoleAsync(user, CoachAppRoles.Trainee)`. If no email is supplied it
     synthesizes `"{userName}@coachapp.local"`.
  2. Create the profile: `Trainee.Create(GuidGenerator.Create(), user.Id, …, CurrentTenant.Id)`.
- Deleting a trainee deletes the profile **and** the linked `IdentityUser`.
- `UpdateAsync` touches profile fields only — never `UserName`, `UserId`, or credentials.
- Because business entities are `IMultiTenant` and creation stamps `CurrentTenant.Id`, the tenant
  filter isolates data automatically. You rarely write tenant conditions yourself.

### 5.8 Auth (OpenIddict) — what a client needs

Wired in `CoachAppHttpApiHostModule` and seeded by `OpenIddictDataSeedContributor`:

- **API resource / scope:** `"CoachApp"` (the audience the API validates).
- **Two seeded clients** (from `DbMigrator/appsettings.json` → `OpenIddict:Applications`):
  - **`CoachApp_App`** — a **public** client (no secret) with grant types
    **`password`, `authorization_code`, `client_credentials`, `refresh_token`** (+ ABP's
    `LinkLogin`, `Impersonation`). This is the client a **mobile app or SPA** uses. The
    password/refresh-token pair is the classic mobile flow.
  - **`CoachApp_Swagger`** — authorization-code client for the Swagger UI.
- Token endpoint (OpenIddict convention): `POST /connect/token`. Authority in dev:
  `https://localhost:44370`.
- The host forwards Identity auth to the OpenIddict validation scheme, enables **dynamic claims**,
  configures **CORS** from `App:CorsOrigins`, and turns on multi-tenancy middleware.

### 5.9 Mapping (Mapster) & read-time enrichment

- All entity → DTO mappings are registered in **`CoachAppMapsterConfig.Configure()`**, one
  `TypeAdapterConfig<Entity, Dto>.NewConfig()` per pair. They are plain configs — **no `.Ignore` /
  `.Map` customizations** are needed because computed fields are filled by *enrichers*, not by
  Mapster.
- **`MapsterObjectMapper : IAutoObjectMappingProvider`** is registered in
  `CoachAppApplicationModule` (which does `RemoveAll<IAutoObjectMappingProvider>()` first) so that
  every `ObjectMapper.Map<TSource, TDestination>()` call in the app resolves through Mapster.
- **Enrichers** are the key idea for nutrition/workout read models. Macros and calories are stored
  **only on `Food`, per serving**. `MealItem` / `NutritionLogEntry` store only a `FoodId` and a
  `Quantity` (number of servings). At **read time**, `NutritionPlanEnricher` /
  `NutritionLogEnricher` load the referenced foods and compute
  `Calories/ProteinG/CarbsG/FatG = food.value × quantity`, plus plan/log-level totals. Nothing is
  persisted or recomputed on the entity. Similarly `WorkoutLogEnricher` and the plan services fill
  the display-only `ExerciseName` from the exercise library. **These totals are computed, never
  stored** — an important thing to tell any client developer.

### 5.10 EF Core

- One **`CoachAppDbContext`** that `[ReplaceDbContext]`s the ABP Identity and TenantManagement
  contexts (so you can join across them) and declares a `DbSet` per aggregate **and** per owned
  child (`WorkoutDays`, `WorkoutExercises`, `Meals`, `MealItems`, entries, …).
- Each entity has an **`IEntityTypeConfiguration<T>`** under
  `EntityConfigurations/<Feature>/`, auto-applied by a single
  `builder.ApplyConfigurationsFromAssembly(...)` call — there is **no inline mapping** in the
  DbContext. Configs call `ConfigureByConvention()`, set table name
  `CoachAppConsts.DbTablePrefix + "<Plural>"` (prefix `"App"`, null schema → e.g. `AppTrainees`),
  string lengths (kept in sync with the `*Consts`), `decimal(p,s)` precision for metrics, enum →
  `byte` conversions, and indexes (e.g. unique `{TenantId, UserId}` on trainees; `{TenantId,
  TraineeId, Date}` on progress/notes/logs).
- **Custom repositories** exist *only* to eager-load owned graphs:
  - `EfCoreWorkoutPlanRepository` overrides `WithDetailsAsync()` → `.Include(Days).ThenInclude(Exercises)`.
  - `EfCoreNutritionPlanRepository` overrides `WithDetailsAsync()` → `.Include(Meals).ThenInclude(Items)`.
  This makes `GetAsync(id, includeDetails: true)` return the whole plan graph. Logs don't have a
  custom repo — the services call the parameterized `WithDetailsAsync(x => x.Entries)` overload
  directly.
- **Migrations are the schema mechanism.** The history so far: `Initial` → `Add_Trainees_And_
  TrainingPlans` → `Slice0_AccessFoundation` → `Slice1_ExerciseLibrary` → `Slice2_WorkoutPlans` →
  `Slice3_WorkoutLogs` → `Slice4_FoodLibrary` → `Slice5_NutritionPlans` → `Slice6_NutritionLogs` →
  `Slice7_8_ProgressAndNotes`.

### 5.11 Error handling & localization

- There are **no numeric domain error codes** in use. `CoachAppDomainErrorCodes` is an empty
  placeholder (the `"CoachApp:00001"` pattern is reserved but unused).
- Failures surface two ways:
  - `UserFriendlyException(L["SomeKey"])` for business validation (e.g.
    `"TheSelectedTraineeDoesNotExist"`, `"OneOrMoreExercisesDoNotExist"`,
    `"OneOrMoreFoodsDoNotExist"`).
  - `EntityNotFoundException(typeof(T), id)` for "not found" **and** for the trainee-side ownership
    guard (so a trainee poking at someone else's row gets a clean 404).
- Every user-facing string (labels, permission names, error messages, role names) has a key in
  `Localization/CoachApp/en.json` **and** `ar.json`. Error-code namespaces map via
  `MapCodeNamespace("CoachApp", …)` in `CoachAppDomainSharedModule`.

### 5.12 Consts & enums (the shared vocabulary)

- **Field-length consts** (`<X>Consts` in Domain.Shared) are the *single source of truth* used by
  three places at once: the entity's `Check` calls, the DTO's `[StringLength]` annotations, and the
  EF `HasMaxLength`. Keeping them in one place stops the three from drifting.
- **Enums** (`CoachApp.Enums`, all `: byte`, stored via `HasConversion<byte>()`):
  - `Gender` — Unspecified, Male, Female
  - `TrainingGoal` — General, LoseWeight, BuildMuscle, Maintain, ImproveFitness, Strength
  - `MuscleGroup` — Other, Chest, Back, Shoulders, Arms, Legs, Core, FullBody, Cardio
  - `Equipment` — None, Bodyweight, Barbell, Dumbbell, Machine, Cable, Kettlebell, ResistanceBand, Other

---

## 6. Feature-by-feature reference

Each slice below lists the aggregate's fields, its real invariants, the services + their
permissions, and notes. (DTOs mirror the entities; only differences are called out.)

### 6.1 Trainees (`Entites/Trainees`)

**Entity `Trainee`** — `FullAuditedAggregateRoot<Guid>, IMultiTenant`.
Fields: `UserId`, `UserName`, `FirstName`, `LastName`, `Email?`, `PhoneNumber?`, `Gender`,
`BirthDate?`, `Goal` (`TrainingGoal`), `HeightCm?`, `StartWeightKg?`, `TargetWeightKg?`, `IsActive`.
`Create(...)` validates name/username lengths and (optional) email/phone lengths, sets
`IsActive = true`. Behavior: `Activate()`, `Deactivate()`.

**Services:**
- `TraineeAppService` `[Authorize(Coach.Trainees.Default)]` — `GetAsync`, `GetListAsync` (filter on
  name + goal + active), `CreateAsync` *(+Create; provisions the IdentityUser + Trainee role)*,
  `UpdateAsync` *(+Update; profile fields only)*, `DeleteAsync` *(+Delete; deletes profile + user)*.
- `MyProfileAppService` `[Authorize(Trainee.MyProfile.Default)]` — `GetAsync()` returns the signed-in
  trainee's own profile (by `UserId`).

DTO notes: `CreateTraineeDto` carries `UserName` + `Password` (write-only, never echoed);
`UpdateTraineeDto` has neither. `TraineeDto` never exposes a password.

### 6.2 Exercises (`Entites/Exercises`)

**Entity `Exercise`** — the coach's exercise library. Fields: `Name`, `Description?`,
`Instructions?`, `TargetMuscle` (`MuscleGroup`), `Equipment` (`Equipment`), `VideoUrl?`,
`ImageUrl?`, `IsActive`. `Create` validates lengths, defaults `IsActive = true`. Behavior:
`Activate()`/`Deactivate()`. No children.

**Service:** `ExerciseAppService` `[Authorize(Coach.Exercises.Default)]` — full CRUD; list filters on
name/description text, `TargetMuscle`, `Equipment`, `IsActive`. Coach-only (there is no trainee-side
exercise service; trainees see exercise names via enriched plan/log DTOs).

### 6.3 Workout Plans (`Entites/WorkoutPlans`)

**Aggregate:** `WorkoutPlan` → `WorkoutDay` → `WorkoutExercise`.
- `WorkoutPlan`: `TraineeId`, `Name`, `Description?`, `IsActive`, `Days`. `Create` forces
  `IsActive = false`. Methods: `AddDay`, `ClearDays`, `Activate`, `Deactivate`.
- `WorkoutDay`: `Name`, `Order`, `Exercises`. Method: `AddExercise`.
- `WorkoutExercise` (leaf): `ExerciseId`, `Order`, `Sets`, `Reps?` (free text like `"8-12"`),
  `WeightKg?`, `RestSeconds?`, `Notes?`.

**Invariant:** at most one active plan per trainee — enforced by `SetActiveInternalAsync` in the
coach service (not the entity).

**Services:**
- `WorkoutPlanAppService` `[Authorize(Coach.WorkoutPlans.Default)]` — `GetAsync` (full graph, via
  the custom repo's `WithDetailsAsync`), `GetListAsync` (summaries — days *not* loaded), `CreateAsync`,
  `UpdateAsync` (**full replace** of the day/exercise graph via `ClearDays` + rebuild),
  `DeleteAsync`, `SetActiveAsync` (+Update). Validates the trainee and every referenced exercise
  exist. Fills `ExerciseName` via enrichment.
- `MyWorkoutPlanAppService` `[Authorize(Trainee.MyWorkoutPlans.Default)]` — `GetListAsync()`
  (my summaries), `GetAsync(id)` (mine, with ownership guard), `GetActiveAsync()` (nullable). Read-only.

### 6.4 Workout Logs (`Entites/WorkoutLogs`)

**Aggregate:** `WorkoutLog` → `WorkoutLogEntry`. The trainee records what they actually did.
- `WorkoutLog`: `TraineeId`, `WorkoutPlanId?`, `WorkoutDayId?`, `Date`, `Notes?`, `Entries`.
  Method: `AddEntry`. **No update path** (created whole / deleted whole).
- `WorkoutLogEntry` (leaf): `ExerciseId`, `Order`, `Sets`, `Reps?`, `WeightKg?`, `Notes?`
  — note **no `RestSeconds`** (unlike the plan's `WorkoutExercise`).

**Services:**
- `WorkoutLogAppService` `[Authorize(Coach.Tracking.Default)]` — **read-only** for the coach:
  `GetListAsync` (requires `TraineeId`, optional date range), `GetAsync(id)` (with entries + enriched
  exercise names).
- `MyWorkoutLogAppService` `[Authorize(Trainee.WorkoutLogs.Default)]` — `CreateAsync` (+Create),
  `GetListAsync`, `GetAsync`, `DeleteAsync`. Derives the trainee from the current user, validates
  the referenced exercises exist, and enforces ownership on get/delete.

### 6.5 Foods (`Entites/Foods`)

**Entity `Food`** — the coach's food library; nutrition values are **per one serving**. Fields:
`Name`, `Description?`, `ServingSize`, `ServingUnit` (default `"g"`), `Calories`, `ProteinG`,
`CarbsG`, `FatG`, `IsActive`. `Create` validates name + serving-unit lengths; numeric macros are
guarded only by DTO `[Range(0,100000)]`. Behavior: `Activate`/`Deactivate`. No children, no totals.

**Service:** `FoodAppService` `[Authorize(Coach.Foods.Default)]` — full CRUD; list filters on name +
description text and `IsActive`. Coach-only.

### 6.6 Nutrition Plans (`Entites/NutritionPlans`)

**Aggregate:** `NutritionPlan` → `Meal` → `MealItem`.
- `NutritionPlan`: `TraineeId`, `Name`, `Description?`, `IsActive`, `Meals`. `Create` sets
  `IsActive = false`. Methods: `AddMeal`, `ClearMeals`, `Activate`, `Deactivate`.
- `Meal`: `Name`, `Order`, `Items`. Method: `AddItem`.
- `MealItem` (leaf): `FoodId`, `Order`, `Quantity` (number of servings, e.g. `1.5`). **No stored
  macros.**

**Invariant:** at most one active plan per trainee — enforced by `SetActiveInternalAsync` in the
coach service.

**Services:**
- `NutritionPlanAppService` `[Authorize(Coach.NutritionPlans.Default)]` — `GetAsync` (full graph +
  enriched macros/totals), `GetListAsync` (summaries, no totals), `CreateAsync`, `UpdateAsync`
  (**full replace** via `ClearMeals` + rebuild), `DeleteAsync`, `SetActiveAsync`. Validates the
  trainee and every referenced food exist.
- `MyNutritionPlanAppService` `[Authorize(Trainee.MyNutritionPlans.Default)]` — `GetListAsync()`,
  `GetAsync(id)` (ownership-guarded, enriched), `GetActiveAsync()` (nullable). Read-only.

Macros/calories on `MealItemDto` and the plan totals are **computed at read time** by
`NutritionPlanEnricher` (`food.value × quantity`, summed) — never stored.

### 6.7 Nutrition Logs (`Entites/NutritionLogs`)

**Aggregate:** `NutritionLog` → `NutritionLogEntry`. The trainee logs what they ate.
- `NutritionLog`: `TraineeId`, `NutritionPlanId?` (which plan the day followed), `Date`, `Notes?`,
  `Entries`. Method: `AddEntry`. No update path.
- `NutritionLogEntry` (leaf): `FoodId`, `Order`, `Quantity`, `Notes?`.

**Services:**
- `NutritionLogAppService` `[Authorize(Coach.Tracking.Default)]` — read-only for the coach
  (`GetListAsync` requires `TraineeId` + optional date range; `GetAsync` with entries + enriched
  macros/totals). This is the coach's **adherence** view — note the actual macro totals are
  computed by the enricher; there is **no automated plan-vs-actual "adherence score"** in code.
- `MyNutritionLogAppService` `[Authorize(Trainee.NutritionLogs.Default)]` — `CreateAsync` (+Create),
  `GetListAsync`, `GetAsync`, `DeleteAsync`; validates referenced foods, enforces ownership.

### 6.8 Progress Entries (`Entites/ProgressEntries`)

**Entity `ProgressEntry`** — a dated body-metric snapshot. Fields: `TraineeId`, `Date`, `WeightKg?`,
`BodyFatPercent?`, `ChestCm?`, `WaistCm?`, `HipsCm?`, `ArmCm?`, `ThighCm?`, `Notes?` (max 512).
**There are no photo fields.** `Create` validates only the notes length. No behavior methods
(services mutate fields directly).

**Services:**
- `ProgressEntryAppService` `[Authorize(Coach.Progress.Default)]` — full CRUD keyed on a
  `TraineeId`; list filters `TraineeId` + date range, default sort `Date desc`.
- `MyProgressAppService` `[Authorize(Trainee.MyProgress.Default)]` — `CreateAsync` (+Create),
  `GetListAsync`, `GetAsync`, `DeleteAsync` — **no update** for the trainee. Derives the trainee
  from the current user; ownership-guards single-item ops.

### 6.9 Trainee Notes (`Entites/TraineeNotes`)

**Entity `TraineeNote`** — a note the coach writes about/for a trainee; the author is the audit
`CreatorId`; visible to the trainee. Fields: `TraineeId`, `Date`, `Text` (required, max 2000).
`Create` validates the text. No behavior methods.

**Services:**
- `TraineeNoteAppService` `[Authorize(Coach.Notes.Default)]` — full CRUD keyed on `TraineeId`.
- `MyNoteAppService` `[Authorize(Trainee.MyNotes.Default)]` — **read-only** (`GetListAsync`,
  `GetAsync` with ownership guard). Trainees cannot create/edit/delete notes.

---

## 7. Data model at a glance

All business tables carry the `App` prefix, in the default schema, and (being `IMultiTenant`) a
`TenantId`, plus ABP's full-audit columns (`CreationTime`, `CreatorId`, `LastModificationTime`,
`IsDeleted`, …).

```
IdentityUser ──1:1── Trainee ──┬──< WorkoutPlan ──< WorkoutDay ──< WorkoutExercise ─┐
 (ABP module)     (UserId)     │                                                     ├─ ExerciseId ─> Exercise
                               ├──< WorkoutLog  ──< WorkoutLogEntry ─────────────────┘
                               │
                               ├──< NutritionPlan ──< Meal ──< MealItem ─┐
                               │                                          ├─ FoodId ─> Food
                               ├──< NutritionLog  ──< NutritionLogEntry ─┘
                               │
                               ├──< ProgressEntry
                               └──< TraineeNote
```

`──<` = owns many (composition, cascade). `─>` = loose GUID reference (no FK navigation; validated
in the app service). Solid `1:1` between `IdentityUser` and `Trainee` is by `Trainee.UserId` with a
unique `{TenantId, UserId}` index.

---

## 8. Adding a new feature (the checklist)

Build the slice **bottom-up**, mirroring `Trainee`:

1. **Domain.Shared** — add `Enums/…` (if any) and `Entites/<Feature>/<X>Consts.cs`; reserve an
   error-code/localization key if you need one.
2. **Domain** — add `Entites/<Feature>/<Entity>.cs`: `FullAuditedAggregateRoot<Guid>` (+
   `IMultiTenant`), public setters, a `protected` ctor, a validating `static Create(...)`, and
   behavior methods for real invariants + owned-collection mutation.
3. **Application.Contracts** — add DTOs (`<Entity>Dto`, `CreateUpdate<Entity>Dto` or split
   Create/Update, `Get<Feature>ListInput : PagedAndSortedResultRequestDto`) and the
   `I<Entity>AppService` interface(s) — add a `MyXxxAppService` interface if trainees get a
   self-service view.
4. **Permissions** — add the node(s) to `CoachAppPermissions` (under `Coach` and/or `Trainee`) and
   declare them in `CoachAppPermissionDefinitionProvider` (use `AddCrud` for coach CRUD).
5. **Application** — add `Apis/<Feature>/<Entity>AppService.cs` (fat service: inline
   `WhereIf`/Dynamic-LINQ/paging, cross-aggregate checks, map out) with `[Authorize]` on the class
   and write methods; add the `MyXxxAppService` if needed; add **one Mapster line** in
   `CoachAppMapsterConfig`. Add an enricher if the DTO has computed fields.
6. **EntityFrameworkCore** — add `EntityConfigurations/<Feature>/<Entity>Configuration.cs`
   (`ConfigureByConvention`, table name, lengths, precision, enum conversions, indexes) and the
   `DbSet`(s) in `CoachAppDbContext`. Add a custom repository *only* if you must eager-load an owned
   graph.
7. **Localization** — add keys to **both** `en.json` and `ar.json`.
8. **Migration** — `dotnet ef migrations add <Name> …` (writing the file is fine; **applying** it
   needs the user's OK).
9. **Tests** — domain tests for `Create` + invariants, application tests for the service (mirror
   `test/CoachApp.*.Tests`).

The specialist agents in `.claude/agents/` (`coachapp-architect`, `-backend`, `-db`, `-tester`,
`-reviewer`, `-security`, `-runner`) automate and enforce this checklist.

---

## 9. Build, run, test, migrate

```bash
# Build the whole solution (must be clean)
dotnet build CoachApp.slnx

# Run the API + Swagger  → https://localhost:44370/swagger
dotnet run --project src/CoachApp.HttpApi.Host

# Run tests (xUnit + Shouldly)
dotnet test

# Add a migration (writing files is fine)
dotnet ef migrations add <Name> \
  --project src/CoachApp.EntityFrameworkCore \
  --startup-project src/CoachApp.EntityFrameworkCore

# Apply migrations + seed (roles, permissions, admin, demo tenant, OpenIddict clients)
dotnet run --project src/CoachApp.DbMigrator
```

> ⚠️ **Applying a migration or running `DbMigrator` writes to the database.** In this project the
> rule is: *generating* migration files is fine; **applying** them (or running `DbMigrator`) must be
> confirmed with the repo owner first.

Default local config (`appsettings.json`): SQL Server LocalDB `Database=CoachApp`; authority
`https://localhost:44370`; seeded admin `admin` / `1q2w3E*`; demo tenant `"demo"`.

---

## 10. Naming & gotchas cheat-sheet

- **`Entites`** (sic) is the correct spelling everywhere — namespaces and folders.
- Feature namespaces drop the layer name: entity + DTOs + interface live in
  `CoachApp.Entites.<Feature>`; the service in `CoachApp.Apis.<Feature>`; enums in `CoachApp.Enums`;
  EF config in `CoachApp.EntityConfigurations.<Feature>`; permissions in `CoachApp.Permissions`.
- **Macros/calories are stored only on `Food`** and everything else computes them at read time — do
  not look for macro columns on plans/logs/items; they don't exist.
- **Logs have no update** — they are create/read/delete only.
- **Trainees never update progress** and **never touch notes** (read-only).
- **"Active plan" is a service-level rule**, not a DB constraint.
- **Coach log views use `Coach.Tracking`**, not a per-feature permission.
- Trainee-side "not yours" always surfaces as **404 (`EntityNotFoundException`)**, never 403.
- No MediatR / AutoMapper / Mapperly / FluentValidation — Mapster + DataAnnotations + `Check` +
  fat services only.

---

*This guide reflects the code as of the `redesign/single-app-and-access-foundation` branch
(Phase 1 + Phase 2, slices 0–8). If the code changes, update this file — the code remains the
source of truth.*
