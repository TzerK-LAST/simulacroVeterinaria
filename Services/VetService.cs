using VeterinaraApp.Data;
using VeterinaraApp.Models;

namespace VeterinaraApp.Services;

public class VetService
{
    private readonly VetDbContext _db;

    public VetService(VetDbContext db) => _db = db;

    public List<Vet> GetAll() => _db.Vets.ToList();

    public Vet GetById(int id) =>
        _db.Vets.Find(id) ?? throw new Exception($"Veterinario con ID {id} no encontrado.");

    public Vet Create(string name, string specialty, TimeSpan workStart, TimeSpan workEnd)
    {
        if (_db.Vets.Any(v => v.Name == name && v.Specialty == specialty))
            throw new Exception("Ya existe un veterinario con ese nombre y especialidad.");
        if (workEnd <= workStart)
            throw new Exception("La hora de fin debe ser mayor a la de inicio.");

        var vet = new Vet { Name = name, Specialty = specialty, WorkStart = workStart, WorkEnd = workEnd };
        _db.Vets.Add(vet);
        _db.SaveChanges();
        return vet;
    }

    public Vet Update(int id, string name, string specialty, TimeSpan workStart, TimeSpan workEnd)
    {
        var vet       = GetById(id);
        vet.Name      = name;
        vet.Specialty = specialty;
        vet.WorkStart = workStart;
        vet.WorkEnd   = workEnd;
        _db.SaveChanges();
        return vet;
    }

    public void Delete(int id)
    {
        var vet = GetById(id);
        if (_db.Appointments.Any(a => a.VetId == id && a.Status == AppointmentStatus.Programada))
            throw new Exception("No se puede eliminar un veterinario con citas programadas.");
        _db.Vets.Remove(vet);
        _db.SaveChanges();
    }
}
