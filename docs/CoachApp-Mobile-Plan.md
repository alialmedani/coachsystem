# CoachApp Mobile — Master Development Roadmap

> **Status:** Planning / architecture proposal (no implementation yet).
> **Backend:** ABP 10.4.0 / .NET 10 single-app, OpenIddict, multi-tenant (tenant-per-coach). Treated as the source of truth.
> **Scope:** Zero → production-ready Flutter mobile app for Coach + Trainee personas.
> **Rule:** The code (backend) is the spec. This plan does not redesign the backend.

---

## How this plan is ordered (the ordering principle)

Not by feature category, but by **what must physically exist before the next thing can compile and run**. Three ordering forces:

1. **Infrastructure before features.** Networking, auth/session, routing, and the design system are prerequisites for *every* screen.
2. **Producers before consumers of reusable components.** Libraries produce the **Exercise/Food pickers** the Plan builders consume. The Plan builders produce the **NestedBuilder + PlanViewer** that Templates and the trainee read-side consume. Libraries and plans produce the read-only **LogView / MacroSummary / DashboardCards / ProgressChart** that both Coach Tracking *and* Trainee logging consume.
3. **Data producers before data readers.** A Coach must create a trainee, a plan, and set it active **before** the Trainee's Today/logging screens have anything to show — so the coach management slice is sequenced ahead of the trainee experience for end-to-end testability.

**Two reorders vs. a naive feature order:**
- **Trainee "My Plans" moves *before* Trainee Today and logging** — Today and manual logging read plan data and pull exercise/food IDs from the active plan.
- **A "Shared Domain Views" bridge phase** (read-only LogView, MacroSummary, DashboardCards, ProgressChart) is built **once** between the Coach and Trainee slices, because both personas render the same log/macro/dashboard/progress data.

---

## Decisions required BEFORE implementation (blocking)

