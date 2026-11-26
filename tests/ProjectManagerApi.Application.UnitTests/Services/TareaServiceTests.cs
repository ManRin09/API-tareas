using FluentAssertions;
using Moq;
using ProjectManagerApi.Application.DTOs;
using ProjectManagerApi.Application.Services;
using ProjectManagerApi.Domain.Entities;
using ProjectManagerApi.Domain.Enums;
using ProjectManagerApi.Domain.Exceptions;
using ProjectManagerApi.Domain.Interfaces;

namespace ProjectManagerApi.Application.UnitTests.Services;

public class TareaServiceTests
{
    private readonly Mock<ITareaRepository> _tareaRepositoryMock = new();
    private readonly Mock<IProyectoRepository> _proyectoRepositoryMock = new();
    private readonly TareaService _sut;

    public TareaServiceTests()
    {
        _sut = new TareaService(_tareaRepositoryMock.Object, _proyectoRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_CuandoProyectoNoExiste_DebeLanzarNotFoundException()
    {
        // Arrange
        var dto = new CreateTareaDto
        {
            Titulo = "Implementar autenticación",
            Prioridad = PrioridadTarea.Alta,
            FechaLimite = DateTime.UtcNow.AddDays(5),
            ProyectoId = 404
        };

        _proyectoRepositoryMock
            .Setup(r => r.ExistsAsync(dto.ProyectoId))
            .ReturnsAsync(false);

        // Act
        var act = () => _sut.CreateAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"No se encontró el proyecto con ID {dto.ProyectoId}.");

        _tareaRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Tarea>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_CuandoProyectoExiste_DebeCrearYRetornarTarea()
    {
        // Arrange
        var dto = new CreateTareaDto
        {
            Titulo = "Diseñar esquema DB",
            Descripcion = "Tablas en PostgreSQL",
            Estado = EstadoTarea.Pendiente,
            Prioridad = PrioridadTarea.Alta,
            FechaLimite = DateTime.UtcNow.AddDays(3),
            ProyectoId = 1
        };

        _proyectoRepositoryMock.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        _tareaRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Tarea>()))
            .ReturnsAsync((Tarea t) =>
            {
                t.Id = 7;
                t.Proyecto = new Proyecto { Id = 1, Nombre = "Backend API" };
                return t;
            });

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.Id.Should().Be(7);
        result.Titulo.Should().Be("Diseñar esquema DB");
        result.Estado.Should().Be(EstadoTarea.Pendiente);
        result.Prioridad.Should().Be(PrioridadTarea.Alta);
        result.NombreProyecto.Should().Be("Backend API");
    }

    [Fact]
    public async Task CambiarEstadoAsync_CuandoTareaExiste_DebeActualizarEstado()
    {
        // Arrange
        const int tareaId = 5;
        var tarea = new Tarea
        {
            Id = tareaId,
            Titulo = "Escribir pruebas",
            Estado = EstadoTarea.Pendiente,
            Prioridad = PrioridadTarea.Media,
            FechaLimite = DateTime.UtcNow.AddDays(2),
            ProyectoId = 1,
            Proyecto = new Proyecto { Id = 1, Nombre = "Backend API" }
        };

        _tareaRepositoryMock.Setup(r => r.GetByIdAsync(tareaId)).ReturnsAsync(tarea);

        // Act
        var result = await _sut.CambiarEstadoAsync(tareaId, EstadoTarea.Completada);

        // Assert
        result.Estado.Should().Be(EstadoTarea.Completada);
        _tareaRepositoryMock.Verify(
            r => r.UpdateAsync(It.Is<Tarea>(t => t.Id == tareaId && t.Estado == EstadoTarea.Completada)),
            Times.Once);
    }

    [Fact]
    public async Task CambiarEstadoAsync_CuandoTareaNoExiste_DebeLanzarNotFoundException()
    {
        // Arrange
        _tareaRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Tarea?)null);

        // Act
        var act = () => _sut.CambiarEstadoAsync(999, EstadoTarea.EnProgreso);

        // Assert
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("No se encontró la tarea con ID 999.");

        _tareaRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Tarea>()), Times.Never);
    }

    [Fact]
    public async Task GetTareasVencidasAsync_DebeRetornarTareasVencidasNoCompletadas()
    {
        // Arrange
        var tareasVencidas = new List<Tarea>
        {
            new()
            {
                Id = 1,
                Titulo = "Tarea vencida pendiente",
                Estado = EstadoTarea.Pendiente,
                Prioridad = PrioridadTarea.Alta,
                FechaLimite = DateTime.UtcNow.AddDays(-2),
                ProyectoId = 1,
                Proyecto = new Proyecto { Id = 1, Nombre = "Core" }
            },
            new()
            {
                Id = 2,
                Titulo = "Tarea vencida en progreso",
                Estado = EstadoTarea.EnProgreso,
                Prioridad = PrioridadTarea.Media,
                FechaLimite = DateTime.UtcNow.AddHours(-5),
                ProyectoId = 1,
                Proyecto = new Proyecto { Id = 1, Nombre = "Core" }
            }
        };

        _tareaRepositoryMock
            .Setup(r => r.GetOverdueUncompletedAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(tareasVencidas);

        // Act
        var result = await _sut.GetTareasVencidasAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(t => t.Estado != EstadoTarea.Completada);
        _tareaRepositoryMock.Verify(
            r => r.GetOverdueUncompletedAsync(It.Is<DateTime>(d => d.Kind == DateTimeKind.Utc)),
            Times.Once);
    }

    [Fact]
    public async Task GetByProyectoIdAsync_CuandoProyectoNoExiste_DebeLanzarNotFoundException()
    {
        // Arrange
        _proyectoRepositoryMock.Setup(r => r.ExistsAsync(123)).ReturnsAsync(false);

        // Act
        var act = () => _sut.GetByProyectoIdAsync(123);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _tareaRepositoryMock.Verify(r => r.GetByProyectoIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_CuandoProyectoDestinoNoExiste_DebeLanzarNotFoundException()
    {
        // Arrange
        var tarea = new Tarea { Id = 1, Titulo = "Original", ProyectoId = 1 };
        var dto = new UpdateTareaDto
        {
            Titulo = "Actualizada",
            Estado = EstadoTarea.EnProgreso,
            Prioridad = PrioridadTarea.Baja,
            FechaLimite = DateTime.UtcNow.AddDays(4),
            ProyectoId = 999
        };

        _tareaRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(tarea);
        _proyectoRepositoryMock.Setup(r => r.ExistsAsync(999)).ReturnsAsync(false);

        // Act
        var act = () => _sut.UpdateAsync(1, dto);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _tareaRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Tarea>()), Times.Never);
    }
}
