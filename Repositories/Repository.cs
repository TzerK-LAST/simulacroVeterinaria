using Microsoft.EntityFrameworkCore;
using VeterinaraApp.Data;

namespace VeterinaraApp.Repositories;

/// <summary>
/// Repositorio genérico: encapsula el acceso directo a EF Core.
/// Los Services usan esto en lugar de tocar el DbContext directamente.
/// </summary>
public class Repository<T> where T : class
{
    protected readonly VetDbContext _db;
    protected readonly DbSet<T> _set;

    public Repository(VetDbContext db)
    {
        _db = db;
        _set = db.Set<T>();
    }

    public List<T> GetAll() => _set.ToList();

    public T? GetById(int id) => _set.Find(id);

    public void Add(T entity)
    {
        _set.Add(entity);
        _db.SaveChanges();
    }

    public void Update(T entity)
    {
        _set.Update(entity);
        _db.SaveChanges();
    }

    public void Delete(T entity)
    {
        _set.Remove(entity);
        _db.SaveChanges();
    }
}
