namespace ProjectManagerApi.Application.DTOs;

/// <summary>
/// Representación de salida de un proyecto con métricas de progreso calculadas.
/// </summary>
public class ProyectoResponseDto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public int TotalTareas { get; set; }

    public int TareasCompletadas { get; set; }

    public double PorcentajeAvance { get; set; }
}
