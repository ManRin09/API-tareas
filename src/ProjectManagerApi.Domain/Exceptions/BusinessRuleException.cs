namespace ProjectManagerApi.Domain.Exceptions;

/// <summary>
/// Excepción lanzada cuando se incumple una regla de negocio del dominio.
/// </summary>
public class BusinessRuleException(string message) : Exception(message);
