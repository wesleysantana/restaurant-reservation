namespace RestaurantReservation.Application.Utils;

public enum ProblemCode
{
    TableUnavailable,
    ReservationNotFound,
    InvalidReservationCancellation,
    UnauthorizedUser,
    ForbiddenReservationCancellation,
    InvalidBusinessHours
}