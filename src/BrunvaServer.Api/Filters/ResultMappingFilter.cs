using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using BrunvaServer.Api.Problems;
using BrunvaServer.Application.Common;

namespace BrunvaServer.Api.Filters;

public sealed class ResultMappingFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult { Value: IStatusResult status })
        {
            context.Result = status.IsSuccess
                ? Success(status)
                : ProblemResults.Create(context.HttpContext, status.StatusCode, status.Code ?? "error", status.Title ?? "Error", status.Detail ?? "The request failed.", status.Errors);
        }

        await next();
    }

    private static IActionResult Success(IStatusResult status) =>
        status.StatusCode == StatusCodes.Status204NoContent
            ? new StatusCodeResult(StatusCodes.Status204NoContent)
            : new ObjectResult(status.Body) { StatusCode = status.StatusCode };
}
