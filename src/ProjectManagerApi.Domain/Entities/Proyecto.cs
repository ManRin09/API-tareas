using ProjectManagerApi.Domain.Enums;

namespace ProjectManagerApi.Domain.Entities;

/// <summary>
/// Entidad de dominio que representa un proyecto y agrupa sus tareas asociadas.
/// </summary>
public class Proyecto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public ICollection<Tarea> Tareas { get; set; } = new List<Tarea>();

    /// <summary>
    /// Calcula el porcentaje de avance del proyecto en función de sus tareas completadas.
    /// </summary>
    /// <returns>Porcentaje redondeado a 2 decimales, o 0 si el proyecto no tiene tareas.</returns>
    public double CalcularPorcentajeAvance()
    {
        if (Tareas is null || Tareas.Count == 0)
        {
            return 0;
        }

        var completadas = Tareas.Count(t => t.Estado == EstadoTarea.Completada);
        return Math.Round((double)completadas / Tareas.Count * 100, 2);
    }
}
