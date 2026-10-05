# FUE Graduation Report — Chapters 1–3 (Full Draft)

**Project Title:** SmileAnalysis: An AI-Integrated Dental Clinic Management System for Orthodontic Assessment

**Author:** [Your Name]  
**Supervisor:** [Supervisor Name]  
**Faculty:** Faculty of Computers and Information Technology — Artificial Intelligence  
**Academic Year:** 2025–2026

---

# Chapter 1: Introduction

## 1.1 Background of the Study

Orthodontic and aesthetic dentistry has undergone significant transformation as digital imaging, computer vision, and artificial intelligence become increasingly available in clinical practice. Two assessment domains are particularly important in modern orthodontic care: the evaluation of excessive gingival display, commonly referred to as a gummy smile, and the interpretation of radiographic images such as panoramic radiographs and lateral cephalograms. These assessments traditionally depend on manual clinical judgment and time-consuming tracing procedures, which can introduce variability between clinicians and limit throughput in busy dental clinics.

The broader context of digital health has created demand for integrated information systems that connect patient administration with advanced diagnostic support. Practice management software has long supported scheduling, billing, and basic patient records; however, such systems typically do not embed state-of-the-art machine learning models for smile aesthetics or cephalometric analysis. Conversely, research prototypes and standalone AI demonstrations can perform impressive image analysis but rarely provide the operational infrastructure required in a real clinic, including role-based access control, appointment linkage, prescription management, and persistent storage of analysis results.

Artificial intelligence offers timely opportunities to assist clinicians without replacing their judgment. Deep learning models can detect anatomical landmarks, classify skeletal relationships, stage cervical vertebral maturation (CVM), and quantify smile characteristics from photographs. Large language models adapted to dentistry, such as DentalGemma, can further synthesize structured findings into narrative reports. Despite these advances, a gap remains between AI capability and everyday clinical workflow. Small and medium orthodontic clinics need practical systems that unify patient records with AI-assisted analysis in a single, secure web portal.

The SmileAnalysis project addresses this need by developing an AI-integrated dental clinic management system built on ASP.NET Core 9. The system manages core clinic operations—patients, doctors, staff, appointments, payments, and prescriptions—while orchestrating two external AI microservices hosted on Google Cloud Run: one for gummy smile analysis and one for orthodontic X-ray interpretation. By persisting images, JSON snapshots, and generated reports in a SQL Server database, the system ensures that AI outputs remain accessible for clinical review, audit, and longitudinal comparison. This project is therefore both a software engineering effort and an applied artificial intelligence integration study, positioned at the intersection of health informatics, computer vision, and modern web development.

## 1.2 Problem Statement

Dental and orthodontic clinics frequently operate with fragmented tools: paper or legacy electronic records for administration, separate imaging software for radiographs, and no standardized pipeline for AI-assisted smile assessment. Commercial practice management systems focus on billing and scheduling but offer limited support for orthodontic computer vision workflows. Standalone AI tools, meanwhile, lack patient management, multi-role authorization, and durable storage of results within the clinic's own database.

Existing solutions are insufficient because they either manage clinic operations without AI or provide AI analysis without clinical context. Clinicians must therefore switch between applications, manually associate findings with patients, and risk losing analysis history. The core problem addressed by this project is the absence of an integrated system that links clinic operations with gummy-smile and X-ray AI analysis in a secure, role-aware, and persistent manner.

## 1.3 Aim and Objectives

### General Aim

The general aim of this project is to design and implement a web-based dental clinic management system that unifies patient administration with AI-assisted gummy-smile and orthodontic X-ray analysis.

### Specific Objectives

1. To implement role-based access control supporting SuperAdmin, Admin, Doctor, and Staff roles using ASP.NET Core Identity.
2. To develop full CRUD functionality for patients, doctors, staff, appointments, and payment records with appropriate role restrictions.
3. To integrate the Gummy Smile AI service through a complete clinical wizard from image upload to downloadable PDF report.
4. To integrate the X-Ray Analysis AI service for panoramic findings, CVM staging, cephalometric measurements, full-case synthesis, and optional DentalGemma narrative reports.
5. To persist AI case data, uploaded images, and analysis snapshots in a SQL Server database for clinical review and audit.
6. To generate prescription documents in PDF format linked to appointments using QuestPDF.

