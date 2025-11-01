using RestaurantReservation.Domain.Exceptions;
using System.Net.Mail;

namespace RestaurantReservation.Domain.ValueObjects;
public record Email
{
    public string Value { get; private set; }

    public Email(string value)
    {
        Validate(value);
        Value = value;
    }

    private static void Validate(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("The email field cannot be empty or null");

        try
        {
            var addr = new MailAddress(email);            
        }
        catch (FormatException)
        {
            throw new DomainException("Invalid Email");
        }
    }
}