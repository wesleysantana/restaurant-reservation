using FluentResults;
using Microsoft.AspNetCore.Mvc;
using RestaurantReservation.Application.Utils;

namespace RestaurantReservation.WebApi.Extensions;

public static class ResultExtensions
{
    private static ProblemDetails ToProblemDetails<T>(this Result<T> result)
    {
        var firstError = result.Errors.FirstOrDefault() ?? new Error("Unknown error");

        var code = firstError.Metadata.TryGetValue(ErrorMetadataKeys.Code, out var codeObj)
            ? codeObj?.ToString()
            : null;

        var statusCode = code switch
        {
            var c when c == ProblemCode.TableUnavailable.ToString()
                => StatusCodes.Status409Conflict,

            // aqui você pode mapear outros ProblemCode futuramente
            _ => StatusCodes.Status400BadRequest
        };

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = "Business rule violation",
            Detail = string.Join("; ", result.Errors.Select(e => e.Message)),
            Type = $"https://httpstatuses.com/{statusCode}"
        };

        problem.Extensions["errors"] = result.Errors
            .Select(e => new
            {
                e.Message,
                Code = e.Metadata.TryGetValue(ErrorMetadataKeys.Code, out var c) ? c : null
            })
            .ToArray();

        return problem;
    }

    public static IActionResult ToActionResult<T>(this Result<T> result, ControllerBase controller)
    {
        if (result.IsSuccess)
            return controller.Ok(result.Value);

        var problem = result.ToProblemDetails();
        var statusCode = problem.Status ?? StatusCodes.Status400BadRequest;

        return controller.StatusCode(statusCode, problem);
    }
}
