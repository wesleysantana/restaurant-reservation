using FluentResults;

namespace RestaurantReservation.Application.Extensions;

public static class ErrorExtensions
{
    public static Error WithCode(this Error error, string code)
    {
        return error.WithMetadata("Code", code);
    }
}