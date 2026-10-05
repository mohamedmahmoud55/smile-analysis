# FUE Graduation Report — Chapters 4–6 (Full Draft)

**Project Title:** SmileAnalysis: An AI-Integrated Dental Clinic Management System for Orthodontic Assessment

**Author:** [Your Name]  
**Supervisor:** [Supervisor Name]  
**Faculty:** Faculty of Computers and Information Technology — Artificial Intelligence  
**Academic Year:** 2025–2026

---

# Chapter 4: System Design

## 4.1 Introduction

This chapter presents the detailed design of the SmileAnalysis system. Building upon the requirements and architectural overview in Chapter 3, it describes the development tools and technologies employed, the layered system structure, the website navigation map, user interface design principles, and the relational database schema. The design emphasizes separation of concerns, security through role-based access, and clean integration with external AI microservices.

## 4.2 Development Tools and Technologies

The following tools and frameworks were used throughout the project lifecycle:

| Tool / Technology | Version | Purpose |
|-------------------|---------|---------|
| .NET SDK | 9.0 | Application runtime and build toolchain |
| ASP.NET Core MVC | 9.0 | Web framework: routing, controllers, views, middleware |
| C# | 12 | Primary programming language |
| Entity Framework Core | 9.0 | Object-relational mapping, migrations, LINQ queries |
| Microsoft SQL Server | 2019+ | Relational database management system |
| ASP.NET Core Identity | 9.0 | Authentication, authorization, user and role management |
| Bootstrap | 5.x | CSS framework for responsive layout and components |
| Bootstrap Icons | 1.x | Iconography in navigation and action buttons |
| jQuery | 3.x | DOM manipulation and unobtrusive validation |
| jQuery Validation | 1.x | Client-side form validation |
| QuestPDF | Latest stable | Server-side PDF generation for prescriptions |
| HttpClient | Built-in | REST communication with external AI APIs |
| JetBrains Rider | 2024+ | Primary integrated development environment |
| Visual Studio Code | Latest | Secondary editor with launch configuration |
| Git | Latest | Version control |
| Google Cloud Run | — | Hosting platform for Gummy Smile and X-Ray AI services |

Package dependencies are declared in project files (`SmileAnalysisPl.csproj`, `SmileAnalysisBl.csproj`, `SmileAnalysisDal.csproj`) and restored via NuGet during build.

## 4.3 System Architecture

The system is organized into three logical layers within the solution, plus external AI services.

### 4.3.1 Presentation Layer (Frontend)

**Project:** `SmileAnalysisPl`

Responsibilities:
- Receive HTTP requests and route them to appropriate controllers
- Render Razor views with view models from the business layer
- Serve static assets (CSS, JavaScript, images) from `wwwroot`
- Apply authentication and authorization middleware
- Configure dependency injection and application startup in `Program.cs`

Eleven controllers manage distinct functional areas: `AccountController`, `HomeController`, `PatientController`, `DoctorController`, `StaffController`, `AppointmentController`, `PaymentController`, `PrescriptionController`, `GummySmileController`, `XRayAnalysisController`, and `AdminManagementController`.

### 4.3.2 Business Layer (Backend)

**Project:** `SmileAnalysisBl`

Responsibilities:
- Implement domain logic through service interfaces and classes
- Define view models for presentation binding
- Encapsulate external API communication in `GummySmileApiService` and `XRayAnalysisApiService`
- Generate prescription PDFs via `PrescriptionPdfService`
- Provide display helpers for formatting AI results

Key service registrations in `Program.cs` include `IPatientService`, `IGummySmileCaseService`, `IXRayAnalysisCaseService`, `IAppointmentService`, `IPaymentService`, `IPrescriptionService`, and `IAnalyticsService`.

### 4.3.3 Data Layer (Database)

**Project:** `SmileAnalysisDal`

Responsibilities:
- Define entity classes mapped to database tables
- Configure relationships and constraints via fluent API (`IEntityTypeConfiguration<T>`)
- Provide `SmileAnalysisDbContext` extending `IdentityDbContext<ApplicationUser>`
- Implement generic repository and unit-of-work patterns
- Manage schema migrations and seed data