| # | Decision | Why it's blocking | Options / recommendation | Needed by |
|---|---|---|---|---|
| D1 | **Refresh tokens / `offline_access`** | Determines whether we build silent session restore or fall back to re-login on expiry | Backend adds `offline_access` to the seeded `CoachApp_App` scopes (re-seed). **Recommend fixing before Phase 1.** | Phase 1 |
| D2 | **Tenant onboarding UX** (tenant-per-coach) | The app must set `__tenant`; how does a user supply it? | (a) "Gym/Coach code" field → tenant **name** *(recommended)*; (b) invite/deep-link; (c) host directory lookup *(doesn't exist)*. | Phase 1 |
| D3 | **Trainee manual-logging scope** | Trainees have **no** endpoint to browse the Exercise/Food libraries; manual (off-plan) logging needs IDs | (a) **v1: trainees log only items in their active plan** (no backend change) *(recommended)*; (b) backend adds read-only `My*` browse (see Backend Changes B2). | Phases 13–14 |
| D4 | **Coach "active trainee" context model** | Most coach features are trainee-scoped; re-selecting per screen is bad UX | Sticky selected-trainee in a session-scoped provider. **Recommend sticky context.** | Phase 4 |
| D5 | **Min OS versions & target devices** | Affects plugins, CI, store config | Recommend iOS 13+, Android 8+ (API 26+). | Phase 0 |
| D6 | **Design language / branding** | Colors, logo, typography, app name, bundle IDs | Brand assets or approval to use a neutral CoachApp theme. | Phase 0 / 3 |
| D7 | **Crash reporting / analytics vendor** | Adds SDK + config + privacy | Recommend Sentry or Firebase Crashlytics; analytics optional. | Phase 0 stub / 21 |
| D8 | **Push notifications in scope?** | No backend support today | Recommend **out of v1**. | Post-v1 |

---

## Backend / configuration dependencies (must be true for the app to work)

- **`CoachApp_App` public client**, `grant_type=password` + `refresh_token`, scope `CoachApp` — present ✅
- **`offline_access` scope** on the client — **missing** (D1) ⚠️
- **`/api/abp/application-configuration`** bootstrap + **`/connect/token`** — present ✅
- **`__tenant` header** resolution — default ABP, works ✅
- **Device-reachable HTTPS** for dev/staging (a physical device can't use `localhost` + dev cert) — ops requirement
- **Trainee library-browse endpoints** — absent (D3)
- **Committed secrets / empty CORS** — ops hygiene (CORS only matters if we add Flutter Web)

---

## Recommended Stack (summary)

| Concern | Choice |
|---|---|
| Framework / language | **Flutter (stable)** + **Dart 3** (sound null-safety) |
| State management | **Riverpod v2** (+ `riverpod_generator`) |
| Navigation | **go_router** (declarative auth + role guards) |
| HTTP | **Dio** + interceptors (auth, `__tenant`, refresh-on-401, error mapping) |
| Models / JSON | **freezed** + **json_serializable** (+ optional **Retrofit**) |
| Secure storage | **flutter_secure_storage** (tokens + tenant) |
| Local cache | **Hive** (config, lookups, last-known Today) |
| DI | Riverpod providers |
| Form validation | validators mirroring backend `*Consts` / `[Range]` / `[Required]` |
| Localization | Flutter `gen-l10n` (ARB) — English + Arabic + RTL |
| Design system | Material 3 + custom theme (light/dark) + CoachApp component library |
| Charts | `fl_chart` |
| Offline | cache-first reads (revalidate), online-only writes |

**Why Flutter:** first-class RTL/bidi for Arabic; excellent ergonomics for the many nested data-entry forms (plan→day→exercise→sets; plan→meal→food→qty); pixel-consistent rendering of the builder screens.

---

# PHASE 0 — Project Foundation

**Goal:** A running, empty app with the full infrastructure spine (networking, DI, config, storage, theme, localization scaffold, logging, error plumbing) so any feature can be added without touching architecture.

**Tasks:**
- Flutter (stable) + Dart 3; single app; placeholder org/bundle id.
- Feature-first + light layering: `core/`, `shared/`, `features/coach/*`, `features/trainee/*`.
- Riverpod v2 (providers as DI) + `riverpod_generator`.
- `--dart-define` flavors **dev / staging / prod**: `apiBaseUrl`, `authority`, `oidcClientId=CoachApp_App`, `defaultTenant?`, `logLevel`, `crashReportingKey?`. An `AppConfig` provider reads them.
- Per-flavor API base URL (never hardcoded in features).
- Dio single instance + dev `LogInterceptor` + timeouts. Retrofit for typed services.
- freezed + json_serializable; a generic `PagedResultDto<T>`; enum codecs (`Gender/TrainingGoal/MuscleGroup/Equipment` byte→int, `DayOfWeek` int).
- Thin repositories returning `Result<T, AppFailure>`.
- `flutter_secure_storage` wrapper (`TokenStore`) for access/refresh tokens + tenant.
- Hive box(es) for config + lookups (scaffold).
- `Logger` abstraction (verbose dev, redacted prod — no tokens/PII).
- `AppFailure` + `ErrorMapper` (Dio → failure) + error-code constants (`CoachApp:00001..00005`).
- `connectivity_plus` + `ConnectivityService` + global offline banner primitive.
- `gen-l10n` wired; empty `app_en.arb` + `app_ar.arb`; `MaterialApp.router` with localization delegates + `supportedLocales`; force-RTL dev toggle.
- `ThemeData` light + dark from a `DesignTokens` file (finalized Phase 3).
- Placeholder icons/splash (`flutter_launcher_icons`, `flutter_native_splash`).
- Analyzer + lints; CI stub (build + analyze + test); pre-commit format.

**Screens:** none (a "Hello, health-check" placeholder pinging `application-configuration` anonymously).

**APIs:** connectivity smoke test only.

**Dependencies:** none.

**Reusable components produced:** `AppConfig`, `DioClient`, `TokenStore`, `AppFailure`/`ErrorMapper`, `PagedResultDto<T>`, enum codecs, `ConnectivityService`, `Logger`, theme tokens.

**Tests:** unit — enum codecs round-trip; `PagedResultDto` parse; `ErrorMapper` maps sample ABP envelopes; `AppConfig` flavor resolution. CI green.

**Definition of done:** app boots in all three flavors on iOS + Android; analyzer clean; CI passes; smoke test reaches the server.

**Risks:** iOS flavor schemes; Dio interceptor ordering; Hive/secure-storage init before first use.

---

# PHASE 1 — Authentication, Session & Tenant

**Goal:** A user can enter tenant + credentials, obtain/store tokens, be bootstrapped with role+permissions+tenant, stay logged in across restarts, auto-refresh, and log out. Gates every feature.

**Tasks:**
- **Login:** tenant field (D2) → `POST /connect/token` (`grant_type=password`, `client_id=CoachApp_App`, `scope=CoachApp offline_access profile email phone roles`, `__tenant` header).
- **Token storage:** persist access + refresh + tenant in `TokenStore`.
- **Auth interceptor:** attach `Authorization: Bearer` + `__tenant` on every request.
- **Refresh interceptor:** on 401, single-flight `grant_type=refresh_token`; queue + replay in-flight; on failure → clear session → `/login`. (If D1 unresolved: 401 → logout.)
- **Bootstrap:** `GET /api/abp/application-configuration` → parse `currentUser` (id, userName, email, tenantId, roles), `auth.grantedPolicies`, `currentTenant`.
- **Session model:** `sessionProvider` (`AsyncNotifier`): `Unauthenticated | Authenticating | Authenticated{user, roles, policies, tenant} | Error`.
- **Role detection:** `isCoach` / `isTrainee` from `roles`.
- **Permission system:** `can(policyName)` over `grantedPolicies`; typed policy constants from the backend tree.
- **Session restoration:** cold start → refresh + re-bootstrap if refresh token present; else login.
- **Logout:** clear tokens + caches + providers; route to login (optional `/connect/logout`).
- **Navigation guards (foundation):** `go_router` `refresh`-listenable on `sessionProvider`; redirect unauth→`/login` and route by role.
- **Change-password (self):** wire `POST /api/account/my-profile/change-password` (surfaces in Profile phases).

**Coach vs Trainee session separation:** identical auth pipeline; the **role from bootstrap selects the shell + policy set**. No separate login. The session provider is the single source; guards and visibility branch on `roles` + `grantedPolicies`. A user is one role per tenant; coach UI is never shown to a trainee token even via deep link.

**Screens (Shared):** Login/Tenant, Splash/Session-restore, Session-expired interstitial.

**APIs:** `/connect/token` (password + refresh), `application-configuration`, `application-localization` (optional preload).

**Dependencies:** Phase 0.

**Reusable components produced:** `sessionProvider`, `can()`, auth/refresh/tenant interceptors, `AuthRepository`, policy constants, `AuthGuard`.

**Tests:** unit — token store; refresh single-flight (concurrent 401s → one refresh); bootstrap parse; role/permission derivation. Widget — login validation, error surfacing. Integration — login→bootstrap→role route; expired→refresh→replay; refresh-fail→logout.

**Definition of done:** coach + trainee accounts log in against a real tenant, land on their shell, survive restart, auto-refresh, log out cleanly; 401 handling verified.

**Risks:** **D1 (refresh token issuance)**; `__tenant` correctness at token time; refresh races; clock skew.

---

# PHASE 2 — App Shells & Role Routing

**Goal:** Two navigable shells (bottom nav + nested stacks), entered by role, with permission-based tab visibility. Placeholder bodies; real wiring.

**Tasks:**
- go_router tree: `/login`, `/coach/*`, `/trainee/*`; `redirect` enforces auth + role; cross-role deep links bounce to the correct shell.
- **Coach shell tabs:** Dashboard, Trainees, Library (Exercises|Foods), Plans (Workout|Nutrition|Templates), More (Tracking / Profile / Settings). Independent `Navigator` per tab (StatefulShellRoute).
- **Trainee shell tabs:** Today, Workout, Nutrition, Progress, Profile.
- Permission-based visibility via `can(policy)`.
- Deep-link scheme + a couple of routes (trainee detail, today) for future notifications; validated against role.
- Nav conventions: list→detail→edit pushed; pickers as bottom sheets; confirms as dialogs.

**Screens:** Coach Shell, Trainee Shell (placeholder tab bodies).

**APIs:** none (uses loaded session role/policies).

**Dependencies:** Phase 1; Phase 3 primitives for the nav bar (stub then swap).

**Reusable components produced:** `CoachShell`, `TraineeShell`, `AppBottomNav`, route table, role/permission guards.

**Tests:** widget — coach token → coach tabs; trainee token → trainee tabs; unauth redirect; cross-role deep-link guard; tab-stack preservation.

**Definition of done:** each role lands on the correct shell; all tabs navigable to placeholders; per-tab back-stack preserved; unauthorized routes blocked.

**Risks:** StatefulShellRoute stack quirks; redirect loops.

---

# PHASE 3 — Design System & Shared UI (build-once components)

**Goal:** The full catalog of reusable primitives so no feature reinvents UI. Finalize theme tokens. Gates all feature phases.

**Finalize tokens:** typography scale (tabular figures for numbers), 4-pt spacing, semantic + macro color palette (calories/protein/carbs/fat), light+dark, AA contrast, RTL-verified.

**Primitives (built once, used everywhere):**
- **Chrome:** `AppScaffold`, `AppBar` (RTL-aware), `AppBottomNav`, `SectionHeader`.
- **Buttons:** `AppButton` (primary/secondary/destructive/text, loading/disabled).
- **Inputs:** `AppTextField`, `AppNumberField`, `AppWeightField` (kg), `AppDecimalField` (macros/qty), `AppRepsField` (free-form string), `AppDropdown<Enum>` (Gender/Goal/Muscle/Equipment/DayOfWeek), `AppDatePicker`, `AppSearchField` (debounced).
- **Filters:** `FilterBar` / `FilterChips`.
- **Containers/lists:** `AppCard`, `AppListTile`, `PagedListView` (SkipCount/MaxResultCount/Sorting + infinite scroll), `PullToRefresh`.
- **State views:** `LoadingState` (skeletons), `EmptyState`, `ErrorState` (retry), `AsyncValueView<T>` (one widget mapping AsyncValue→three states, used by every screen).
- **Overlays:** `AppBottomSheet` (generic + list-picker), `ConfirmDialog` (destructive + typed-confirm), `InputDialog`, `AppSnackbar`/toast.
- **Status/metrics:** `StatusBadge` (active/inactive, active-plan, rest-day, logged-today), `MacroProgressBar` (visually capped at 100%, true % label), `CompletionRing`.
- **Forms:** `FormScaffold` (keyboard handling, submit/loading, error summary), `Validators` (mirror backend), **UnsavedChangesGuard** mixin (all editors).

**Screens:** Component Gallery (debug-only), light/dark/RTL.

**APIs:** none.

**Dependencies:** Phase 0 theme; independent of auth (parallel with Phases 1–2).

**Reusable components produced:** the entire catalog above.

**Tests:** widget/golden — each component in light/dark/RTL; `AsyncValueView` transitions; `Validators` vs backend limits; `PagedListView` paging + refresh; `UnsavedChangesGuard`.

**Definition of done:** gallery renders all components in 2 themes × 2 directions, no overflow; validators match backend; state views standardized.

**Risks:** RTL mirroring; number/decimal input + locale (Arabic digits) — decide here.

---

# PHASE 4 — Coach: Trainees (first real feature)

**Goal:** Full trainee lifecycle. Produces **TraineePicker** + sticky **active-trainee context** (D4) reused by every later coach feature.

**Flows:** list (paged, search name, filter goal/active); detail; create (username+password+profile, credential-share confirmation using typed values — **never returned by API**); edit (profile + `IsActive`); activate/deactivate (`IsActive` → drives login lockout, warn); reset password (dedicated, no current pw); destructive delete (cascades login+data → typed confirm); validation mirroring `TraineeConsts`; Identity `UserFriendlyException` handling.

**Screens & endpoints:**

| Screen | Type | Endpoint(s) |
|---|---|---|
| Trainees List | Read | `GET /api/app/trainee` (Filter/Goal/IsActive, paged) |
| Trainee Detail | Read | `GET /api/app/trainee/{id}` |
| Create Trainee | Create | `POST /api/app/trainee` |
| Edit Trainee | Edit | `PUT /api/app/trainee/{id}` |
| Reset Password | Edit (dialog) | `POST /api/app/trainee/{id}/reset-password` |
| (Delete) | action | `DELETE /api/app/trainee/{id}` |

**Permissions:** `Coach.Trainees[.Create/.Update/.Delete/.ResetPassword]`.

**Dependencies:** Phases 1–3.

**Reusable components produced:** `TraineePicker`, `activeTraineeProvider`, `TraineeCard`, credential-share sheet, password-generator helper.

**Tests:** unit — trainee repo, list-input mapping, validators. Widget — create form, credential-share, deactivate warning, delete typed-confirm. Integration — create→list→edit→reset-password→deactivate blocks login.

**Definition of done:** coach creates a working trainee login end-to-end (verified by logging in as that trainee), edits, deactivates (login blocked), resets password, deletes; all states + errors handled.

**Risks:** credentials-never-returned UX; lockout clarity; destructive delete safety.

---

# PHASE 5 — Coach: Exercise Library

**Goal:** Exercise CRUD + reusable **ExercisePicker** required by Workout Plans/Templates (and possibly trainee logging).

**Tasks:** list (search name/desc, filter muscle/equipment/active, paged); detail; create/edit (`CreateUpdateExerciseDto`); delete with **`ExerciseInUse` (`CoachApp:00004`)** → "Deactivate instead"; enum dropdowns.

**Screens & endpoints:** List/Detail/Editor → `GET/POST /exercise`, `GET/PUT/DELETE /exercise/{id}`.

**Permissions:** `Coach.Exercises[.Create/.Update/.Delete]`.

**Dependencies:** Phases 1–3.

**Reusable components produced:** **`ExercisePicker`** (searchable sheet over `GET /exercise`), `ExerciseCard`, muscle/equipment chips.

**Tests:** unit — repo + in-use mapping. Widget — filters, delete-in-use→deactivate, picker. Integration — CRUD round-trip; delete blocked when referenced.

**Definition of done:** full CRUD; in-use delete handled; ExercisePicker returns a valid `ExerciseId`.

**Risks:** URL validation; picker performance (server paging + debounce).

---

# PHASE 6 — Coach: Food Library

**Goal:** Food CRUD + reusable **FoodPicker** required by Nutrition Plans/Templates (and trainee nutrition logging).

**Tasks:** list (search, filter active, paged); detail; create/edit (`CreateUpdateFoodDto`, **per-serving** macros); delete with **`FoodInUse` (`CoachApp:00005`)** → "Deactivate instead".

**Screens & endpoints:** List/Detail/Editor → `GET/POST /food`, `GET/PUT/DELETE /food/{id}`.

**Permissions:** `Coach.Foods[.Create/.Update/.Delete]`.

**Dependencies:** Phases 1–3.

**Reusable components produced:** **`FoodPicker`** (searchable sheet, shows per-serving macros), `FoodCard`, `MacroChips`.

**Tests:** unit — repo + in-use; per-serving display. Widget — picker; delete-in-use. Integration — CRUD; delete blocked when referenced.

**Definition of done:** full CRUD; in-use delete handled; FoodPicker returns a valid `FoodId` + per-serving macros.

**Risks:** decimal precision/rounding; serving-unit clarity.

---

# PHASE 7 — Coach: Workout Plans (nested builder — major)

**Goal:** Nested Workout Plan builder. Produces reusable **NestedBuilderScaffold**, **PrescribedExerciseEditor**, **DayScheduleChip**, **PlanViewer**.

**UX (nested editor):**
- **List** (`GET /workout-plan?TraineeId=`, **summaries — no days**) → active badge, set-active, delete.
- **Detail** (`GET /workout-plan/{id}` → full tree via `PlanViewer`).
- **Builder (create/edit):** editable tree in a `WorkoutPlanBuilderController`:
  - Plan header: name, description, trainee (via `TraineePicker` / active-trainee context).
  - **Days** reorderable cards: name, **`ScheduledDay` weekday chip (DayOfWeek?, nullable = unscheduled)**, `Order` implicit by position.
  - **Exercises** per day: reorderable rows; tap → sheet with **ExercisePicker** + **PrescribedExerciseEditor** (`Sets` int, **`Reps` free-form string** — text field not stepper, `WeightKg` decimal, `RestSeconds` int, `Notes`). **No per-set grid, no Tempo.**
  - Add/remove/reorder days & exercises; live validation vs `WorkoutPlanConsts`.
- **Save strategy:** **single POST/PUT of the entire tree** (`CreateUpdateWorkoutPlanDto`); update is **full replace** (server `ClearDays`+rebuild; child IDs regenerate) — controller always resends the complete graph; no diffing.
- **Set active:** `POST /workout-plan/{id}/set-active` (single-active enforced server-side).
- **Unsaved-changes protection** on back/tab-switch.
- Errors: `TraineeNotFound`, `ExercisesNotFound`.

**Screens & endpoints:** List/Detail/Builder + set-active → `GET/POST/PUT/DELETE /workout-plan(/{id})`, `POST …/{id}/set-active`.

**Permissions:** `Coach.WorkoutPlans[.Create/.Update/.Delete]` (set-active = `.Update`).

**Dependencies:** Phase 4 (trainee), Phase 5 (ExercisePicker), Phase 3 (NestedBuilder primitives).

**Reusable components produced:** **`NestedBuilderScaffold`** (add/reorder/remove + dirty tracking + unsaved guard), **`PrescribedExerciseEditor`**, **`DayScheduleChip`**, **`PlanViewer`**, **`WorkoutDayCard`**.

**Tests:** unit — builder controller (add/reorder/remove; full-tree serialization; enrich). Widget — nested add/reorder; reps-as-text; unsaved prompt; set-active. Integration — create nested plan→GET detail matches→edit full-replace→set-active deactivates others.

**Definition of done:** coach builds a multi-day scheduled plan with exercises, saves, re-opens (round-trips), edits via full replace, activates (single-active enforced).

**Risks:** **full-replace semantics**; list-omits-children (must GET detail); reorder + `Order` integrity; large payloads; deep-form keyboard management.

---

# PHASE 8 — Coach: Nutrition Plans (nested builder + targets)

**Goal:** Nutrition Plan builder reusing NestedBuilder + FoodPicker; adds **targets** + **server-computed totals**. Produces **MacroSummaryCard** (reused widely).

**UX:**
- **List** (`GET /nutrition-plan?TraineeId=`, summaries) → active badge, set-active, delete.
- **Detail** (`GET /nutrition-plan/{id}` → meals + items + **enriched per-item macros + totals**).
- **Builder:** header (name/desc/trainee) + **optional targets** (`TargetCalories/Protein/Carbs/Fat`); **Meals** (reorderable) → **Items** via **FoodPicker** + **`Quantity` (servings)**. Show a **live client-side estimate** (per-serving × qty); on save, re-fetch **authoritative server totals** (`MacroSummaryCard`). Targets unset → meal totals act as targets downstream.
- **Save:** single POST/PUT full tree (`CreateUpdateNutritionPlanDto`); update = full replace.
- **Set active:** `POST /nutrition-plan/{id}/set-active`.
- Unsaved-changes guard; validation (`FoodsNotFound`, `TraineeNotFound`, ranges).

**Screens & endpoints:** List/Detail/Builder + set-active → `GET/POST/PUT/DELETE /nutrition-plan(/{id})`, `POST …/{id}/set-active`.

**Permissions:** `Coach.NutritionPlans[.Create/.Update/.Delete]`.

**Dependencies:** Phase 6 (FoodPicker), Phase 7 (NestedBuilder), Phase 4 (trainee).

**Reusable components produced:** **`MacroSummaryCard`**, **`MealCard`**, **`QuantityEditor`**, target-inputs group.

**Tests:** unit — builder controller; estimate vs server-total reconciliation. Widget — meal/item add/reorder; targets; unsaved guard. Integration — create→totals match server→edit full-replace→set-active.

**Definition of done:** coach builds a plan with meals/foods/quantities + optional targets, sees correct server totals, edits, activates.

**Risks:** estimate vs server-total divergence (trust server); decimal quantity UX; targets-vs-totals fallback clarity.

---

# PHASE 9 — Coach: Templates (Workout + Nutrition)

**Goal:** Reusable templates + the two-way relationship with trainee plans, reusing Phase 7/8 builders.

**Template ↔ plan relationships (explicit in UI):**
- A **template has no trainee and no active state** — a reusable blueprint (ScheduledDays and nutrition targets preserved).
- **Clone → trainee** creates a **real, inactive plan** for a trainee (returns `WorkoutPlanDto` / `NutritionPlanDto`); coach must **set active** separately. Requires **`Coach.WorkoutPlans.Create` / `Coach.NutritionPlans.Create`** (not a template permission).
- **Save-as-template** snapshots an existing trainee plan into a new template (drops trainee/active).

**Flows (both):** list (summaries); detail (`GET …/{id}`); create/edit (**reuse plan builders** minus trainee/active); delete; **clone-to-trainee** (pick trainee + optional name/desc override → land on new plan, prompt to activate); **save-plan-as-template** (entry from plan detail in Phases 7/8).

**Screens & endpoints:**
- Workout: `GET/POST/PUT/DELETE /workout-plan-template(/{id})`, `POST …/{id}/clone-to-trainee`, `POST …/save-as-template`.
- Nutrition: `GET/POST/PUT/DELETE /nutrition-plan-template(/{id})`, `POST …/{id}/clone-to-trainee`, `POST …/save-as-template`.

**Permissions:** `Coach.WorkoutPlanTemplates[.Create/.Update/.Delete]`, `Coach.NutritionPlanTemplates[.Create/.Update/.Delete]`; **clone = `Coach.*Plans.Create`**.

**Dependencies:** Phases 7 & 8; Phase 4 (trainee for clone).

**Reusable components produced:** template list/builder wrappers (thin over plan builders), clone dialog, save-as-template dialog.

**Tests:** unit — template↔plan mappers; clone request. Widget — clone (trainee + overrides), save-as-template. Integration — create template→clone→inactive plan appears→activate; save plan as template→edit→clone.

**Definition of done:** coach manages templates, clones into (inactive) trainee plans, activates, saves plans back as templates; relationship clear.

**Risks:** conveying "clone creates an inactive plan you must activate"; permission nuance (clone needs Plans.Create); builder reuse without trainee/active fields.

---

# PHASE 10 — Shared Domain Views (bridge layer)

**Goal:** Build **once** the read-only viewers both Coach Tracking and the Trainee experience consume — prevents duplication across later phases.

**Components:**
- **`WorkoutLogView`** — renders `WorkoutLogDto` with **`PrescribedVsActualRow`** (prescribed snapshot vs actual sets/reps/weight, enriched names). Read-only (coach) + editable hook (trainee, Phase 14).
- **`NutritionLogView`** — renders `NutritionLogDto` with per-item macros + **daily totals** (reuses `MacroSummaryCard`).
- **`DashboardCards`** — `NutritionAdherenceCard` (consumed vs target, **uncapped %** → bar capped, label true value, null-safe) + `WorkoutCompletionCard` (completed/planned, weeks, %). Shared coach + trainee.
- **`ProgressChart` / `MetricTrend`** — weight/body-fat/measurement trends from `ProgressEntryDto[]`. Shared coach + trainee.
- **`DateRangePicker`** for tracking/dashboards.

**Screens:** none (components + gallery entry).

**APIs:** none directly (renders passed-in DTOs).

**Dependencies:** Phases 7–8 (DTO shapes, MacroSummaryCard), Phase 3.

**Reusable components produced:** `WorkoutLogView`, `NutritionLogView`, `PrescribedVsActualRow`, `DashboardCards`, `ProgressChart`, `DateRangePicker`.

**Tests:** widget/golden — prescribed-vs-actual (incl. null prescribed for manual logs); uncapped-%; empty/no-plan; RTL; chart with 0/1/many points.

**Definition of done:** all four viewers render correctly for empty, partial, and full data in both themes/directions.

**Risks:** getting prescribed-vs-actual + uncapped-% right *once*; chart lib (`fl_chart`).

---

# PHASE 11 — Coach: Tracking, Dashboard, Progress, Notes

**Goal:** The coach's read/monitor surface, all scoped to the **active trainee** (D4), reusing Phase 10 viewers.

**Trainee-scoping model:** coach selects a trainee **once** (`TraineePicker` → `activeTraineeProvider`); Tracking, Dashboard, Progress, Notes read that context and pass `TraineeId`. A persistent header shows the selected trainee with a "change" affordance. Deep-linking to a trainee sets the context.

**Screens & endpoints:**

| Screen | Type | Endpoint(s) |
|---|---|---|
| Coach Dashboard | Read | `GET /trainee-dashboard/summary` (+ adherence/completion) |
| Workout Logs (list/detail) | Read | `GET /workout-log?TraineeId=&FromDate=&ToDate=`, `GET /workout-log/{id}` |
| Nutrition Logs (list/detail) | Read | `GET /nutrition-log?TraineeId=…`, `GET /nutrition-log/{id}` |
| Progress (CRUD) | C/E/R | `GET/POST/PUT/DELETE /progress-entry(/{id})` |
| Notes (CRUD) | C/E/R | `GET/POST/PUT/DELETE /trainee-note(/{id})` |

- **Dashboard:** date + range; `DashboardCards`; null/empty (no active plan) + uncapped-% handled.
- **Logs:** date-range filter; list → detail via `WorkoutLogView` / `NutritionLogView` (read-only).
- **Progress:** list (date range, `Date desc`) + `ProgressChart`; create/edit/delete (`CreateUpdateProgressEntryDto`, no photos).
- **Notes:** list (`Date desc`) + create/edit/delete (`CreateUpdateTraineeNoteDto`, text ≤2000).

**Permissions:** `Coach.Tracking` (logs + dashboards), `Coach.Progress[.Create/.Update/.Delete]`, `Coach.Notes[.Create/.Update/.Delete]`.

**Dependencies:** Phase 4 (active trainee), Phase 10 (viewers).

**Reusable components produced:** active-trainee header, progress editor form, note editor form.

**Tests:** unit — dashboard/adherence mapping; progress/note repos. Widget — trainee-scope header; date-range; progress/note CRUD; uncapped-%/empty. Integration — trainee logs data → coach sees it scoped to the selected trainee only.

**Definition of done:** coach picks a trainee and sees that trainee's dashboard, logs, progress (CRUD), notes (CRUD), correctly scoped; empty/no-plan clean.

**Risks:** consistent scope across tabs; date-range/timezone; **no roster-wide overview** endpoint (gap B3; v1 composes per-trainee).

---

# PHASE 12 — Trainee: My Plans (read) *(moved before Today)*

**Goal:** Read-only trainee plan layer — a hard dependency of Today and manual logging (source of exercise/food IDs, D3).

**Screens & endpoints:**

| Screen | Endpoint | Note |
|---|---|---|
| My Workout Plans (list) | `GET /my-workout-plan` | **unpaged `List<>`** |
| My Workout Plan (detail) | `GET /my-workout-plan/{id}` | full tree via `PlanViewer` |
| My Active Workout Plan | `GET /my-workout-plan/active` | nullable |
| My Nutrition Plans (list) | `GET /my-nutrition-plan` | unpaged |
| My Nutrition Plan (detail) | `GET /my-nutrition-plan/{id}` | meals + macros |
| My Active Nutrition Plan | `GET /my-nutrition-plan/active` | nullable |

- Respect **summary-list vs detail** (list items lack children).
- Ownership server-enforced (404 if not yours).

**Permissions:** `Trainee.MyWorkoutPlans`, `Trainee.MyNutritionPlans`.

**Dependencies:** Phases 1–3, 7–8 (`PlanViewer`, `MacroSummaryCard`).

**Reusable components produced:** trainee plan-list + detail wrappers; **active-plan providers** (`myActiveWorkoutPlanProvider`, `myActiveNutritionPlanProvider`) reused by Today + logging.

**Tests:** unit — unpaged list parse; active nullable. Widget — plan viewer read-only; empty. Integration — trainee sees exactly the assigned/activated plans.

**Definition of done:** trainee reads plans + active plan; providers expose active-plan data downstream.

**Risks:** unpaged list shape; nullable active plan handling.

---

# PHASE 13 — Trainee: Today (primary experience)

**Goal:** Trainee home composing scheduled workout + nutrition + logs, with correct **local-date** handling; entry point into logging.

**Rules:**
- **Local date:** client sends **trainee-local `Date`** to `GET /my-today?Date=` (never server UTC). Handle timezone + day rollover.
- **Render `MyTodayDto`:** active workout plan flag + `WorkoutPlanId`; **`ScheduledWorkoutDays`** for today's weekday (prescribed exercises via `PlanViewer`); **`IsRestDay`**; **`AlreadyLoggedWorkoutToday`** + `LatestWorkoutLog` (via `WorkoutLogView`); active nutrition plan (`MacroSummaryCard`); **`NutritionAdherence`** (always present, `DashboardCards`); `AlreadyLoggedNutritionToday`.
- **Entry points:** "Log this workout" → from-day logging (Phase 14); "Log nutrition" → from-plan logging (Phase 15); view latest log; states for no-plan / rest-day / already-logged.
- **Refresh:** pull-to-refresh; invalidate after any log write.

**Screens & endpoints:** Today → `GET /my-today?Date=`.

**Permissions:** `Trainee.MyToday`.

**Dependencies:** Phase 12 (plans/active providers), Phase 10 (log/macro/dashboard views).

**Reusable components produced:** Today composition; local-date service (reused by logging defaults).

**Tests:** unit — local-date computation; DTO composition. Widget — rest-day, no-plan, already-logged, scheduled-day; RTL. Integration — coach schedules a day → Today shows it on the right weekday; logging updates already-logged + adherence.

**Definition of done:** Today accurately reflects schedule/logs for the correct local date across all states and links into logging.

**Risks:** **timezone correctness**; multiple-logs-per-day (most recent); adherence null/empty.

---

# PHASE 14 — Trainee: Workout Logging

**Goal:** Full workout logging with the **prescribed-snapshot rule** respected.

**Rules:**
- **Log from scheduled day:** `POST /my-workout-log/from-day` (`WorkoutDayId` + local `Date`) — server pre-fills entries **and snapshots prescribed**. Then edit **actuals** via `PrescribedVsActualRow`.
- **Manual log:** `POST /my-workout-log` (prescribed null). **D3:** exercise selection limited to the trainee's **plan exercises** in v1 (no library-browse endpoint).
- **View:** `GET /my-workout-log/{id}` via `WorkoutLogView`.
- **Edit:** `PUT /my-workout-log/{id}` — **send actual fields only**; **never send `Prescribed*`** (server preserves them keyed by `ExerciseId+Order`). Prescribed fields non-editable in UI.
- **Delete:** `DELETE /my-workout-log/{id}`.
- **History:** `GET /my-workout-log` (date range).
- Ownership server-enforced (404).

**Screens & endpoints:** Log (from-day/manual), View, Edit, History → `/my-workout-log*`.

**Permissions:** `Trainee.WorkoutLogs[.Create/.Update]` (delete guarded by Default — missing `.Delete` policy).

**Dependencies:** Phase 13 (Today entry), Phase 12 (plan exercises for manual), Phase 10 (`PrescribedVsActualRow`).

**Reusable components produced:** editable `WorkoutLogEditor`, plan-derived exercise picker.

**Tests:** unit — create/update DTO builders **exclude prescribed**; from-day mapping. Widget — prescribed read-only; actual editing; delete confirm. Integration — from-day seeds prescribed→edit actuals→prescribed unchanged; manual log; ownership 404.

**Definition of done:** trainee logs from schedule and manually, edits actuals without touching prescribed, deletes, browses history; prescribed snapshot verified immutable.

**Risks:** **accidental prescribed mutation** (read-only design); D3 manual-picker limitation; timezone on `Date`.

---

# PHASE 15 — Trainee: Nutrition Logging

**Goal:** Full nutrition logging with server-computed macros.

**Rules:**
- **Log from plan:** `POST /my-nutrition-log/from-plan` (`NutritionPlanId` + local `Date`) — server flattens meals→items. Adjust quantities/remove after.
- **Manual log:** `POST /my-nutrition-log` — food selection limited to **plan foods** in v1 (D3).
- **View:** `GET /my-nutrition-log/{id}` via `NutritionLogView` (per-item macros + **daily totals**).
- **Edit:** `PUT /my-nutrition-log/{id}` (entries; plan ref untouched).
- **Delete:** `DELETE /my-nutrition-log/{id}`.
- **History:** `GET /my-nutrition-log` (date range).
- Macros/totals always from server (`MacroSummaryCard`).

**Screens & endpoints:** Log (from-plan/manual), View, Edit, History → `/my-nutrition-log*`.

**Permissions:** `Trainee.NutritionLogs[.Create/.Update]` (delete via Default).

**Dependencies:** Phase 13 (Today entry), Phase 12 (plan foods for manual), Phases 6/8/10 (FoodPicker/MacroSummary/NutritionLogView).

**Reusable components produced:** editable `NutritionLogEditor`, plan-derived food picker.

**Tests:** unit — from-plan flatten; create/update builders. Widget — quantity edit; totals; delete confirm. Integration — from-plan seeds entries→totals correct→edit→delete; ownership 404.

**Definition of done:** trainee logs from plan and manually, sees correct server macro totals, edits, deletes, browses history.

**Risks:** D3 manual-picker limitation; decimal quantity UX; totals trust-server.

---

# PHASE 16 — Trainee: Dashboard, Progress, Notes, Profile

**Goal:** Complete the trainee self-service surface, reusing Phase 10 viewers.

**Screens & endpoints:**

| Screen | Type | Endpoint(s) |
|---|---|---|
| My Dashboard | Read | `GET /my-dashboard/summary` (+ adherence/completion) |
| My Progress (list/add/delete) | C/R | `POST /my-progress`, `GET /my-progress`, `GET …/{id}`, `DELETE …/{id}` (**no update**) |
| My Notes (list/detail) | Read-only | `GET /my-note`, `GET /my-note/{id}` |
| My Profile | Read-only | `GET /my-profile` |
| Change Password | Form | `POST /api/account/my-profile/change-password` |

- **Dashboard:** date ranges; `DashboardCards`; null/empty + uncapped-% handled.
- **Progress:** `ProgressChart` history + add (`CreateMyProgressEntryDto`, no TraineeId, no photos) + delete; **no edit** (backend has none).
- **Notes:** read-only (coach-authored).
- **Profile:** read-only fields; **trainee cannot edit profile** (no `MyProfile.Update`); only self-write is password change.

**Permissions:** `Trainee.MyDashboard`, `Trainee.MyProgress[.Create]`, `Trainee.MyNotes`, `Trainee.MyProfile`, authenticated (change-password).

**Dependencies:** Phase 10 (`DashboardCards`, `ProgressChart`), Phase 1 (change-password wiring).

**Reusable components produced:** trainee progress add form, read-only profile view.

**Tests:** unit — my-progress create/list; adherence mapping. Widget — read-only profile/notes; progress add/delete (no edit); uncapped-%/empty; change-password validation. Integration — add progress→appears in chart + coach view; change password→re-login.

**Definition of done:** trainee sees dashboard, manages progress (add/delete), reads notes, views profile, changes password; read-only constraints respected.

**Risks:** conveying read-only profile; no-progress-edit expectation; change-password error surfacing.

---

# PHASE 17 — Localization, RTL & Formatting pass

**Goal:** Full bilingual polish (woven continuously, hardened here).

**Tasks:** complete `app_en.arb` + `app_ar.arb` (all UI strings — **no hardcoded text**); hydrate **enum + error-code + permission labels** from `GET /api/abp/application-localization` (server-owned, keeps 21-lang parity) or mirror needed enum keys; RTL audit of every screen (builders, charts, pickers, Today); date/number/decimal formatting via `intl` (+ Arabic-digit decision from Phase 3); `Accept-Language`/`.culture` sent so server messages localize; language switcher in Profile persists choice.

**Dependencies:** all feature phases.

**Tests:** widget/golden — key screens in en + ar/RTL; pseudo-loc for missing-string detection; date/number format per locale.

**Definition of done:** app fully usable in English + Arabic with correct RTL and localized server messages; zero hardcoded strings (lint/CI check).

**Risks:** RTL regressions in nested builders/charts; server-vs-client label drift.

---

# PHASE 18 — Polish (cross-cutting UX)

**Goal:** Production-grade UX across the whole app.

**Tasks:** unified loading (skeletons) / empty / error states (audit coverage); retry behavior; pull-to-refresh everywhere; **pagination** for all coach lists (SkipCount/MaxResultCount/Sorting) + infinite scroll; **search debounce**; keyboard handling (scroll-to-field, done actions) especially in builders; **form-state preservation** on rotation/background; **unsaved-changes warnings** on all editors; accessibility (labels, contrast, tap targets, font scaling); network-failure banners + offline read-cache (Today/plans/libraries) with revalidate; consistent snackbars.

**Dependencies:** all feature phases.

**Tests:** widget — pagination edges; debounce; unsaved-changes on all editors; large-font/accessibility; offline-cache reads. Manual QA on device matrix.

**Definition of done:** every list paginates + refreshes; every editor guards unsaved changes; all async surfaces have loading/empty/error; accessible; graceful offline reads.

**Risks:** scope creep; regressions from broad changes (rely on test suite).

---

# PHASE 19 — Security Review

**Goal:** Verify the app is safe before release.

**Checklist:** token storage (Keychain/Keystore only, never logs/prefs); refresh single-flight + rotation; logout clears **all** state/caches; every API call authenticated + `__tenant`; **permission-based UI** matches server (hidden actions also server-rejected); role-based navigation (no cross-role via deep link); no sensitive data in logs/crash reports (redaction); **debug flags off** in prod; environment config (no dev URLs/secrets in prod); API URLs HTTPS only; certificate handling (consider pinning); screenshot/secure-flag on sensitive screens (credentials/reset) if warranted; verify committed-secrets is a backend/ops item, not shipped in the app.

**Dependencies:** Phases 1, 18.

**Security tests:** role restriction (trainee token → coach endpoint → 403); permission restriction (missing policy → hidden + server-rejected); session expiration → refresh/logout; unauthorized responses handled; token never in logs (automated scan).

**Definition of done:** all checklist items pass; documented sign-off; no secrets/PII in logs or crash payloads.

**Risks:** cert pinning vs rotation; over-trusting client permission gating (server is authority).

---

# PHASE 20 — Testing Strategy (continuous; final consolidation)

**Goal:** A layered suite that gates every phase.

- **Unit:** DTO (de)serialization incl. enums & `PagedResultDto`; repositories (mock Dio); services (auth/refresh/session/tenant/local-date); state controllers (builders, list, logging); validators vs backend limits.
- **Widget/UI:** forms (validation, submit, error), lists (paging/empty/error), navigation (guards, shells), all `AsyncValueView` states, RTL goldens.
- **Integration (seeded dev tenant):** login (password + refresh + tenant); coach flows (create trainee→plan→activate→template clone); trainee flows (Today→from-day workout log→edit actuals→nutrition from-plan→progress); prescribed-snapshot immutability; ownership 404s.
- **Security tests:** role/permission restrictions, session expiration, logout, unauthorized responses.
- **Contract guard:** small real-API suite to detect backend contract drift.

**Per-phase gate:** a phase is done only when its unit + widget tests pass and its happy-path integration test is green.

**Definition of done:** target coverage on core logic; CI runs unit+widget per PR, integration nightly; all critical flows covered.

**Risks:** integration flakiness (stable seeded tenant + reset strategy); device-farm cost for goldens.

---

# PHASE 21 — Release Readiness

**Goal:** Ship-able Android + iOS builds.

**Tasks:** finalize **app name / bundle & package IDs**; **icons + splash**; flavor→environment (**prod API + authority + `CoachApp_App`**, HTTPS, `RequireHttpsMetadata=true`); **crash reporting** (Sentry/Crashlytics) + optional analytics with privacy review; **versioning** (semver + build numbers); **signing** (Android keystore, iOS certs/profiles); release configs (obfuscation/minify, strip logs); store assets (screenshots, descriptions, privacy policy, data-safety/App-Privacy forms); **final QA checklist** (device matrix, both locales/RTL, cold-start session restore, offline, deep links); confirm backend prod readiness (D1 fixed, secrets moved, tenant onboarding decided).

**Dependencies:** all prior phases; D1/D5/D6/D7 resolved.

**Tests:** release smoke on physical iOS + Android against staging→prod; store pre-submission validation.

**Definition of done:** signed release builds install and run against prod, pass the QA checklist, meet store requirements; crash reporting live.

**Risks:** iOS signing/provisioning; store privacy forms; prod cert/HTTPS reachability; D1 not fixed → broken sessions.

---

# Dependency Graph

```text
                         ┌─────────────────────────────┐
                         │ Phase 0  Foundation          │
                         │ (DI, net, ser/storage, theme)│
                         └───────────────┬──────────────┘
                                         │
                 ┌───────────────────────┼───────────────────────┐
                 ▼                       ▼                        ▼
        ┌────────────────┐      ┌────────────────┐      (parallelizable)
        │ P1 Auth/Session│      │ P3 Design System│
        │ Tenant/Config  │      │ + Shared UI      │
        └───────┬────────┘      └────────┬─────────┘
                └───────────┬────────────┘
                            ▼
                   ┌─────────────────┐
                   │ P2 App Shells    │  (role routing + permission gating)
                   └───────┬─────────┘
                            ▼
                   ┌─────────────────┐
                   │ P4 Coach:Trainees│──► TraineePicker + activeTrainee (D4)
                   └───────┬─────────┘
              ┌────────────┼────────────┐
              ▼                         ▼
     ┌─────────────────┐       ┌─────────────────┐
     │ P5 Exercise Lib │       │ P6 Food Library │
     │ ► ExercisePicker│       │ ► FoodPicker    │
     └───────┬─────────┘       └───────┬─────────┘
             ▼                         ▼
     ┌─────────────────┐       ┌─────────────────┐
     │ P7 Workout Plans│       │ P8 Nutrition Pl.│
     │ ► NestedBuilder │       │ ► MacroSummary  │
     │ ► PlanViewer    │       │   (+targets)    │
     └───────┬─────────┘       └───────┬─────────┘
             └────────────┬────────────┘
                          ▼
                 ┌─────────────────┐
                 │ P9 Templates     │ (clone→inactive plan; save-as-template)
                 └───────┬─────────┘
                          ▼
                 ┌───────────────────────────────┐
                 │ P10 Shared Domain Views (bridge)│
                 │ LogView / PrescribedVsActual /  │
                 │ DashboardCards / ProgressChart  │
                 └───────┬───────────────┬─────────┘
                         ▼               ▼
        ┌────────────────────────┐   ┌───────────────────────────────┐
        │ P11 Coach Tracking/     │   │ P12 Trainee: My Plans (read)   │
        │ Dashboard/Progress/Notes│   │ ► active-plan providers        │
        │ (scoped to activeTrainee)│  └───────┬───────────────────────┘
        └─────────────────────────┘           ▼
                                      ┌─────────────────┐
                                      │ P13 Today        │
                                      └───────┬─────────┘
                                    ┌─────────┴─────────┐
                                    ▼                   ▼
                          ┌──────────────────┐ ┌──────────────────┐
                          │ P14 Workout Log  │ │ P15 Nutrition Log│
                          └──────────────────┘ └──────────────────┘
                                    └─────────┬─────────┘
                                              ▼
                                  ┌───────────────────────────┐
                                  │ P16 Trainee Dashboard/     │
                                  │ Progress/Notes/Profile     │
                                  └───────────┬───────────────┘
                                              ▼
             P17 Localization/RTL ─► P18 Polish ─► P19 Security ─► P20 Testing ─► P21 Release
             (P17/P20 run continuously; shown here as the hardening gates)
```

**Key structural facts:** Auth + Design System are independent (parallelizable); Libraries gate Plans (via pickers); Plans gate Templates + the bridge; the bridge (P10) gates both Coach Tracking and the whole Trainee experience; My Plans (P12) gates Today + logging.

---

# Complete Screen Inventory

*Type = Read-only (R) / Create (C) / Edit (E). "Reusable" = the screen or its core component is reused elsewhere.*

## Coach Screens

| Screen | Purpose | API(s) | Depends on | Permission | Nav source | Nav destination | Type | Reusable |
|---|---|---|---|---|---|---|---|---|
| Coach Dashboard | Selected trainee adherence/completion | `GET /trainee-dashboard/summary` | P4,P10 | `Coach.Tracking` | Shell tab | Logs, Progress | R | DashboardCards |
| Trainees List | Browse/search trainees | `GET /trainee` | P4 | `Coach.Trainees` | Shell tab | Detail, Create | R | TraineeCard |
| Trainee Detail | Trainee hub | `GET /trainee/{id}` | P4 | `Coach.Trainees` | List | Edit, Reset-pw, Plans, Tracking | R | — |
| Create Trainee | Provision login+profile | `POST /trainee` | P4 | `Coach.Trainees.Create` | List | Detail | C | credential-share |
| Edit Trainee | Profile + active toggle | `PUT /trainee/{id}` | P4 | `Coach.Trainees.Update` | Detail | Detail | E | — |
| Reset Password | Set new password | `POST /trainee/{id}/reset-password` | P4 | `Coach.Trainees.ResetPassword` | Detail | — | E(dialog) | — |
| Exercises List | Manage exercises | `GET /exercise` | P5 | `Coach.Exercises` | Shell tab | Editor | R | ExerciseCard |
| Exercise Editor | Add/edit exercise | `POST/PUT /exercise` | P5 | `Coach.Exercises.Create/Update` | List | List | C/E | — |
| Foods List | Manage foods | `GET /food` | P6 | `Coach.Foods` | Shell tab | Editor | R | FoodCard |
| Food Editor | Add/edit food | `POST/PUT /food` | P6 | `Coach.Foods.Create/Update` | List | List | C/E | — |
| Workout Plans List | Trainee's plans | `GET /workout-plan?TraineeId=` | P7 | `Coach.WorkoutPlans` | Plans tab | Detail, Builder | R | — |
| Workout Plan Detail | View full plan | `GET /workout-plan/{id}` | P7 | `Coach.WorkoutPlans` | List | Builder, Save-as-template | R | PlanViewer |
| Workout Plan Builder | Build/edit nested plan | `POST/PUT /workout-plan(/{id})`, `…/set-active` | P5,P7 | `Coach.WorkoutPlans.*` | List/Detail | Detail | C/E | NestedBuilder |
| Nutrition Plans List | Trainee's plans | `GET /nutrition-plan?TraineeId=` | P8 | `Coach.NutritionPlans` | Plans tab | Detail, Builder | R | — |
| Nutrition Plan Detail | View plan + totals | `GET /nutrition-plan/{id}` | P8 | `Coach.NutritionPlans` | List | Builder, Save-as-template | R | PlanViewer, MacroSummary |
| Nutrition Plan Builder | Build/edit nested plan+targets | `POST/PUT /nutrition-plan(/{id})`, `…/set-active` | P6,P8 | `Coach.NutritionPlans.*` | List/Detail | Detail | C/E | NestedBuilder |
| Workout Templates List | Manage templates | `GET /workout-plan-template` | P9 | `Coach.WorkoutPlanTemplates` | Plans tab | Builder, Clone | R | — |
| Workout Template Builder | Build/edit template | `POST/PUT /workout-plan-template(/{id})` | P7,P9 | `Coach.WorkoutPlanTemplates.*` | List | List | C/E | NestedBuilder |
| Clone Workout Template | Template→trainee plan | `POST /workout-plan-template/{id}/clone-to-trainee` | P9 | `Coach.WorkoutPlans.Create` | Template list | New plan | C(dialog) | clone dialog |
| Nutrition Templates List | Manage templates | `GET /nutrition-plan-template` | P9 | `Coach.NutritionPlanTemplates` | Plans tab | Builder, Clone | R | — |
| Nutrition Template Builder | Build/edit template | `POST/PUT /nutrition-plan-template(/{id})` | P8,P9 | `Coach.NutritionPlanTemplates.*` | List | List | C/E | NestedBuilder |
| Clone Nutrition Template | Template→trainee plan | `POST /nutrition-plan-template/{id}/clone-to-trainee` | P9 | `Coach.NutritionPlans.Create` | Template list | New plan | C(dialog) | clone dialog |
| Workout Logs (Coach) | Read trainee logs | `GET /workout-log?TraineeId=`, `/{id}` | P10,P11 | `Coach.Tracking` | Tracking | Log Detail | R | WorkoutLogView |
| Nutrition Logs (Coach) | Read trainee logs | `GET /nutrition-log?TraineeId=`, `/{id}` | P10,P11 | `Coach.Tracking` | Tracking | Log Detail | R | NutritionLogView |
| Coach Progress | Trainee metrics CRUD | `GET/POST/PUT/DELETE /progress-entry(/{id})` | P10,P11 | `Coach.Progress.*` | Tracking | Editor | C/E/R | ProgressChart |
| Coach Notes | Trainee notes CRUD | `GET/POST/PUT/DELETE /trainee-note(/{id})` | P11 | `Coach.Notes.*` | Tracking | Editor | C/E/R | — |

## Trainee Screens

| Screen | Purpose | API(s) | Depends on | Permission | Nav source | Nav destination | Type | Reusable |
|---|---|---|---|---|---|---|---|---|
| Today | Home / scheduled day + macros | `GET /my-today?Date=` | P12,P13 | `Trainee.MyToday` | Shell tab | Logging flows | R | Today composite |
| My Workout Plans | List plans | `GET /my-workout-plan` | P12 | `Trainee.MyWorkoutPlans` | Workout tab | Plan Detail | R | — |
| My Workout Plan Detail | View plan | `GET /my-workout-plan/{id}` | P12 | `Trainee.MyWorkoutPlans` | List/Today | — | R | PlanViewer |
| Log Workout | Manual / from-day | `POST /my-workout-log`, `…/from-day` | P13,P14 | `Trainee.WorkoutLogs.Create` | Today/Workout | View Log | C | WorkoutLogEditor |
| Edit Workout Log | Edit actuals | `PUT /my-workout-log/{id}` | P14 | `Trainee.WorkoutLogs.Update` | View Log | View Log | E | PrescribedVsActual |
| Workout Log History | Past logs | `GET /my-workout-log`, `/{id}` | P14 | `Trainee.WorkoutLogs` | Workout tab | View/Edit | R | WorkoutLogView |
| My Nutrition Plans | List plans | `GET /my-nutrition-plan` | P12 | `Trainee.MyNutritionPlans` | Nutrition tab | Plan Detail | R | — |
| My Nutrition Plan Detail | View plan + totals | `GET /my-nutrition-plan/{id}` | P12 | `Trainee.MyNutritionPlans` | List/Today | — | R | PlanViewer, MacroSummary |
| Log Nutrition | Manual / from-plan | `POST /my-nutrition-log`, `…/from-plan` | P13,P15 | `Trainee.NutritionLogs.Create` | Today/Nutrition | View Log | C | NutritionLogEditor |
| Edit Nutrition Log | Edit entries | `PUT /my-nutrition-log/{id}` | P15 | `Trainee.NutritionLogs.Update` | View Log | View Log | E | — |
| Nutrition Log History | Past logs + totals | `GET /my-nutrition-log`, `/{id}` | P15 | `Trainee.NutritionLogs` | Nutrition tab | View/Edit | R | NutritionLogView |
| My Dashboard | Adherence/completion | `GET /my-dashboard/summary` | P10,P16 | `Trainee.MyDashboard` | Progress/Today | — | R | DashboardCards |
| My Progress | Add/list/delete metrics | `POST/GET/DELETE /my-progress(/{id})` | P10,P16 | `Trainee.MyProgress.*` | Progress tab | Add | C/R | ProgressChart |
| My Notes | Read coach notes | `GET /my-note`, `/{id}` | P16 | `Trainee.MyNotes` | Progress/Profile | Note detail | R | — |
| My Profile | View profile (read-only) | `GET /my-profile` | P16 | `Trainee.MyProfile` | Profile tab | Change Password | R | — |

## Shared Screens

| Screen | Purpose | API(s) | Depends on | Permission | Nav source | Type | Reusable |
|---|---|---|---|---|---|---|---|
| Login / Tenant | Auth + tenant selection | `POST /connect/token`, `application-configuration` | P1 | — | App start | C | — |
| Splash / Session Restore | Silent refresh + bootstrap | `refresh_token`, `application-configuration` | P1 | — | Cold start | R | — |
| Session Expired / Unauthorized | Re-auth interstitial | — | P1 | — | Any 401 | R | — |
| Change Password | Self password change | `POST /api/account/my-profile/change-password` | P1,P16 | authenticated | Profile (both roles) | E | ✔ both roles |
| Component Gallery (debug) | Visual QA of components | — | P3 | dev only | Dev menu | R | — |

---

# API Coverage Check

**Every relevant endpoint is covered.**

**Framework/auth (multi-screen):** `/connect/token` (password+refresh — Login/Splash/interceptor), `application-configuration` (Login/Splash/permission refresh), `application-localization` (Phase 17), `/api/account/my-profile/change-password` (Change Password).

**Coach — all mapped:** `trainee` CRUD + `reset-password`; `exercise` CRUD; `food` CRUD; `workout-plan` CRUD + `set-active`; `workout-plan-template` CRUD + `clone-to-trainee` + `save-as-template`; `nutrition-plan` CRUD + `set-active`; `nutrition-plan-template` CRUD + `clone-to-trainee` + `save-as-template`; `workout-log` (list/detail); `nutrition-log` (list/detail); `progress-entry` CRUD; `trainee-note` CRUD; `trainee-dashboard` (adherence/completion/summary).

**Trainee — all mapped:** `my-profile`; `my-today`; `my-dashboard` (×3); `my-workout-plan` (list/detail/active); `my-workout-log` (create/from-day/list/detail/update/delete); `my-nutrition-plan` (list/detail/active); `my-nutrition-log` (create/from-plan/list/detail/update/delete); `my-progress` (create/list/detail/delete); `my-note` (list/detail).

**Endpoints used by multiple screens:** `application-configuration`; `trainee` list (Trainees List + `TraineePicker` in every trainee-scoped coach feature); `exercise`/`food` list (libraries + pickers in plan builders + trainee manual logging); `workout-plan`/`nutrition-plan` detail (coach detail + builder + save-as); `my-*-plan` detail (My Plans + Today + manual logging).

**Endpoints needing special handling:**
- **List omits children** — `workout-plan`, `nutrition-plan`, both templates: fetch `…/{id}` for the tree.
- **Full-replace writes** — plan/template/log `PUT`: resend entire tree.
- **Server-owned prescribed snapshot** — `my-workout-log` update must exclude `Prescribed*`.
- **Unpaged** — `my-workout-plan`/`my-nutrition-plan` `GetList` return plain `List<>`.
- **Local-date required** — `my-today` (and log `Date` defaults).
- **`__tenant`** — every request incl. token.
- **Uncapped %** — dashboard/adherence: cap bar, label true value.
- **In-use delete** — `exercise`/`food` delete → `CoachApp:00004/5`.

**List/detail behavior:** all coach `GetList` are paged `PagedResultDto<T>` (SkipCount/MaxResultCount/Sorting); details are single-item full graphs.

**Endpoints that should NOT be called directly from UI:** `/connect/token` + refresh (auth layer only), `application-configuration` (session bootstrap only), and all writes via repositories (never from widgets) so caching/invalidation/error-mapping stay centralized.

---

# Backend Changes Required

*(Reporting only — no backend modifications. Every item traces to a code fact; none invented.)*

| # | Item | Why / which flow | Current limitation | Recommended change | Migration? | Priority |
|---|---|---|---|---|---|---|
| B1 | **`offline_access` scope on `CoachApp_App`** | Silent session restore / long-lived login (Phase 1, all) | Client scope list lacks `offline_access` though `refresh_token` grant enabled → password flow may not issue a refresh token | Add `offline_access` to `commonScopes` + client permissions in `OpenIddictDataSeedContributor`; re-seed | No (data seed; re-run DbMigrator) | **HIGH** |
| B2 | **Trainee-facing Exercise/Food browse** | Trainee **manual** (off-plan) logging (Phases 14–15, D3) | No `My*` exercise/food endpoint; manual log needs IDs trainees can't obtain | Add read-only `MyExerciseAppService`/`MyFoodAppService` under a `Trainee.*` policy — or accept v1 "log from plan items only" | No | **HIGH** (if off-plan manual logging required) |
| B3 | **Coach roster overview** | Coach home across all trainees (Phase 11) | `trainee-dashboard` requires a single `TraineeId`; no aggregate | Add a coach roster-summary endpoint | No (computed) | **MEDIUM** (v1 composes per-trainee) |
| B4 | **Progress photos / media** | Progress photos (Phase 16) | `ProgressEntry` has no photo fields; no file endpoint | Add photo fields + BLOB storage endpoint | **Yes** | **MEDIUM / product decision** (omit v1) |
| B5 | **Push notifications / device tokens** | Reminders, new-note alerts (post-v1) | No device-token storage or notification service | Device-registration + notification service | **Yes** | **MEDIUM / future** |
| B6 | **Trainee edit own profile** | Self-service edits (Phase 16) | `MyProfile` is read-only | Add `MyProfile.UpdateAsync` (profile-only) | No | **LOW / product decision** |
| B7 | **`.Delete` sub-permissions** for `Trainee.WorkoutLogs`/`NutritionLogs`/`MyProgress` | Fine-grained gating | Delete guarded only by class `Default` (still works) | Add `.Delete` nodes if gating desired | No | **LOW** |
| B8 | **Ops/security (not app features)** | Prod hardening (Phase 19/21) | Committed secrets in host `appsettings.json`; empty CORS (web only); dev `localhost` cert unreachable by device | Move secrets to user-secrets/KeyVault; set CORS if adding Web; provide device-reachable HTTPS for dev/staging | No | **HIGH before prod** |

**Only B1 (and B2 depending on D3) affects the build order.** Everything else is deferrable or ops.

---

# Final Master Plan (dependency-ordered sequence)

```text
FOUNDATION LAYER
 1. Project foundation & architecture .................. Phase 0
 2. Environment/flavor config + API base URLs ......... Phase 0
 3. Networking (Dio) + serialization + storage + errors Phase 0
 4. Authentication (token/refresh/tenant) ............. Phase 1
 5. Session + config bootstrap + role/permission system Phase 1
 6. Routing + navigation guards ....................... Phase 1→2
 7. Design system + shared UI components .............. Phase 3   (parallel with 4–6)
 8. Coach & Trainee app shells ........................ Phase 2

COACH DATA-PRODUCER LAYER
 9. Coach: Trainees (+ TraineePicker, active-trainee) . Phase 4
10. Coach: Exercise Library (+ ExercisePicker) ........ Phase 5
11. Coach: Food Library (+ FoodPicker) ................ Phase 6
12. Coach: Workout Plans (NestedBuilder, PlanViewer) .. Phase 7
13. Coach: Nutrition Plans (+ MacroSummary, targets) .. Phase 8
14. Coach: Templates (clone / save-as-template) ....... Phase 9

BRIDGE LAYER
15. Shared domain views (LogView, Dashboard, Progress) . Phase 10

COACH READER LAYER
16. Coach: Tracking + Dashboard ....................... Phase 11
17. Coach: Progress + Notes ........................... Phase 11

TRAINEE LAYER
18. Trainee: My Plans (read) — moved before Today ..... Phase 12
19. Trainee: Today (local-date composition) ........... Phase 13
20. Trainee: Workout Logging (prescribed-safe) ........ Phase 14
21. Trainee: Nutrition Logging (server macros) ........ Phase 15
22. Trainee: Dashboard + Progress + Notes + Profile ... Phase 16

HARDENING LAYER
23. Localization + RTL + formatting ................... Phase 17  (continuous)
24. Polish (pagination, states, offline reads, a11y) .. Phase 18
25. Security review ................................... Phase 19
26. Testing consolidation (unit/widget/integration) ... Phase 20  (continuous)
27. Release readiness (Android/iOS, signing, store) ... Phase 21
```

Every phase carries its own **Goal / Tasks / Screens / APIs / Dependencies / Reusable components / Tests / Definition of Done / Risks**, so we can execute **one phase at a time** with a green gate between each — no mid-project re-architecture.

---

## Recommended first step

Before Phase 0, lock the blocking decisions that actually change what we build:
- **D1** — `offline_access` / refresh tokens (backend re-seed).
- **D2** — tenant onboarding UX (recommend "gym/coach code" → tenant name).
- **D3** — trainee manual-logging scope (recommend v1 = log from plan items only).

Once these are confirmed and the architecture approved, implementation begins at **Phase 0**.

---

*Companion document: `docs/CoachApp-Code-Guide.md` (backend architecture, the source of truth). This plan does not modify the backend.*
