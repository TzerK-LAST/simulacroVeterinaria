using Microsoft.EntityFrameworkCore;
using VeterinaraApp.Models;

namespace VeterinaraApp.Data;

public class VetDbContext : DbContext
{
    public DbSet<Owner> Owners { get; set; }
    public DbSet<Pet> Pets { get; set; }
    public DbSet<Vet> Vets { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Treatment> Treatments { get; set; }
    public DbSet<Medication> Medications { get; set; }
    public DbSet<TreatmentMedication> TreatmentMedications { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseNpgsql("Host=localhost;Database=veterinaria;Username=postgres;Password=postgres");
        // MySQL: options.UseMySql("server=localhost;database=veterinaria;user=root;password=root",
        //     ServerVersion.AutoDetect("server=localhost;database=veterinaria;user=root;password=root"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Owner>()
            .HasIndex(o => o.Document).IsUnique();
        modelBuilder.Entity<Owner>()
            .HasIndex(o => o.Email).IsUnique();

        modelBuilder.Entity<Vet>()
            .HasIndex(v => new { v.Name, v.Specialty }).IsUnique();

        modelBuilder.Entity<Pet>()
            .HasOne(p => p.Owner).WithMany(o => o.Pets).HasForeignKey(p => p.OwnerId);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Pet).WithMany(p => p.Appointments).HasForeignKey(a => a.PetId);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Vet).WithMany(v => v.Appointments).HasForeignKey(a => a.VetId);

        modelBuilder.Entity<Treatment>()
            .HasOne(t => t.Appointment).WithOne(a => a.Treatment)
            .HasForeignKey<Treatment>(t => t.AppointmentId);

        modelBuilder.Entity<TreatmentMedication>()
            .HasOne(tm => tm.Treatment).WithMany(t => t.TreatmentMedications)
            .HasForeignKey(tm => tm.TreatmentId);

        modelBuilder.Entity<TreatmentMedication>()
            .HasOne(tm => tm.Medication).WithMany()
            .HasForeignKey(tm => tm.MedicationId);
    }
}
