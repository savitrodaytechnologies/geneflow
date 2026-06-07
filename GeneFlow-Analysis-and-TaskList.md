# GeneFlow — qPCR Lab Workflow App
## Analysis, Architecture Notes & 1-Week Task List

---

## 0. Project Infrastructure

| Item | Detail |
|---|---|
| **GitHub Repo** | https://github.com/savitrodaytechnologies/geneflow |
| **Deployment — API Server** | AWS EC2 (new instance, to be provisioned Week 2) |
| **Deployment — Database** | Existing AWS RDS — SQL Server Express database to be created Week 2 |
| **Development (Week 1)** | Fully local — local SQL Server + localhost API |
| **Deployment Target** | Week 2 — after MVP local dev is working |

> **SQL Server Express on RDS:** 10GB database size limit. Fine for MVP. No additional Microsoft licensing cost on top of RDS instance cost.

---

## 1. What We Are Building

A **mobile-first shared qPCR lab workflow web app** for research labs.

This is NOT a desktop qPCR calculator. It is a **shared lab workspace** that covers the entire experiment lifecycle:

```
Plan experiment → Set up 96-well plate → Upload Ct CSV → Run analysis → View results → Export reports → Share with lab
```

The primary device is a **mobile phone**. Tablet and laptop are secondary.

---

## 2. Core Problem Being Solved

Currently in a research lab, qPCR analysis requires:
- Manual experiment setup
- Manual 96-well plate layout on paper or Excel
- Separate notes per researcher
- Raw Ct values manually exported from the qPCR instrument
- Manual copying into Excel
- Manual ΔCt, ΔΔCt, and fold-change calculations
- Manual graph preparation
- Manual report preparation
- No easy way to share experiment records with the lab team

**Result:** Data entry errors, incorrect well-to-sample mapping, missing observations, inconsistent calculations, and poor lab-level visibility.

---

## 3. Technology Stack

| Layer | Technology |
|---|---|
| Frontend | Ionic React + TypeScript (mobile-first PWA) |
| Backend | ASP.NET Core 8 Web API |
| Database | Microsoft SQL Server |
| ORM | Entity Framework Core |
| Authentication | JWT (Bearer token) |
| CSV Parsing | CsvHelper for .NET |
| Charts | Chart.js or Plotly.js |
| PDF Export | QuestPDF or HTML-to-PDF |
| File Storage | Local file system (MVP), cloud later |

> **Why Ionic React:** Mobile-style navigation, bottom tabs, touch-friendly controls, PWA support, and future Android/iOS packaging. Alternative is React + Material UI if pure responsive web is the goal.

---

## 4. Architecture Overview

```
Ionic React Mobile Frontend (PWA)
           |
     REST API (JWT)
           |
  ASP.NET Core 8 Web API
           |
   Entity Framework Core
           |
   MS SQL Server
           |
   File Storage (CSV uploads, PDF/PNG exports)
```

---

## 5. Data Hierarchy

```
Lab
└── Users (with roles: LabAdmin, PI, Researcher, Viewer)
└── Projects
    └── Experiments
        └── Plate Layout (96-well)
            └── Plate Wells (A01–H12)
                └── Uploaded Ct Data (raw CSV)
                └── Analysis Runs
                    └── Analysis Results (ΔCt, ΔΔCt, Fold Change)
                    └── Analysis Warnings
                    └── Graphs
                    └── Export Files (CSV, PDF, PNG)
        └── Notes
        └── Audit History
```

---

## 6. User Roles

### System Level
- **SystemAdmin** — manages all labs and system users (in DB from day one, not critical for MVP UI)

### Lab Level
| Role | Key Permissions |
|---|---|
| **LabAdmin / PI** | View all lab experiments, manage users, finalize/unlock experiments, delete/archive |
| **Researcher** | Create & edit own experiments, upload CSV, run analysis, add notes, export |
| **Viewer** | View and export allowed experiments, cannot edit |

---

## 7. Experiment Status Lifecycle

```
Draft → PlateDesigned → DataUploaded → Analyzed → Finalized → Archived
```

