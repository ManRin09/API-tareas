using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProjectManagerApi.Domain.Entities;
using ProjectManagerApi.Domain.Enums;
using ProjectManagerApi.Infrastructure.Persistence;
using ProjectManagerApi.Infrastructure.Repositories;

namespace ProjectManagerApi.Infrastructure.UnitTests.Repositories;

public class ProyectoRepositoryTests
{
    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task HasPendingTasksAsync_CuandoExistenTareasNoCompletadas_DebeRetornarTrue()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var proyecto = new Proyecto { Nombre = "Proyecto Activo", FechaInicio = DateTime.UtcNow };
        context.Proyectos.Add(proyecto);
        await context.SaveChangesAsync();

        context.Tareas.AddRange(
            new Tarea
            {
                Titulo = "Completada",
                Estado = EstadoTarea.Completada,
                Prioridad = PrioridadTarea.Baja,
                FechaLimite = DateTime.UtcNow.AddDays(1),
                ProyectoId = proyecto.Id
            },
            new Tarea
            {
                Titulo = "En progreso",
                Estado = EstadoTarea.EnProgreso,
                Prioridad = PrioridadTarea.Alta,
                FechaLimite = DateTime.UtcNow.AddDays(2),
                ProyectoId = proyecto.Id
            });
        await context.SaveChangesAsync();

        var repository = new ProyectoRepository(context);

        // Act
        var hasPending = await repository.HasPendingTasksAsync(proyecto.Id);

        // Assert
        hasPending.Should().BeTrue();
    }

    [Fact]
    public async Task HasPendingTasksAsync_CuandoTodasLasTareasEstanCompletadas_DebeRetornarFalse()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var proyecto = new Proyecto { Nombre = "Proyecto Cerrado", FechaInicio = DateTime.UtcNow };
        context.Proyectos.Add(proyecto);
        await context.SaveChangesAsync();

        context.Tareas.Add(new Tarea
        {
            Titulo = "Hecha",
            Estado = EstadoTarea.Completada,
            Prioridad = PrioridadTarea.Media,
            FechaLimite = DateTime.UtcNow.AddDays(1),
            ProyectoId = proyecto.Id
        });
        await context.SaveChangesAsync();

        var repository = new ProyectoRepository(context);

        // Act
        var hasPending = await repository.HasPendingTasksAsync(proyecto.Id);

        // Assert
        hasPending.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllWithTareasAsync_DebeIncluirTareasYActualizarCorrectamente()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new ProyectoRepository(context);

        var creado = await repository.AddAsync(new Proyecto
        {
            Nombre = "Inicial",
            FechaInicio = DateTime.UtcNow
        });

        context.Tareas.Add(new Tarea
        {
            Titulo = "T1",
            Estado = EstadoTarea.Completada,
            Prioridad = PrioridadTarea.Media,
            FechaLimite = DateTime.UtcNow.AddDays(1),
            ProyectoId = creado.Id
        });
        await context.SaveChangesAsync();

        // Act
        var obtenido = await repository.GetByIdWithTareasAsync(creado.Id);
        obtenido!.Nombre = "Actualizado";
        await repository.UpdateAsync(obtenido);

        var todos = await repository.GetAllWithTareasAsync();

        // Assert
        todos.Should().HaveCount(1);
        todos[0].Nombre.Should().Be("Actualizado");
        todos[0].Tareas.Should().HaveCount(1);
        todos[0].CalcularPorcentajeAvance().Should().Be(100);
    }

    [Fact]
    public async Task DeleteAsync_CuandoProyectoEsLeidoConNoTracking_DebeEliminarSinConflictos()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new ProyectoRepository(context);
        var creado = await repository.AddAsync(new Proyecto
        {
            Nombre = "Para Eliminar",
            FechaInicio = DateTime.UtcNow
        });

        var leido = await repository.GetByIdWithTareasAsync(creado.Id);

        // Act
        await repository.DeleteAsync(leido!);

        // Assert
        var existe = await repository.ExistsAsync(creado.Id);
        existe.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_CuandoTieneTareasCompletadas_DebeEliminarTareasYProyecto()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new ProyectoRepository(context);
        var proyecto = await repository.AddAsync(new Proyecto
        {
            Nombre = "Proyecto Con Completadas",
            FechaInicio = DateTime.UtcNow
        });

        context.Tareas.Add(new Tarea
        {
            Titulo = "Tarea Finalizada",
            Estado = EstadoTarea.Completada,
            Prioridad = PrioridadTarea.Media,
            FechaLimite = DateTime.UtcNow.AddDays(1),
            ProyectoId = proyecto.Id
        });
        await context.SaveChangesAsync();

        var leido = await repository.GetByIdWithTareasAsync(proyecto.Id);

        // Act
        await repository.DeleteAsync(leido!);

        // Assert
        (await repository.ExistsAsync(proyecto.Id)).Should().BeFalse();
        (await context.Tareas.AnyAsync(t => t.ProyectoId == proyecto.Id)).Should().BeFalse();
    }
}
