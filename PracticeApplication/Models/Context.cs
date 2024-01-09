namespace PracticeApplication.Models;
using Microsoft.EntityFrameworkCore;

public partial class Context : DbContext
{
    public Context(DbContextOptions<Context> options) : base(options)
    {
        
    }
    
    public virtual DbSet<User> Users { get; set; } = default!;
    public virtual DbSet<Patient> Patients { get; set; } = default!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=PracticeApplication.db");
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(user =>
        {
            user.HasKey(e => e.Id);
            user.Property(e => e.Id).HasColumnType("INT");
            user.Property(e => e.DateOfBirth).HasColumnType("VARCHAR");
            user.Property(e => e.Gender).HasColumnType("VARCHAR");
            user.Property(e => e.Email).HasColumnType("VARCHAR");
            user.Property(e => e.Address).HasColumnType("VARCHAR");
            user.Property(e => e.DateCreated).HasColumnType("VARCHAR");
            user.Property(e => e.DateEdited).HasColumnType("VARCHAR");
        });

        modelBuilder.Entity<Patient>(patient =>
        {
            patient.HasKey(e => e.Id);
        });
        
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}