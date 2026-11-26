using ProjectManagerApi.Application.DTOs;

namespace ProjectManagerApi.Application.Interfaces;

/// <summary>
/// Contrato de casos de uso para la gestión de proyectos.
/// </summary>
public interface IProyectoService
{
    Task<IReadOnlyList<ProyectoResponseDto>> GetAllAsync();

    Task<ProyectoResponseDto> GetByIdAsync(int id);

    Task<ProyectoResponseDto> CreateAsync(CreateProyectoDto dto);

    Task<ProyectoResponseDto> UpdateAsync(int id, UpdateProyectoDto dto);

    Task DeleteAsync(int id);
}
