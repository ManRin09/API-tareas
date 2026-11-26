using System.ComponentModel.DataAnnotations;
using ProjectManagerApi.Domain.Enums;

namespace ProjectManagerApi.Application.DTOs;

/// <summary>
/// Datos de entrada para actualizar completamente una tarea existente.
/// </summary>
public class UpdateTareaDto
{
    [Required(ErrorMessage = "El título de la tarea es obligatorio.")]
    [MaxLength(150, ErrorMessage = "El título de la tarea no puede superar los 150 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "La descripción no puede superar los 1000 caracteres.")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "El estado es obligatorio.")]
    [EnumDataType(typeof(EstadoTarea), ErrorMessage = "El estado especificado no es válido.")]
    public EstadoTarea Estado { get; set; }

    [Required(ErrorMessage = "La prioridad es obligatoria.")]
    [EnumDataType(typeof(PrioridadTarea), ErrorMessage = "La prioridad especificada no es válida.")]
    public PrioridadTarea Prioridad { get; set; }

    [Required(ErrorMessage = "La fecha límite es obligatoria.")]
    public DateTime FechaLimite { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El ID del proyecto debe ser mayor que cero.")]
    public int ProyectoId { get; set; }
}
