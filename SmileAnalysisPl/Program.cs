using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmileAnalysisBl.Configuration;
using SmileAnalysisBl.DentalGemma;
using SmileAnalysisBl.GummySmile;
using SmileAnalysisBl.XRayAnalysis;
using SmileAnalysisBl.Services.Classes;
using SmileAnalysisBl.Services.Interfaces;
using SmileAnalysisDal.Data.Contexts;
using SmileAnalysisDal.Data.DataSeed;
using SmileAnalysisDal.Entities;
using SmileAnalysisDal.Repositories.Classes;
using SmileAnalysisDal.Repositories.Interfaces;

namespace SmileAnalysisPl;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllersWithViews();
        builder.Services.AddRequestTimeouts();
        builder.Services.Configure<ClinicOptions>(
            builder.Configuration.GetSection(ClinicOptions.SectionName));
        builder.Services.Configure<GummySmileApiOptions>(
            builder.Configuration.GetSection(GummySmileApiOptions.SectionName));
        builder.Services.AddHttpClient<IGummySmileApiService, GummySmileApiService>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<GummySmileApiOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromMinutes(5);
        });
        builder.Services.Configure<XRayAnalysisApiOptions>(
            builder.Configuration.GetSection(XRayAnalysisApiOptions.SectionName));
        builder.Services.AddHttpClient<IXRayAnalysisApiService, XRayAnalysisApiService>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<XRayAnalysisApiOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromMinutes(15);
        });
        builder.Services.Configure<DentalGemmaApiOptions>(
            builder.Configuration.GetSection(DentalGemmaApiOptions.SectionName));
        builder.Services.AddHttpClient<IDentalGemmaApiService, DentalGemmaApiService>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<DentalGemmaApiOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromMinutes(5);
        });
        builder.Services.AddScoped<IDentalGemmaService, DentalGemmaService>();
        builder.Services.AddScoped<IDentalGemmaCaseService, DentalGemmaCaseService>();
        builder.Services.AddDbContext<SmileAnalysisDbContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
        });
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
        builder.Services.AddScoped<IPatientService, PatientService>();
        builder.Services.AddScoped<IGummySmileCaseService, GummySmileCaseService>();
        builder.Services.AddScoped<IXRayAnalysisCaseService, XRayAnalysisCaseService>();
        builder.Services.AddScoped<IDoctorService, DoctorService>();
        builder.Services.AddScoped<IStaffService, StaffService>();
        builder.Services.AddScoped<IAppointmentService, AppointmentService>();
        builder.Services.AddScoped<IPaymentService, PaymentService>();
        builder.Services.AddScoped<IAccountService, AccountService>();
        builder.Services.AddScoped<IAdminService, AdminService>();
        builder.Services.AddScoped<IPrescriptionService, PrescriptionService>();
        builder.Services.AddScoped<IPrescriptionPdfService, PrescriptionPdfService>();
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(config =>
            {
                config.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<SmileAnalysisDbContext>()
            .AddDefaultTokenProviders();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<SmileAnalysisDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var pendingMigrations = dbContext.Database.GetPendingMigrations();
            if (pendingMigrations?.Any() ?? false)
                dbContext.Database.Migrate();
            SmileAnalysisDataSeeding.SeedData(dbContext);
            IdentityDbContextSeeding.SeedData(roleManager, userManager, app.Configuration);
        }

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseRequestTimeouts();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapStaticAssets();
        app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}")
            .WithStaticAssets();

        app.Run();
    }
}
