using Barber.Domain.Enums;

namespace Barber.Application.DTOs.Appointments;

public class AppointmentFilterRequest
{
    public int? BarberId { get; set; }
    public int? ClientId { get; set; }
    public DateTime? Date { get; set; }
    public AppointmentStatus?  Status { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public int? HairCutId { get; set; }
    
}