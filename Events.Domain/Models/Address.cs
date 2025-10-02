namespace Events.Models;

public class Address(string? description, double latitude, double longitude)
{
    public int Id { get; init; }

    public string? Description { get; } = description;

    public double Latitude { get; } = latitude;

    public double Longitude { get; } = longitude;
}