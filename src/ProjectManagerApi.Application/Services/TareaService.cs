using ProjectManagerApi.Application.DTOs;
using ProjectManagerApi.Application.Interfaces;
using ProjectManagerApi.Application.Mappings;
using ProjectManagerApi.Domain.Enums;
using ProjectManagerApi.Domain.Exceptions;
using ProjectManagerApi.Domain.Interfaces;

namespace ProjectManagerApi.Application.Services;

/// <summary>
/// Servicio de aplicación encargado de las operaciones y reglas de negocio de tareas.
/// </summary>
public class TareaService(
    ITareaRepository tareaRepository,
    IProyectoRepository proyectoRepository) : ITareaService
{
    private readonly ITareaRepository _tareaRepository = tareaRepository;
    private readonly IProyectoRepository _proyectoRepository = proyectoRepository;

    public async Task<TareaResponseDto> GetByIdAsync(int id)
    {
        var tarea = await _tareaRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"No se encontró la tarea con ID {id}.");

        return tarea.ToResponseDto();
    }

    public async Task<IReadOnlyList<TareaResponseDto>> GetByProyectoIdAsync(int proyectoId)
    {
        await EnsureProyectoExistsAsync(proyectoId);

        var tareas = await _tareaRepository.GetByProyectoIdAsync(proyectoId);
        return tareas.Select(t => t.ToResponseDto()).ToList();
    }

    public async Task<IReadOnlyList<TareaResponseDto>> GetFilteredAsync(
        EstadoTarea? estado,
        PrioridadTarea? prioridad)
    {
        var tareas = await _tareaRepository.GetFilteredAsync(estado, prioridad);
        return tareas.Select(t => t.ToResponseDto()).ToList();
    }

    public async Task<IReadOnlyList<TareaResponseDto>> GetTareasVencidasAsync()
    {
        var tareas = await _tareaRepository.GetOverdueUncompletedAsync(DateTime.UtcNow);
        return tareas.Select(t => t.ToResponseDto()).ToList();
    }

    public async Task<TareaResponseDto> CreateAsync(CreateTareaDto dto)
    {
        await EnsureProyectoExistsAsync(dto.ProyectoId);

        var tarea = dto.ToEntity();
        var creada = await _tareaRepository.AddAsync(tarea);

        return creada.ToResponseDto();
    }

    public async Task<TareaResponseDto> UpdateAsync(int id, UpdateTareaDto dto)
    {
        var tarea = await _tareaRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"No se encontró la tarea con ID {id}.");

        await EnsureProyectoExistsAsync(dto.ProyectoId);

        tarea.ApplyUpdate(dto);
        await _tareaRepository.UpdateAsync(tarea);

        return tarea.ToResponseDto();
    }

    public async Task<TareaResponseDto> CambiarEstadoAsync(int id, EstadoTarea nuevoEstado)
    {
        var tarea = await _tareaRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"No se encontró la tarea con ID {id}.");

        tarea.Estado = nuevoEstado;
        await _tareaRepository.UpdateAsync(tarea);

        return tarea.ToResponseDto();
    }

    public async Task DeleteAsync(int id)
    {
        var tarea = await _tareaRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"No se encontró la tarea con ID {id}.");

        await _tareaRepository.DeleteAsync(tarea);
    }

    private async Task EnsureProyectoExistsAsync(int proyectoId)
    {
        if (!await _proyectoRepository.ExistsAsync(proyectoId))
        {
            throw new NotFoundException($"No se encontró el proyecto con ID {proyectoId}.");
        }
    }
}
