using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using ProjectManagerApi.API.Middlewares;
using ProjectManagerApi.Domain.Exceptions;

namespace ProjectManagerApi.API.UnitTests.Middlewares;

public class ExceptionMiddlewareTests
{
    private readonly Mock<ILogger<ExceptionMiddleware>> _loggerMock = new();

    [Fact]
    public async Task InvokeAsync_CuandoLanzaNotFoundException_DebeRetornar404ConJsonEstandarizado()
    {
        // Arrange
        var context = CreateHttpContext();
        var middleware = new ExceptionMiddleware(
            _ => throw new NotFoundException("Recurso no localizado."),
            _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var (statusCode, body) = await ReadResponseAsync(context);
        statusCode.Should().Be(StatusCodes.Status404NotFound);
        body!.Status.Should().Be(404);
        body.Title.Should().Be("Recurso no encontrado");
        body.Detail.Should().Be("Recurso no localizado.");
    }

    [Fact]
    public async Task InvokeAsync_CuandoLanzaBusinessRuleException_DebeRetornar409ConJsonEstandarizado()
    {
        // Arrange
        var context = CreateHttpContext();
        var middleware = new ExceptionMiddleware(
            _ => throw new BusinessRuleException("Regla violada."),
            _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var (statusCode, body) = await ReadResponseAsync(context);
        statusCode.Should().Be(StatusCodes.Status409Conflict);
        body!.Status.Should().Be(409);
        body.Title.Should().Be("Conflicto de regla de negocio");
        body.Detail.Should().Be("Regla violada.");
    }

    [Fact]
    public async Task InvokeAsync_CuandoLanzaExcepcionNoControlada_DebeRetornar500SinExponerDetallesInternos()
    {
        // Arrange
        var context = CreateHttpContext();
        var middleware = new ExceptionMiddleware(
            _ => throw new InvalidOperationException("Detalle sensible interno"),
            _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var (statusCode, body) = await ReadResponseAsync(context);
        statusCode.Should().Be(StatusCodes.Status500InternalServerError);
        body!.Status.Should().Be(500);
        body.Title.Should().Be("Error interno del servidor");
        body.Detail.Should().NotContain("Detalle sensible interno");
    }

    private static DefaultHttpContext CreateHttpContext() => new()
    {
        Response = { Body = new MemoryStream() }
    };

    private static async Task<(int StatusCode, ExceptionMiddleware.ErrorResponse? Body)> ReadResponseAsync(
        HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var json = await reader.ReadToEndAsync();
        var body = JsonSerializer.Deserialize<ExceptionMiddleware.ErrorResponse>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return (context.Response.StatusCode, body);
    }
}
