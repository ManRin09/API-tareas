using ProjectManagerApi.Application.DTOs;
using ProjectManagerApi.Domain.Enums;

namespace ProjectManagerApi.Application.Interfaces;

/// <summary>
/// Contrato de casos de uso para la gestión de tareas.
/// </summary>
public interface ITareaService
{
    Task<TareaResponseDto> GetByIdAsync(int id);

    Task<IReadOnlyList<TareaResponseDto>> GetByProyectoIdAsync(int proyectoId);

    Task<IReadOnlyList<TareaResponseDto>> GetFilteredAsync(EstadoTarea? estado, PrioridadTarea? prioridad);

    Task<IReadOnlyList<TareaResponseDto>> GetTareasVencidasAsync();

    Task<TareaResponseDto> CreateAsync(CreateTareaDto dto);

    Task<TareaResponseDto> UpdateAsync(int id, UpdateTareaDto dto);

    Task<TareaResponseDto> CambiarEstadoAsync(int id, EstadoTarea nuevoEstado);

    Task DeleteAsync(int id);
}
