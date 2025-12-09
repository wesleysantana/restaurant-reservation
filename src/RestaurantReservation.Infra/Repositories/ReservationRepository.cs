using Microsoft.EntityFrameworkCore;
using NodaTime;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Domain.Enums;
using RestaurantReservation.Domain.Repositories;
using RestaurantReservation.Infra.Context;

namespace RestaurantReservation.Infra.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly DataContext _context;

    private DbSet<Reservation> Set => _context.Set<Reservation>();

    public ReservationRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<bool> IsTableAvailableAsync(
        Guid tableId,
        Instant startsAt, 
        Instant endsAt,
        CancellationToken cancellationToken)
    {      

        var existsConflict = await Set.AnyAsync(x =>
            x.TableId == tableId &&
            x.Status == StatusReservation.Ativo &&
            x.StartsAt < endsAt &&
            x.EndsAt > startsAt,
            cancellationToken);

        return !existsConflict;
    }

    public async Task<Reservation> MakeReservationAsync(
        Guid customerId,
        Guid tableId,
        Instant startsAt,
        Instant endsAt,
        short numberOfGuests,
        CancellationToken cancellationToken)
    {
        var reservation = new Reservation(
            customerId,
            tableId,
            startsAt,
            endsAt,
            numberOfGuests);

        await Set.AddAsync(reservation, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return reservation;
    }

    public Task<Reservation?> GetReservationAsync(Guid reservationId, CancellationToken cancellationToken)
        => Set.FirstOrDefaultAsync(x => x.Id == reservationId, cancellationToken);

    public async Task CancelReservationAsync(Guid reservationId, CancellationToken cancellationToken)
    {
        var reservation = await Set.FirstOrDefaultAsync(x => x.Id == reservationId, cancellationToken);
        if (reservation != null)
        {
            Set.Remove(reservation);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }   
}
