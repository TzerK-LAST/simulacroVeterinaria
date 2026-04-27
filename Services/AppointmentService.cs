using Microsoft.EntityFrameworkCore;
using VeterinaraApp.Data;
using VeterinaraApp.Models;

namespace VeterinaraApp.Services;

public class AppointmentService
{
    private readonly VetDbContext  _db;
    private readonly EmailService  _emailService;

    public AppointmentService(VetDbContext db, EmailService emailService)
    {
        _db           = db;
        _emailService = emailService;
    }

    public List<Appointment> GetAll() =>
        _db.Appointments
           .Include(a => a.Pet)
           .Include(a => a.Owner)
           .Include(a => a.Vet)
           .OrderBy(a => a.Date).ThenBy(a => a.StartTime)
           .ToList();

    public Appointment GetById(int id) =>
        _db.Appointments
           .Include(a => a.Pet).ThenInclude(p => p.Owner)
           .Include(a => a.Vet)
           .FirstOrDefault(a => a.Id == id)
        ?? throw new Exception($"Cita con ID {id} no encontrada.");

    // ── Crear ─────────────────────────────────────────────────────
    public Appointment Create(int petId, int ownerId, int vetId,
                              DateTime date, TimeSpan start, TimeSpan end)
    {
        var pet = _db.Pets.Include(p => p.Owner).FirstOrDefault(p => p.Id == petId)
            ?? throw new Exception("Mascota no encontrada.");
        var vet = _db.Vets.Find(vetId)
            ?? throw new Exception("Veterinario no encontrado.");

        var appt = new Appointment
        {
            PetId     = petId,
            OwnerId   = ownerId,
            VetId     = vetId,
            Date      = date.Date,
            StartTime = start,
            EndTime   = end,
            Status    = AppointmentStatus.Programada
        };

        Validate(appt, vet);

        _db.Appointments.Add(appt);
        _db.SaveChanges();

        _emailService.SendAppointmentCreated(pet.Owner.Email, pet.Name, date, start);
        return appt;
    }

    // ── Cancelar ──────────────────────────────────────────────────
    public void Cancel(int id)
    {
        var appt = GetById(id);
        if (appt.Status != AppointmentStatus.Programada)
            throw new Exception("Solo se pueden cancelar citas con estado 'Programada'.");

        appt.Status = AppointmentStatus.Cancelada;
        _db.SaveChanges();

        _emailService.SendAppointmentCancelled(appt.Pet.Owner.Email, appt.Pet.Name, appt.Date);
    }

    // ── Actualizar estado ─────────────────────────────────────────
    public void UpdateStatus(int id, AppointmentStatus newStatus)
    {
        var appt = GetById(id);
        if (appt.Status == AppointmentStatus.Cancelada)
            throw new Exception("No se puede cambiar el estado de una cita cancelada.");

        appt.Status = newStatus;
        _db.SaveChanges();
    }

    // ── Historial de citas por mascota ────────────────────────────
    public List<Appointment> GetByPet(int petId) =>
        _db.Appointments
           .Include(a => a.Vet)
           .Include(a => a.Treatment)
           .Where(a => a.PetId == petId)
           .OrderByDescending(a => a.Date)
           .ToList();

    // ── Validaciones centralizadas ────────────────────────────────
    private void Validate(Appointment appt, Vet vet)
    {
        // 1. No fechas pasadas
        if (appt.Date.Date < DateTime.Today)
            throw new Exception("No se pueden agendar citas en fechas pasadas.");

        // 2. Hora fin > hora inicio
        if (appt.EndTime <= appt.StartTime)
            throw new Exception("La hora fin debe ser mayor a la hora inicio.");

        // 3. Veterinario dentro de su horario de atención
        if (appt.StartTime < vet.WorkStart || appt.EndTime > vet.WorkEnd)
            throw new Exception(
                $"El veterinario atiende de {vet.WorkStart:hh\\:mm} a {vet.WorkEnd:hh\\:mm}.");

        // 4. Sin solapamiento de citas por veterinario
        bool vetBusy = _db.Appointments
            .Where(a => a.VetId     == appt.VetId
                     && a.Date      == appt.Date
                     && a.Status    == AppointmentStatus.Programada)
            .Any(a => appt.StartTime < a.EndTime && appt.EndTime > a.StartTime);
        if (vetBusy)
            throw new Exception("El veterinario ya tiene una cita en ese horario.");

        // 5. Sin solapamiento de citas por mascota
        bool petBusy = _db.Appointments
            .Where(a => a.PetId  == appt.PetId
                     && a.Date   == appt.Date
                     && a.Status == AppointmentStatus.Programada)
            .Any(a => appt.StartTime < a.EndTime && appt.EndTime > a.StartTime);
        if (petBusy)
            throw new Exception("La mascota ya tiene una cita en ese horario.");

        // 6. Máximo 2 citas activas por mascota
        int activePet = _db.Appointments
            .Count(a => a.PetId == appt.PetId && a.Status == AppointmentStatus.Programada);
        if (activePet >= 2)
            throw new Exception("La mascota ya tiene 2 citas activas programadas.");

        // 7. Bloqueo por 3 inasistencias en los últimos 7 días
        int noShows = _db.Appointments
            .Count(a => a.PetId  == appt.PetId
                     && a.Status == AppointmentStatus.NoAsistio
                     && a.Date   >= DateTime.Today.AddDays(-7));
        if (noShows >= 3)
            throw new Exception("La mascota está bloqueada: 3 inasistencias en los últimos 7 días.");
    }
}