On application startup, pending EF Core migrations are applied automatically, followed by execution of `SmileAnalysisDataSeeding` and `IdentityDbContextSeeding`.

### 4.3.4 External AI Services

Two REST APIs hosted on Google Cloud Run perform all machine learning inference:

| Service | Configuration Key | Endpoints Used |
|---------|-------------------|----------------|
| Gummy Smile API | `GummySmileApi:BaseUrl` | `api/cases`, `api/cases/{id}/images`, `api/cases/{id}/analyze`, `api/cases/{id}/clinical`, `api/cases/{id}/report.pdf` |
| X-Ray Analysis API | `XRayAnalysisApi:BaseUrl` | `api/analyze/panoramic`, `api/analyze/cvm`, `api/analyze/ceph-landmarks`, `api/analyze/ceph`, `api/analyze/full-case`, `api/analyze/dentalgemma-report` |

**Figure 4.1** reuses the system architecture diagram from Chapter 3 (`docs/diagrams/01-architecture.mmd`).

## 4.4 Website Map

The following site map describes main pages and navigation flow. Access to each area depends on the authenticated user's role.

```
┌─────────────────────────────────────────────────────────────────┐
│                     Account / Login                              │
│              (default route for unauthenticated users)           │
└────────────────────────────┬────────────────────────────────────┘
                             │ successful login
                             ▼
              ┌──────────────────────────────┐
              │   Role-based landing page    │
              └──────────────┬───────────────┘
                             │
     ┌───────────────────────┼───────────────────────┐
     ▼                       ▼                       ▼
┌─────────┐           ┌───────────┐           ┌──────────┐
│Dashboard│           │ Patients  │           │  Admin   │
│(Home)   │           │  Module   │           │ Mgmt     │
└─────────┘           └─────┬─────┘           └──────────┘
                            │
              ┌─────────────┼─────────────┐
              ▼             ▼             ▼
         Create/Edit    Details     Analysis History
                            │
              ┌─────────────┴─────────────┐
              ▼                           ▼
    ┌──────────────────┐       ┌──────────────────┐
    │  Gummy Smile     │       │  X-Ray Analysis  │
    │  Wizard          │       │  Page            │
    ├──────────────────┤       └──────────────────┘
    │ Start            │
    │ → Upload         │
    │ → Analyze        │
    │ → Review         │
    │ → Clinical       │
    │ → Diagnosis      │
    │ → Treatment      │
    │ → Download PDF   │
    └──────────────────┘

Additional top-level modules (role-dependent):
  • Doctors — Index, Create, Edit, Schedule, Availability
  • Staff — Index, Create, Edit
  • Appointments — Index, Create, Edit
  • Financials (Payments) — Index, Receipt
  • Prescriptions — Form, Details (PDF)
```

Navigation links are rendered in `Views/Shared/_OrthoNavItems.cshtml` with conditional visibility per role. SuperAdmin users see Dashboard, Patients, Staff, Doctors, Appointments, Financials, Doctor Schedule, and Admin Management. Doctor-only users see Patients, Doctor Schedule, and a Reports placeholder. Staff users see Patients, Appointments, Financials, and Doctor Schedule.

## 4.5 UI/UX Design

### 4.5.1 Design Principles

The user interface follows these principles:

1. **Role-appropriate navigation:** Users see only the menu items relevant to their responsibilities, reducing cognitive load and preventing unauthorized navigation attempts.
2. **Consistency:** A shared layout (`_Layout.cshtml`) provides uniform header, sidebar, and content area across all pages.
3. **Progressive disclosure:** The Gummy Smile module uses a step wizard (`_WizardSteps.cshtml`) to guide clinicians through a complex multi-stage workflow one step at a time.
4. **Feedback:** Validation errors appear inline on forms. Successful operations and API failures are communicated through TempData alert messages.
5. **Responsive layout:** Bootstrap grid and utility classes ensure usability on standard desktop and tablet viewports.

