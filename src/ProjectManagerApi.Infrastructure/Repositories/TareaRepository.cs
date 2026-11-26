using Microsoft.EntityFrameworkCore;
using ProjectManagerApi.Domain.Entities;
using ProjectManagerApi.Domain.Enums;
using ProjectManagerApi.Domain.Interfaces;
using ProjectManagerApi.Infrastructure.Persistence;

namespace ProjectManagerApi.Infrastructure.Repositories;

/// <summary>
/// Implementación EF Core del repositorio de tareas.
/// </summary>
public class TareaRepository(AppDbContext context) : ITareaRepository
{
    private readonly AppDbContext _context = context;

    public async Task<Tarea?> GetByIdAsync(int id)
    {
        return await _context.Tareas
            .AsNoTracking()
            .Include(t => t.Proyecto)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IReadOnlyList<Tarea>> GetByProyectoIdAsync(int proyectoId)
    {
        return await _context.Tareas
            .AsNoTracking()
            .Include(t => t.Proyecto)
            .Where(t => t.ProyectoId == proyectoId)
            .OrderBy(t => t.FechaLimite)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Tarea>> GetFilteredAsync(
        EstadoTarea? estado,
        PrioridadTarea? prioridad)
    {
        var query = _context.Tareas
            .AsNoTracking()
            .Include(t => t.Proyecto)
            .AsQueryable();

        if (estado.HasValue)
        {
            query = query.Where(t => t.Estado == estado.Value);
        }

        if (prioridad.HasValue)
        {
            query = query.Where(t => t.Prioridad == prioridad.Value);
        }

        return await query
            .OrderBy(t => t.FechaLimite)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Tarea>> GetOverdueUncompletedAsync(DateTime utcNow)
    {
        return await _context.Tareas
            .AsNoTracking()
            .Include(t => t.Proyecto)
            .Where(t => t.FechaLimite < utcNow && t.Estado != EstadoTarea.Completada)
            .OrderBy(t => t.FechaLimite)
            .ToListAsync();
    }

    public async Task<Tarea> AddAsync(Tarea tarea)
    {
        await _context.Tareas.AddAsync(tarea);
        await _context.SaveChangesAsync();
        await _context.Entry(tarea).Reference(t => t.Proyecto).LoadAsync();
        return tarea;
    }

    public async Task UpdateAsync(Tarea tarea)
    {
        var existing = await _context.Tareas.FirstOrDefaultAsync(t => t.Id == tarea.Id);
        if (existing is not null)
        {
            _context.Entry(existing).CurrentValues.SetValues(tarea);
            await _context.SaveChangesAsync();

            tarea.Proyecto = await _context.Proyectos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == existing.ProyectoId) ?? null!;
            return;
        }

        tarea.Proyecto = null!;
        var entry = _context.Entry(tarea);
        entry.State = EntityState.Modified;
        await _context.SaveChangesAsync();
        await _context.Entry(tarea).Reference(t => t.Proyecto).LoadAsync();
    }

    public async Task DeleteAsync(Tarea tarea)
    {
        var local = _context.Tareas.Local.FirstOrDefault(t => t.Id == tarea.Id);
        if (local is not null)
        {
            _context.Tareas.Remove(local);
        }
        else
        {
            var stub = new Tarea { Id = tarea.Id };
            _context.Tareas.Attach(stub);
            _context.Tareas.Remove(stub);
        }

        await _context.SaveChangesAsync();
    }
}