## 1.4 Scope of the Project

### Included

The project includes a three-tier ASP.NET Core MVC web application comprising a presentation layer (`SmileAnalysisPl`), a business logic layer (`SmileAnalysisBl`), and a data access layer (`SmileAnalysisDal`). Functional scope covers user authentication, role-based navigation, patient and clinical history management, doctor and staff administration, appointment scheduling, payment and billing records, prescription creation with PDF export, dashboard analytics, and end-to-end integration with two external REST AI services. All AI inference is performed by microservices deployed on Google Cloud Run; the portal orchestrates API calls and stores returned results locally.

### Excluded

The project does not include training or hosting machine learning models, development of a mobile native application, a patient self-service booking portal, integration with real payment gateways, or a fully implemented standalone Reports module (a navigation placeholder exists). Automated unit and integration test projects are also outside the current scope.

### Constraints

Development is constrained by the academic project timeline, dependency on a SQL Server database instance, requirement for internet connectivity to reach external AI APIs, and the five-minute HTTP timeout configured for AI service calls. Sample clinical images are required for meaningful AI testing.

## 1.5 Significance of the Project

This project benefits multiple stakeholders. Orthodontists and aesthetic dentists gain a structured workflow for AI-assisted smile and radiographic assessment with the ability to review, override, and supplement machine outputs with clinical judgment. Clinic administrative staff benefit from centralized scheduling, financial tracking, and patient records. Patients indirectly benefit through improved care continuity when analysis results are stored alongside their clinical history. Researchers and software engineers gain a documented reference architecture for integrating external AI microservices into a production-style clinic portal using modern .NET technologies.

The innovation lies not in training new models but in demonstrating a practical, secure, and maintainable integration pattern that bridges operational clinic software with specialized orthodontic AI services—a pattern increasingly relevant as AI capabilities are delivered as cloud APIs rather than embedded libraries.

## 1.6 Project Methodology (Brief Overview)

The project followed an iterative Agile-inspired software development lifecycle. Initial requirements were derived from typical orthodontic clinic workflows and the capabilities exposed by the external AI APIs. The system was decomposed into three architectural layers to separate concerns and improve maintainability. Database schema evolution was managed through Entity Framework Core migrations, applied automatically on application startup.

Development proceeded in feature-oriented sprints: core clinic entities and authentication first, followed by the Gummy Smile wizard, X-Ray analysis integration, and prescription PDF generation. Key technologies include ASP.NET Core 9, Entity Framework Core 9, SQL Server, ASP.NET Core Identity, Bootstrap 5, typed HttpClient REST clients, QuestPDF, and Google Cloud Run for hosted AI inference.

## 1.7 Project Organization

The remainder of this report is organized as follows. Chapter 2 presents a literature review of related dental practice systems, orthodontic imaging tools, and AI research relevant to smile and radiographic analysis, along with a summary of technologies selected for implementation. Chapter 3 describes system analysis and design, including functional and non-functional requirements, the deployment environment, and architectural diagrams. Chapter 4 details the system design, including development tools, layered architecture, website map, user interface considerations, and the relational database schema. Chapter 5 documents implementation, source code organization, and manual testing with sample data. Chapter 6 concludes the report, summarizes contributions, and proposes directions for future work.

---

# Chapter 2: Literature Review

## 2.1 Introduction

This chapter reviews existing research, commercial systems, and technologies relevant to the SmileAnalysis project. The focus is on dental practice management, orthodontic imaging and analysis, artificial intelligence applications in dentistry, and the software frameworks used to build an integrated clinic portal. The review establishes the context for the research gap that this project addresses: the lack of unified systems that combine everyday clinic operations with modern AI-assisted orthodontic assessment.

## 2.2 Review of Related Systems and Studies

### Dental Practice Management Systems

Commercial dental practice management systems (PMS) such as Dentrix, Open Dental, and similar platforms provide essential clinic functions including patient demographics, appointment scheduling, billing, and treatment history [Author, Year]. These systems have matured over decades and are widely adopted in general and specialty dental practices. However, their orthodontic and AI capabilities are often limited to imaging storage or third-party plugin integrations rather than native computer-vision analysis pipelines. SmileAnalysis complements this landscape by prioritizing AI workflow integration over breadth of generic PMS features.

