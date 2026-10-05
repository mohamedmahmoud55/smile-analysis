# FUE Graduation Report — Structured Outline

**Project Title:** SmileAnalysis: An AI-Integrated Dental Clinic Management System for Orthodontic Assessment

**Faculty:** Faculty of Computers and Information Technology — Artificial Intelligence  
**Academic Year:** 2025–2026

This document maps every FUE report section to the SmileAnalysis codebase. Full prose drafts are in companion files:

- [Chapters 1–3 Draft](FUE-Report-Chapters-1-3-Draft.md)
- [Chapters 4–6 Draft](FUE-Report-Chapters-4-6-Draft.md)
- Diagrams: [docs/diagrams/](diagrams/)

---

## Solution Overview

| Item | Detail |
|------|--------|
| Type | ASP.NET Core 9 MVC web application |
| Architecture | 3-tier: PL (`SmileAnalysisPl`) → BL (`SmileAnalysisBl`) → DAL (`SmileAnalysisDal`) |
| Database | SQL Server via EF Core 9 |
| Authentication | ASP.NET Core Identity (roles: SuperAdmin, Admin, Doctor, Staff) |
| AI Integration | External REST APIs on Google Cloud Run (Gummy Smile + X-Ray Analysis) |
| Solution file | `SmileAnalysis.sln` |

---

## Chapter 1 — Introduction

### 1.1 Background of the Study

**Topics to cover:**
- Orthodontic and aesthetic dentistry: gummy smile assessment, cephalometric analysis, CVM staging
- Rise of computer vision and AI in dental imaging
- Digital health transformation in clinics
- Gap: AI tools exist but are rarely embedded in clinic management workflows
- Need for unified patient records linking operational data with AI outputs

**Codebase anchors:** `GummySmileController`, `XRayAnalysisController`, `Patient` entity with `GummySmileCases` and `XRayAnalysisCases` collections

**Constraint:** ≤ 1 page (~400–500 words)

### 1.2 Problem Statement

**Core problem:** Dental clinics manage patients, scheduling, and billing separately from AI-assisted diagnostic tools. Generic practice management software lacks orthodontic AI; standalone AI demos lack RBAC, persistence, and clinical workflow integration.

**Why existing solutions fail:**
- Paper/legacy records → no structured AI result storage
- Commercial PMS (Dentrix, Open Dental) → limited orthodontic AI integration
- Research prototypes → no production clinic portal

**Constraint:** ≤ 10 lines

### 1.3 Aim and Objectives

**General Aim:** Develop a web-based dental clinic portal that unifies patient management with AI-assisted gummy-smile and X-ray orthodontic analysis.

**Specific Objectives:**
1. Implement role-based access control for SuperAdmin, Admin, Doctor, and Staff (`IdentityDbContextSeeding.cs`, `[Authorize]` on controllers)
2. Deliver CRUD operations for patients, doctors, staff, appointments, and payments
3. Integrate the Gummy Smile API through a multi-step clinical wizard
4. Integrate the X-Ray Analysis API (panoramic, CVM, cephalometric, full-case, optional DentalGemma report)
5. Persist AI case data and images locally in SQL Server (`GummySmileCase`, `XRayAnalysisCase`)
6. Generate prescription PDFs using QuestPDF (`PrescriptionPdfService`)

### 1.4 Scope of the Project

**In scope:**
- MVC portal with 11 controllers
- SQL Server persistence with EF migrations
- ASP.NET Identity authentication
- Gummy Smile wizard: Start → Upload → Analyze → Review → Clinical → Diagnosis → Treatment → Report PDF
- X-Ray analysis page with panoramic and/or cephalogram upload
- Prescriptions linked to appointments
- Billing accounts and payment records
- Admin user/role management (SuperAdmin only)
- Dashboard analytics for SuperAdmin/Admin (`HomeController` + `AnalyticsService`)

**Out of scope:**
- Training or hosting ML models (external Cloud Run services)
- Mobile native application
- Patient self-service portal
- Real payment gateway integration
- Reports module (navigation placeholder in `_OrthoNavItems.cshtml`)
- Automated unit/integration test project

**Constraints:**
- Internet connectivity required for AI API calls
- SQL Server instance required
- Academic project timeline
- 5-minute HTTP timeout on AI clients (`Program.cs`)