### 4.5.2 Key Screens

| Screen | View Path | Description |
|--------|-----------|-------------|
| Login | `Account/Login.cshtml` | Email/username and password entry; gateway to all features |
| Dashboard | `Home/Index.cshtml` | Analytics cards and welcome message for administrators |
| Patient List | `Patient/Index.cshtml` | Searchable table of patients with action links |
| Patient Details | `Patient/Details.cshtml` | Demographics, clinical history, links to start AI analyses |
| Gummy Smile Upload | `GummySmile/Upload.cshtml` | File upload for rest and smile photographs |
| Gummy Smile Review | `GummySmile/Review.cshtml` | AI results with override controls |
| Gummy Smile Diagnosis | `GummySmile/Diagnosis.cshtml` | Diagnosis summary and severity |
| X-Ray Analysis | `XRayAnalysis/Analyze.cshtml` | Image upload, analysis trigger, results with overlays |
| Prescription Form | `Prescription/Form.cshtml` | Medication line items and notes |
| Appointment Index | `Appointment/Index.cshtml` | Scheduled appointments with status badges |

**Note for report submission:** Insert screenshots of the above screens as figures in the final Word document. Screens should be captured with sample (anonymized) data.

### 4.5.3 Visual Elements

- **Status badges:** `_AppointmentStatusBadge.cshtml` color-codes appointment states.
- **Icons:** Bootstrap Icons (`bi-*`) identify navigation items (dashboard, patients, calendar, wallet).
- **Clinical history editor:** `EditorTemplates/PatientClinicalHistoryInputModel.cshtml` provides a reusable partial for medical history fields.

## 4.6 Database Schema (Relational Model)

