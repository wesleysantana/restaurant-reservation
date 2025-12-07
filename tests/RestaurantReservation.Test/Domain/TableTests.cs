using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Domain.Enums;
using RestaurantReservation.Domain.ValueObjects;

namespace RestaurantReservation.Test.Domain;

public class TableTests
{
    [Fact]
    public void Update_WithAllNull_ShouldKeepOriginalValues()
    {
        // Arrange
        var originalName = new Name("Mesa 01");
        var originalCapacity = new Capacity(4);
        var table = new Table(originalName, originalCapacity, StatusTable.Disponivel);

        // Act
        table.Update();

        // Assert
        Assert.Equal("Mesa 01", table.Name.Value);
        Assert.Equal(4, table.Capacity.Value);
        Assert.Equal(StatusTable.Disponivel, table.Status);
    }

    [Fact]
    public void Update_ShouldChangeOnlyName_WhenNameProvided()
    {
        // Arrange
        var originalName = new Name("Mesa 01");
        var originalCapacity = new Capacity(4);
        var table = new Table(originalName, originalCapacity, StatusTable.Disponivel);

        var newName = new Name("Mesa VIP");

        // Act
        table.Update(name: newName);

        // Assert
        Assert.Equal("Mesa VIP", table.Name.Value);
        Assert.Equal(4, table.Capacity.Value);
        Assert.Equal(StatusTable.Disponivel, table.Status);
    }

    [Fact]
    public void Update_ShouldChangeOnlyCapacity_WhenCapacityProvided()
    {
        // Arrange
        var originalName = new Name("Mesa 01");
        var originalCapacity = new Capacity(4);
        var table = new Table(originalName, originalCapacity, StatusTable.Disponivel);

        var newCapacity = new Capacity(6);

        // Act
        table.Update(capacity: newCapacity);

        // Assert
        Assert.Equal("Mesa 01", table.Name.Value);
        Assert.Equal(6, table.Capacity.Value);
        Assert.Equal(StatusTable.Disponivel, table.Status);
    }

    [Fact]
    public void Update_ShouldChangeStatus_WhenStatusProvided()
    {
        // Arrange
        var table = new Table(new Name("Mesa 01"), new Capacity(4), StatusTable.Disponivel);

        // Act
        table.Update(status: StatusTable.Reservada);

        // Assert
        Assert.Equal(StatusTable.Reservada, table.Status);
    }
}