### 1.5 Significance of the Project

| Stakeholder | Benefit |
|-------------|---------|
| Orthodontists | Faster, structured AI-assisted smile and X-ray assessment with clinician override |
| Clinic staff | Centralized scheduling, billing, patient records |
| Patients | Improved care continuity through integrated records |
| Researchers | Documented pattern for clinic + external AI microservice integration |

### 1.6 Project Methodology (Brief Overview)

- **Method:** Iterative Agile / SDLC
- **Phases:** Requirements → 3-layer design → EF schema → feature sprints (core clinic → Gummy Smile → X-Ray → prescriptions)
- **Technologies:** ASP.NET Core 9, EF Core 9, SQL Server, Bootstrap 5, jQuery, HttpClient, QuestPDF, Google Cloud Run APIs

### 1.7 Project Organization

| Chapter | Content |
|---------|---------|
| 1 | Introduction, problem, objectives, scope |
| 2 | Literature review and related technologies |
| 3 | System analysis, requirements, diagrams |
| 4 | Detailed design: tools, architecture, site map, UI/UX, database schema |
| 5 | Implementation, code organization, testing |
| 6 | Conclusions, contributions, future work |

---

## Chapter 2 — Literature Review

### 2.1 Introduction
Restate focus: AI-integrated orthodontic clinic management with gummy-smile and cephalometric analysis.

### 2.2 Review of Related Systems / Studies

| Area | Examples / Topics |
|------|-------------------|
| Dental PMS | Dentrix, Open Dental, curve Dental — patient records, scheduling, billing |
| Orthodontic imaging | Dolphin Imaging, OnyxCeph — cephalometric tracing, treatment planning |
| Gummy smile research | Aesthetic smile analysis, gingival display measurement [Author, Year] |
| Cephalometric automation | Landmark detection, SNA/SNB/ANB angles [Author, Year] |
| CVM staging | Cervical vertebral maturation for growth assessment [Author, Year] |
| LLM radiology reports | DentalGemma and similar models for narrative reports [Author, Year] |
| Microservice integration | REST APIs for ML inference in healthcare systems [Author, Year] |

### 2.3 Research Gap / Summary of Literature Review

- Commercial systems rarely combine clinic operations with modern CV/LLM orthodontic AI
- Manual cephalometric tracing is time-consuming and operator-dependent
- Gummy smile assessment lacks standardized digital workflow in small clinics
- Standalone AI tools do not provide RBAC, audit trails, or local persistence
- **This project addresses:** integrated portal + external AI microservices + local SQL Server persistence

### 2.4 Summary of Existing Technologies

| Technology | Role in SmileAnalysis |
|------------|----------------------|
| ASP.NET Core MVC 9 | Web framework, controllers, Razor views |
| EF Core 9 | ORM, migrations, fluent configurations |
| ASP.NET Identity | Authentication, roles, password management |
| SQL Server | Relational persistence |
| Bootstrap 5 | Responsive UI, icons |
| HttpClient | Typed REST clients for AI APIs |
| Google Cloud Run | Hosted Gummy Smile and X-Ray AI services |
| QuestPDF | Prescription PDF generation |

**Config references:** `SmileAnalysisPl/Program.cs`, `SmileAnalysisPl/appsettings.json` (describe connection strings generically in report — no credentials)

---

## Chapter 3 — System Analysis and Design

### 3.1 System Overview / Proposed Concept

Role-based clinic portal where authorized users manage patients and launch AI analyses from patient detail pages. Results are stored in `GummySmileCase` and `XRayAnalysisCase` entities linked to `Patient`.

**Diagram:** [diagrams/01-architecture.mmd](diagrams/01-architecture.mmd) → Figure 3.1

### 3.2 Functional Requirements ("Shall" Statements)

#### Authentication & Authorization
- FR-AUTH-01: The system shall require authentication for all protected pages via ASP.NET Identity.
- FR-AUTH-02: The system shall default unauthenticated requests to `Account/Login`.
- FR-AUTH-03: The system shall enforce role-based authorization per controller action.

#### Patient Management
- FR-PAT-01: The system shall allow SuperAdmin and Staff to create, read, update, and delete patient records.
- FR-PAT-02: The system shall allow Doctors to view and edit patients they have appointments with.
- FR-PAT-03: The system shall store clinical history per patient (`PatientClinicalHistory`).
- FR-PAT-04: The system shall display gummy-smile and X-ray analysis history on the patient details page.