- **Finalized:** plate, Ct data, analysis all frozen. Only PI/LabAdmin can unlock.
- Users can **duplicate** a finalized experiment to start a new version.

---

## 8. Visibility / Sharing Model

Each experiment has a visibility setting:
- **Private** — only the owner
- **Project** — project members
- **Lab** — all lab members (MVP default)

---

## 9. Core qPCR Calculation Logic

| Formula | Rule |
|---|---|
| ΔCt | Ct(target) - Ct(reference gene) |
| ΔΔCt | ΔCt(sample) - ΔCt(control sample) |
| Fold Change | 2 ^ (-ΔΔCt) |
| Log2 Fold Change | -ΔΔCt |

**Calculation rules:**
- Excluded wells are ignored
- Blank wells are ignored
- NTC wells are not used in fold-change calculation
- Reference gene is set at the experiment level
- Control sample is set at the experiment level
- Technical replicates are averaged
- Standard deviation calculated per replicate group
- Every analysis run is saved separately — previous runs are never overwritten

---

## 10. Warning Rules (auto-generated during analysis)

| Warning | Threshold |
|---|---|
| High Ct | Ct > 35 |
| High replicate variation | SD > 0.5 |
| NTC amplification | NTC Ct exists and Ct < 35 |
| Missing Ct | No value found for a well |
| Missing reference gene Ct | No matching reference Ct for sample |
| Missing control ΔCt | No ΔCt for control sample |
| CSV mismatch | Well in CSV not in plate, or well in plate not in CSV |

---

## 11. Mobile Plate Setup Methods (key design decision)

The full 96-well grid is **not** the primary entry method on mobile. It is used only for review. Primary mobile entry methods:

1. **Quick Fill by Range** — fill A01–A03 with one sample/target in one step
2. **Upload Plate Layout CSV** — upload a pre-built layout file
3. **Copy Previous Experiment** — reuse layout from past run
4. **Use Template** — pre-defined standard layouts (Phase 2, but design must support)
5. **Manual Well Entry** — one well at a time, for corrections only

---

## 12. Complete Database Tables (18 tables)

| Table | Purpose |
|---|---|
| Users | User accounts, hashed passwords, system role |
| Labs | Lab workspaces |
| LabUsers | User-to-lab membership + LabRole |
| Projects | Projects within a lab |
| ProjectUsers | Project membership |
| Experiments | qPCR experiment records |
| ExperimentTargets | *(implied by spec)* |
| PlateLayouts | 96-well plate layout per experiment |
| PlateWells | Individual wells (A01–H12, 96 per plate) |
| UploadedFiles | Metadata for uploaded CSV files |
| RawCtData | Raw Ct values parsed from CSV |
| AnalysisRuns | Each analysis run (immutable once created) |
| AnalysisResults | Per-sample results (ΔCt, ΔΔCt, FoldChange) |
| AnalysisWarnings | Warnings per run |
| ExperimentNotes | Text notes with type tagging |
| ExportFiles | Metadata for generated CSV/PDF/PNG exports |
| AuditLogs | Full audit trail of all changes |

> All PKs: `UNIQUEIDENTIFIER`. All timestamps: `DATETIME2`. Ct/calculation values: `DECIMAL`. Soft delete on important records.

---

## 13. API Modules

| Module | Key Routes |
|---|---|
| Auth | POST /api/auth/login, register, GET /api/auth/me |
| Dashboard | GET /api/dashboard/mobile, my, lab, project |
| Labs | GET current lab, manage users, invite, change role |
| Projects | CRUD + list experiments |
| Experiments | CRUD + duplicate + finalize + unlock |
| Plate | Get plate, list view, bulk update, quick-fill, upload layout CSV, exclude/include wells |
| Upload | Upload Ct CSV, map columns, list uploads |
| Analysis | Run analysis, get latest, mobile summary, results, warnings |
| Notes | List + create notes per experiment |
| Export | CSV, PDF, PNG export + download |

---

## 14. Backend Services