### Orthodontic Imaging and Cephalometric Software

Specialized orthodontic imaging platforms, including Dolphin Imaging and OnyxCeph, support cephalometric tracing, landmark identification, and treatment planning visualization [Author, Year]. Such tools excel at radiographic measurement but are typically licensed separately, may not include general clinic billing, and do not always incorporate recent deep-learning landmark detectors or large language model report generation. The X-Ray Analysis integration in SmileAnalysis draws on similar clinical concepts—panoramic evaluation, CVM staging, and cephalometric angles such as SNA, SNB, and ANB—while delegating inference to a dedicated cloud API.

### Gummy Smile and Aesthetic Smile Analysis

Excessive gingival display is a recognized aesthetic concern in dentistry and orthodontics. Clinical assessment considers lip dynamics, gingival levels, and tooth proportions [Author, Year]. Recent computer vision research has explored automated smile analysis, facial landmark detection, and gingival exposure quantification from photographs [Author, Year]. These studies demonstrate feasibility but rarely deliver end-to-end clinic portals with user management and persistent case records. The Gummy Smile module in this project implements a practitioner-facing wizard that uploads rest and smile images, invokes AI analysis, allows clinician overrides, and stores results locally.

### Cephalometric Automation and CVM Staging

Automated cephalometric landmark detection has been an active research area, with convolutional and transformer-based models improving speed and consistency relative to manual tracing [Author, Year]. Cervical vertebral maturation (CVM) staging from lateral cephalograms supports growth prediction and treatment timing decisions [Author, Year]. SmileAnalysis consumes these capabilities through the X-Ray Analysis API, which returns landmark coordinates, skeletal classification, CVM stage, and synthesized full-case recommendations.

### AI-Generated Clinical Reports

Large language models adapted to medical and dental domains, including approaches related to DentalGemma, can generate narrative summaries from structured findings [Author, Year]. While such outputs require clinician verification, they can reduce documentation time. The optional DentalGemma report feature in SmileAnalysis illustrates how LLM-generated text can be integrated alongside quantitative measurements within a unified analysis page.

### Microservice Architectures for Healthcare AI

Deploying machine learning models as independent REST microservices—such as containers on Google Cloud Run—has become a common pattern for scalability and separation of concerns [Author, Year]. The clinic portal remains lightweight while GPU-intensive inference runs in the cloud. This architectural choice aligns with modern MLOps practice and is central to the SmileAnalysis design.

## 2.3 Research Gap and Summary of Literature Review

The literature and commercial landscape reveal a persistent divide. Practice management systems manage operations but seldom embed cutting-edge orthodontic AI. Research prototypes demonstrate AI accuracy but lack production concerns: authentication, authorization, multi-user workflows, relational persistence, and prescription linkage. Manual cephalometric tracing remains common in clinics without specialized software, and gummy smile assessment lacks standardized digital workflows in smaller practices.

SmileAnalysis contributes to filling this gap by implementing a unified, role-based web portal that orchestrates external AI microservices, persists all case artifacts in SQL Server, and embeds AI workflows within the patient record context. The project does not claim novelty in model training; rather, it addresses systems integration and clinical workflow design for AI-augmented orthodontic practice.

## 2.4 Summary of Existing Technologies

The following technologies were selected based on maturity, ecosystem support, and suitability for a layered clinic application:

| Technology | Justification |
|------------|---------------|
| **ASP.NET Core 9 MVC** | Robust web framework with built-in dependency injection, security middleware, and Razor view engine for server-rendered clinic UIs |
| **Entity Framework Core 9** | Code-first migrations, fluent API configurations, and strong SQL Server support |
| **ASP.NET Core Identity** | Industry-standard authentication and role management |
| **SQL Server** | Relational integrity for patient, appointment, and analysis case relationships |
| **Bootstrap 5** | Responsive, accessible UI components with minimal custom CSS |
| **HttpClient** | Typed REST clients with configurable timeouts for long-running AI requests |
| **Google Cloud Run** | Serverless hosting for containerized AI inference endpoints |
| **QuestPDF** | Programmatic PDF generation for prescriptions |

