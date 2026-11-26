using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProjectManagerApi.Domain.Entities;
using ProjectManagerApi.Domain.Enums;
using ProjectManagerApi.Infrastructure.Persistence;
using ProjectManagerApi.Infrastructure.Repositories;

namespace ProjectManagerApi.Infrastructure.UnitTests.Repositories;

public class TareaRepositoryTests
{
    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetFilteredAsync_DebeFiltrarPorEstadoYPrioridadDinamicamente()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var proyecto = new Proyecto { Nombre = "Demo", FechaInicio = DateTime.UtcNow };
        context.Proyectos.Add(proyecto);
        await context.SaveChangesAsync();

        context.Tareas.AddRange(
            new Tarea
            {
                Titulo = "Pendiente Alta",
                Estado = EstadoTarea.Pendiente,
                Prioridad = PrioridadTarea.Alta,
                FechaLimite = DateTime.UtcNow.AddDays(1),
                ProyectoId = proyecto.Id
            },
            new Tarea
            {
                Titulo = "Pendiente Baja",
                Estado = EstadoTarea.Pendiente,
                Prioridad = PrioridadTarea.Baja,
                FechaLimite = DateTime.UtcNow.AddDays(2),
                ProyectoId = proyecto.Id
            },
            new Tarea
            {
                Titulo = "Completada Alta",
                Estado = EstadoTarea.Completada,
                Prioridad = PrioridadTarea.Alta,
                FechaLimite = DateTime.UtcNow.AddDays(3),
                ProyectoId = proyecto.Id
            });
        await context.SaveChangesAsync();

        var repository = new TareaRepository(context);

        // Act
        var todas = await repository.GetFilteredAsync(null, null);
        var soloPendientes = await repository.GetFilteredAsync(EstadoTarea.Pendiente, null);
        var pendientesAltas = await repository.GetFilteredAsync(EstadoTarea.Pendiente, PrioridadTarea.Alta);

        // Assert
        todas.Should().HaveCount(3);
        soloPendientes.Should().HaveCount(2);
        pendientesAltas.Should().ContainSingle(t => t.Titulo == "Pendiente Alta");
    }

    [Fact]
    public async Task GetOverdueUncompletedAsync_DebeExcluirCompletadasYFuturas()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var proyecto = new Proyecto { Nombre = "Demo", FechaInicio = DateTime.UtcNow };
        context.Proyectos.Add(proyecto);
        await context.SaveChangesAsync();

        var now = new DateTime(2026, 11, 25, 12, 0, 0, DateTimeKind.Utc);
        context.Tareas.AddRange(
            new Tarea
            {
                Titulo = "Vencida Pendiente",
                Estado = EstadoTarea.Pendiente,
                Prioridad = PrioridadTarea.Alta,
                FechaLimite = now.AddDays(-1),
                ProyectoId = proyecto.Id
            },
            new Tarea
            {
                Titulo = "Vencida Completada",
                Estado = EstadoTarea.Completada,
                Prioridad = PrioridadTarea.Alta,
                FechaLimite = now.AddDays(-1),
                ProyectoId = proyecto.Id
            },
            new Tarea
            {
                Titulo = "Futura Pendiente",
                Estado = EstadoTarea.Pendiente,
                Prioridad = PrioridadTarea.Media,
                FechaLimite = now.AddDays(1),
                ProyectoId = proyecto.Id
            });
        await context.SaveChangesAsync();

        var repository = new TareaRepository(context);

        // Act
        var vencidas = await repository.GetOverdueUncompletedAsync(now);

        // Assert
        vencidas.Should().ContainSingle(t => t.Titulo == "Vencida Pendiente");
    }

    [Fact]
    public async Task UpdateAndDelete_ConEntidadDesatachada_DebePersistirCambios()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var proyecto = new Proyecto { Nombre = "Demo", FechaInicio = DateTime.UtcNow };
        var proyectoDestino = new Proyecto { Nombre = "Destino", FechaInicio = DateTime.UtcNow };
        context.Proyectos.AddRange(proyecto, proyectoDestino);
        await context.SaveChangesAsync();

        var repository = new TareaRepository(context);
        var creada = await repository.AddAsync(new Tarea
        {
            Titulo = "Inicial",
            Estado = EstadoTarea.Pendiente,
            Prioridad = PrioridadTarea.Media,
            FechaLimite = DateTime.UtcNow.AddDays(5),
            ProyectoId = proyecto.Id
        });

        // Act
        var leida = await repository.GetByIdAsync(creada.Id);
        leida!.Estado = EstadoTarea.Completada;
        leida.ProyectoId = proyectoDestino.Id;
        await repository.UpdateAsync(leida);

        leida.Proyecto.Nombre.Should().Be("Destino");

        var verificada = await repository.GetByIdAsync(creada.Id);
        verificada!.Estado.Should().Be(EstadoTarea.Completada);
        verificada.Proyecto.Nombre.Should().Be("Destino");

        await repository.DeleteAsync(verificada);
        var eliminada = await repository.GetByIdAsync(creada.Id);

        // Assert
        eliminada.Should().BeNull();
    }
}
