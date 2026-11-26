using ProjectManagerApi.Domain.Entities;
using ProjectManagerApi.Domain.Enums;

namespace ProjectManagerApi.Domain.Interfaces;

/// <summary>
/// Contrato de acceso a datos para la entidad <see cref="Tarea"/>.
/// </summary>
public interface ITareaRepository
{
    Task<Tarea?> GetByIdAsync(int id);

    Task<IReadOnlyList<Tarea>> GetByProyectoIdAsync(int proyectoId);

    Task<IReadOnlyList<Tarea>> GetFilteredAsync(EstadoTarea? estado, PrioridadTarea? prioridad);

    Task<IReadOnlyList<Tarea>> GetOverdueUncompletedAsync(DateTime utcNow);

    Task<Tarea> AddAsync(Tarea tarea);

    Task UpdateAsync(Tarea tarea);

    Task DeleteAsync(Tarea tarea);
}