Configuration of API base URLs and database connections is centralized in application settings, with dependency injection wiring services at startup in `Program.cs`. This technology stack provides a maintainable foundation for future extensions such as automated testing, additional AI modules, and external EHR interoperability.

---

# Chapter 3: System Analysis and Design

## 3.1 System Overview and Proposed Concept

SmileAnalysis is conceived as a role-based dental clinic portal that unifies administrative workflows with AI-assisted clinical analysis. Authorized users authenticate through ASP.NET Core Identity and access features according to their assigned role: SuperAdmin, Admin, Doctor, or Staff. SuperAdmin users have full system access including user and role management. Admin users manage clinic operations and view analytics. Doctors focus on patients, schedules, prescriptions, and AI analyses. Staff handle patient registration, appointments, and financial records.

The central workflow begins at the patient record. From the patient details page, a clinician can initiate a Gummy Smile analysis or an X-Ray analysis. Each workflow communicates with an external AI microservice, receives structured results, and persists images and JSON snapshots in local database tables (`GummySmileCase` and `XRayAnalysisCase`). Clinicians may review AI outputs, apply overrides, enter supplementary clinical data, and download PDF reports. This design keeps the human clinician in control while leveraging AI for measurement, detection, and documentation support.

**Figure 3.1** illustrates the high-level system architecture. The presentation layer (`SmileAnalysisPl`) contains MVC controllers and Razor views. The business layer (`SmileAnalysisBl`) implements domain services and HTTP clients for external APIs. The data layer (`SmileAnalysisDal`) provides Entity Framework Core persistence through a unit-of-work and repository pattern. External Gummy Smile and X-Ray Analysis services run on Google Cloud Run. (See `docs/diagrams/01-architecture.mmd`.)

## 3.2 Functional Requirements

Functional requirements are expressed as "shall" statements derived from implemented controller behavior and service contracts.

### Authentication and Authorization

- **FR-AUTH-01:** The system shall require a valid authenticated session for all protected resources.
- **FR-AUTH-02:** The system shall redirect unauthenticated users to the login page by default.
- **FR-AUTH-03:** The system shall enforce role-based authorization on controller actions using declarative `[Authorize(Roles = "...")]` attributes.
- **FR-AUTH-04:** The system shall support password-based sign-in through ASP.NET Core Identity with unique email addresses.

### Patient Management

- **FR-PAT-01:** The system shall allow SuperAdmin and Staff users to create, view, edit, and delete patient records.
- **FR-PAT-02:** The system shall allow Doctor users to view and edit patients with whom they share appointments.
- **FR-PAT-03:** The system shall store optional clinical history information per patient.
- **FR-PAT-04:** The system shall display historical Gummy Smile and X-Ray analysis cases on the patient details page.

### Gummy Smile Analysis

- **FR-GS-01:** The system shall create a remote AI case and a corresponding local `GummySmileCase` record when analysis is started for a patient.
- **FR-GS-02:** The system shall require both a rest (repose) image and a smile image before proceeding to analysis.
- **FR-GS-03:** The system shall submit images to the external API and invoke the analyze endpoint to obtain AI results.
- **FR-GS-04:** The system shall present AI outputs for clinician review and accept manual overrides.
- **FR-GS-05:** The system shall collect supplementary clinical inputs and persist draft and submitted clinical JSON.
- **FR-GS-06:** The system shall display diagnosis and treatment plan views based on finalized case data.
- **FR-GS-07:** The system shall provide downloadable PDF and JSON reports for completed cases.

### X-Ray Analysis

- **FR-XR-01:** The system shall accept optional panoramic and/or lateral cephalogram image uploads.
- **FR-XR-02:** The system shall invoke panoramic analysis when a panoramic image is supplied.
- **FR-XR-03:** The system shall invoke cephalometric landmark detection, CVM staging, and cephalometric summary when a cephalogram is supplied.
- **FR-XR-04:** The system shall invoke full-case synthesis when panoramic, CVM, and cephalometric summary data are all available.
- **FR-XR-05:** The system shall optionally invoke DentalGemma narrative report generation when requested by the clinician.
- **FR-XR-06:** The system shall persist uploaded images, summary fields, and complete results JSON in `XRayAnalysisCase`.

