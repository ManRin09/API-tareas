using ProjectManagerApi.Application.DTOs;
using ProjectManagerApi.Domain.Entities;

namespace ProjectManagerApi.Application.Mappings;

/// <summary>
/// Extensiones de mapeo entre la entidad <see cref="Tarea"/> y sus DTOs.
/// </summary>
public static class TareaMapper
{
    public static Tarea ToEntity(this CreateTareaDto dto) => new()
    {
        Titulo = dto.Titulo.Trim(),
        Descripcion = dto.Descripcion?.Trim(),
        Estado = dto.Estado,
        Prioridad = dto.Prioridad,
        FechaLimite = dto.FechaLimite,
        ProyectoId = dto.ProyectoId
    };

    public static void ApplyUpdate(this Tarea tarea, UpdateTareaDto dto)
    {
        tarea.Titulo = dto.Titulo.Trim();
        tarea.Descripcion = dto.Descripcion?.Trim();
        tarea.Estado = dto.Estado;
        tarea.Prioridad = dto.Prioridad;
        tarea.FechaLimite = dto.FechaLimite;
        tarea.ProyectoId = dto.ProyectoId;
    }

    public static TareaResponseDto ToResponseDto(this Tarea tarea) => new()
    {
        Id = tarea.Id,
        Titulo = tarea.Titulo,
        Descripcion = tarea.Descripcion,
        Estado = tarea.Estado,
        Prioridad = tarea.Prioridad,
        FechaLimite = tarea.FechaLimite,
        ProyectoId = tarea.ProyectoId,
        NombreProyecto = tarea.Proyecto?.Nombre
    };
}
