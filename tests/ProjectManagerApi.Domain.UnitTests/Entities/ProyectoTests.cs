using FluentAssertions;
using ProjectManagerApi.Domain.Entities;
using ProjectManagerApi.Domain.Enums;

namespace ProjectManagerApi.Domain.UnitTests.Entities;

public class ProyectoTests
{
    [Fact]
    public void CalcularPorcentajeAvance_CuandoNoTieneTareas_DebeRetornarCero()
    {
        // Arrange
        var proyecto = new Proyecto
        {
            Id = 1,
            Nombre = "Proyecto vacío",
            FechaInicio = DateTime.UtcNow
        };

        // Act
        var avance = proyecto.CalcularPorcentajeAvance();

        // Assert
        avance.Should().Be(0);
    }

    [Fact]
    public void CalcularPorcentajeAvance_CuandoLaMitadEstanCompletadas_DebeRetornarCincuenta()
    {
        // Arrange
        var proyecto = new Proyecto
        {
            Id = 1,
            Nombre = "Proyecto al 50%",
            FechaInicio = DateTime.UtcNow,
            Tareas =
            [
                new Tarea { Id = 1, Titulo = "T1", Estado = EstadoTarea.Completada },
                new Tarea { Id = 2, Titulo = "T2", Estado = EstadoTarea.Pendiente },
                new Tarea { Id = 3, Titulo = "T3", Estado = EstadoTarea.Completada },
                new Tarea { Id = 4, Titulo = "T4", Estado = EstadoTarea.EnProgreso }
            ]
        };

        // Act
        var avance = proyecto.CalcularPorcentajeAvance();

        // Assert
        avance.Should().Be(50);
    }

    [Fact]
    public void CalcularPorcentajeAvance_CuandoTodasEstanCompletadas_DebeRetornarCien()
    {
        // Arrange
        var proyecto = new Proyecto
        {
            Id = 1,
            Nombre = "Proyecto finalizado",
            FechaInicio = DateTime.UtcNow,
            Tareas =
            [
                new Tarea { Id = 1, Titulo = "T1", Estado = EstadoTarea.Completada },
                new Tarea { Id = 2, Titulo = "T2", Estado = EstadoTarea.Completada }
            ]
        };

        // Act
        var avance = proyecto.CalcularPorcentajeAvance();

        // Assert
        avance.Should().Be(100);
    }

    [Fact]
    public void CalcularPorcentajeAvance_ConFraccionPeriodica_DebeRedondearADosDecimales()
    {
        // Arrange
        var proyecto = new Proyecto
        {
            Id = 1,
            Nombre = "Proyecto 1/3",
            FechaInicio = DateTime.UtcNow,
            Tareas =
            [
                new Tarea { Id = 1, Titulo = "T1", Estado = EstadoTarea.Completada },
                new Tarea { Id = 2, Titulo = "T2", Estado = EstadoTarea.Pendiente },
                new Tarea { Id = 3, Titulo = "T3", Estado = EstadoTarea.EnProgreso }
            ]
        };

        // Act
        var avance = proyecto.CalcularPorcentajeAvance();

        // Assert
        avance.Should().Be(33.33);
    }
}
