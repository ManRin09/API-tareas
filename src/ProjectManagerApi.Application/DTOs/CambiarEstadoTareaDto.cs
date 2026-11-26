using System.ComponentModel.DataAnnotations;
using ProjectManagerApi.Domain.Enums;

namespace ProjectManagerApi.Application.DTOs;

/// <summary>
/// Datos de entrada para cambiar únicamente el estado de una tarea.
/// </summary>
public class CambiarEstadoTareaDto
{
    [Required(ErrorMessage = "El estado es obligatorio.")]
    [EnumDataType(typeof(EstadoTarea), ErrorMessage = "El estado especificado no es válido.")]
    public EstadoTarea Estado { get; set; }
}
