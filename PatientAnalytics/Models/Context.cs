using PatientAnalytics.Models.PatientMetrics;

namespace PatientAnalytics.Models;
using Microsoft.EntityFrameworkCore;

public class Context : DbContext
{
    public Context(DbContextOptions<Context> options) : base(options)
    {
        
    }
    
    public virtual DbSet<User> Users { get; private set; } = default!;
    public virtual DbSet<Patient> Patients { get; private set; } = default!;
    public virtual DbSet<PatientTemperature> PatientTemperatures { get; private set; } = default!;
    public virtual DbSet<PatientBloodPressure> PatientBloodPressures { get; private set; } = default!;
    public virtual DbSet<PatientHeight> PatientHeights { get; private set; } = default!;
    public virtual DbSet<PatientWeight> PatientWeights { get; private set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(user =>
        {
            user.HasKey(e => e.Id);
        });

        modelBuilder.Entity<Patient>()
            .HasMany(e => e.BloodPressures)
            .WithOne(e => e.Patient)
            .HasForeignKey(e => e.PatientId)
            .IsRequired(false);
        
        modelBuilder.Entity<Patient>()
            .HasMany(e => e.Temperatures)
            .WithOne(e => e.Patient)
            .HasForeignKey(e => e.PatientId)
            .IsRequired(false);
        
        modelBuilder.Entity<Patient>()
            .HasMany(e => e.Heights)
            .WithOne(e => e.Patient)
            .HasForeignKey(e => e.PatientId)
            .IsRequired(false);
      
        modelBuilder.Entity<Patient>()
            .HasMany(e => e.Weights)
            .WithOne(e => e.Patient)
            .HasForeignKey(e => e.PatientId)
            .IsRequired(false);

        modelBuilder.Entity<PatientBloodPressure>()
            .Ignore(e => e.Patient)
            .HasOne(e => e.Doctor)
            .WithMany()
            .HasForeignKey(e => e.DoctorId);

        modelBuilder.Entity<PatientTemperature>()
            .Ignore(e => e.Patient)
            .HasOne(e => e.Doctor)
            .WithMany()
            .HasForeignKey(e => e.DoctorId);
        
        modelBuilder.Entity<PatientHeight>()
            .Ignore(e => e.Patient)
            .HasOne(e => e.Doctor)
            .WithMany()
            .HasForeignKey(e => e.DoctorId);
        
        modelBuilder.Entity<PatientWeight>()
            .Ignore(e => e.Patient)
            .HasOne(e => e.Doctor)
            .WithMany()
            .HasForeignKey(e => e.DoctorId);
    }
}
