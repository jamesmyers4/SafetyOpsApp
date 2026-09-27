using Microsoft.EntityFrameworkCore;
using SafetyOps.Api.Domain;

namespace SafetyOps.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Person> People => Set<Person>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<TrainingClass> TrainingClasses => Set<TrainingClass>();
    public DbSet<Stressor> Stressors => Set<Stressor>();
    public DbSet<WorkTask> WorkTasks => Set<WorkTask>();
    public DbSet<MedicalAppointment> MedicalAppointments => Set<MedicalAppointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
