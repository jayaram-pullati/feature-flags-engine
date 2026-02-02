using System.Text.Json;
using FeatureFlags.Api.Middleware;
using FeatureFlags.Core.Errors;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace FeatureFlags.Tests.Api;

public sealed class ExceptionHandlingMiddlewareTests
{
  private static DefaultHttpContext NewHttpContext()
  {
    var ctx = new DefaultHttpContext();
    ctx.Response.Body = new MemoryStream();
    return ctx;
  }

  private static async Task<ProblemDetails> ReadProblemAsync(HttpResponse response)
  {
    response.Body.Position = 0;

    using var reader = new StreamReader(response.Body);
    var json = await reader.ReadToEndAsync();

    var problem = JsonSerializer.Deserialize<ProblemDetails>(json, new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    });

    problem.Should().NotBeNull("middleware should write ProblemDetails JSON");
    return problem!;
  }

  [Fact]
  public async Task Invoke_PassesThrough_WhenNoException()
  {
    // Arrange
    var ctx = NewHttpContext();

    RequestDelegate next = http =>
    {
      http.Response.StatusCode = StatusCodes.Status204NoContent;
      return Task.CompletedTask;
    };

    var logger = NullLogger<ExceptionHandlingMiddleware>.Instance;
    var sut = new ExceptionHandlingMiddleware(next, logger);

    // Act
    await sut.Invoke(ctx);

    // Assert
    ctx.Response.StatusCode.Should().Be(StatusCodes.Status204NoContent);
    ctx.Response.ContentType.Should().BeNull(); // we didn't set content type in next
  }

  [Fact]
  public async Task Invoke_Returns400ProblemDetails_WhenValidationExceptionThrown()
  {
    // Arrange
    var ctx = NewHttpContext();

    RequestDelegate next = _ => throw new ValidationException("Invalid input.");
    var logger = NullLogger<ExceptionHandlingMiddleware>.Instance;
    var sut = new ExceptionHandlingMiddleware(next, logger);

    // Act
    await sut.Invoke(ctx);

    // Assert
    ctx.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    ctx.Response.ContentType.Should().Be("application/problem+json");

    var problem = await ReadProblemAsync(ctx.Response);
    problem.Status.Should().Be(StatusCodes.Status400BadRequest);
    problem.Title.Should().Be("Validation error");
    problem.Detail.Should().Be("Invalid input.");
  }

  [Fact]
  public async Task Invoke_Returns404ProblemDetails_WhenFeatureNotFoundExceptionThrown()
  {
    // Arrange
    var ctx = NewHttpContext();

    RequestDelegate next = _ => throw new FeatureNotFoundException("missing-flag");
    var logger = NullLogger<ExceptionHandlingMiddleware>.Instance;
    var sut = new ExceptionHandlingMiddleware(next, logger);

    // Act
    await sut.Invoke(ctx);

    // Assert
    ctx.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    ctx.Response.ContentType.Should().Be("application/problem+json");

    var problem = await ReadProblemAsync(ctx.Response);
    problem.Status.Should().Be(StatusCodes.Status404NotFound);
    problem.Title.Should().Be("Not found");
    problem.Detail.Should().Contain("missing-flag");
  }

  [Fact]
  public async Task Invoke_Returns409ProblemDetails_WhenDomainExceptionThrown()
  {
    // Arrange
    var ctx = NewHttpContext();

    // Concrete subclass so we can instantiate it
    var ex = new TestDomainException("Domain conflict.");

    RequestDelegate next = _ => throw ex;

    var logger = NullLogger<ExceptionHandlingMiddleware>.Instance;
    var sut = new ExceptionHandlingMiddleware(next, logger);

    // Act
    await sut.Invoke(ctx);

    // Assert
    ctx.Response.StatusCode.Should().Be(StatusCodes.Status409Conflict);
    ctx.Response.ContentType.Should().Be("application/problem+json");

    var problem = await ReadProblemAsync(ctx.Response);
    problem.Status.Should().Be(StatusCodes.Status409Conflict);
    problem.Title.Should().Be("Domain error");
    problem.Detail.Should().Be("Domain conflict.");
  }

  /// <summary>
  /// Concrete subclass so DomainException can be thrown in tests.
  /// </summary>
  private sealed class TestDomainException : DomainException
  {
    public TestDomainException(string message) : base(message) { }
  }

  [Fact]
  public async Task Invoke_Returns500ProblemDetails_WhenUnhandledExceptionThrown()
  {
    // Arrange
    var ctx = NewHttpContext();

    RequestDelegate next = _ => throw new InvalidOperationException("boom");
    var logger = NullLogger<ExceptionHandlingMiddleware>.Instance;
    var sut = new ExceptionHandlingMiddleware(next, logger);

    // Act
    await sut.Invoke(ctx);

    // Assert
    ctx.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    ctx.Response.ContentType.Should().Be("application/problem+json");

    var problem = await ReadProblemAsync(ctx.Response);
    problem.Status.Should().Be(StatusCodes.Status500InternalServerError);
    problem.Title.Should().Be("Server error");
    problem.Detail.Should().Be("An unexpected error occurred.");
  }
}
