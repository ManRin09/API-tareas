using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagerApi.Domain.Entities;

namespace ProjectManagerApi.Infrastructure.Configurations;

/// <summary>
/// Configuración Fluent API de Entity Framework Core para la entidad <see cref="Proyecto"/>.
/// </summary>
public class ProyectoConfiguration : IEntityTypeConfiguration<Proyecto>
{
    public void Configure(EntityTypeBuilder<Proyecto> builder)
    {
        builder.ToTable("Proyectos");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nombre)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.Descripcion)
            .HasMaxLength(1000);

        builder.Property(p => p.FechaInicio)
            .IsRequired();

        builder.HasMany(p => p.Tareas)
            .WithOne(t => t.Proyecto)
            .HasForeignKey(t => t.ProyectoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