#### Gummy Smile Analysis
- FR-GS-01: The system shall create a remote AI case and local `GummySmileCase` when analysis is started.
- FR-GS-02: The system shall require rest and smile image uploads before analysis.
- FR-GS-03: The system shall invoke the external analyze endpoint and display AI results.
- FR-GS-04: The system shall allow clinicians to override AI outputs and submit clinical data.
- FR-GS-05: The system shall generate diagnosis and treatment plan views and downloadable PDF reports.

#### X-Ray Analysis
- FR-XR-01: The system shall accept panoramic and/or lateral cephalogram uploads.
- FR-XR-02: The system shall run panoramic analysis when a panoramic image is provided.
- FR-XR-03: The system shall run CVM, landmark detection, and cephalometric summary when a cephalogram is provided.
- FR-XR-04: The system shall run full-case synthesis when panoramic, CVM, and cephalometric data are available.
- FR-XR-05: The system shall optionally generate a DentalGemma narrative report.
- FR-XR-06: The system shall persist images and JSON results in `XRayAnalysisCase`.

#### Appointments
- FR-APT-01: The system shall allow SuperAdmin, Admin, and Staff to manage appointments.
- FR-APT-02: The system shall associate appointments with patients, doctors, and optional staff.

#### Doctors & Staff
- FR-DOC-01: The system shall manage doctor profiles, specialties, and availability schedules.
- FR-DOC-02: The system shall allow doctors to view their own schedule.
- FR-STF-01: The system shall allow SuperAdmin/Admin to manage staff records.

#### Payments
- FR-PAY-01: The system shall record payments linked to appointments and patients.
- FR-PAY-02: The system shall support patient billing accounts with discounts.

#### Prescriptions
- FR-RX-01: The system shall allow Doctors and SuperAdmin to create prescriptions for appointments.
- FR-RX-02: The system shall generate downloadable prescription PDFs.

#### Administration
- FR-ADM-01: The system shall allow SuperAdmin to create admin users, assign roles, and reset passwords.

#### Dashboard
- FR-DASH-01: The system shall display orthodontic dashboard analytics for SuperAdmin and Admin.

**Source files:** `SmileAnalysisPl/Controllers/*.cs`

### 3.3 Non-Functional Requirements

| ID | Category | Requirement |
|----|----------|-------------|
| NFR-01 | Performance | AI inference delegated to Cloud Run; HttpClient timeout 5 minutes |
| NFR-02 | Performance | EF Core queries with indexed primary keys; migrations applied on startup |
| NFR-03 | Usability | Bootstrap 5 responsive layout; role-specific sidebar navigation |
| NFR-04 | Security | RBAC on all sensitive actions; `[ValidateAntiForgeryToken]` on POST |
| NFR-05 | Security | Unique email constraint for Identity users |
| NFR-06 | Reliability | API errors surfaced to user via TempData; local case state persisted |
| NFR-07 | Maintainability | 3-layer separation (PL/BL/DAL); repository + unit-of-work pattern |
| NFR-08 | Scalability | AI services externalized as independent microservices |

### 3.4 System Environment

| Component | Specification |
|-----------|---------------|
| **Hardware (dev)** | Standard PC/laptop (macOS or Windows), ≥ 8 GB RAM |
| **Server DB** | SQL Server (local or Docker instance) |
| **Network** | Internet access for Cloud Run AI APIs |
| **OS** | macOS / Windows 10+ |
| **Runtime** | .NET 9 SDK |
| **IDE** | JetBrains Rider / Visual Studio Code |
| **Browser** | Chrome, Edge, or Firefox (latest) |
| **Dev URLs** | `http://localhost:5138`, `https://localhost:7003` |

### 3.5 System Architecture

**Figure 3.1:** Three-layer architecture with external AI services — see [diagrams/01-architecture.mmd](diagrams/01-architecture.mmd)

### 3.6 Class Diagram

**Figure 3.2:** Core entities and services — see [diagrams/02-class-diagram.mmd](diagrams/02-class-diagram.mmd)

