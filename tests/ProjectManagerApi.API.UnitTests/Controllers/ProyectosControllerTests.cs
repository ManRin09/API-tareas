using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProjectManagerApi.API.Controllers;
using ProjectManagerApi.Application.DTOs;
using ProjectManagerApi.Application.Interfaces;
using ProjectManagerApi.Domain.Enums;

namespace ProjectManagerApi.API.UnitTests.Controllers;

public class ProyectosControllerTests
{
    private readonly Mock<IProyectoService> _proyectoServiceMock = new();
    private readonly Mock<ITareaService> _tareaServiceMock = new();
    private readonly ProyectosController _sut;

    public ProyectosControllerTests()
    {
        _sut = new ProyectosController(_proyectoServiceMock.Object, _tareaServiceMock.Object);
    }

    [Fact]
    public async Task Create_DebeRetornarCreatedAtActionConProyectoCreado()
    {
        // Arrange
        var input = new CreateProyectoDto
        {
            Nombre = "Proyecto A",
            FechaInicio = DateTime.UtcNow
        };
        var expected = new ProyectoResponseDto
        {
            Id = 42,
            Nombre = "Proyecto A",
            FechaInicio = input.FechaInicio
        };

        _proyectoServiceMock.Setup(s => s.CreateAsync(input)).ReturnsAsync(expected);

        // Act
        var result = await _sut.Create(input);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(ProyectosController.GetById));
        createdResult.RouteValues!["id"].Should().Be(42);
        createdResult.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Delete_DebeRetornarNoContent()
    {
        // Arrange
        _proyectoServiceMock.Setup(s => s.DeleteAsync(1)).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Delete(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _proyectoServiceMock.Verify(s => s.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetTareasByProyecto_DebeRetornarOkConListaDeTareas()
    {
        // Arrange
        var tareas = new List<TareaResponseDto>
        {
            new()
            {
                Id = 1,
                Titulo = "Tarea 1",
                Estado = EstadoTarea.Pendiente,
                Prioridad = PrioridadTarea.Alta,
                ProyectoId = 3
            }
        };

        _tareaServiceMock.Setup(s => s.GetByProyectoIdAsync(3)).ReturnsAsync(tareas);

        // Act
        var result = await _sut.GetTareasByProyecto(3);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(tareas);
    }
}
