using Barber.Domain.Enums;

namespace Barber.Application.DTOs.Appointments;

public class AppointmentResponse
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string? ClientName {get; set;}
    public int BarberId { get; set; }
    public string? BarberName {get; set;}
    public int HairCutId { get; set; }
    public string? HairCutName {get; set;}
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public AppointmentStatus Status { get; set; }
    public PaymentStatus  PaymentStatus { get; set; }
}