**Key classes:**
- Entities: `Patient`, `Doctor`, `Appointment`, `GummySmileCase`, `XRayAnalysisCase`, `Prescription`, `Payment`
- Services: `PatientService`, `GummySmileCaseService`, `XRayAnalysisCaseService`, `PrescriptionService`
- API clients: `GummySmileApiService`, `XRayAnalysisApiService`

### 3.7 Activity Diagram (Optional)

**Figure 3.3:** X-Ray analysis pipeline — see [diagrams/05-activity-xray.mmd](diagrams/05-activity-xray.mmd)

Flow: Upload images → Panoramic API (if provided) → Ceph landmarks → CVM → Ceph summary → Full-case (if all data) → DentalGemma (optional) → Persist `XRayAnalysisCase`

**Source:** `XRayAnalysisCaseService.RunAnalysisAsync`

### 3.8 Use Case Diagram

**Figure 3.4:** Actors and use cases — see [diagrams/03-use-case.mmd](diagrams/03-use-case.mmd)

**Actors:** SuperAdmin, Admin, Doctor, Staff

### 3.9 Sequence Diagram

**Figure 3.5:** Gummy Smile analysis sequence — see [diagrams/04-sequence-gummy-smile.mmd](diagrams/04-sequence-gummy-smile.mmd)

### 3.10 ERD Diagram

**Figure 3.6:** Relational model — see [diagrams/06-erd.mmd](diagrams/06-erd.mmd)

**DbSets:** Patients, PatientClinicalHistories, Doctors, DoctorAvailabilities, StaffMembers, Appointments, Payments, PatientBillingAccounts, GummySmileCases, XRayAnalysisCases, Prescriptions, PrescriptionItems + ASP.NET Identity tables

---

## Chapter 4 — System Design

### 4.1 Introduction
Transition from analysis to detailed design: tools, layers, navigation, UI, and database schema.

### 4.2 Development Tools and Technologies

| Tool / Technology | Version | Purpose |
|-------------------|---------|---------|
| .NET SDK | 9.0 | Application runtime |
| ASP.NET Core MVC | 9.0 | Web framework |
| EF Core | 9.0 | ORM |
| SQL Server | 2019+ | Database |
| Bootstrap | 5.x | UI framework |
| jQuery | 3.x | Client scripting, validation |
| QuestPDF | Latest | PDF generation |
| HttpClient | Built-in | AI API communication |
| Rider / VS Code | — | IDE |

### 4.3 System Architecture (Layers)

| Layer | Project | Responsibility |
|-------|---------|----------------|
| Frontend / Presentation | `SmileAnalysisPl` | Controllers, Razor views, static assets |
| Backend / Business | `SmileAnalysisBl` | Services, view models, API clients |
| Database / Data | `SmileAnalysisDal` | Entities, DbContext, repositories, migrations |

**Diagram:** Reuse Figure 3.1 or [diagrams/01-architecture.mmd](diagrams/01-architecture.mmd)

### 4.4 Website Map

```
Account/Login
    └── [authenticated] → role-based home
        ├── Home/Index (Dashboard) — SuperAdmin, Admin
        ├── Patient/Index
        │   ├── Patient/Create, Edit, Details
        │   ├── Patient/GummySmileAnalysis (history)
        │   ├── Patient/XRayAnalysis (history)
        │   ├── GummySmile/Start → Upload → Analyze → Review → Clinical → Diagnosis → Treatment → DownloadPdf
        │   └── XRayAnalysis/Analyze
        ├── Doctor/Index, Create, Edit, Schedule, SchedulePicker, MySchedule, Availability
        ├── Staff/Index, Create, Edit
        ├── Appointment/Index, Create, Edit
        ├── Payment/Index, Receipt
        ├── Prescription/Form, Details (PDF download)
        └── AdminManagement/Index, Create, Edit, Delete, ManageRoles, ResetPassword — SuperAdmin only
```

**Navigation source:** `Views/Shared/_OrthoNavItems.cshtml`

### 4.5 UI/UX

**Design approach:**
- Sidebar navigation with role-specific menu items
- Orthodontic-themed layout (`_Layout.cshtml`)
- Wizard steps for Gummy Smile (`GummySmile/_WizardSteps.cshtml`)
- Status badges for appointments (`_AppointmentStatusBadge.cshtml`)
- Bootstrap Icons for visual cues

