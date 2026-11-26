using ProjectManagerApi.Domain.Enums;

namespace ProjectManagerApi.Domain.Entities;

/// <summary>
/// Entidad de dominio que representa una tarea perteneciente a un proyecto.
/// </summary>
public class Tarea
{
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public EstadoTarea Estado { get; set; } = EstadoTarea.Pendiente;

    public PrioridadTarea Prioridad { get; set; } = PrioridadTarea.Media;

    public DateTime FechaLimite { get; set; }

    public int ProyectoId { get; set; }

    public Proyecto Proyecto { get; set; } = null!;
}