| Service | Purpose |
|---|---|
| AuthService | Login, JWT generation, user lookup |
| LabService | Lab management, membership |
| ProjectService | Project CRUD, membership |
| ExperimentService | Experiment CRUD, status transitions |
| PermissionService | Role + visibility checks |
| PlateService | Well management, quick fill, CSV layout upload |
| CsvParsingService | Parse uploaded Ct CSV, column mapping |
| CtMappingService | Map CSV Ct values to plate wells |
| QpcrAnalysisService | ΔCt, ΔΔCt, fold change, SD calculations |
| GraphService | Generate graph data / images |
| ExportService | CSV, PDF, PNG generation |
| ExperimentNoteService | Note CRUD |
| AuditLogService | Write audit log entries |
| FileStorageService | Save/retrieve uploaded and exported files |

---

## 15. Helper Classes

### WellIdHelper
- `NormalizeWellId(input)` — e.g. `A1` → `A01`, `a1` → `A01`
- `IsValidWellId(input)` — validates row A–H, column 01–12
- `Generate96WellIds()` — full list A01–H12
- `GenerateWellRange(fromWell, toWell)` — range for quick fill

### MathHelper
- `Mean(values)`
- `SampleStandardDeviation(values)`
- `FoldChange(deltaDeltaCt)` — `Math.Pow(2, -deltaDeltaCt)`

---

## 16. Key Mobile Screens (MVP)

| Screen | Notes |
|---|---|
| Login | Email + password |
| Mobile Lab Dashboard | Cards: My Drafts, Pending Analysis, Warnings, Recent Reports |
| My Experiments | Card list with Open / Duplicate actions |
| Projects | Card list with project detail |
| Create Experiment | Fields: Name, Project, Date, Objective, Hypothesis, Sample Source, Treatment, Instrument, Reference Gene, Control Sample, Visibility |
| Plate Setup Method Selection | Choose: Template / Quick Fill / CSV Upload / Copy Previous / Manual |
| Quick Fill | From/To well, Sample, Target, Reference, Type, Replicate Group |
| Plate List View | List of wells with Ct values, filterable |
| Plate Review Grid | Zoomable grid, tap well for details, color-coded |
| Upload Ct CSV | Upload file, mobile-friendly preview, column mapping |
| Analysis Screen | Cards per sample (ΔCt, ΔΔCt, Fold Change), warning badges |
| Graph Screen | Bar chart, swipe between targets, export PNG |
| Notes Screen | Add/view notes with type (General, SetupObservation, AnalysisNote, etc.) |
| Experiment History | Audit trail |

---

## 17. Export Outputs

| Type | Contents |
|---|---|
| **CSV** | Full analysis table (Name, Project, Date, Researcher, Sample, Target, Reference, Mean Ct, SD Ct, ΔCt, ΔΔCt, Fold Change, Log2FC, Warnings). Formula injection protected. |
| **PDF** | Lab name, project, experiment metadata, plate summary, analysis table, graph, warnings, excluded wells, notes/conclusion, generated timestamp |
| **PNG** | Fold-change bar graph image |

---

## 18. What NOT to Build in MVP

- Real-time collaboration
- Full electronic lab notebook
- Inventory management
- Primer database
- AI analysis
- Direct qPCR machine integration
- Advanced statistics
- Regulatory compliance workflow
- External collaborator portal
- PowerPoint export
- 384-well support
- Native mobile app (Ionic PWA is sufficient)

---

---

# 1-WEEK BUILD TASK LIST

> **Target: Phase 1 + Phase 2 fully complete, Phase 3 started**  
> Phases are from the original spec. Daily plan is approximate.

---

## DAY 1 — Project Setup & Solution Scaffold

### Backend
- [ ] **T01** — Create solution folder `GeneFlow` and `GeneFlow.sln`
- [ ] **T02** — Create `GeneFlow.Api` — ASP.NET Core 8 Web API project
- [ ] **T03** — Create `GeneFlow.Core` — class library (entities, interfaces, DTOs)
- [ ] **T04** — Create `GeneFlow.Infrastructure` — class library (EF Core, services, file storage)
- [ ] **T05** — Add project references (`Api → Core, Infrastructure → Core, Api → Infrastructure`)
- [ ] **T06** — Add NuGet packages:
  - `Microsoft.EntityFrameworkCore.SqlServer`
  - `Microsoft.EntityFrameworkCore.Tools`
  - `Microsoft.AspNetCore.Authentication.JwtBearer`
  - `CsvHelper`
  - `BCrypt.Net-Next`
