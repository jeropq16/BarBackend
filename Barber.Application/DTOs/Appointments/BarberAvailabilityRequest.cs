namespace Barber.Application.DTOs.Appointments;

public class BarberAvailabilityRequest
{
    public int BarberId { get; set; }
    public DateTime Date { get; set; }
    public int HairCutId { get; set; }
}