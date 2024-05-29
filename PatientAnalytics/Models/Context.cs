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

        modelBuilder.Entity<Patient>(patient =>
        {
            patient.HasKey(e => e.Id);
        });

        modelBuilder.Entity<PatientTemperature>(temperature =>
        {
            temperature.HasKey(e => e.Id);

            temperature
                .HasOne(e => e.Doctor)
                .WithMany()
                .HasForeignKey(e => e.DoctorId);

            temperature
                .HasOne(e => e.Patient)
                .WithMany()
                .HasForeignKey(e => e.PatientId);
        });
        
        modelBuilder.Entity<PatientBloodPressure>(bloodPressure =>
        {
            bloodPressure.HasKey(e => e.Id);

            bloodPressure
                .HasOne(e => e.Doctor)
                .WithMany()
                .HasForeignKey(e => e.DoctorId);

            bloodPressure
                .HasOne(e => e.Patient)
                .WithMany()
                .HasForeignKey(e => e.PatientId);
        });
        
        modelBuilder.Entity<PatientHeight>(height =>
        {
            height.HasKey(e => e.Id);

            height
                .HasOne(e => e.Doctor)
                .WithMany()
                .HasForeignKey(e => e.DoctorId);

            height
                .HasOne(e => e.Patient)
                .WithMany()
                .HasForeignKey(e => e.PatientId);
        });
        
        modelBuilder.Entity<PatientWeight>(weight =>
        {
            weight.HasKey(e => e.Id);

            weight
                .HasOne(e => e.Doctor)
                .WithMany()
                .HasForeignKey(e => e.DoctorId);

            weight
                .HasOne(e => e.Patient)
                .WithMany()
                .HasForeignKey(e => e.PatientId);
        });
    }
}