- [ ] **T07** — Configure `appsettings.json`: connection string, JWT secret/issuer/audience
- [ ] **T08** — Configure `Program.cs`: EF Core, JWT auth, CORS (allow Ionic dev origin)

### Frontend
- [ ] **T09** — Scaffold Ionic React TypeScript project: `ionic start GeneFlow.Mobile tabs --type=react --capacitor`
- [ ] **T10** — Install Axios and React Query (or similar HTTP client)
- [ ] **T11** — Create `/src/api/apiClient.ts` — Axios instance with base URL + JWT interceptor
- [ ] **T12** — Create `/src/config/env.ts` — API base URL config

---

## DAY 2 — Database Entities & Migrations

### Core Entities (in `GeneFlow.Core/Entities/`)
- [ ] **T13** — Create `User.cs` entity
- [ ] **T14** — Create `Lab.cs` entity
- [ ] **T15** — Create `LabUser.cs` entity (with LabRole enum: LabAdmin, PI, Researcher, Viewer)
- [ ] **T16** — Create `Project.cs` entity
- [ ] **T17** — Create `ProjectUser.cs` entity
- [ ] **T18** — Create `Experiment.cs` entity (all fields from spec including Status, Visibility, IsDeleted)
- [ ] **T19** — Create `PlateLayout.cs` entity
- [ ] **T20** — Create `PlateWell.cs` entity (WellId, RowLabel, ColumnNumber, SampleName, TargetGene, CtValue, IsExcluded, etc.)
- [ ] **T21** — Create `UploadedFile.cs`, `RawCtData.cs`
- [ ] **T22** — Create `AnalysisRun.cs`, `AnalysisResult.cs`, `AnalysisWarning.cs`
- [ ] **T23** — Create `ExperimentNote.cs`, `ExportFile.cs`, `AuditLog.cs`

### DbContext & Migrations
- [ ] **T24** — Create `GeneFlowDbContext.cs` (all DbSets, relationships, unique constraints)
- [ ] **T25** — Configure EF Core relationships and constraints in `OnModelCreating`
- [ ] **T26** — Add initial EF Core migration: `dotnet ef migrations add InitialCreate`
- [ ] **T27** — Apply migration to create the database: `dotnet ef database update`
- [ ] **T28** — Add `DataSeeder.cs` — seed one Lab, one LabAdmin user (password hashed), one Researcher user

---

## DAY 3 — Authentication & Permission Layer

### AuthService
- [ ] **T29** — Create `IAuthService` + `AuthService`
  - `Login(email, password)` → returns JWT
  - `Register(...)` (admin-initiated for MVP)
  - `GetCurrentUser(userId)`
- [ ] **T30** — Create `JwtTokenService` — generate JWT with claims (UserId, LabId, SystemRole)
- [ ] **T31** — Create `PasswordService` — BCrypt hash + verify

### PermissionService
- [ ] **T32** — Create `IPermissionService` + `PermissionService`
  - `CanViewExperiment(userId, experiment)`
  - `CanEditExperiment(userId, experiment)`
  - `IsLabAdmin(userId, labId)`
  - `IsPI(userId, labId)`

### Auth API Controller
- [ ] **T33** — Create `AuthController`
  - `POST /api/auth/login`
  - `GET /api/auth/me`
- [ ] **T34** — Add `[Authorize]` middleware, extract `UserId` from JWT claims in controllers via `CurrentUserService`

### Test
- [ ] **T35** — Test login returns JWT, `/api/auth/me` returns user info

---

## DAY 4 — Lab, Project & Experiment APIs

### Lab APIs
- [ ] **T36** — Create `LabController`
  - `GET /api/labs/current` — returns current user's lab
  - `GET /api/labs/{labId}/users` — list members with roles

