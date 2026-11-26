using Microsoft.AspNetCore.Mvc;
using ProjectManagerApi.Application.DTOs;
using ProjectManagerApi.Application.Interfaces;

namespace ProjectManagerApi.API.Controllers;

/// <summary>
/// Controlador REST para gestionar proyectos y consultar sus tareas asociadas.
/// </summary>
[ApiController]
[Route("api/proyectos")]
[Produces("application/json")]
public class ProyectosController(
    IProyectoService proyectoService,
    ITareaService tareaService) : ControllerBase
{
    private readonly IProyectoService _proyectoService = proyectoService;
    private readonly ITareaService _tareaService = tareaService;

    /// <summary>
    /// Obtiene todos los proyectos registrados con su porcentaje de avance.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProyectoResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProyectoResponseDto>>> GetAll()
    {
        var proyectos = await _proyectoService.GetAllAsync();
        return Ok(proyectos);
    }

    /// <summary>
    /// Obtiene un proyecto específico por su identificador.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProyectoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProyectoResponseDto>> GetById(int id)
    {
        var proyecto = await _proyectoService.GetByIdAsync(id);
        return Ok(proyecto);
    }

    /// <summary>
    /// Crea un nuevo proyecto.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProyectoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProyectoResponseDto>> Create([FromBody] CreateProyectoDto dto)
    {
        var creado = await _proyectoService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
    }

    /// <summary>
    /// Actualiza los datos de un proyecto existente.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProyectoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProyectoResponseDto>> Update(
        int id,
        [FromBody] UpdateProyectoDto dto)
    {
        var actualizado = await _proyectoService.UpdateAsync(id, dto);
        return Ok(actualizado);
    }

    /// <summary>
    /// Elimina un proyecto siempre que no tenga tareas pendientes.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        await _proyectoService.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Obtiene todas las tareas asociadas a un proyecto específico.
    /// </summary>
    [HttpGet("{id:int}/tareas")]
    [ProducesResponseType(typeof(IReadOnlyList<TareaResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<TareaResponseDto>>> GetTareasByProyecto(int id)
    {
        var tareas = await _tareaService.GetByProyectoIdAsync(id);
        return Ok(tareas);
    }
}
