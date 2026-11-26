using System.ComponentModel.DataAnnotations;

namespace ProjectManagerApi.Application.DTOs;

/// <summary>
/// Datos de entrada para actualizar un proyecto existente.
/// </summary>
public class UpdateProyectoDto
{
    [Required(ErrorMessage = "El nombre del proyecto es obligatorio.")]
    [MaxLength(150, ErrorMessage = "El nombre del proyecto no puede superar los 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "La descripción no puede superar los 1000 caracteres.")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    public DateTime FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }
}
