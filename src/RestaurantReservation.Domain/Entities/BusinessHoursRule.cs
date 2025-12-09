using RestaurantReservation.Domain.Entities.Base;
using RestaurantReservation.Domain.Enums;
using RestaurantReservation.Domain.Exceptions;

public class BusinessHoursRule : DomainBase
{
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public DateOnly? SpecificDate { get; private set; }
    public WeekDay? WeekDay { get; private set; }
    public TimeOnly? Open { get; private set; }
    public TimeOnly? Close { get; private set; }
    public bool IsClosed { get; private set; }

    private BusinessHoursRule() { }

    public BusinessHoursRule(
        DateOnly startDate,
        DateOnly endDate,
        DateOnly? specificDate,
        WeekDay? weekDay,
        TimeOnly? open,
        TimeOnly? close,
        bool isClosed)
    {
        ApplyChanges(startDate, endDate, specificDate, weekDay, open, close, isClosed);
    }

    public void Update(
        DateOnly startDate,
        DateOnly endDate,
        DateOnly? specificDate,
        WeekDay? weekDay,
        TimeOnly? open,
        TimeOnly? close,
        bool isClosed)
    {
        ApplyChanges(startDate, endDate, specificDate, weekDay, open, close, isClosed);
    }

    private void ApplyChanges(
        DateOnly startDate,
        DateOnly endDate,
        DateOnly? specificDate,
        WeekDay? weekDay,
        TimeOnly? open,
        TimeOnly? close,
        bool isClosed)
    {
        StartDate = startDate;
        EndDate = endDate;
        SpecificDate = specificDate;
        WeekDay = weekDay;
        Open = open;
        Close = close;
        IsClosed = isClosed;

        Validate();
    }

    private void Validate()
    {
        if (StartDate > EndDate)
            throw new DomainException("StartDate cannot be after EndDate.");

        // --- Regras específicas por dia ---
        if (SpecificDate is not null)
        {
            // Para regra de dia específico, Start/End devem bater com SpecificDate.
            if (StartDate != SpecificDate || EndDate != SpecificDate)
                throw new DomainException("For SpecificDate rules, StartDate and EndDate must match SpecificDate.");

            // Não faz sentido ter WeekDay junto com SpecificDate.
            if (WeekDay is not null)
                throw new DomainException("SpecificDate rules cannot define WeekDay.");
        }

        // --- Pelo menos uma forma de direcionar a regra ---
        if (SpecificDate is null && WeekDay is null)
            throw new DomainException("Either SpecificDate or WeekDay must be provided.");

        // --- Horário / fechamento ---
        if (!IsClosed)
        {
            if (Open is null || Close is null)
                throw new DomainException("Open and Close times are required when IsClosed is false.");

            if (Open >= Close)
                throw new DomainException("Open time must be earlier than Close time.");
        }
        else
        {
            // Se estiver fechado, não deve ter horário
            if (Open is not null || Close is not null)
                throw new DomainException("Closed rules cannot have Open or Close times.");
        }
    }

    public static BusinessHoursRule CreateClosedDate(DateOnly specificDate)
        => new BusinessHoursRule(
            specificDate,
            specificDate,
            specificDate,
            null,
            null,
            null,
            true);
}