The database `SmileAnalysis` is managed through Entity Framework Core code-first migrations. ASP.NET Identity tables (`AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, etc.) coexist with application tables in the same database context.

### 4.6.1 Core Tables

| Table | Description | Key Columns |
|-------|-------------|-------------|
| `Patients` | Patient demographics | Id, FirstName, LastName, Gender, DateOfBirth, Phone, Email |
| `PatientClinicalHistories` | Medical/dental history | PatientId (FK), allergy and history fields |
| `PatientBillingAccounts` | Billing profile and discounts | PatientId (FK), balance, discount settings |
| `Doctors` | Doctor profiles | Id, Specialty, LicenseNumber, ApplicationUserId (optional FK) |
| `DoctorAvailabilities` | Weekly schedule slots | DoctorId (FK), day, start/end time |
| `StaffMembers` | Non-clinical staff | Id, contact information |
| `Appointments` | Scheduled visits | PatientId, DoctorId, StaffId, AppointmentTime, Status |
| `Payments` | Financial transactions | PatientId, AppointmentId, Amount, Method, Status |
| `GummySmileCases` | Gummy smile AI cases | PatientId, GsCaseId, images, JSON snapshots, ReportPdf |
| `XRayAnalysisCases` | X-ray AI cases | PatientId, images, ResultsJson, CvmStage, SkeletalClass |
| `Prescriptions` | Prescription headers | AppointmentId, Notes |
| `PrescriptionItems` | Medication line items | PrescriptionId, MedicationName, Dosage, Instructions |

### 4.6.2 Relationship Summary

- Each `Patient` may have at most one `PatientClinicalHistory` and one `PatientBillingAccount`.
- Each `Patient` may have many `Appointments`, `GummySmileCases`, `XRayAnalysisCases`, and `Payments`.
- Each `Appointment` belongs to one `Patient` and one `Doctor`, optionally one `Staff` member.
- Each `Appointment` may have at most one `Prescription` with many `PrescriptionItems`.
- `GummySmileCase` and `XRayAnalysisCase` records are always linked to a `Patient` and store binary image data and serialized JSON results.

### 4.6.3 Schema Configuration

Fluent API configurations in `SmileAnalysisDal/Data/Configurations/` enforce:

- Required fields and maximum string lengths (e.g., patient names ≤ 100 characters)
- Foreign key relationships with appropriate delete behaviors
- Table naming conventions (e.g., `StaffMembers` for the `Staff` entity)

**Figure 4.2** presents the entity-relationship diagram (`docs/diagrams/06-erd.mmd`).

### 4.6.4 Migration History

Schema evolution is tracked through timestamped migrations:

| Migration | Purpose |
|-----------|---------|
| `20260410205138_Initial` | Core clinic schema |
| `20260411185831_PatientClinicalHistory` | Clinical history table |
| `20260411190937_AddDoctorAvaliablity` | Doctor availability |
| `20260415220000_FlexiblePatientBilling` | Billing accounts |
| `20260613000000_AddGummySmileCases` | Gummy smile persistence |
| `20260614153921_AddPrescriptions` | Prescription module |
| `20260615124313_AddXRayAnalysisCases` | X-ray analysis persistence |

---

# Chapter 5: System Implementation and Testing

## 5.1 Introduction

This chapter describes the implementation of the SmileAnalysis system, the organization of source code, the user interface structure, and the approach taken to verify core functionality. The implementation realizes the design presented in Chapters 3 and 4 using ASP.NET Core 9 with a three-project solution structure.

## 5.2 System Tools

Development and deployment rely on the tools listed in Section 4.2. The primary build commands are:

```bash
dotnet restore SmileAnalysis.sln
dotnet build SmileAnalysis.sln
dotnet run --project SmileAnalysisPl
```

Entity Framework migrations are applied automatically on startup when pending migrations exist. For manual migration management during development:

```bash
dotnet ef migrations add <Name> --project SmileAnalysisDal --startup-project SmileAnalysisPl
dotnet ef database update --project SmileAnalysisDal --startup-project SmileAnalysisPl
```

Configuration settings for database connectivity and AI API endpoints are stored in `appsettings.json` and may be overridden per environment using `appsettings.Development.json` or environment variables. Sensitive credentials should be stored outside source control using .NET User Secrets in production-oriented deployments.

## 5.3 Core Features Implemented

The following table summarizes implemented features and their operational status:

| Module | Controller | Operations | Status |
|--------|------------|------------|--------|
| Authentication | `AccountController` | Login, access denied | Implemented |
| Dashboard | `HomeController` | Analytics view (admin roles) | Implemented |
| Patients | `PatientController` | Create, Read, Update, Delete (role-scoped) | Implemented |
| Doctors | `DoctorController` | CRUD, schedule, availability | Implemented |
| Staff | `StaffController` | CRUD (SuperAdmin) | Implemented |
| Appointments | `AppointmentController` | Create, Read, Update | Implemented |
| Payments | `PaymentController` | Create, Read, receipts | Implemented |
| Prescriptions | `PrescriptionController` | Create, Read, Update, PDF | Implemented |
| Gummy Smile | `GummySmileController` | Full wizard workflow | Implemented |
| X-Ray Analysis | `XRayAnalysisController` | Upload and analyze | Implemented |
| Admin Management | `AdminManagementController` | User CRUD, roles, password reset | Implemented |

### 5.3.1 Gummy Smile Implementation Highlights

`GummySmileCaseService` coordinates between the local database and `GummySmileApiService`. When a case is started, a remote case is created via `POST api/cases` and the returned identifier is stored in `GummySmileCase.GsCaseId`. Images are uploaded with role query parameters (`rest` or `smile`). Analysis is triggered via `POST api/cases/{id}/analyze`. Clinician overrides and clinical data are synchronized with the remote API and cached locally as JSON columns.

### 5.3.2 X-Ray Analysis Implementation Highlights

`XRayAnalysisCaseService.RunAnalysisAsync` orchestrates a conditional pipeline based on which images are uploaded. Panoramic-only, cephalogram-only, and combined analyses are all supported. Results are serialized to `ResultsJson` and summary fields (`CvmStage`, `SkeletalClass`, `LandmarkCount`, `ConfidencePercent`) are denormalized for quick display.

### 5.3.3 Prescription PDF Implementation

`PrescriptionPdfService` uses QuestPDF to render a formatted prescription document including clinic header information from `ClinicOptions` configuration, patient and doctor details, medication line items, and notes.

## 5.4 User Interface Organized and Documented

The presentation layer contains 49 Razor view files organized by controller name under `SmileAnalysisPl/Views/`:

| Folder | Views | Purpose |
|--------|-------|---------|
| `Account/` | Login, AccessDenied | Authentication |
| `Home/` | Index | Dashboard |
| `Patient/` | Index, Create, Edit, Details, GummySmileAnalysis, XRayAnalysis | Patient management and analysis entry |
| `GummySmile/` | Upload, Analyze, Review, Clinical, Diagnosis, Treatment, _WizardSteps | Gummy smile wizard |
| `XRayAnalysis/` | Analyze | X-ray upload and results |
| `Doctor/` | Index, Create, Edit, Schedule, SchedulePicker, MySchedule, Availability | Doctor management |
| `Staff/` | Index, Create, Edit | Staff management |
| `Appointment/` | Index, Create, Edit | Appointment management |
| `Payment/` | Index, Receipt | Financial records |
| `Prescription/` | Form, Details | Prescription management |
| `AdminManagement/` | Index, Create, Edit, Delete, ManageRoles, ResetPassword | User administration |
| `Shared/` | _Layout, _OrthoNavItems, partials | Common layout and components |

Shared imports in `_ViewImports.cshtml` provide tag helpers and namespace imports across views. Client-side validation is included via `_ValidationScriptsPartial.cshtml`.

## 5.5 Source Code Organized and Documented

### 5.5.1 Solution Structure

```
SmileAnalysis.sln
├── SmileAnalysisPl/                 # Presentation Layer
│   ├── Controllers/                 # 11 MVC controllers
│   ├── Views/                       # Razor templates
│   ├── wwwroot/                     # Static files
│   ├── Program.cs                   # Application entry point
│   └── appsettings.json             # Configuration
├── SmileAnalysisBl/                 # Business Layer
│   ├── Services/
│   │   ├── Interfaces/              # Service contracts
│   │   └── Classes/                 # Service implementations
│   ├── GummySmile/                  # Gummy Smile API client + DTOs
│   ├── XRayAnalysis/                # X-Ray API client + DTOs
│   ├── ViewModels/                  # Presentation models
│   └── Configuration/               # Options classes
└── SmileAnalysisDal/                # Data Layer
    ├── Entities/                    # Domain entities + enums
    ├── Data/
    │   ├── Contexts/                  # DbContext
    │   ├── Configurations/            # Fluent API
    │   ├── Migrations/                # EF migrations
    │   └── DataSeed/                  # Seed data
    └── Repositories/                # Unit of Work + Generic Repository
```

### 5.5.2 Design Patterns Used

| Pattern | Application |
|---------|-------------|
| MVC | Separation of UI, control flow, and models |
| Repository + Unit of Work | Abstracted data access via `IUnitOfWork` and `IGenericRepository<T>` |
| Dependency Injection | All services registered in `Program.cs` and injected via constructors |
| Options Pattern | `GummySmileApiOptions`, `XRayAnalysisApiOptions`, `ClinicOptions` |
| Typed HttpClient | `AddHttpClient<IGummySmileApiService, GummySmileApiService>()` |
| DTO / ViewModel | API responses mapped to DTOs; UI bound to dedicated view models |

## 5.6 Test Plan and Test Cases

Because no automated test project is included in the repository, verification was planned as manual functional testing against a local development environment with seeded data and live AI API connectivity.

### 5.6.1 Test Objectives

1. Verify authentication and role-based access control.
2. Confirm CRUD operations for all core clinic entities.
3. Validate end-to-end Gummy Smile and X-Ray analysis workflows.
4. Confirm PDF generation for prescriptions and gummy smile reports.
5. Verify error handling when API services are unavailable or inputs are invalid.

### 5.6.2 Test Environment

| Component | Configuration |
|-----------|---------------|
| Application | `dotnet run --project SmileAnalysisPl` |
| Database | Local SQL Server with auto-migration |
| Seed data | `IdentityDbContextSeeding`, `SmileAnalysisDataSeeding` |
| AI services | Cloud Run endpoints configured in appsettings |
| Browser | Chrome (latest) |

### 5.6.3 Test Cases

| ID | Module | Test Case | Steps | Expected Result |
|----|--------|-----------|-------|-----------------|
| TC-01 | Auth | Valid SuperAdmin login | Enter valid credentials on Login page | Redirect to Dashboard |
| TC-02 | Auth | Invalid login | Enter incorrect password | Error message; remain on Login |
| TC-03 | Auth | Unauthorized access | Staff navigates to `/AdminManagement` | Access Denied response |
| TC-04 | Patient | Create patient | SuperAdmin submits Create form with valid data | Patient listed with ID |
| TC-05 | Patient | Edit patient | Update phone on Edit form | Details shows new phone |
| TC-06 | Patient | Doctor patient scope | Login as Doctor; open patient list | Only associated patients visible |
| TC-07 | Patient | Delete patient | SuperAdmin deletes test patient | Patient removed from index |
| TC-08 | Appointment | Create appointment | Staff links patient, doctor, date/time | Appointment appears as Scheduled |
| TC-09 | Appointment | Update status | Edit appointment to Completed | Status badge updates |
| TC-10 | Doctor | Add doctor | SuperAdmin creates doctor record | Doctor visible in index |
| TC-11 | Doctor | Set availability | Add weekly time slots | Schedule page shows slots |
| TC-12 | Payment | Record payment | Staff records payment for appointment | Payment listed in Financials |
| TC-13 | Prescription | Create prescription | Doctor adds items to appointment prescription | Details page shows items |
| TC-14 | Prescription | Download PDF | Click PDF download on prescription | PDF file downloads |
| TC-15 | Gummy Smile | Full wizard | Start → upload → analyze → review → clinical → diagnosis → treatment | Case completes; PDF available |
| TC-16 | Gummy Smile | Validation | Submit upload without smile image | Validation error shown |
| TC-17 | X-Ray | Panoramic analysis | Upload panoramic only; run analysis | Findings displayed; case saved |
| TC-18 | X-Ray | Cephalogram pipeline | Upload ceph; enable DentalGemma | CVM, measurements, report shown |
| TC-19 | Admin | Create user | SuperAdmin creates Admin account | New user can log in |
| TC-20 | Dashboard | Load analytics | SuperAdmin opens Dashboard | Metrics display without error |

## 5.7 Testing the Core Features with Sample Data

### 5.7.1 Seeded Identity Data

On first run, `IdentityDbContextSeeding` creates four roles (SuperAdmin, Admin, Doctor, Staff) and default administrative users if the database is empty. Test accounts should be used locally; credentials must not be published in the graduation report.

### 5.7.2 Seeded Clinic Data

`SmileAnalysisDataSeeding` inserts sample doctors (Abdallah Mohamed — Orthodontics; Amr Gamal — Periodontics) and staff (Mahmoud Azab) when the respective tables are empty.

### 5.7.3 AI Testing Data

Gummy Smile testing requires paired rest and smile photographs of adequate resolution. X-Ray testing requires de-identified panoramic and/or lateral cephalogram images. Tests TC-15 through TC-18 depend on network connectivity to the configured Cloud Run endpoints.

### 5.7.4 Observed Results

During development testing, core CRUD operations functioned correctly with role restrictions enforced as designed. Gummy Smile and X-Ray workflows successfully communicated with external APIs, persisted results to SQL Server, and rendered analysis pages with overlay images and measurement tables. Prescription PDF generation produced readable documents with clinic header information. API timeout and validation error paths displayed user-facing messages without application crashes.

---

# Chapter 6: Conclusions and Future Work

## 6.1 Introduction

This final chapter summarizes the SmileAnalysis project, evaluates the extent to which stated objectives were achieved, highlights contributions, and proposes directions for future enhancement. The project demonstrates that a modern ASP.NET Core clinic portal can effectively integrate external AI microservices for orthodontic assessment while maintaining conventional practice management capabilities.

## 6.2 Project Summary

SmileAnalysis was developed as a three-tier web application to address the fragmentation between dental clinic administration and AI-assisted diagnostic tools. The system provides role-based access for SuperAdmin, Admin, Doctor, and Staff users; manages patients, doctors, staff, appointments, payments, and prescriptions; and embeds two major AI workflows—gummy smile analysis and orthodontic X-ray interpretation—within the patient record context.

Technically, the solution comprises `SmileAnalysisPl` (presentation), `SmileAnalysisBl` (business logic and API clients), and `SmileAnalysisDal` (persistence). Microsoft SQL Server stores all operational and analysis data. External Gummy Smile and X-Ray Analysis services hosted on Google Cloud Run perform machine learning inference, while the portal orchestrates requests, caches results, and supports clinician review and override.

All six specific objectives outlined in Chapter 1 were addressed: role-based security, CRUD clinic operations, Gummy Smile wizard integration, X-Ray multi-stage analysis including optional DentalGemma reports, local persistence of AI artifacts, and prescription PDF generation.

## 6.3 Project Contributions

The project makes the following contributions:

1. **Integrated clinical workflow:** Demonstrates a practical pattern for launching AI analyses directly from a patient record without switching applications.
2. **Local persistence of AI outputs:** Stores images, JSON snapshots, and PDF reports in `GummySmileCase` and `XRayAnalysisCase` tables, supporting audit, revisit, and comparison over time.
3. **Clinician-in-the-loop design:** Provides review, override, and clinical input stages that preserve professional judgment over raw AI recommendations.
4. **Microservice integration reference:** Documents how typed HttpClient services, options-based configuration, and structured DTOs connect an ASP.NET Core application to cloud-hosted AI endpoints.
5. **Academic software engineering artifact:** Delivers a complete layered architecture with migrations, seed data, and role-based UI suitable as a graduation project exemplar in health informatics and applied AI.

## 6.4 Conclusion and Future Work

### Conclusion

SmileAnalysis successfully bridges the gap between conventional dental clinic management and modern orthodontic AI capabilities. By externalizing model inference to cloud microservices and focusing the portal on workflow, security, and persistence, the project achieves a balance between technical ambition and deliverable scope appropriate for an academic timeline. The system is functional, extensible, and grounded in real clinical scenarios involving smile aesthetics and radiographic assessment.

### Future Work

Several enhancements would strengthen the system for production deployment:

1. **Automated testing:** Add xUnit test projects for service layer logic, API client mocking, and integration tests against a test database.
2. **Reports module:** Implement the analytics and reporting section currently represented as a navigation placeholder.
3. **Secrets management:** Move database and API credentials to .NET User Secrets, Azure Key Vault, or environment variables; remove plain-text secrets from configuration files.
4. **Patient portal:** Develop a patient-facing interface for appointment requests and viewing approved analysis summaries.
5. **Mobile support:** Progressive Web App or native mobile client for doctors reviewing cases on tablets.
6. **Payment gateway:** Integrate Stripe, PayPal, or regional payment providers for online billing.
7. **Interoperability:** Export patient and analysis data using HL7 FHIR standards for integration with hospital EHR systems.
8. **On-premises AI option:** Support private deployment of AI models for clinics with data sovereignty requirements.
9. **Performance optimization:** Add caching, asynchronous background processing for long AI jobs, and progress indicators.
10. **Documentation:** Publish a README with setup instructions, architecture overview, and API dependency requirements.

With these improvements, SmileAnalysis could evolve from an academic prototype into a deployable solution for orthodontic and aesthetic dental clinics seeking AI-augmented workflows.

---

*End of Chapters 4–6 Draft*

*Companion documents: [FUE-Report-Outline.md](FUE-Report-Outline.md) | [FUE-Report-Chapters-1-3-Draft.md](FUE-Report-Chapters-1-3-Draft.md)*
