using FluentResults;

namespace RestaurantReservation.Application.Utils;

public static class ErrorExtensions
{
    public static Error WithCode(this Error error, string code)
    {
        return error.WithMetadata("Code", code);
    }
}