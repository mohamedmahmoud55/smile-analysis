using SmileAnalysisDal.Data.Contexts;
using SmileAnalysisDal.Entities;

namespace SmileAnalysisDal.Data.DataSeed;

public static class SmileAnalysisDataSeeding
{
    public static void SeedData(SmileAnalysisDbContext dbContext)
    {
        if (!dbContext.Doctors.Any())
        {
            dbContext.Doctors.AddRange(
                new Doctor
                {
                    FirstName = "Abdallah",
                    LastName = "Mohamed",
                    Specialty = "Orthodontics",
                    LicenseNumber = "ORT-0001",
                    Email = "abdallah.mohamed@clinic.com",
                    Phone = "+1 555-0101",
                    BirthDate = new DateOnly(1980, 3, 15),
                    CreatedAt = DateTime.UtcNow
                },
                new Doctor
                {
                    FirstName = "Amr",
                    LastName = "Gamal",
                    Specialty = "Periodontics",
                    LicenseNumber = "PER-0001",
                    Email = "amr.gamal@clinic.com",
                    Phone = "+1 555-0102",
                    BirthDate = new DateOnly(1985, 11, 2),
                    CreatedAt = DateTime.UtcNow
                });
        }

        if (!dbContext.StaffMembers.Any())
        {
            dbContext.StaffMembers.Add(new Staff
            {
                FirstName = "Mahmoud",
                LastName = "Azab",
                Email = "mahmoud.azab@clinic.com",
                Phone = "+1 555-0201",
                BirthDate = new DateOnly(1992, 7, 22),
                CreatedAt = DateTime.UtcNow
            });
        }

        dbContext.SaveChanges();
    }
}
