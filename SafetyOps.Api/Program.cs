using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SafetyOps.Api.Data;
using SafetyOps.Api.Features.Common;
using SafetyOps.Api.Features.MedicalSurveillance;
using SafetyOps.Api.Features.Personnel;
using SafetyOps.Api.Features.Training;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new WireDateOnlyConverter()));
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

app.UseDefaultFiles();
app.MapStaticAssets();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
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