### Project APIs
- [ ] **T37** — Create `ProjectService` (CRUD)
- [ ] **T38** — Create `ProjectController`
  - `GET /api/projects`
  - `POST /api/projects`
  - `GET /api/projects/{projectId}`
  - `PUT /api/projects/{projectId}`
  - `DELETE /api/projects/{projectId}` (soft delete)
  - `GET /api/projects/{projectId}/experiments`

### Experiment APIs
- [ ] **T39** — Create `ExperimentService`
  - `CreateExperiment(...)` — auto-creates PlateLayout + 96 PlateWells (A01–H12)
  - `GetExperiment(id)`
  - `UpdateExperiment(id, dto)`
  - `DeleteExperiment(id)` (soft delete)
  - `DuplicateExperiment(id)` — copy layout, metadata; skip Ct/analysis/warnings/exports
  - `FinalizeExperiment(id)` — set LockedAt, block edits
  - `UnlockExperiment(id)` — PI/LabAdmin only
- [ ] **T40** — Create `WellIdHelper` static class (NormalizeWellId, IsValidWellId, Generate96WellIds, GenerateWellRange)
- [ ] **T41** — Create `ExperimentController`
  - `GET /api/experiments`
  - `POST /api/experiments`
  - `GET /api/experiments/{experimentId}`
  - `PUT /api/experiments/{experimentId}`
  - `DELETE /api/experiments/{experimentId}`
  - `POST /api/experiments/{experimentId}/duplicate`
  - `POST /api/experiments/{experimentId}/finalize`
  - `POST /api/experiments/{experimentId}/unlock`

### Dashboard API
- [ ] **T42** — Create `DashboardController`
  - `GET /api/dashboard/mobile` — returns summary card counts + recent experiments

---

## DAY 5 — Plate Setup APIs + Frontend: Login & Navigation

### Plate APIs (Backend)
- [ ] **T43** — Create `PlateService`
  - `GetPlate(experimentId)` — 96-well grid data
  - `GetPlateList(experimentId)` — flat list view
  - `UpdateWell(plateWellId, dto)`
  - `BulkUpdateWells(experimentId, dto[])`
  - `QuickFill(experimentId, fromWell, toWell, dto)` — uses `WellIdHelper.GenerateWellRange`
  - `UploadLayoutCsv(experimentId, file)` — parse layout CSV, map to wells
  - `ExcludeWells(experimentId, wellIds[])`
  - `IncludeWells(experimentId, wellIds[])`
  - `ClearWells(experimentId, wellIds[])`
- [ ] **T44** — Create `PlateController` with all plate routes
- [ ] **T45** — Validate QuickFill: well range valid, experiment not finalized, audit log entry

### Frontend: Login & Navigation (Ionic)
- [ ] **T46** — Create `AuthContext` + `useAuth` hook (store JWT, userId, labId in localStorage)
- [ ] **T47** — Create `LoginPage` — email/password form, call `POST /api/auth/login`, store JWT
- [ ] **T48** — Create `ProtectedRoute` — redirect to login if not authenticated
- [ ] **T49** — Set up bottom navigation tabs:
  - Home (Dashboard)
  - Projects
  - Experiments
  - Upload
  - More
- [ ] **T50** — Create mobile tab routing in `App.tsx`

---

## DAY 6 — Frontend: Dashboard, Projects, Experiments

### Mobile Lab Dashboard (Frontend)
- [ ] **T51** — Create `DashboardPage`
  - Summary cards: My Drafts, Pending Analysis, Experiments With Warnings, Recent Reports
  - Recent experiment cards (name, project, owner, status, warning count, Open button)
  - `+ New Experiment` button

### Projects Page (Frontend)
- [ ] **T52** — Create `ProjectsPage` — card list of projects
- [ ] **T53** — Create `ProjectDetailPage` — project info, members, experiment list
- [ ] **T54** — Create `CreateProjectModal` or page

