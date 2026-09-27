using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SafetyOps.Api.Data;
using SafetyOps.Api.Features.Common;
using Scalar.AspNetCore;
using SafetyOps.Api.Features.MedicalSurveillance;
using SafetyOps.Api.Features.Personnel;
using SafetyOps.Api.Features.Training;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
    {
        // Validation errors are keyed by JSON property name (camelCase), matching the request body.
        options.ModelMetadataDetailsProviders.Add(new SystemTextJsonValidationMetadataProvider());
        // Required-ness is declared explicitly with [Required]; a malformed body shouldn't also report "request is required".
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    })
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new IsoDateOnlyConverter()));
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(ResolveSqliteConnectionString(builder.Configuration, builder.Environment)));
builder.Services.AddScoped<IPersonnelService, PersonnelService>();
builder.Services.AddScoped<ITrainingService, TrainingService>();
builder.Services.AddScoped<IMedicalSurveillanceService, MedicalSurveillanceService>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Database:SeedDemoData"))
    {
        await DemoDataSeeder.SeedAsync(db, scope.ServiceProvider.GetRequiredService<TimeProvider>());
    }
}

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseDefaultFiles();
app.MapStaticAssets();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Unknown API routes get a 404 problem response; everything else falls through to the SPA.
app.MapFallback("/api/{**path}", () => Results.Problem(statusCode: StatusCodes.Status404NotFound));
app.MapFallbackToFile("/index.html");

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
