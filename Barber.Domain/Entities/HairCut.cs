namespace Barber.Domain.Models;

public class HairCut
{
    public int Id { get; set; }
    public string? Name { get; set; } = null;
    public string? Description { get; set; }
    public double Price { get; set; }
    public int DurationMinutes { get; set; }
    public bool IsActive { get; set; } = true;

}