using Microsoft.EntityFrameworkCore;
using VeterinaraApp.Data;
using VeterinaraApp.Models;

namespace VeterinaraApp.Services;

public class OwnerService
{
    private readonly VetDbContext _db;

    public OwnerService(VetDbContext db) => _db = db;

    // ── Propietarios ─────────────────────────────────────────────

    public List<Owner> GetAllOwners() =>
        _db.Owners.Include(o => o.Pets).ToList();

    public Owner GetOwnerById(int id) =>
        _db.Owners.Include(o => o.Pets).FirstOrDefault(o => o.Id == id)
        ?? throw new Exception($"Propietario con ID {id} no encontrado.");

    public Owner CreateOwner(string name, string document, string phone, string email)
    {
        if (_db.Owners.Any(o => o.Document == document))
            throw new Exception("Ya existe un propietario con ese documento.");
        if (_db.Owners.Any(o => o.Email == email))
            throw new Exception("Ya existe un propietario con ese email.");

        var owner = new Owner { Name = name, Document = document, Phone = phone, Email = email };
        _db.Owners.Add(owner);
        _db.SaveChanges();
        return owner;
    }

    public Owner UpdateOwner(int id, string name, string phone, string email)
    {
        var owner = GetOwnerById(id);
        if (_db.Owners.Any(o => o.Email == email && o.Id != id))
            throw new Exception("Ese email ya está en uso por otro propietario.");

        owner.Name  = name;
        owner.Phone = phone;
        owner.Email = email;
        _db.SaveChanges();
        return owner;
    }

    public void DeleteOwner(int id)
    {
        var owner = GetOwnerById(id);
        if (owner.Pets.Any())
            throw new Exception("No se puede eliminar un propietario con mascotas registradas.");
        _db.Owners.Remove(owner);
        _db.SaveChanges();
    }

    // ── Mascotas ──────────────────────────────────────────────────

    public List<Pet> GetPetsByOwner(int ownerId) =>
        _db.Pets.Where(p => p.OwnerId == ownerId).ToList();

    public Pet GetPetById(int id) =>
        _db.Pets.Include(p => p.Owner).FirstOrDefault(p => p.Id == id)
        ?? throw new Exception($"Mascota con ID {id} no encontrada.");

    public Pet CreatePet(int ownerId, string name, string species, string breed, int age, double weight)
    {
        var owner = GetOwnerById(ownerId);
        var pet = new Pet
        {
            Name     = name,
            Species  = species,
            Breed    = breed,
            Age      = age,
            Weight   = weight,
            OwnerId  = ownerId
        };
        _db.Pets.Add(pet);
        _db.SaveChanges();
        return pet;
    }

    public Pet UpdatePet(int id, string name, string species, string breed, int age, double weight)
    {
        var pet    = GetPetById(id);
        pet.Name   = name;
        pet.Species = species;
        pet.Breed  = breed;
        pet.Age    = age;
        pet.Weight = weight;
        _db.SaveChanges();
        return pet;
    }

    public void DeletePet(int id)
    {
        var pet = GetPetById(id);
        _db.Pets.Remove(pet);
        _db.SaveChanges();
    }
}
