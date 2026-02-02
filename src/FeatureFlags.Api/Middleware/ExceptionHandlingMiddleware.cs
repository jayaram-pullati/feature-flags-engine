using FeatureFlags.Core.Errors;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace FeatureFlags.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
  public async Task Invoke(HttpContext context)
  {
    try
    {
      await next(context);
    }
    catch (ValidationException ex)
    {
      await WriteProblem(context, StatusCodes.Status400BadRequest, "Validation error", ex.Message);
    }
    catch (FeatureNotFoundException ex)
    {
      await WriteProblem(context, StatusCodes.Status404NotFound, "Not found", ex.Message);
    }
    catch (DomainException ex)
    {
      await WriteProblem(context, StatusCodes.Status409Conflict, "Domain error", ex.Message);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Unhandled exception");
      await WriteProblem(context, StatusCodes.Status500InternalServerError, "Server error", "An unexpected error occurred.");
    }
  }

  private static async Task WriteProblem(HttpContext ctx, int status, string title, string detail)
  {
    ctx.Response.StatusCode = status;
    ctx.Response.ContentType = "application/problem+json";

    var problem = new ProblemDetails
    {
      Status = status,
      Title = title,
      Detail = detail
    };

    await ctx.Response.WriteAsync(JsonSerializer.Serialize(problem));
  }
}
