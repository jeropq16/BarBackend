namespace Barber.Application.DTOs.Appointments;

public class CreateAppointmentRequest
{
    public int ClientId { get; set; }
    public int BarberId { get; set; }
    public int HairCutId { get; set; }
    public DateTime StartTime { get; set; }
}