using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SchoolAdmission.Api.BackgroundServices;
using SchoolAdmission.Api.Endpoints;
using SchoolAdmission.Api.Infrastructure.Data;
using SchoolAdmission.Api.Middleware;
using SchoolAdmission.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Database Configuration
var useInMemory = builder.Configuration.GetValue<bool>("UseInMemoryDatabase") ||
                  string.Equals(Environment.GetEnvironmentVariable("USE_IN_MEMORY_DB"), "true", StringComparison.OrdinalIgnoreCase);

builder.Services.AddDbContext<AdmissionDbContext>(options =>
{
    if (useInMemory)
    {
        options.UseInMemoryDatabase("SchoolAdmissionDb");
    }
    else
    {
        var connectionString = DbConnectionHelper.ResolveConnectionString(builder.Configuration);
        options.UseNpgsql(connectionString);
    }
});

// 2. Service Registrations
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<IHoldService, HoldService>();
builder.Services.AddHostedService<HoldCleanupBackgroundService>();

// 3. CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 4. OpenAPI / Swagger Setup
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "School Admission API",
        Version = "v1",
        Description = "High-concurrency School Admission System API handling temporary seat holds and FCFS admissions."
    });
});

var app = builder.Build();

// 5. Ensure database schema is initialized on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AdmissionDbContext>();
    await db.Database.EnsureCreatedAsync();

    // Seed sample classes if none exist for easy UI testing
    if (!await db.Classes.AnyAsync())
    {
        db.Classes.AddRange(
            new SchoolAdmission.Api.Domain.Entities.Class
            {
                Name = "Class 6 - Morning Shift",
                TotalSeats = 10,
                CreatedAt = DateTime.UtcNow
            },
            new SchoolAdmission.Api.Domain.Entities.Class
            {
                Name = "Class 6 - Day Shift",
                TotalSeats = 15,
                CreatedAt = DateTime.UtcNow
            }
        );
        await db.SaveChangesAsync();
    }
}

// 6. Global Exception Handler Middleware
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// 7. Swagger UI (available for testing in development & container)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "School Admission API v1");
});

app.UseCors();

// 8. Health Check Endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
   .WithName("HealthCheck")
   .WithOpenApi();

// 9. Map Domain Endpoints
app.MapClassEndpoints();
app.MapHoldEndpoints();

app.Run();

// Required for WebApplicationFactory in integration tests
public partial class Program { }
