using Barber.Domain.Enums;

namespace Barber.Application.DTOs.Appointments;

public class UpdatePaymentStatusRequest
{
    public int AppointmentId { get; set; }
    public PaymentStatus  PaymentStatus { get; set; }
}