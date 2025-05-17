using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ClassManagementSystem.Data;
using ClassManagementSystem.Data.Models;
using ClassManagementSystem.Data.Dto;
using Serilog;
using Serilog.Events;
using System.Net.Http.Json;

var builder = WebApplication.CreateBuilder(args);

// ─── Serilog Configuration ────────────────────────────────────────────────
// Install-Package Serilog.AspNetCore 
// Install-Package Serilog.Sinks.File
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    // Console for development
    .WriteTo.Console()
    // audit.log captures Information and above
    .WriteTo.File(
        path: "Logs/audit.log",
        restrictedToMinimumLevel: LogEventLevel.Information,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14)
    // error.log captures Error and above
    .WriteTo.File(
        path: "Logs/error.log",
        restrictedToMinimumLevel: LogEventLevel.Error,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .CreateLogger();

builder.Host.UseSerilog();

// ─── EF & Identity ────────────────────────────────────────────────────────
var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// ─── HttpClient for Holidays ───────────────────────────────────────────────
builder.Services.AddHttpClient("HolidayApi", client =>
{
    client.BaseAddress = new Uri("https://date.nager.at/api/v3/");
});

// ─── Authorization Policies & Razor Pages ─────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy",      policy => policy.RequireRole("Admin"));
    options.AddPolicy("InstructorPolicy", policy => policy.RequireRole("Instructor"));
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeAreaFolder("Admin",      "/", "AdminPolicy");
    options.Conventions.AuthorizeAreaFolder("Instructor", "/", "InstructorPolicy");
});

var app = builder.Build();

// ─── Sync Holidays at Startup ─────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var httpFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
    var httpClient  = httpFactory.CreateClient("HolidayApi");
    var db          = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger      = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    async Task SyncHolidaysAsync(int year, string countryCode)
    {
        try
        {
            var list = await httpClient.GetFromJsonAsync<List<HolidayDto>>(
                $"PublicHolidays/{year}/{countryCode}"
            );
            if (list is null) return;

            foreach (var dto in list)
            {
                var dt = DateTime.Parse(dto.date);
                if (!db.Holidays.Any(h => h.Date == dt))
                {
                    db.Holidays.Add(new Holiday { Date = dt, Name = dto.localName });
                    logger.LogInformation("Imported holiday {Holiday} on {Date}", dto.localName, dt);
                }
            }

            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to sync holidays for {Year}/{Country}", year, countryCode);
        }
    }

    await SyncHolidaysAsync(DateTime.UtcNow.Year,             "TR");
    await SyncHolidaysAsync(DateTime.UtcNow.AddYears(1).Year, "TR");
}

// ─── HTTP & Error Handling ────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    // Central error‐handler will write to error.log
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

// Flush and close Serilog on shutdown
app.Lifetime.ApplicationStopped.Register(Log.CloseAndFlush);

app.Run();