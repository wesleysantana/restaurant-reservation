using Bogus;
using FluentAssertions;
using RestaurantReservation.Domain.Domains;
using RestaurantReservation.Domain.Enums;
using RestaurantReservation.Domain.Exceptions;
using RestaurantReservation.Domain.ValueObjects;

namespace RestaurantReservation.UnitTests.Domains;

[Trait("Domain", "User")]
public class UserTests
{
    private static User CreateUserValid()
    {
        var faker = new Faker();
        return new User(
            new Name(faker.Name.FullName()),
            new Email(faker.Internet.Email()),
            new Password(Guid.NewGuid().ToString()),
            (new Random()).Next(0, 2) == 0 ? Roles.Customer : Roles.Administrator
        );
    }

    [Fact(DisplayName = "Create User")]
    public void CreateUser()
    {
        var user = CreateUserValid();

        Action action = () => new User(user.Name, user.Email, user.Password, user.Role);
        action.Should().NotThrow();
    }

    [Theory(DisplayName = "Empty or null name")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void NameEmptyOrNull(string? name)
    {
        Action action = () => new Name(name!);

        action.Should()
            .Throw<DomainException>()
            .WithMessage("The name field cannot be empty or null");
    }

    [Fact(DisplayName = "Create Name with less than 3 characters should throw exception")]
    public void NameShorterThan3Characters()
    {
        var faker = new Faker();
        var shortName = faker.Name.FullName()[..2]; // 2 characters

        Action action = () => new Name(shortName);

        action.Should()
            .Throw<DomainException>()
            .WithMessage("The name must be between 3 and 255 characters long");
    }

    [Fact(DisplayName = "Create Name with more than 255 characters should throw exception")]
    public void NameLongerThan255Characters()
    {
        var faker = new Faker();
        var longName = faker.Random.String(256); // 256 caracteres

        Action action = () => new Name(longName);

        action.Should()
            .Throw<DomainException>()
            .WithMessage("The name must be between 3 and 255 characters long");
    }

    [Fact(DisplayName = "Create Email Invalid")]
    public void EmailInvalid()
    {
        var faker = new Faker();
        var emailInvalid = faker.Internet.Email().Replace("@", "_");

        Action action = () => new Email(emailInvalid);

        action.Should()
            .Throw<DomainException>()
            .WithMessage("Invalid Email");
    }

    [Theory(DisplayName = "Empty or null email")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void EmailEmptyOrNull(string? email)
    {
        Action action = () => new Email(email!);

        action.Should()
            .Throw<DomainException>()
            .WithMessage("The email field cannot be empty or null");
    }

    [Theory(DisplayName = "Empty or null password")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void PasswordEmptyOrNull(string? password)
    {
        Action action = () => new Password(password!);

        action.Should()
            .Throw<DomainException>()
            .WithMessage("The password field cannot be empty or null");
    }

    [Fact(DisplayName = "Create Password with less than 6 characters should throw exception")]
    public void PasswordShorterThan6Characters()
    {
        var shortPassword = Guid.NewGuid().ToString()[..5]; // 5 characters

        Action action = () => new Password(shortPassword);

        action.Should()
            .Throw<DomainException>()
            .WithMessage("The password must be between 6 and 255 characters long");
    }

    [Fact(DisplayName = "Create Password with more than 255 characters should throw exception")]
    public void PasswordLongerThan255Characters()
    {
        var faker = new Faker();
        var longPassword = faker.Random.String(256); // 256 caracteres

        Action action = () => new Password(longPassword);

        action.Should()
            .Throw<DomainException>()
            .WithMessage("The password must be between 6 and 255 characters long");
    }

    [Fact(DisplayName = "Update - Name only")]
    public void UpdateNameOnly()
    {
        var user = CreateUserValid();
        var originalId = user.Id;
        var originalEmail = user.Email;
        var originalRole = user.Role;
        var originalPassword = user.Password;

        var newName = new Name("New Name");
        user.Update(name: newName);

        user.Name.Should().Be(newName);
        user.Email.Should().Be(originalEmail);
        user.Password.Should().Be(originalPassword);
        user.Role.Should().Be(originalRole);
        user.Id.Should().Be(originalId);
    }

    [Fact(DisplayName = "Update - Email only")]
    public void UpdateEmailOnly()
    {
        var user = CreateUserValid();
        var originalName = user.Name;
        var originalId = user.Id;

        var newEmail = new Email("new.email@test.com");
        user.Update(email: newEmail);

        user.Email.Should().Be(newEmail);
        user.Name.Should().Be(originalName);
        user.Id.Should().Be(originalId);
    }

    [Fact(DisplayName = "Update - Password only")]
    public void UpdatePasswordOnly()
    {
        var user = CreateUserValid();
        var originalId = user.Id;

        var newPassword = new Password("password-super-safe");
        user.Update(password: newPassword);

        user.Password.Should().Be(newPassword);
        user.Id.Should().Be(originalId);
    }

    [Fact(DisplayName = "Update - Role only")]
    public void Update_Role_Only()
    {
        var user = CreateUserValid();
        var originalId = user.Id;

        user.Update(role: Roles.Administrator);

        user.Role.Should().Be(Roles.Administrator);
        user.Id.Should().Be(originalId);
    }

    [Fact(DisplayName = "Update - Multiple fields")]
    public void UpdateMultipleFields()
    {
        var user = CreateUserValid();
        var originalId = user.Id;

        var newName = new Name("Name Updated");
        var newEmail = new Email("updated@test.com");
        var newPassword = new Password("new-password");
        var newRole = Roles.Administrator;

        user.Update(newName, newEmail, newPassword, newRole);

        user.Name.Should().Be(newName);
        user.Email.Should().Be(newEmail);
        user.Password.Should().Be(newPassword);
        user.Role.Should().Be(newRole);
        user.Id.Should().Be(originalId);
    }

    [Fact(DisplayName = "Update - All null (no-op) should keep values")]
    public void UpdateAllNullShouldKeepValues()
    {
        var user = CreateUserValid();
        var originalId = user.Id;
        var originalName = user.Name;
        var originalEmail = user.Email;
        var originalPassword = user.Password;
        var originalRole = user.Role;

        user.Update();

        user.Name.Should().Be(originalName);
        user.Email.Should().Be(originalEmail);
        user.Password.Should().Be(originalPassword);
        user.Role.Should().Be(originalRole);
        user.Id.Should().Be(originalId);
    }

    [Fact(DisplayName = "Update - With Invalid Email")]    
    public void UpdateWithInvalidEmailThrows()
    {
        var user = CreateUserValid();

        Action act = () => user.Update(email: new Email("foo"));

        act.Should().Throw<DomainException>()
           .WithMessage("Invalid Email");
    }

    [Theory(DisplayName = "Update - Update With Empty Email")]
    [InlineData("")]                
    [InlineData("   ")]            
    public void UpdateWithEmptyEmailThrows(string invalidEmail)
    {
        var user = CreateUserValid();

        Action act = () => user.Update(email: new Email(invalidEmail));

        act.Should().Throw<DomainException>()
           .WithMessage("The email field cannot be empty or null");
    }

    [Fact(DisplayName = "Update - With Invalid Name")]   
    public void UpdateWithInvalidNameThrows()
    {
        var user = CreateUserValid();

        Action act = () => user.Update(name: new Name("Jo"));

        act.Should().Throw<DomainException>()
           .WithMessage("The name must be between 3 and 255 characters long");
    }

    [Theory(DisplayName = "Update - Update Empty Name")]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateWithEmptyNameThrows(string invalidName)
    {
        var user = CreateUserValid();

        Action act = () => user.Update(name: new Name(invalidName));

        act.Should().Throw<DomainException>()
           .WithMessage("The name field cannot be empty or null");
    }

    [Fact(DisplayName = "Update - With Invalid Password")]   
    public void UpdateWithInvalidPasswordThrows()
    {
        var user = CreateUserValid();

        Action act = () => user.Update(password: new Password("12345"));

        act.Should().Throw<DomainException>()
           .WithMessage("The password must be between 6 and 255 characters long");
    }

    [Theory(DisplayName = "Update - With Empty Password")]
    [InlineData("")]      
    [InlineData("   ")]
    public void UpdateWithEmptyPasswordThrows(string invalidPassword)
    {
        var user = CreateUserValid();

        Action act = () => user.Update(password: new Password(invalidPassword));

        act.Should().Throw<DomainException>()
           .WithMessage("The password field cannot be empty or null");
    }

    [Fact(DisplayName = "Update - With Malformed Email")]
    public void Update_WithMalformedEmail_ThrowsExactMessage()
    {
        var user = CreateUserValid();

        Action act = () => user.Update(email: new Email("sem-arroba-e-dominio"));

        act.Should().Throw<DomainException>()
           .WithMessage("Invalid Email");
    }
}