**Screens to screenshot for report:**
1. Login (`Account/Login.cshtml`)
2. Dashboard (`Home/Index.cshtml`)
3. Patient list and details
4. Gummy Smile wizard steps (Upload, Review, Diagnosis)
5. X-Ray analysis results (`XRayAnalysis/Analyze.cshtml`)
6. Prescription form and PDF output

### 4.6 Database Schema (Relational Model)

**Configuration files:** `SmileAnalysisDal/Data/Configurations/`

| Table | Key Relationships |
|-------|-------------------|
| Patients | 1:1 ClinicalHistory, 1:1 BillingAccount; 1:N Appointments, GummySmileCases, XRayAnalysisCases, Payments |
| Doctors | 1:N Availabilities, Appointments; optional link to ApplicationUser |
| Appointments | N:1 Patient, Doctor, Staff; 1:N Payments; 1:1 Prescription |
| GummySmileCases | N:1 Patient; stores images, JSON snapshots, PDF report |
| XRayAnalysisCases | N:1 Patient; stores images, ResultsJson, summary fields |
| Prescriptions | 1:1 Appointment; 1:N PrescriptionItems |
| AspNetUsers / Roles | Identity tables via `IdentityDbContext` |

**Diagram:** [diagrams/06-erd.mmd](diagrams/06-erd.mmd)

---

## Chapter 5 — System Implementation & Testing

### 5.1 Introduction
Describe transition from design to working implementation across three projects.

### 5.2 System Tools
Same table as Section 4.2; add development workflow: `dotnet build`, `dotnet run --project SmileAnalysisPl`, EF migrations.

### 5.3 Core Features Implemented

| Module | Controller | CRUD Status |
|--------|------------|-------------|
| Patients | `PatientController` | Full CRUD (role-scoped) |
| Doctors | `DoctorController` | Full CRUD + schedule |
| Staff | `StaffController` | Full CRUD |
| Appointments | `AppointmentController` | Create, Read, Update |
| Payments | `PaymentController` | Create, Read |
| Prescriptions | `PrescriptionController` | Create, Read, Update |
| Admin users | `AdminManagementController` | Full CRUD + roles |
| Gummy Smile | `GummySmileController` | Workflow (not traditional CRUD) |
| X-Ray | `XRayAnalysisController` | Analysis workflow |
| Dashboard | `HomeController` | Read analytics |

### 5.4 User Interface Organized and Documented

- 49 Razor views under `SmileAnalysisPl/Views/`
- Shared layout, navigation partials, validation scripts
- Role-based menu in `_OrthoNavItems.cshtml`

### 5.5 Source Code Organized and Documented

```
SmileAnalysis.sln
├── SmileAnalysisPl/     # Presentation — Controllers, Views, Program.cs
├── SmileAnalysisBl/     # Business — Services, GummySmile/, XRayAnalysis/, ViewModels/
└── SmileAnalysisDal/    # Data — Entities, Contexts, Configurations, Migrations, Repositories
```

**Patterns:** Dependency injection, repository + unit of work, typed HttpClient, options pattern for API URLs.

### 5.6 Test Plan and Test Cases

**Approach:** Manual functional testing (no automated test project in repository).

**Test environment:** Local SQL Server, seeded users from `IdentityDbContextSeeding`, sample doctors/staff from `SmileAnalysisDataSeeding`, internet for AI APIs.

