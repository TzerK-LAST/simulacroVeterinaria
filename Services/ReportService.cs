using Microsoft.EntityFrameworkCore;
using VeterinaraApp.Data;
using VeterinaraApp.Models;

namespace VeterinaraApp.Services;

public class ReportService
{
    private readonly VetDbContext _db;

    public ReportService(VetDbContext db) => _db = db;

    // ── 1. Veterinario con más citas ──────────────────────────────
    public void VetWithMostAppointments()
    {
        var result = _db.Appointments
            .Include(a => a.Vet)
            .GroupBy(a => new { a.VetId, a.Vet.Name, a.Vet.Specialty })
            .OrderByDescending(g => g.Count())
            .Select(g => new { g.Key.Name, g.Key.Specialty, Total = g.Count() })
            .FirstOrDefault();

        Console.WriteLine("\n── Veterinario con más citas ──");
        if (result == null) Console.WriteLine("  Sin datos.");
        else Console.WriteLine($"  {result.Name} ({result.Specialty}): {result.Total} citas");
    }

    // ── 2. Mascotas más atendidas ─────────────────────────────────
    public void MostAttendedPets(int top = 5)
    {
        var results = _db.Appointments
            .Include(a => a.Pet)
            .Where(a => a.Status == AppointmentStatus.Atendida)
            .GroupBy(a => new { a.PetId, a.Pet.Name, a.Pet.Species })
            .OrderByDescending(g => g.Count())
            .Take(top)
            .Select(g => new { g.Key.Name, g.Key.Species, Total = g.Count() })
            .ToList();

        Console.WriteLine($"\n── Top {top} mascotas más atendidas ──");
        if (!results.Any()) { Console.WriteLine("  Sin datos."); return; }
        foreach (var r in results)
            Console.WriteLine($"  {r.Name} ({r.Species}): {r.Total} atenciones");
    }

    // ── 3. Medicamentos más utilizados ───────────────────────────
    public void MostUsedMedications(int top = 5)
    {
        var results = _db.TreatmentMedications
            .Include(tm => tm.Medication)
            .GroupBy(tm => new { tm.MedicationId, tm.Medication.Name })
            .OrderByDescending(g => g.Count())
            .Take(top)
            .Select(g => new { g.Key.Name, Total = g.Count() })
            .ToList();

        Console.WriteLine($"\n── Top {top} medicamentos más usados ──");
        if (!results.Any()) { Console.WriteLine("  Sin datos."); return; }
        foreach (var r in results)
            Console.WriteLine($"  {r.Name}: {r.Total} usos");
    }

    // ── 4. Tasa de inasistencia ───────────────────────────────────
    public void AbsenceRate()
    {
        int total    = _db.Appointments.Count();
        int noShows  = _db.Appointments.Count(a => a.Status == AppointmentStatus.NoAsistio);
        double rate  = total > 0 ? (double)noShows / total * 100 : 0;

        Console.WriteLine("\n── Tasa de inasistencia ──");
        Console.WriteLine($"  Total citas : {total}");
        Console.WriteLine($"  Inasistencias: {noShows}");
        Console.WriteLine($"  Tasa         : {rate:F1}%");
    }

    // ── Resumen general ───────────────────────────────────────────
    public void FullReport()
    {
        Console.WriteLine("\n╔══════════════════════════════════╗");
        Console.WriteLine("║        REPORTE GENERAL           ║");
        Console.WriteLine("╚══════════════════════════════════╝");
        VetWithMostAppointments();
        MostAttendedPets();
        MostUsedMedications();
        AbsenceRate();
    }
}
