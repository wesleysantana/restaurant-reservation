using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using RestaurantReservation.Application.Utils;
using RestaurantReservation.WebApi.Localization;

namespace RestaurantReservation.WebApi.Extensions;

public static class ResultExtensions
{
    public static ProblemDetails ToProblemDetails<T>(
       this Result<T> result,
       IStringLocalizer<SharedResource> localizer)
    {
        return CreateProblemDetails(result, localizer);
    }

    public static ProblemDetails ToProblemDetails(
      this Result result,
      IStringLocalizer<SharedResource> localizer)
    {
        return CreateProblemDetails(result, localizer);
    }

    private static ProblemDetails CreateProblemDetails(
        ResultBase resultBase,
        IStringLocalizer<SharedResource> localizer)
    {
        var firstError = resultBase.Errors.FirstOrDefault() ?? new Error("Unknown error");

        var code = firstError.Metadata.TryGetValue(ErrorMetadataKeys.Code, out var codeObj)
            ? codeObj?.ToString()
            : null;

        var titleKey = code is not null ? $"ErrorTitle.{code}" : "ErrorTitle.Generic";
        var detailKey = code is not null ? $"ErrorDetail.{code}" : "ErrorDetail.Generic";

        var localizedTitle = localizer[titleKey];
        var localizedDetail = localizer[detailKey];

        string finalTitle = localizedTitle.ResourceNotFound
            ? "Business rule violation"
            : localizedTitle.Value;

        string finalDetail = localizedDetail.ResourceNotFound
            ? string.Join("; ", resultBase.Errors.Select(e => e.Message))
            : localizedDetail.Value;

        var statusCode = code switch
        {
            nameof(ProblemCode.TableUnavailable) => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = finalTitle,
            Detail = finalDetail,
            Type = $"https://httpstatuses.com/{statusCode}"
        };

        problem.Extensions["errors"] = resultBase.Errors
            .Select(e => new
            {
                e.Message,
                Code = e.Metadata.TryGetValue(ErrorMetadataKeys.Code, out var c) ? c : null
            })
            .ToArray();

        return problem;
    } 

    public static IActionResult ToActionResult<T>(
       this Result<T> result,
       ControllerBase controller,
       IStringLocalizer<SharedResource> localizer,
       Func<T, IActionResult> onSuccess)
    {
        if (result.IsSuccess)                    
            return onSuccess(result.Value);        

        var problem = result.ToProblemDetails(localizer);
        var statusCode = problem.Status ?? StatusCodes.Status400BadRequest;

        return controller.StatusCode(statusCode, problem);
    }

    public static IActionResult ToActionResult(
        this Result result,
        ControllerBase controller,
        IStringLocalizer<SharedResource> localizer,
        Func<IActionResult> onSuccess)
    {
        if (result.IsSuccess)        
            return onSuccess();        

        var problem = result.ToProblemDetails(localizer);
        var statusCode = problem.Status ?? StatusCodes.Status400BadRequest;

        return controller.StatusCode(statusCode, problem);
    }
}