| ID | Module | Test Case | Steps | Expected Result | Status |
|----|--------|-----------|-------|-----------------|--------|
| TC-01 | Auth | Valid login | Enter valid SuperAdmin credentials on Login page | Redirect to Dashboard; sidebar visible | |
| TC-02 | Auth | Invalid login | Enter wrong password | Error message; remain on Login | |
| TC-03 | Auth | Access denied | Staff navigates to AdminManagement URL | 403 / Access Denied page | |
| TC-04 | Patient | Create patient | SuperAdmin fills Create form with valid data | Patient appears in list with generated ID | |
| TC-05 | Patient | Edit patient | Update phone number on Edit form | Details page shows updated phone | |
| TC-06 | Patient | Doctor scope | Doctor views patient list | Only patients with shared appointments shown | |
| TC-07 | Patient | Delete patient | SuperAdmin deletes test patient | Patient removed from index | |
| TC-08 | Appointment | Create appointment | Staff creates appointment linking patient + doctor | Appointment listed with Scheduled status | |
| TC-09 | Appointment | Edit status | Change appointment status to Completed | Badge updates on index | |
| TC-10 | Doctor | Create doctor | SuperAdmin adds new doctor record | Doctor appears in doctor index | |
| TC-11 | Doctor | Schedule | Set weekly availability slots | Schedule view shows available times | |
| TC-12 | Payment | Record payment | Staff records payment for appointment | Payment reflected in Financials index | |
| TC-13 | Prescription | Create Rx | Doctor creates prescription with items for appointment | Prescription details page loads | |
| TC-14 | Prescription | PDF export | Click download PDF on prescription details | Valid PDF file downloads | |
| TC-15 | Gummy Smile | Full wizard | Start from patient → upload rest+smile → analyze → review → clinical → diagnosis → treatment | Case completes; PDF downloadable | |
| TC-16 | Gummy Smile | Missing image | Submit upload without smile image | Validation error displayed | |
| TC-17 | X-Ray | Panoramic only | Upload panoramic image and run analysis | Panoramic findings displayed; case saved | |
| TC-18 | X-Ray | Full ceph pipeline | Upload cephalogram; enable DentalGemma | CVM stage, ceph measurements, optional report shown | |
| TC-19 | Admin | Create user | SuperAdmin creates Admin user with role | User can log in with assigned role | |
| TC-20 | Dashboard | Analytics load | SuperAdmin opens Dashboard | Metrics cards populate without error | |

### 5.7 Testing with Sample Data

**Seeded data:**
- Roles: SuperAdmin, Admin, Doctor, Staff
- Default SuperAdmin user (see seeding code — use test credentials locally, not in report)
- Sample doctors: Abdallah Mohamed (Orthodontics), Amr Gamal (Periodontics)
- Sample staff: Mahmoud Azab

**AI testing:** Requires sample rest/smile photos and panoramic/ceph X-ray images; Cloud Run APIs must be reachable.

---

## Chapter 6 — Conclusions and Future Work

### 6.1 Introduction
Summarize project completion and reflect on objectives.

### 6.2 Project Summary
- Built 3-tier ASP.NET Core 9 clinic portal
- Integrated two AI microservices for smile and X-ray analysis
- Implemented RBAC, CRUD for clinic entities, prescriptions with PDF export
- Persisted AI results locally for clinical review

### 6.3 Project Contributions
1. Unified workflow from patient record to AI analysis without leaving the portal
2. Local persistence of AI outputs (images, JSON, PDF) for audit and revisit
3. Clinician override and clinical input layer on top of AI recommendations
4. Demonstrated microservice integration pattern for academic AI projects

### 6.4 Conclusion and Future Work

**Future work:**
- Automated unit and integration tests (xUnit)
- Implement Reports module (currently nav placeholder)
- Move secrets to User Secrets / environment variables
- Patient-facing appointment booking portal
- Mobile-responsive PWA or native app
- On-premises or private-cloud AI deployment for data sovereignty
- Real payment gateway (Stripe, PayPal)
- HL7/FHIR interoperability for EHR integration

---

## Known Gaps (Report Honestly)

| Gap | Report Handling |
|-----|-----------------|
| No README | Note in Ch.5; recommend adding project documentation |
| No automated tests | Manual test plan in Ch.5.6 |
| AI models external | Explain Cloud Run integration architecture |
| Reports nav placeholder | Out of scope / future work |
| No patient portal | Scope exclusion in Ch.1.4 |

---

## Diagram Index

| File | Figure | Description |
|------|--------|-------------|
| `diagrams/01-architecture.mmd` | 3.1 | 3-layer architecture + external AI |
| `diagrams/02-class-diagram.mmd` | 3.2 | Core classes and relationships |
| `diagrams/03-use-case.mmd` | 3.4 | Actors and use cases |
| `diagrams/04-sequence-gummy-smile.mmd` | 3.5 | Gummy Smile sequence |
| `diagrams/05-activity-xray.mmd` | 3.3 | X-Ray analysis activity |
| `diagrams/06-erd.mmd` | 3.6 | Entity-relationship diagram |

Export `.mmd` files to PNG via [Mermaid Live Editor](https://mermaid.live) or VS Code Mermaid extension for Word insertion.