### My Experiments Page (Frontend)
- [ ] **T55** — Create `ExperimentsPage` — card list: name, project, status, last updated, Open / Duplicate actions
- [ ] **T56** — Create `CreateExperimentPage`
  - Fields: Name, Project, Date, Objective, Hypothesis, Sample Source, Treatment Condition, Instrument, Reference Gene, Control Sample Name, Visibility
  - On submit → call `POST /api/experiments` → redirect to experiment detail

### Experiment Detail & Step Navigation (Frontend)
- [ ] **T57** — Create `ExperimentDetailPage` with step-based sub-navigation:
  - Details → Plate Setup → Upload Data → Analysis → Graphs → Notes → History

---

## DAY 7 — Plate Setup Screens (Frontend) + Integration Testing

### Plate Setup Method Screen (Frontend)
- [ ] **T58** — Create `PlateSetupMethodPage` — show 5 method options as cards/buttons

### Quick Fill Screen (Frontend)
- [ ] **T59** — Create `QuickFillPage`
  - Fields: From Well, To Well, Sample Name, Target Gene, Reference Gene, Sample Type, Replicate Group
  - Submit → call `POST /api/experiments/{id}/plate/quick-fill`
  - Show success confirmation

### Plate List View (Frontend)
- [ ] **T60** — Create `PlateListPage`
  - Filterable list: Sample, Target, Type, Missing Ct, Warnings, Excluded
  - Tap well to open manual edit

### Plate Review Grid (Frontend)
- [ ] **T61** — Create `PlateGridPage`
  - 8×12 grid, color-coded by sample type
  - Tap well to view details popup
  - Show warning indicator on wells
  - Pinch-to-zoom or zoom controls

### Upload Plate Layout CSV (Frontend)
- [ ] **T62** — Create `UploadLayoutCsvPage`
  - File picker, submit to `POST /api/experiments/{id}/plate/upload-layout`
  - Show well count mapped, any warnings

### Manual Well Entry Screen (Frontend)
- [ ] **T63** — Create `ManualWellPage`
  - Fields: Well, Sample, Target, Reference, Type, Replicate, Notes
  - Buttons: Save, Previous Well, Next Well, Copy From Previous, Clear Well, Exclude Well

### Integration & Smoke Testing
- [ ] **T64** — End-to-end test: Login → Create Project → Create Experiment → Quick Fill plate → View plate list
- [ ] **T65** — Verify 96 wells are auto-created on experiment creation
- [ ] **T66** — Verify permission rules: Researcher cannot access another lab's experiments
- [ ] **T67** — Verify finalized experiment blocks edits

---

## PHASE 2 PRIORITIES (Week 2 — carry-over if needed)

These are the next natural items beyond the 1-week plan:

| Priority | Item |
|---|---|
| High | Ct CSV upload, column mapping, `CsvParsingService`, `CtMappingService` |
| High | `QpcrAnalysisService` — full ΔCt, ΔΔCt, fold change, SD, warnings |
| High | Mobile analysis summary screen (cards per sample) |
| Medium | Fold-change bar graph (Chart.js) |
| Medium | CSV export (with formula injection protection) |
| Medium | PDF export (QuestPDF or HTML-to-PDF) |
| Medium | PNG graph export |
| Medium | Notes screens (add, view, type filter) |
| Lower | Experiment finalization UI + unlock flow |
| Lower | Audit log display |
| Lower | Copy Previous Experiment UI |
| Lower | Lab user management (invite, change role) |

---

## Key Design Rules to Respect Throughout Development

1. **LabId must be on every experiment from day one** — no orphaned experiments
2. **Visibility field must exist from day one** — default to `Lab`
3. **Status field must exist from day one** — start as `Draft`
4. **Never overwrite an analysis run** — always create a new `AnalysisRun` row
5. **Quick Fill and Plate List View are the primary mobile plate entry methods** — the grid is for review only
6. **NTC wells are not included in fold-change calculation** — only warn if Ct < 35
7. **Excluded wells are completely ignored in all calculations**
8. **All Ct/calculation columns use `DECIMAL` not `FLOAT`** — precision matters in lab data
9. **CSV export must prefix `=`, `+`, `-`, `@` with apostrophe** — prevent formula injection
10. **Finalized experiments are fully read-only** — only PI/LabAdmin can unlock

