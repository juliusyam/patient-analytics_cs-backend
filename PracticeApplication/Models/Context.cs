namespace PracticeApplication.Models;
using Microsoft.EntityFrameworkCore;

public class Context : DbContext
{
    public Context(DbContextOptions<Context> options) : base(options)
    {
        
    }
    
    public virtual DbSet<User> Users { get; set; } = default!;
    public virtual DbSet<Patient> Patients { get; set; } = default!;

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
    }
}