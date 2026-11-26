using Microsoft.AspNetCore.Mvc;
using ProjectManagerApi.Application.DTOs;
using ProjectManagerApi.Application.Interfaces;
using ProjectManagerApi.Domain.Enums;

namespace ProjectManagerApi.API.Controllers;

/// <summary>
/// Controlador REST para gestionar tareas, filtros, vencimientos y cambios de estado.
/// </summary>
[ApiController]
[Route("api/tareas")]
[Produces("application/json")]
public class TareasController(ITareaService tareaService) : ControllerBase
{
    private readonly ITareaService _tareaService = tareaService;

    /// <summary>
    /// Lista tareas con filtro opcional por estado y/o prioridad.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TareaResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TareaResponseDto>>> GetFiltered(
        [FromQuery] EstadoTarea? estado,
        [FromQuery] PrioridadTarea? prioridad)
    {
        var tareas = await _tareaService.GetFilteredAsync(estado, prioridad);
        return Ok(tareas);
    }

    /// <summary>
    /// Obtiene las tareas cuya fecha límite ya venció y aún no están completadas.
    /// </summary>
    [HttpGet("vencidas")]
    [ProducesResponseType(typeof(IReadOnlyList<TareaResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TareaResponseDto>>> GetVencidas()
    {
        var tareas = await _tareaService.GetTareasVencidasAsync();
        return Ok(tareas);
    }

    /// <summary>
    /// Obtiene una tarea específica por su identificador.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TareaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TareaResponseDto>> GetById(int id)
    {
        var tarea = await _tareaService.GetByIdAsync(id);
        return Ok(tarea);
    }

    /// <summary>
    /// Crea una nueva tarea asociada a un proyecto existente.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TareaResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TareaResponseDto>> Create([FromBody] CreateTareaDto dto)
    {
        var creada = await _tareaService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = creada.Id }, creada);
    }

    /// <summary>
    /// Actualiza completamente los datos de una tarea existente.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(TareaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TareaResponseDto>> Update(
        int id,
        [FromBody] UpdateTareaDto dto)
    {
        var actualizada = await _tareaService.UpdateAsync(id, dto);
        return Ok(actualizada);
    }

    /// <summary>
    /// Cambia únicamente el estado de una tarea existente.
    /// </summary>
    [HttpPatch("{id:int}/estado")]
    [ProducesResponseType(typeof(TareaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TareaResponseDto>> CambiarEstado(
        int id,
        [FromBody] CambiarEstadoTareaDto dto)
    {
        var actualizada = await _tareaService.CambiarEstadoAsync(id, dto.Estado);
        return Ok(actualizada);
    }

    /// <summary>
    /// Elimina una tarea existente por su identificador.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _tareaService.DeleteAsync(id);
        return NoContent();
    }
}