---

## File/Folder Structure (Planned)

```
GeneFlow/
├── GeneFlow.sln
├── GeneFlow.Api/
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── DashboardController.cs
│   │   ├── LabController.cs
│   │   ├── ProjectController.cs
│   │   ├── ExperimentController.cs
│   │   ├── PlateController.cs
│   │   ├── UploadController.cs
│   │   ├── AnalysisController.cs
│   │   ├── NotesController.cs
│   │   └── ExportController.cs
│   ├── Program.cs
│   └── appsettings.json
├── GeneFlow.Core/
│   ├── Entities/
│   │   ├── User.cs, Lab.cs, LabUser.cs
│   │   ├── Project.cs, ProjectUser.cs
│   │   ├── Experiment.cs, PlateLayout.cs, PlateWell.cs
│   │   ├── UploadedFile.cs, RawCtData.cs
│   │   ├── AnalysisRun.cs, AnalysisResult.cs, AnalysisWarning.cs
│   │   ├── ExperimentNote.cs, ExportFile.cs, AuditLog.cs
│   ├── DTOs/          (request/response models)
│   ├── Enums/         (LabRole, ExperimentStatus, Visibility, NoteType)
│   └── Interfaces/    (IAuthService, IPlateService, etc.)
├── GeneFlow.Infrastructure/
│   ├── Data/
│   │   ├── GeneFlowDbContext.cs
│   │   ├── Migrations/
│   │   └── DataSeeder.cs
│   ├── Services/
│   │   ├── AuthService.cs, JwtTokenService.cs
│   │   ├── LabService.cs, ProjectService.cs
│   │   ├── ExperimentService.cs, PermissionService.cs
│   │   ├── PlateService.cs
│   │   ├── CsvParsingService.cs, CtMappingService.cs
│   │   ├── QpcrAnalysisService.cs
│   │   ├── GraphService.cs, ExportService.cs
│   │   ├── ExperimentNoteService.cs
│   │   ├── AuditLogService.cs, FileStorageService.cs
│   └── Helpers/
│       ├── WellIdHelper.cs
│       └── MathHelper.cs
└── GeneFlow.Mobile/   (Ionic React TypeScript)
    ├── src/
    │   ├── api/apiClient.ts
    │   ├── config/env.ts
    │   ├── context/AuthContext.tsx
    │   ├── hooks/useAuth.ts
    │   ├── pages/
    │   │   ├── LoginPage.tsx
    │   │   ├── DashboardPage.tsx
    │   │   ├── ProjectsPage.tsx
    │   │   ├── ExperimentsPage.tsx
    │   │   ├── CreateExperimentPage.tsx
    │   │   ├── ExperimentDetailPage.tsx
    │   │   ├── plate/PlateSetupMethodPage.tsx
    │   │   ├── plate/QuickFillPage.tsx
    │   │   ├── plate/PlateListPage.tsx
    │   │   ├── plate/PlateGridPage.tsx
    │   │   ├── plate/UploadLayoutCsvPage.tsx
    │   │   └── plate/ManualWellPage.tsx
    │   ├── components/
    │   │   └── (shared cards, form components)
    │   └── App.tsx
    ├── ionic.config.json
    └── package.json
```

---

## Summary Task Count

| Day | Focus | Tasks |
|---|---|---|
| Day 1 | Solution + project scaffolding | T01–T12 |
| Day 2 | All 18 DB entities + migrations + seeding | T13–T28 |
| Day 3 | Auth, JWT, permissions layer | T29–T35 |
| Day 4 | Lab / Project / Experiment / Dashboard APIs | T36–T42 |
| Day 5 | Plate APIs + Frontend login & navigation | T43–T50 |
| Day 6 | Frontend: Dashboard, Projects, Experiments | T51–T57 |
| Day 7 | Frontend: Plate screens + integration testing | T58–T67 |

**Total MVP Week 1 tasks: 67**

---

*Document created: 2026-06-06*  
*Source: qPCR Lab Workflow App — Final Mobile-First Copilot Design Document*
