using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using RestaurantReservation.Application.Extensions;
using RestaurantReservation.Application.Utils;
using RestaurantReservation.WebApi.Extensions;
using RestaurantReservation.WebApi.Localization;

namespace RestaurantReservation.Test.WebApi;

public class ResultExtensionsTests
{
    private readonly IStringLocalizer<SharedResource> _localizer;

    public ResultExtensionsTests()
    {
        // Mock de localizer que simplesmente devolve a chave como Value
        var localizerMock = new Mock<IStringLocalizer<SharedResource>>();
        localizerMock
            .Setup(x => x[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        _localizer = localizerMock.Object;
    }

    private ProblemDetails CreateProblemDetailsForCode(ProblemCode code)
    {
        var error = new Error("any error")
            .WithCode(code.ToString());

        var result = Result.Fail(error);

        return result.ToProblemDetails(_localizer);
    }

    [Fact]
    public void TableUnavailable_ShouldMapTo409()
    {
        var problem = CreateProblemDetailsForCode(ProblemCode.TableUnavailable);

        Assert.Equal(StatusCodes.Status409Conflict, problem.Status);
    }

    [Fact]
    public void ReservationNotFound_ShouldMapTo404()
    {
        var problem = CreateProblemDetailsForCode(ProblemCode.ReservationNotFound);

        Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
    }

    [Fact]
    public void InvalidReservationCancellation_ShouldMapTo422()
    {
        var problem = CreateProblemDetailsForCode(ProblemCode.InvalidReservationCancellation);

        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problem.Status);
    }

    [Fact]
    public void UnauthorizedUser_ShouldMapTo401()
    {
        var problem = CreateProblemDetailsForCode(ProblemCode.UnauthorizedUser);

        Assert.Equal(StatusCodes.Status401Unauthorized, problem.Status);
    }

    [Fact]
    public void ForbiddenReservationCancellation_ShouldMapTo403()
    {
        var problem = CreateProblemDetailsForCode(ProblemCode.ForbiddenReservationCancellation);

        Assert.Equal(StatusCodes.Status403Forbidden, problem.Status);
    }
}