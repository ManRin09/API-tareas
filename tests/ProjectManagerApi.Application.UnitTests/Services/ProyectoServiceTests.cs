using FluentAssertions;
using Moq;
using ProjectManagerApi.Application.DTOs;
using ProjectManagerApi.Application.Services;
using ProjectManagerApi.Domain.Entities;
using ProjectManagerApi.Domain.Enums;
using ProjectManagerApi.Domain.Exceptions;
using ProjectManagerApi.Domain.Interfaces;

namespace ProjectManagerApi.Application.UnitTests.Services;

public class ProyectoServiceTests
{
    private readonly Mock<IProyectoRepository> _proyectoRepositoryMock = new();
    private readonly ProyectoService _sut;

    public ProyectoServiceTests()
    {
        _sut = new ProyectoService(_proyectoRepositoryMock.Object);
    }

    [Fact]
    public async Task DeleteAsync_CuandoProyectoTieneTareasPendientes_DebeLanzarBusinessRuleException()
    {
        // Arrange
        const int proyectoId = 1;
        var proyecto = new Proyecto { Id = proyectoId, Nombre = "API Core" };

        _proyectoRepositoryMock
            .Setup(r => r.GetByIdWithTareasAsync(proyectoId))
            .ReturnsAsync(proyecto);
        _proyectoRepositoryMock
            .Setup(r => r.HasPendingTasksAsync(proyectoId))
            .ReturnsAsync(true);

        // Act
        var act = () => _sut.DeleteAsync(proyectoId);

        // Assert
        await act.Should()
            .ThrowAsync<BusinessRuleException>()
            .WithMessage("No se puede eliminar un proyecto con tareas pendientes.");

        _proyectoRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Proyecto>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_CuandoNoTieneTareasPendientes_DebeEliminarProyecto()
    {
        // Arrange
        const int proyectoId = 2;
        var proyecto = new Proyecto { Id = proyectoId, Nombre = "API Completada" };

        _proyectoRepositoryMock
            .Setup(r => r.GetByIdWithTareasAsync(proyectoId))
            .ReturnsAsync(proyecto);
        _proyectoRepositoryMock
            .Setup(r => r.HasPendingTasksAsync(proyectoId))
            .ReturnsAsync(false);

        // Act
        await _sut.DeleteAsync(proyectoId);

        // Assert
        _proyectoRepositoryMock.Verify(r => r.DeleteAsync(proyecto), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_CuandoProyectoNoExiste_DebeLanzarNotFoundException()
    {
        // Arrange
        _proyectoRepositoryMock
            .Setup(r => r.GetByIdWithTareasAsync(99))
            .ReturnsAsync((Proyecto?)null);

        // Act
        var act = () => _sut.DeleteAsync(99);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _proyectoRepositoryMock.Verify(r => r.HasPendingTasksAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_CuandoFechaFinEsMenorAFechaInicio_DebeLanzarBusinessRuleException()
    {
        // Arrange
        var dto = new CreateProyectoDto
        {
            Nombre = "Proyecto Inválido",
            FechaInicio = new DateTime(2026, 12, 10, 0, 0, 0, DateTimeKind.Utc),
            FechaFin = new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var act = () => _sut.CreateAsync(dto);

        // Assert
        await act.Should()
            .ThrowAsync<BusinessRuleException>()
            .WithMessage("La fecha de fin no puede ser anterior a la fecha de inicio.");

        _proyectoRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Proyecto>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_CuandoFechaFinEsMenorAFechaInicio_DebeLanzarBusinessRuleException()
    {
        // Arrange
        var dto = new UpdateProyectoDto
        {
            Nombre = "Proyecto Inválido",
            FechaInicio = new DateTime(2026, 12, 10, 0, 0, 0, DateTimeKind.Utc),
            FechaFin = new DateTime(2026, 12, 5, 0, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var act = () => _sut.UpdateAsync(1, dto);

        // Assert
        await act.Should()
            .ThrowAsync<BusinessRuleException>()
            .WithMessage("La fecha de fin no puede ser anterior a la fecha de inicio.");

        _proyectoRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Proyecto>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ConDatosValidos_DebeCrearYRetornarProyectoResponseDto()
    {
        // Arrange
        var fechaInicio = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc);
        var fechaFin = new DateTime(2026, 11, 30, 0, 0, 0, DateTimeKind.Utc);
        var dto = new CreateProyectoDto
        {
            Nombre = "  Nuevo Proyecto  ",
            Descripcion = "  Descripción  ",
            FechaInicio = fechaInicio,
            FechaFin = fechaFin
        };

        _proyectoRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Proyecto>()))
            .ReturnsAsync((Proyecto p) =>
            {
                p.Id = 10;
                return p;
            });

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.Id.Should().Be(10);
        result.Nombre.Should().Be("Nuevo Proyecto");
        result.Descripcion.Should().Be("Descripción");
        result.TotalTareas.Should().Be(0);
        result.TareasCompletadas.Should().Be(0);
        result.PorcentajeAvance.Should().Be(0);
    }

    [Fact]
    public async Task GetAllAsync_DebeRetornarProyectosConMetricasCalculadas()
    {
        // Arrange
        var proyectos = new List<Proyecto>
        {
            new()
            {
                Id = 1,
                Nombre = "P1",
                FechaInicio = DateTime.UtcNow,
                Tareas =
                [
                    new Tarea { Id = 1, Estado = EstadoTarea.Completada },
                    new Tarea { Id = 2, Estado = EstadoTarea.Pendiente }
                ]
            }
        };

        _proyectoRepositoryMock
            .Setup(r => r.GetAllWithTareasAsync())
            .ReturnsAsync(proyectos);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].TotalTareas.Should().Be(2);
        result[0].TareasCompletadas.Should().Be(1);
        result[0].PorcentajeAvance.Should().Be(50);
    }
}
