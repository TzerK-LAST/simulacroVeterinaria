using Microsoft.EntityFrameworkCore;
using VeterinaraApp.Data;
using VeterinaraApp.Models;

namespace VeterinaraApp.Services;

public class TreatmentService
{
    private readonly VetDbContext _db;
    private readonly EmailService _emailService;

    public TreatmentService(VetDbContext db, EmailService emailService)
    {
        _db           = db;
        _emailService = emailService;
    }

    // ── Registrar tratamiento ─────────────────────────────────────
    public Treatment Create(int appointmentId, string diagnosis,
                            string observations, List<int> medicationIds)
    {
        var appt = _db.Appointments
            .Include(a => a.Pet).ThenInclude(p => p.Owner)
            .Include(a => a.Treatment)
            .FirstOrDefault(a => a.Id == appointmentId)
            ?? throw new Exception("Cita no encontrada.");

        if (appt.Status != AppointmentStatus.Atendida)
            throw new Exception("Solo se pueden registrar tratamientos en citas con estado 'Atendida'.");

        if (appt.Treatment != null)
            throw new Exception("Esta cita ya tiene un tratamiento registrado.");

        // Validar stock de cada medicamento (simulado)
        foreach (var medId in medicationIds)
        {
            var med = _db.Medications.Find(medId)
                ?? throw new Exception($"Medicamento con ID {medId} no encontrado.");
            if (med.Stock <= 0)
                throw new Exception($"El medicamento '{med.Name}' no tiene stock disponible.");
            med.Stock--; // descontar stock
        }

        var treatment = new Treatment
        {
            AppointmentId        = appointmentId,
            Diagnosis            = diagnosis,
            Observations         = observations,
            TreatmentMedications = medicationIds
                .Select(id => new TreatmentMedication { MedicationId = id })
                .ToList()
        };

        _db.Treatments.Add(treatment);
        _db.SaveChanges();

        _emailService.SendTreatmentAssigned(
            appt.Pet.Owner.Email, appt.Pet.Name, diagnosis);

        return treatment;
    }

    // ── Historial clínico por mascota ─────────────────────────────
    public void PrintHistory(int petId)
    {
        var pet = _db.Pets.Find(petId)
            ?? throw new Exception("Mascota no encontrada.");

        var history = _db.Appointments
            .Include(a => a.Vet)
            .Include(a => a.Treatment)
                .ThenInclude(t => t!.TreatmentMedications)
                    .ThenInclude(tm => tm.Medication)
            .Where(a => a.PetId == petId)
            .OrderByDescending(a => a.Date)
            .ToList();

        Console.WriteLine($"\n══════════════════════════════════════════");
        Console.WriteLine($"  HISTORIAL CLÍNICO: {pet.Name}");
        Console.WriteLine($"══════════════════════════════════════════");

        if (!history.Any())
        {
            Console.WriteLine("  Sin historial registrado.");
            return;
        }

        // Usar Dictionary para agrupar por año
        var byYear = history
            .GroupBy(a => a.Date.Year)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var (year, appts) in byYear.OrderByDescending(k => k.Key))
        {
            Console.WriteLine($"\n  ── {year} ─────────────────────────────");
            foreach (var a in appts)
            {
                Console.WriteLine($"  [{a.Id}] {a.Date:dd/MM/yyyy} {a.StartTime:hh\\:mm}-{a.EndTime:hh\\:mm}");
                Console.WriteLine($"       Veterinario: {a.Vet.Name} | Estado: {a.Status}");

                if (a.Treatment != null)
                {
                    Console.WriteLine($"       Diagnóstico: {a.Treatment.Diagnosis}");
                    Console.WriteLine($"       Observaciones: {a.Treatment.Observations}");

                    var meds = a.Treatment.TreatmentMedications;
                    if (meds.Any())
                    {
                        Console.WriteLine("       Medicamentos:");
                        foreach (var tm in meds)
                            Console.WriteLine($"         · {tm.Medication.Name} — {tm.Medication.Dose} ({tm.Medication.Frequency})");
                    }
                }
                Console.WriteLine();
            }
        }
    }

    // ── Medicamentos ──────────────────────────────────────────────
    public List<Medication> GetAllMedications() => _db.Medications.ToList();

    public Medication CreateMedication(string name, string dose, string frequency, int stock)
    {
        var med = new Medication { Name = name, Dose = dose, Frequency = frequency, Stock = stock };
        _db.Medications.Add(med);
        _db.SaveChanges();
        return med;
    }
}
