using ProjectManagerApi.Application.DTOs;
using ProjectManagerApi.Application.Interfaces;
using ProjectManagerApi.Application.Mappings;
using ProjectManagerApi.Domain.Exceptions;
using ProjectManagerApi.Domain.Interfaces;

namespace ProjectManagerApi.Application.Services;

/// <summary>
/// Servicio de aplicación encargado de las operaciones y reglas de negocio de proyectos.
/// </summary>
public class ProyectoService(IProyectoRepository proyectoRepository) : IProyectoService
{
    private readonly IProyectoRepository _proyectoRepository = proyectoRepository;

    public async Task<IReadOnlyList<ProyectoResponseDto>> GetAllAsync()
    {
        var proyectos = await _proyectoRepository.GetAllWithTareasAsync();
        return proyectos.Select(p => p.ToResponseDto()).ToList();
    }

    public async Task<ProyectoResponseDto> GetByIdAsync(int id)
    {
        var proyecto = await _proyectoRepository.GetByIdWithTareasAsync(id)
            ?? throw new NotFoundException($"No se encontró el proyecto con ID {id}.");

        return proyecto.ToResponseDto();
    }

    public async Task<ProyectoResponseDto> CreateAsync(CreateProyectoDto dto)
    {
        ValidarRangoFechas(dto.FechaInicio, dto.FechaFin);

        var proyecto = dto.ToEntity();
        var creado = await _proyectoRepository.AddAsync(proyecto);

        return creado.ToResponseDto();
    }

    public async Task<ProyectoResponseDto> UpdateAsync(int id, UpdateProyectoDto dto)
    {
        ValidarRangoFechas(dto.FechaInicio, dto.FechaFin);

        var proyecto = await _proyectoRepository.GetByIdWithTareasAsync(id)
            ?? throw new NotFoundException($"No se encontró el proyecto con ID {id}.");

        proyecto.ApplyUpdate(dto);
        await _proyectoRepository.UpdateAsync(proyecto);

        return proyecto.ToResponseDto();
    }

    public async Task DeleteAsync(int id)
    {
        var proyecto = await _proyectoRepository.GetByIdWithTareasAsync(id)
            ?? throw new NotFoundException($"No se encontró el proyecto con ID {id}.");

        if (await _proyectoRepository.HasPendingTasksAsync(id))
        {
            throw new BusinessRuleException("No se puede eliminar un proyecto con tareas pendientes.");
        }

        await _proyectoRepository.DeleteAsync(proyecto);
    }

    private static void ValidarRangoFechas(DateTime fechaInicio, DateTime? fechaFin)
    {
        if (fechaFin.HasValue && fechaFin.Value < fechaInicio)
        {
            throw new BusinessRuleException("La fecha de fin no puede ser anterior a la fecha de inicio.");
        }
    }
}
