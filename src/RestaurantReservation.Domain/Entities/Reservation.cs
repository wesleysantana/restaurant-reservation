using NodaTime;
using RestaurantReservation.Domain.Entities.Base;
using RestaurantReservation.Domain.Enums;
using RestaurantReservation.Domain.Exceptions;

namespace RestaurantReservation.Domain.Entities;

public class Reservation : DomainBase
{
    public Guid UserId { get; private set; }
    public Guid TableId { get; private set; }
    public Instant StartsAt { get; private set; }
    public Instant EndsAt { get; private set; }
    public StatusReservation Status { get; private set; }
    public short NumberOfGuests { get; private set; }

    private Reservation()
    { }

    public Reservation(Guid userId, Guid tableId, Instant startsAt, Instant endsAt, short numberOfGuests)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserId inválido.");

        if (tableId == Guid.Empty)
            throw new DomainException("TableId inválido.");

        if (numberOfGuests < 1)
            throw new DomainException("A reserva deve ter pelo menos 1 pessoa.");
        
        if (startsAt <= Instant.FromDateTimeUtc(DateTime.UtcNow))
            throw new DomainException("A reserva deve ser feita para um horário futuro.");        

        if (endsAt <= startsAt)
            throw new DomainException("A data final deve ser maior que a data inicial.");

        UserId = userId;
        TableId = tableId;
        StartsAt = startsAt;
        EndsAt = endsAt;
        NumberOfGuests = numberOfGuests;
        Status = StatusReservation.Ativo;
    }

    public bool IsActive => Status == StatusReservation.Ativo;

    public void Cancel()
    {
        if (Status == StatusReservation.Cancelado)
            return;

        Status = StatusReservation.Cancelado;
    }
}