using ProjectManagerApi.Domain.Enums;

namespace ProjectManagerApi.Application.DTOs;

/// <summary>
/// Representación de salida de una tarea.
/// </summary>
public class TareaResponseDto
{
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public EstadoTarea Estado { get; set; }

    public PrioridadTarea Prioridad { get; set; }

    public DateTime FechaLimite { get; set; }

    public int ProyectoId { get; set; }

    public string? NombreProyecto { get; set; }
}
