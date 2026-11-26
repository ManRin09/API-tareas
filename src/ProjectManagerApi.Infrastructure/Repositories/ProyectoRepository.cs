using Microsoft.EntityFrameworkCore;
using ProjectManagerApi.Domain.Entities;
using ProjectManagerApi.Domain.Enums;
using ProjectManagerApi.Domain.Interfaces;
using ProjectManagerApi.Infrastructure.Persistence;

namespace ProjectManagerApi.Infrastructure.Repositories;

/// <summary>
/// Implementación EF Core del repositorio de proyectos.
/// </summary>
public class ProyectoRepository(AppDbContext context) : IProyectoRepository
{
    private readonly AppDbContext _context = context;

    public async Task<IReadOnlyList<Proyecto>> GetAllWithTareasAsync()
    {
        return await _context.Proyectos
            .AsNoTracking()
            .Include(p => p.Tareas)
            .OrderBy(p => p.Id)
            .ToListAsync();
    }

    public async Task<Proyecto?> GetByIdWithTareasAsync(int id)
    {
        return await _context.Proyectos
            .AsNoTracking()
            .Include(p => p.Tareas)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Proyectos
            .AsNoTracking()
            .AnyAsync(p => p.Id == id);
    }

    public async Task<Proyecto> AddAsync(Proyecto proyecto)
    {
        await _context.Proyectos.AddAsync(proyecto);
        await _context.SaveChangesAsync();
        return proyecto;
    }

    public async Task UpdateAsync(Proyecto proyecto)
    {
        var existing = await _context.Proyectos.FirstOrDefaultAsync(p => p.Id == proyecto.Id);
        if (existing is not null)
        {
            // Copia solo propiedades escalares para evitar conflictos de tracking con la colección Tareas.
            _context.Entry(existing).CurrentValues.SetValues(proyecto);
            await _context.SaveChangesAsync();
            return;
        }

        var entry = _context.Entry(proyecto);
        entry.State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Proyecto proyecto)
    {
        // Elimina primero las tareas completadas restantes antes de borrar el proyecto (FK con DeleteBehavior.Restrict).
        var tareasAsociadas = await _context.Tareas
            .Where(t => t.ProyectoId == proyecto.Id)
            .ToListAsync();

        if (tareasAsociadas.Count > 0)
        {
            _context.Tareas.RemoveRange(tareasAsociadas);
        }

        var local = _context.Proyectos.Local.FirstOrDefault(p => p.Id == proyecto.Id);
        if (local is not null)
        {
            _context.Proyectos.Remove(local);
        }
        else
        {
            // Se adjunta un stub sin colección navegada para evitar conflictos de tracking con DeleteBehavior.Restrict.
            var stub = new Proyecto { Id = proyecto.Id };
            _context.Proyectos.Attach(stub);
            _context.Proyectos.Remove(stub);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasPendingTasksAsync(int proyectoId)
    {
        return await _context.Tareas
            .AsNoTracking()
            .AnyAsync(t => t.ProyectoId == proyectoId && t.Estado != EstadoTarea.Completada);
    }
}
