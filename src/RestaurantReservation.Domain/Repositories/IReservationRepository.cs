using NodaTime;
using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Domain.Repositories;

//public interface IReservationRepository
//{
//    Task<bool> IsTableAvailableAsync(        
//        Guid tableId, 
//        DateTime reservationDate, 
//        TimeSpan reservationTime, 
//        TimeSpan duration,
//        CancellationToken cancellationToken);

//    Task<Reservation> MakeReservationAsync(        
//        Guid customerId, Guid tableId, 
//        DateTime reservationDate, 
//        short numberOfGuests,
//        CancellationToken cancellationToken);

//    Task<Reservation> GetReservationAsync(Guid reservationId, CancellationToken cancellationToken);

//    Task CancelReservationAsync(Guid reservationId);
//}

public interface IReservationRepository
{
    Task<bool> IsTableAvailableAsync(
        Guid tableId,
        Instant startsAt, 
        Instant endsAt,
        CancellationToken cancellationToken);

    Task<Reservation> MakeReservationAsync(
        Guid customerId,
        Guid tableId,
        Instant startsAt,
        Instant endsAt,
        short numberOfGuests,
        CancellationToken cancellationToken);

    Task<Reservation?> GetReservationAsync(Guid reservationId, CancellationToken cancellationToken);

    Task CancelReservationAsync(Guid reservationId, CancellationToken cancellationToken);
}
