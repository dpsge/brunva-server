using BrunvaServer.Api.Problems;

namespace BrunvaServer.Api.Middleware;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ILogger<CorrelationIdMiddleware> logger)
    {
        var correlationId = context.Request.Headers[Correlation.HeaderName].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(correlationId))
            correlationId = context.TraceIdentifier;

        context.Items[Correlation.HeaderName] = correlationId;
        context.Response.Headers[Correlation.HeaderName] = correlationId;
        using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
            await next(context);
    }
}
