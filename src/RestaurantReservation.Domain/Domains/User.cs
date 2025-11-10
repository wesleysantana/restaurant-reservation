using RestaurantReservation.Domain.Enums;
using RestaurantReservation.Domain.ValueObjects;

namespace RestaurantReservation.Domain.Domains;

public class User : DomainBase
{
    public Name Name { get; private set; }
    public Email Email { get; private set; }
    public Password Password { get; private set; }
    public Roles Role { get; private set; }

    public User(Name name, Email email, Password password, Roles role)
    {
        Name = name;
        Email = email;
        Password = password;
        Role = role;
    }

    public void Update(Name? name = null, Email? email = null, Password? password = null, Roles? role = null)
    {
        Name = name ?? Name;
        Email = email ?? Email;
        Password = password ?? Password;
        Role = role ?? Role;
    }
}