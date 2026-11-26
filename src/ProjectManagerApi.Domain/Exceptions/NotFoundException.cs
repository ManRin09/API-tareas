namespace ProjectManagerApi.Domain.Exceptions;

/// <summary>
/// Excepción lanzada cuando un recurso solicitado no existe.
/// </summary>
public class NotFoundException(string message) : Exception(message);
