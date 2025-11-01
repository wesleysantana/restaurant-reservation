namespace RestaurantReservation.Domain.Enums;

public enum Roles
{
    Customer,
    Administrator
}

public enum StatusTable
{
    Available,
    Reserved,
    Inactive
}

public enum StatusReservation
{
    Active,
    Canceled
}