using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SafetyOps.Api.Data;
using SafetyOps.Api.Domain;
using SafetyOps.Api.Features.Auth;
using SafetyOps.Api.Features.Common;
using SafetyOps.Api.Features.Incidents;
using SafetyOps.Api.Features.MedicalSurveillance;
using SafetyOps.Api.Features.Personnel;
using SafetyOps.Api.Features.Training;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
    {
        // Validation errors are keyed by JSON property name (camelCase), matching the request body.
        options.ModelMetadataDetailsProviders.Add(new SystemTextJsonValidationMetadataProvider());
        // Required-ness is declared explicitly with [Required]; a malformed body shouldn't also report "request is required".
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    })
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new IsoDateOnlyConverter());
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(ResolveSqliteConnectionString(builder.Configuration, builder.Environment)));
builder.Services.AddSafetyOpsAuth(builder.Configuration);
builder.Services.AddScoped<IPersonnelService, PersonnelService>();
builder.Services.AddScoped<ITrainingService, TrainingService>();
builder.Services.AddScoped<IMedicalSurveillanceService, MedicalSurveillanceService>();
builder.Services.AddScoped<IIncidentService, IncidentService>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await UserSeeder.SeedAsync(db,
        scope.ServiceProvider.GetRequiredService<IOptions<AuthOptions>>().Value,
        scope.ServiceProvider.GetRequiredService<IPasswordHasher<AppUser>>());
    if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Database:SeedDemoData"))
    {
        await DemoDataSeeder.SeedAsync(db, scope.ServiceProvider.GetRequiredService<TimeProvider>());
    }
}

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseDefaultFiles();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Every endpoint requires a signed-in user (fallback policy) except these and [AllowAnonymous] actions.
app.MapStaticAssets().AllowAnonymous();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference().AllowAnonymous();
}

app.MapControllers();

// Unknown API routes get a 404 problem response; everything else falls through to the SPA.
app.MapFallback("/api/{**path}", () => Results.Problem(statusCode: StatusCodes.Status404NotFound)).AllowAnonymous();
app.MapFallbackToFile("/index.html").AllowAnonymous();

app.Run();

// Relative SQLite paths resolve against the content root (not the working directory),
// and the containing folder is created on first run.
static string ResolveSqliteConnectionString(IConfiguration configuration, IWebHostEnvironment environment)
{
    var csb = new SqliteConnectionStringBuilder(
        configuration.GetConnectionString("Default") ?? "Data Source=App_Data/safetyops.db");

    if (csb.DataSource != ":memory:" && csb.Mode != SqliteOpenMode.Memory && !Path.IsPathRooted(csb.DataSource))
    {
        csb.DataSource = Path.Combine(environment.ContentRootPath, csb.DataSource);
    }
    if (csb.Mode != SqliteOpenMode.Memory && Path.GetDirectoryName(csb.DataSource) is { Length: > 0 } dir)
    {
        Directory.CreateDirectory(dir);
    }
    return csb.ToString();
}

/// <summary>Entry point; public so integration tests can host the app with <c>WebApplicationFactory</c>.</summary>
public partial class Program;
