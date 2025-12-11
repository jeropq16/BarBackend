using Barber.Domain.Enums;

namespace Barber.Application.DTOs.Appointments;

public class AppointmentResponse
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public int BarberId { get; set; }
    public int HairCutId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public AppointmentStatus Status { get; set; }
    public PaymentStatus  PaymentStatus { get; set; }
}