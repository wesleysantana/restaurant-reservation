using FluentAssertions;
using RestaurantReservation.Domain.Domains;
using RestaurantReservation.Domain.Enums;
using RestaurantReservation.Domain.Exceptions;
using RestaurantReservation.Domain.ValueObjects;

namespace RestaurantReservation.UnitTests.Domains;

[Trait("Domain", "Table")]
public class TableTests
{
    private static Table CreateTable(string name = "Mesa 01", int capacity = 5, StatusTable status = StatusTable.Available)
    {
        var capacityDomain = new Capacity(capacity);
        return new(new Name(name), capacityDomain, status);
    }
        

    [Fact(DisplayName = "Constructor - populates properties correctly.")]
    public void ConstructorFillsProperties()
    {
        var table = CreateTable("Mesa A", 6, StatusTable.Reserved);

        table.Name.Value.Should().Be("Mesa A");
        table.Capacity.Value.Should().Be(6);
        table.Status.Should().Be(StatusTable.Reserved);
        table.Id.Should().NotBe(Guid.Empty);
    }

    [Fact(DisplayName = "Update - Name only")]
    public void UpdateNameOnly()
    {
        var table = CreateTable();
        var originalId = table.Id;
        var originalCapacity = table.Capacity;
        var originalStatus = table.Status;

        table.Update(name: new Name("Mesa X"));

        table.Name.Value.Should().Be("Mesa X");
        table.Capacity.Should().Be(originalCapacity);
        table.Status.Should().Be(originalStatus);
        table.Id.Should().Be(originalId);
    }

    [Fact(DisplayName = "Update - Capacity only")]
    public void UpdateCapacityOnly()
    {
        var table = CreateTable(capacity: 2);
        var originalId = table.Id;
        var originalName = table.Name;
        var originalStatus = table.Status;

        table.Update(capacity: new Capacity(10));

        table.Name.Should().Be(originalName);
        table.Capacity.Value.Should().Be(10);
        table.Status.Should().Be(originalStatus);
        table.Id.Should().Be(originalId);
    }

    [Fact(DisplayName = "Update - Status only")]
    public void UpdateStatusOnly()
    {
        var table = CreateTable(status: StatusTable.Available);
        var originalId = table.Id;
        var originalName = table.Name;
        var originalCapacity = table.Capacity;

        table.Update(status: StatusTable.Inactive);

        table.Name.Should().Be(originalName);
        table.Capacity.Should().Be(originalCapacity);
        table.Status.Should().Be(StatusTable.Inactive);
        table.Id.Should().Be(originalId);
    }

    [Fact(DisplayName = "Update - multiple fields")]
    public void UpdateMultipleFields()
    {
        var table = CreateTable("Mesa 1", 4, StatusTable.Available);
        var originalId = table.Id;

        table.Update(
            name: new Name("Mesa 1B"),
            capacity: new Capacity(8),
            status: StatusTable.Reserved);

        table.Name.Value.Should().Be("Mesa 1B");
        table.Capacity.Value.Should().Be(8);
        table.Status.Should().Be(StatusTable.Reserved);
        table.Id.Should().Be(originalId);
    }

    [Fact(DisplayName = "Update - all null (no-op)")]
    public void UpdateAllNullNoOp()
    {
        var table = CreateTable("Mesa Z", 3, StatusTable.Reserved);
        var originalId = table.Id;
        var originalName = table.Name;
        var originalCapacity = table.Capacity;
        var originalStatus = table.Status;

        table.Update();

        table.Name.Should().Be(originalName);
        table.Capacity.Should().Be(originalCapacity);
        table.Status.Should().Be(originalStatus);
        table.Id.Should().Be(originalId);
    }

    [Theory(DisplayName = "Update - capacity zero/negative")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void UpdateCapacityZeroOrNegative(int newCapacity)
    {
        var table = CreateTable(capacity: 4);

        Action act = () => table.Update(capacity: new Capacity(newCapacity));

        act.Should()
            .Throw<DomainException>()
            .WithMessage("The capacity must have a value greater than zero");
    }
}