### Appointments

- **FR-APT-01:** The system shall allow SuperAdmin, Admin, and Staff users to create, view, and edit appointments.
- **FR-APT-02:** The system shall associate each appointment with a patient, a doctor, and optionally a staff member.
- **FR-APT-03:** The system shall track appointment status (e.g., Scheduled, Completed, Cancelled).

### Doctors and Staff

- **FR-DOC-01:** The system shall maintain doctor profiles including specialty and license number.
- **FR-DOC-02:** The system shall support weekly availability schedules and schedule viewing for doctors.
- **FR-STF-01:** The system shall allow SuperAdmin and Admin users to manage staff member records.

### Payments and Billing

- **FR-PAY-01:** The system shall record payments linked to patients and appointments.
- **FR-PAY-02:** The system shall support patient billing accounts with configurable discounts.

### Prescriptions

- **FR-RX-01:** The system shall allow Doctor and SuperAdmin users to create and edit prescriptions associated with appointments.
- **FR-RX-02:** The system shall generate downloadable prescription PDF documents.

### Administration

- **FR-ADM-01:** The system shall allow SuperAdmin users to create, edit, and delete administrative user accounts.
- **FR-ADM-02:** The system shall allow SuperAdmin users to assign roles and reset user passwords.

### Dashboard

- **FR-DASH-01:** The system shall display orthodontic dashboard analytics to SuperAdmin and Admin users on login.

## 3.3 Non-Functional Requirements

| ID | Category | Requirement |
|----|----------|-------------|
| NFR-01 | Performance | AI inference shall be delegated to external Cloud Run services to avoid blocking local compute resources. |
| NFR-02 | Performance | HTTP clients for AI services shall use a five-minute timeout to accommodate large image processing. |
| NFR-03 | Usability | The user interface shall use Bootstrap 5 for responsive layout across desktop browsers. |
| NFR-04 | Usability | Navigation menus shall adapt to the authenticated user's role. |
| NFR-05 | Security | All state-changing form submissions shall validate anti-forgery tokens. |
| NFR-06 | Security | Sensitive operations shall be restricted by role-based authorization policies. |
| NFR-07 | Reliability | API failures shall be reported to the user through error messages without corrupting local case state. |
| NFR-08 | Reliability | Partial and complete analysis results shall be persisted to the database for recovery and review. |
| NFR-09 | Maintainability | The codebase shall follow a three-layer architecture with dependency injection. |
| NFR-10 | Scalability | AI services shall remain independently deployable microservices decoupled from the web portal. |

## 3.4 System Environment

### Hardware Requirements

| Component | Minimum Specification |
|-----------|----------------------|
| Development workstation | Multi-core CPU, 8 GB RAM, 256 GB storage |
| Database server | SQL Server-compatible instance (local or containerized) |
| Network | Broadband internet for Cloud Run API access |

### Software Requirements

| Component | Version / Detail |
|-----------|------------------|
| Operating system | Windows 10+, macOS 12+, or Linux |
| .NET SDK | 9.0 |
| Database | Microsoft SQL Server 2019 or later |
| Web browser | Chrome, Edge, or Firefox (latest) |
| IDE | JetBrains Rider or Visual Studio Code |

### Deployment Configuration

During development, the application runs at `http://localhost:5138` and `https://localhost:7003` as defined in launch settings. Database connectivity is configured through a connection string in application settings (credentials shall be managed securely and not embedded in documentation). Gummy Smile and X-Ray API base URLs are configured under `GummySmileApi` and `XRayAnalysisApi` sections respectively.

## 3.5 System Architecture

**Figure 3.1 — System Architecture Diagram**

The architecture follows a classic three-tier pattern extended with external AI microservices:

