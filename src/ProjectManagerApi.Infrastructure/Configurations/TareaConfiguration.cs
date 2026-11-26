using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagerApi.Domain.Entities;

namespace ProjectManagerApi.Infrastructure.Configurations;

/// <summary>
/// Configuración Fluent API de Entity Framework Core para la entidad <see cref="Tarea"/>.
/// </summary>
public class TareaConfiguration : IEntityTypeConfiguration<Tarea>
{
    public void Configure(EntityTypeBuilder<Tarea> builder)
    {
        builder.ToTable("Tareas");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Titulo)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(t => t.Descripcion)
            .HasMaxLength(1000);

        builder.Property(t => t.Estado)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(t => t.Prioridad)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(t => t.FechaLimite)
            .IsRequired();

        builder.HasIndex(t => t.ProyectoId);
        builder.HasIndex(t => new { t.Estado, t.Prioridad });
        builder.HasIndex(t => t.FechaLimite);
    }
}
