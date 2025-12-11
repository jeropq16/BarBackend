using Barber.Domain.Enums;

namespace Barber.Domain.Models;

public class Appointment
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public int BarberId { get; set; }
    public int HairCutId { get; set; }
    
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    public PaymentStatus PaymentStatus { get; set; } =  PaymentStatus.Pending;
    
    public DateTime CreatedAt { get; set; } =  DateTime.Now;
    
}