1. **Presentation Layer (`SmileAnalysisPl`):** Handles HTTP requests, renders Razor views, serves static assets, and enforces authentication middleware.
2. **Business Layer (`SmileAnalysisBl`):** Contains domain services (`PatientService`, `GummySmileCaseService`, `XRayAnalysisCaseService`, etc.), view models, and typed API clients.
3. **Data Layer (`SmileAnalysisDal`):** Defines entities, EF Core `DbContext`, fluent configurations, migrations, and repository abstractions.
4. **External Services:** Gummy Smile and X-Ray Analysis APIs hosted on Google Cloud Run perform all machine learning inference.

Source: `docs/diagrams/01-architecture.mmd`

## 3.6 Class Diagram

**Figure 3.2 — Core Class Diagram**

The domain model centers on `Patient`, which aggregates appointments, payments, gummy smile cases, and X-ray analysis cases. `Doctor` and `Staff` relate to `Appointment`. `Prescription` links one-to-one with `Appointment` and contains multiple `PrescriptionItem` entries. Service classes in the business layer orchestrate operations on these entities and delegate API calls to `GummySmileApiService` and `XRayAnalysisApiService`.

Key entity classes reside in `SmileAnalysisDal/Entities/`. Service implementations reside in `SmileAnalysisBl/Services/Classes/`.

Source: `docs/diagrams/02-class-diagram.mmd`

## 3.7 Activity Diagram

**Figure 3.3 — X-Ray Analysis Activity Diagram**

The X-Ray workflow begins when a clinician uploads one or both image types. If a panoramic image is present, `AnalyzePanoramicAsync` is called. If a cephalogram is present, the system sequentially invokes landmark detection, CVM analysis, and cephalometric summary computation. When panoramic and cephalometric data are both available, `AnalyzeFullCaseAsync` synthesizes integrated treatment logic. If the clinician enables DentalGemma, a narrative report is requested. Finally, all results are serialized and stored in `XRayAnalysisCase`, and the analysis page renders measurements and overlay images.

Source: `docs/diagrams/05-activity-xray.mmd`; implementation in `XRayAnalysisCaseService.RunAnalysisAsync`.

## 3.8 Use Case Diagram

**Figure 3.4 — Use Case Diagram**

Four actors interact with the system:

- **SuperAdmin:** Full access including user management, dashboard, all clinic modules, and AI analyses.
- **Admin:** Dashboard, patients, doctors, staff, appointments, payments.
- **Doctor:** Patients (scoped), own schedule, prescriptions, Gummy Smile and X-Ray analyses, report downloads.
- **Staff:** Patients, appointments, payments, doctor schedule viewing.

Source: `docs/diagrams/03-use-case.mmd`

## 3.9 Sequence Diagram

**Figure 3.5 — Gummy Smile Analysis Sequence Diagram**

The sequence illustrates the collaboration between the doctor, `GummySmileController`, `GummySmileCaseService`, `GummySmileApiService`, the external Gummy Smile API, and SQL Server. Case creation occurs first, followed by image uploads, analysis invocation, clinical data submission, and PDF report retrieval. At each step, the business service updates the local `GummySmileCase` record to maintain consistency even if external API calls are retried.

Source: `docs/diagrams/04-sequence-gummy-smile.mmd`

## 3.10 ERD Diagram

**Figure 3.6 — Entity-Relationship Diagram**

The relational schema comprises twelve application tables plus ASP.NET Identity tables. Primary relationships include:

- `Patients` → `PatientClinicalHistories` (one-to-one)
- `Patients` → `PatientBillingAccounts` (one-to-one)
- `Patients` → `Appointments`, `GummySmileCases`, `XRayAnalysisCases`, `Payments` (one-to-many)
- `Doctors` → `DoctorAvailabilities`, `Appointments` (one-to-many)
- `Appointments` → `Prescriptions` (one-to-one)
- `Prescriptions` → `PrescriptionItems` (one-to-many)

Entity configurations in `SmileAnalysisDal/Data/Configurations/` define column constraints and relationship delete behaviors. Migrations in `SmileAnalysisDal/Data/Migrations/` document schema evolution chronologically.

Source: `docs/diagrams/06-erd.mmd`

---

*End of Chapters 1–3 Draft*

*Companion documents: [FUE-Report-Outline.md](FUE-Report-Outline.md) | [FUE-Report-Chapters-4-6-Draft.md](FUE-Report-Chapters-4-6-Draft.md)*
