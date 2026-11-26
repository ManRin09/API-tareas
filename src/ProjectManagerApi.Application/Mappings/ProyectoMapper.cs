using ProjectManagerApi.Application.DTOs;
using ProjectManagerApi.Domain.Entities;
using ProjectManagerApi.Domain.Enums;

namespace ProjectManagerApi.Application.Mappings;

/// <summary>
/// Extensiones de mapeo entre la entidad <see cref="Proyecto"/> y sus DTOs.
/// </summary>
public static class ProyectoMapper
{
    public static Proyecto ToEntity(this CreateProyectoDto dto) => new()
    {
        Nombre = dto.Nombre.Trim(),
        Descripcion = dto.Descripcion?.Trim(),
        FechaInicio = dto.FechaInicio,
        FechaFin = dto.FechaFin
    };

    public static void ApplyUpdate(this Proyecto proyecto, UpdateProyectoDto dto)
    {
        proyecto.Nombre = dto.Nombre.Trim();
        proyecto.Descripcion = dto.Descripcion?.Trim();
        proyecto.FechaInicio = dto.FechaInicio;
        proyecto.FechaFin = dto.FechaFin;
    }

    public static ProyectoResponseDto ToResponseDto(this Proyecto proyecto)
    {
        var totalTareas = proyecto.Tareas?.Count ?? 0;
        var tareasCompletadas = proyecto.Tareas?.Count(t => t.Estado == EstadoTarea.Completada) ?? 0;

        return new ProyectoResponseDto
        {
            Id = proyecto.Id,
            Nombre = proyecto.Nombre,
            Descripcion = proyecto.Descripcion,
            FechaInicio = proyecto.FechaInicio,
            FechaFin = proyecto.FechaFin,
            TotalTareas = totalTareas,
            TareasCompletadas = tareasCompletadas,
            PorcentajeAvance = proyecto.CalcularPorcentajeAvance()
        };
    }
}
