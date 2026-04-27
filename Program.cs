using System.Globalization;
using VeterinaraApp.Data;
using VeterinaraApp.Models;
using VeterinaraApp.Services;

// ── Inyección de dependencias manual ────────────────────────────
var db             = new VetDbContext();
db.Database.EnsureCreated();

var emailService   = new EmailService();
var ownerService   = new OwnerService(db);
var vetService     = new VetService(db);
var apptService    = new AppointmentService(db, emailService);
var treatService   = new TreatmentService(db, emailService);
var reportService  = new ReportService(db);

// ── Helpers de lectura ───────────────────────────────────────────
static string Ask(string label)
{
    Console.Write($"{label}: ");
    return Console.ReadLine()!.Trim();
}
static int AskInt(string label)
{
    Console.Write($"{label}: ");
    return int.Parse(Console.ReadLine()!.Trim());
}
static DateTime AskDate(string label)
    => DateTime.ParseExact(Ask(label + " (dd/MM/yyyy)"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
static TimeSpan AskTime(string label)
    => TimeSpan.Parse(Ask(label + " (HH:mm)"));

// ── Menú principal ───────────────────────────────────────────────
bool running = true;
while (running)
{
    Console.WriteLine("""

    ╔══════════════════════════════════════╗
    ║     SISTEMA VETERINARIA (Capas)      ║
    ╠══════════════════════════════════════╣
    ║  PROPIETARIOS & MASCOTAS             ║
    ║   1. Listar propietarios             ║
    ║   2. Crear propietario               ║
    ║   3. Actualizar propietario          ║
    ║   4. Eliminar propietario            ║
    ║   5. Listar mascotas de propietario  ║
    ║   6. Agregar mascota                 ║
    ║   7. Actualizar mascota              ║
    ║   8. Eliminar mascota                ║
    ║  VETERINARIOS                        ║
    ║   9. Listar veterinarios             ║
    ║  10. Crear veterinario               ║
    ║  11. Actualizar veterinario          ║
    ║  12. Eliminar veterinario            ║
    ║  CITAS                               ║
    ║  13. Listar citas                    ║
    ║  14. Crear cita                      ║
    ║  15. Cancelar cita                   ║
    ║  16. Actualizar estado cita          ║
    ║  TRATAMIENTOS & HISTORIAL            ║
    ║  17. Listar medicamentos             ║
    ║  18. Agregar medicamento             ║
    ║  19. Registrar tratamiento           ║
    ║  20. Ver historial de mascota        ║
    ║  REPORTES                            ║
    ║  21. Reporte completo                ║
    ║  22. Vet con más citas               ║
    ║  23. Mascotas más atendidas          ║
    ║  24. Medicamentos más usados         ║
    ║  25. Tasa de inasistencia            ║
    ║   0. Salir                           ║
    ╚══════════════════════════════════════╝
    """);

    Console.Write("Opción: ");

    try
    {
        switch (Console.ReadLine()!.Trim())
        {
            // ── Propietarios ──────────────────────────────────
            case "1":
                foreach (var o in ownerService.GetAllOwners())
                {
                    Console.WriteLine($"\n  [{o.Id}] {o.Name} | Doc: {o.Document} | Tel: {o.Phone} | Email: {o.Email}");
                    foreach (var p in o.Pets)
                        Console.WriteLine($"       Mascota: [{p.Id}] {p.Name} ({p.Species})");
                }
                break;

            case "2":
                var newOwner = ownerService.CreateOwner(
                    Ask("Nombre"), Ask("Documento"), Ask("Teléfono"), Ask("Email"));
                Console.WriteLine($"Propietario creado — ID: {newOwner.Id}");
                break;

            case "3":
                var updOwner = ownerService.UpdateOwner(
                    AskInt("ID propietario"), Ask("Nuevo nombre"), Ask("Nuevo teléfono"), Ask("Nuevo email"));
                Console.WriteLine($"Propietario {updOwner.Id} actualizado.");
                break;

            case "4":
                ownerService.DeleteOwner(AskInt("ID propietario"));
                Console.WriteLine("Propietario eliminado.");
                break;

            // ── Mascotas ──────────────────────────────────────
            case "5":
                foreach (var p in ownerService.GetPetsByOwner(AskInt("ID propietario")))
                    Console.WriteLine($"  [{p.Id}] {p.Name} — {p.Species} ({p.Breed}) | Edad: {p.Age} | Peso: {p.Weight}kg");
                break;

            case "6":
                var newPet = ownerService.CreatePet(
                    AskInt("ID propietario"), Ask("Nombre"), Ask("Especie"),
                    Ask("Raza"), AskInt("Edad"), double.Parse(Ask("Peso (kg)")));
                Console.WriteLine($"Mascota creada — ID: {newPet.Id}");
                break;

            case "7":
                var updPet = ownerService.UpdatePet(
                    AskInt("ID mascota"), Ask("Nombre"), Ask("Especie"),
                    Ask("Raza"), AskInt("Edad"), double.Parse(Ask("Peso (kg)")));
                Console.WriteLine($"Mascota {updPet.Id} actualizada.");
                break;

            case "8":
                ownerService.DeletePet(AskInt("ID mascota"));
                Console.WriteLine("Mascota eliminada.");
                break;

            // ── Veterinarios ──────────────────────────────────
            case "9":
                foreach (var v in vetService.GetAll())
                    Console.WriteLine($"  [{v.Id}] {v.Name} | {v.Specialty} | {v.WorkStart:hh\\:mm}-{v.WorkEnd:hh\\:mm}");
                break;

            case "10":
                var newVet = vetService.Create(
                    Ask("Nombre"), Ask("Especialidad"), AskTime("Hora inicio"), AskTime("Hora fin"));
                Console.WriteLine($"Veterinario creado — ID: {newVet.Id}");
                break;

            case "11":
                var updVet = vetService.Update(
                    AskInt("ID veterinario"), Ask("Nombre"), Ask("Especialidad"),
                    AskTime("Hora inicio"), AskTime("Hora fin"));
                Console.WriteLine($"Veterinario {updVet.Id} actualizado.");
                break;

            case "12":
                vetService.Delete(AskInt("ID veterinario"));
                Console.WriteLine("Veterinario eliminado.");
                break;

            // ── Citas ─────────────────────────────────────────
            case "13":
                foreach (var a in apptService.GetAll())
                    Console.WriteLine($"  [{a.Id}] {a.Date:dd/MM/yyyy} {a.StartTime:hh\\:mm}-{a.EndTime:hh\\:mm} | Mascota: {a.Pet.Name} | Vet: {a.Vet.Name} | {a.Status}");
                break;

            case "14":
                var newAppt = apptService.Create(
                    AskInt("ID mascota"), AskInt("ID propietario"), AskInt("ID veterinario"),
                    AskDate("Fecha"), AskTime("Hora inicio"), AskTime("Hora fin"));
                Console.WriteLine($"Cita creada — ID: {newAppt.Id}");
                break;

            case "15":
                apptService.Cancel(AskInt("ID cita"));
                Console.WriteLine("Cita cancelada.");
                break;

            case "16":
                Console.WriteLine("Estado: 1) Atendida  2) NoAsistio  3) Programada");
                var newStatus = Console.ReadLine() switch
                {
                    "1" => AppointmentStatus.Atendida,
                    "2" => AppointmentStatus.NoAsistio,
                    "3" => AppointmentStatus.Programada,
                    _   => throw new Exception("Opción inválida.")
                };
                apptService.UpdateStatus(AskInt("ID cita"), newStatus);
                Console.WriteLine($"Estado actualizado a {newStatus}.");
                break;

            // ── Medicamentos ──────────────────────────────────
            case "17":
                foreach (var m in treatService.GetAllMedications())
                    Console.WriteLine($"  [{m.Id}] {m.Name} | Dosis: {m.Dose} | Frec: {m.Frequency} | Stock: {m.Stock}");
                break;

            case "18":
                var newMed = treatService.CreateMedication(
                    Ask("Nombre"), Ask("Dosis"), Ask("Frecuencia"), AskInt("Stock inicial"));
                Console.WriteLine($"Medicamento creado — ID: {newMed.Id}");
                break;

            // ── Tratamientos & Historial ──────────────────────
            case "19":
                Console.Write("IDs medicamentos (separados por coma): ");
                var medIds = Console.ReadLine()!.Split(',')
                    .Select(s => int.Parse(s.Trim())).ToList();
                var newTreat = treatService.Create(
                    AskInt("ID cita"), Ask("Diagnóstico"), Ask("Observaciones"), medIds);
                Console.WriteLine($"Tratamiento registrado — ID: {newTreat.Id}");
                break;

            case "20":
                treatService.PrintHistory(AskInt("ID mascota"));
                break;

            // ── Reportes ──────────────────────────────────────
            case "21": reportService.FullReport();                break;
            case "22": reportService.VetWithMostAppointments();   break;
            case "23": reportService.MostAttendedPets();          break;
            case "24": reportService.MostUsedMedications();       break;
            case "25": reportService.AbsenceRate();               break;

            case "0":
                running = false;
                break;

            default:
                Console.WriteLine("Opción inválida.");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n  ⚠ Error: {ex.Message}");
    }
}

Console.WriteLine("\n¡Hasta luego!");
