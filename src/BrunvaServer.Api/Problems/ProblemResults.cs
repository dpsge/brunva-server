using Microsoft.AspNetCore.Mvc;

namespace BrunvaServer.Api.Problems;

public static class ProblemResults
{
    public static ObjectResult Create(
        HttpContext context,
        int status,
        string code,
        string title,
        string detail,
        IReadOnlyDictionary<string, string[]>? errors = null)
    {
        return new ObjectResult(Details(context, status, code, title, detail, errors))
        {
            StatusCode = status,
            ContentTypes = { "application/problem+json" }
        };
    }

    public static ProblemDetails Details(
        HttpContext context,
        int status,
        string code,
        string title,
        string detail,
        IReadOnlyDictionary<string, string[]>? errors = null)
    {
        var problem = new ProblemDetails
        {
            Type = $"https://getbrunva.ge/problems/{code}",
            Title = title,
            Status = status,
            Detail = detail
        };
        problem.Extensions["code"] = code;
        problem.Extensions["traceId"] = context.Items[Correlation.HeaderName] as string ?? context.TraceIdentifier;
        if (errors is { Count: > 0 })
            problem.Extensions["errors"] = errors;
        return problem;
    }
}

public static class Correlation
{
    public const string HeaderName = "X-Correlation-Id";
}
