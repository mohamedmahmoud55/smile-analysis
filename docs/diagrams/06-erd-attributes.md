# ERD Attributes Reference — SmileAnalysis

Use this when drawing the ERD in draw.io, Lucidchart, or Word.  
Mermaid file: [06-erd.mmd](06-erd.mmd) — attributes are inside each entity `{ }` block.

**Legend:** PK = Primary Key | FK = Foreign Key | UK = Unique

---

## AspNetUsers

| Column | Type | Key |
|--------|------|-----|
| Id | varchar(450) | PK |
| FirstName | varchar(50) | |
| LastName | varchar(50) | |
| UserName | nvarchar(256) | |
| NormalizedUserName | nvarchar(256) | |
| Email | nvarchar(256) | UK |
| NormalizedEmail | nvarchar(256) | |
| EmailConfirmed | bit | |
| PasswordHash | nvarchar(max) | |
| SecurityStamp | nvarchar(max) | |
| ConcurrencyStamp | nvarchar(max) | |
| PhoneNumber | nvarchar(max) | |
| PhoneNumberConfirmed | bit | |
| TwoFactorEnabled | bit | |
| LockoutEnd | datetimeoffset | |
| LockoutEnabled | bit | |
| AccessFailedCount | int | |

---

## AspNetRoles

| Column | Type | Key |
|--------|------|-----|
| Id | varchar(450) | PK |
| Name | nvarchar(256) | |
| NormalizedName | nvarchar(256) | |
| ConcurrencyStamp | nvarchar(max) | |

---

## AspNetUserRoles

| Column | Type | Key |
|--------|------|-----|
| UserId | varchar(450) | PK, FK → AspNetUsers |
| RoleId | varchar(450) | PK, FK → AspNetRoles |

---

## Patients

| Column | Type | Key |
|--------|------|-----|
| Id | int | PK |
| FirstName | varchar(100) | |
| LastName | varchar(100) | |
| Gender | int | |
| DateOfBirth | datetime | |
| Phone | varchar(30) | |
| Email | varchar(256) | |
| CreatedAt | datetime | |
| UpdatedAt | datetime | |

---

## PatientClinicalHistories

| Column | Type | Key |
|--------|------|-----|
| Id | int | PK |
| PatientId | int | FK, UK → Patients |
| ChronicDiseases | nvarchar(4000) | |
| CurrentMedications | nvarchar(4000) | |
| Allergies | nvarchar(4000) | |
| PreviousSurgeries | nvarchar(4000) | |
| HeartOrBleedingConditions | nvarchar(4000) | |
| LastDentalVisit | datetime | |
| CurrentPainDetails | nvarchar(4000) | |
| PreviousDentalTreatments | nvarchar(4000) | |
| GumProblems | nvarchar(4000) | |
| OralHabits | nvarchar(4000) | |
| CreatedAt | datetime | |
| UpdatedAt | datetime | |

---

## PatientBillingAccounts

| Column | Type | Key |
|--------|------|-----|
| Id | int | PK |
| PatientId | int | FK, UK → Patients |
| BaseTreatmentCost | decimal(12,2) | |
| AdditionalCosts | decimal(12,2) | |
| DiscountType | int | |
| DiscountPercent | decimal(9,4) | |
| DiscountFixedAmount | decimal(12,2) | |
| CreatedAt | datetime | |
| UpdatedAt | datetime | |

---

## Doctors

| Column | Type | Key |
|--------|------|-----|
| Id | int | PK |
| FirstName | varchar(100) | |
| LastName | varchar(100) | |
| Specialty | varchar(200) | |
| LicenseNumber | varchar(80) | |
| Email | varchar(256) | |
| Phone | varchar(30) | |
| BirthDate | date | |
| ApplicationUserId | varchar(450) | FK, UK → AspNetUsers |
| CreatedAt | datetime | |
| UpdatedAt | datetime | |

---

## DoctorAvailabilities

| Column | Type | Key |
|--------|------|-----|
| Id | int | PK |
| DoctorId | int | FK → Doctors |
| DayOfWeek | int | |
| StartTime | time | |
| EndTime | time | |
| CreatedAt | datetime | |
| UpdatedAt | datetime | |

---

## Staff

| Column | Type | Key |
|--------|------|-----|
| Id | int | PK |
| FirstName | varchar(100) | |
| LastName | varchar(100) | |
| Email | varchar(256) | |
| Phone | varchar(30) | |
| BirthDate | date | |
| CreatedAt | datetime | |
| UpdatedAt | datetime | |

---

## Appointments

| Column | Type | Key |
|--------|------|-----|
| Id | int | PK |
| PatientId | int | FK → Patients |
| DoctorId | int | FK → Doctors |
| StaffId | int | FK → Staff |
| AppointmentTime | datetime | |
| DurationMinutes | int | |
| Status | int | |
| CreatedAt | datetime | |
| UpdatedAt | datetime | |

---

## Payments

| Column | Type | Key |
|--------|------|-----|
| Id | int | PK |
| PatientId | int | FK → Patients |
| AppointmentId | int | FK → Appointments |
| Amount | decimal(12,2) | |
| PaymentDate | datetime | |
| Method | int | |
| Status | int | |
| CreatedAt | datetime | |
| UpdatedAt | datetime | |

---

## Prescriptions

| Column | Type | Key |
|--------|------|-----|
| Id | int | PK |
| AppointmentId | int | FK, UK → Appointments |
| Notes | nvarchar(2000) | |
| CreatedAt | datetime | |
| UpdatedAt | datetime | |

---

## PrescriptionItems

| Column | Type | Key |
|--------|------|-----|
| Id | int | PK |
| PrescriptionId | int | FK → Prescriptions |
| MedicationName | nvarchar(200) | |
| Dosage | nvarchar(100) | |
| Duration | nvarchar(100) | |
| Instructions | nvarchar(500) | |
| CreatedAt | datetime | |
| UpdatedAt | datetime | |

---

## GummySmileCases

| Column | Type | Key |
|--------|------|-----|
| Id | int | PK |
| PatientId | int | FK → Patients |
| GsCaseId | varchar(64) | UK |
| GsStatus | varchar(32) | |
| DiagnosisSummary | nvarchar(500) | |
| Severity | nvarchar(50) | |
| ReportJson | nvarchar(max) | |
| ReportPdf | varbinary(max) | |
| ClinicalDraftJson | nvarchar(max) | |
| ClinicalRequestJson | nvarchar(max) | |
| OverridesRequestJson | nvarchar(max) | |
| CaseSnapshotJson | nvarchar(max) | |
| RestImage | varbinary(max) | |
| RestImageContentType | varchar(128) | |
| SmileImage | varbinary(max) | |
| SmileImageContentType | varchar(128) | |
| CachedAssetsJson | nvarchar(max) | |
| CompletedAt | datetime | |
| CreatedAt | datetime | |
| UpdatedAt | datetime | |

---

## XRayAnalysisCases

| Column | Type | Key |
|--------|------|-----|
| Id | int | PK |
| PatientId | int | FK → Patients |
| Summary | nvarchar(500) | |
| CvmStage | nvarchar(32) | |
| SkeletalClass | nvarchar(64) | |
| LandmarkCount | int | |
| ConfidencePercent | int | |
| PanoramicImage | varbinary(max) | |
| PanoramicImageContentType | varchar(128) | |
| CephImage | varbinary(max) | |
| CephImageContentType | varchar(128) | |
| ResultsJson | nvarchar(max) | |
| CompletedAt | datetime | |
| CreatedAt | datetime | |
| UpdatedAt | datetime | |
