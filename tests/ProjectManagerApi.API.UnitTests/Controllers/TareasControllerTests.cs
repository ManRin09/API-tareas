using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProjectManagerApi.API.Controllers;
using ProjectManagerApi.Application.DTOs;
using ProjectManagerApi.Application.Interfaces;
using ProjectManagerApi.Domain.Enums;

namespace ProjectManagerApi.API.UnitTests.Controllers;

public class TareasControllerTests
{
    private readonly Mock<ITareaService> _tareaServiceMock = new();
    private readonly TareasController _sut;

    public TareasControllerTests()
    {
        _sut = new TareasController(_tareaServiceMock.Object);
    }

    [Fact]
    public async Task GetFiltered_DebePasarParametrosYRetornarOk()
    {
        // Arrange
        var tareas = new List<TareaResponseDto>
        {
            new() { Id = 1, Titulo = "T1", Estado = EstadoTarea.EnProgreso, Prioridad = PrioridadTarea.Alta }
        };

        _tareaServiceMock
            .Setup(s => s.GetFilteredAsync(EstadoTarea.EnProgreso, PrioridadTarea.Alta))
            .ReturnsAsync(tareas);

        // Act
        var result = await _sut.GetFiltered(EstadoTarea.EnProgreso, PrioridadTarea.Alta);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(tareas);
    }

    [Fact]
    public async Task GetVencidas_DebeRetornarOkConTareasVencidas()
    {
        // Arrange
        var vencidas = new List<TareaResponseDto>
        {
            new() { Id = 2, Titulo = "Vencida", Estado = EstadoTarea.Pendiente }
        };

        _tareaServiceMock.Setup(s => s.GetTareasVencidasAsync()).ReturnsAsync(vencidas);

        // Act
        var result = await _sut.GetVencidas();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(vencidas);
    }

    [Fact]
    public async Task CambiarEstado_DebeRetornarOkConEstadoActualizado()
    {
        // Arrange
        var actualizada = new TareaResponseDto
        {
            Id = 5,
            Titulo = "Despliegue",
            Estado = EstadoTarea.Completada
        };

        _tareaServiceMock
            .Setup(s => s.CambiarEstadoAsync(5, EstadoTarea.Completada))
            .ReturnsAsync(actualizada);

        // Act
        var result = await _sut.CambiarEstado(5, new CambiarEstadoTareaDto { Estado = EstadoTarea.Completada });

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(actualizada);
    }
}
