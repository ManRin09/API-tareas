using ProjectManagerApi.Domain.Entities;

namespace ProjectManagerApi.Domain.Interfaces;

/// <summary>
/// Contrato de acceso a datos para la entidad <see cref="Proyecto"/>.
/// </summary>
public interface IProyectoRepository
{
    Task<IReadOnlyList<Proyecto>> GetAllWithTareasAsync();

    Task<Proyecto?> GetByIdWithTareasAsync(int id);

    Task<bool> ExistsAsync(int id);

    Task<Proyecto> AddAsync(Proyecto proyecto);

    Task UpdateAsync(Proyecto proyecto);

    Task DeleteAsync(Proyecto proyecto);

    Task<bool> HasPendingTasksAsync(int proyectoId);
}
