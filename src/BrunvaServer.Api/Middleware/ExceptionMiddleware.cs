using BrunvaServer.Api.Problems;

namespace BrunvaServer.Api.Middleware;

public sealed class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception");
            if (context.Response.HasStarted)
                throw;

            var result = ProblemResults.Create(
                context,
                StatusCodes.Status500InternalServerError,
                "unexpected",
                "Unexpected error",
                "The request could not be completed.");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(result.Value);
        }
    }
}
