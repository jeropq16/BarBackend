using System.Reflection;
using Barber.Application.DTOs.Appointments;
using Barber.Application.Interfaces;
using Barber.Domain.Enums;
using Barber.Domain.Interfaces;
using Barber.Domain.Models;

namespace Barber.Application.Services.Appointments;

public class AppointmentService
{
    private readonly IAppointmentRepository _appointmentRepo;
    private readonly IHairCutRepository _haircutRepo;
    private readonly IUserRepository _userRepo;
    private readonly IEmailService _emailService;

    public AppointmentService(
        IAppointmentRepository appointmentRepo, IHairCutRepository haircutRepo,  IUserRepository userRepo,  IEmailService emailService)
    {
        _appointmentRepo = appointmentRepo;
        _haircutRepo = haircutRepo;
        _userRepo = userRepo;
        _emailService = emailService;
    }
    
    
    public async Task<IEnumerable<BarberAvailabilitySlot>> GetAvailabilityAsync(int barberId, DateTime date, int haircutId)
    {
        // 1. Validar servicio
        var hairCut = await _haircutRepo.GetByIdAsync(haircutId);
        if (hairCut == null)
            throw new Exception("Servicio no encontrado.");

        int duration = hairCut.DurationMinutes;

        // 2. Horarios estándar del barbero
        var workStart = new TimeSpan(9, 0, 0);
        var workEnd = new TimeSpan(18, 0, 0);

        // 3. Generamos todos los slots posibles
        var slots = new List<BarberAvailabilitySlot>();

        var current = date.Date.Add(workStart);
        var endOfDay = date.Date.Add(workEnd);

        while (current.AddMinutes(duration) <= endOfDay)
        {
            slots.Add(new BarberAvailabilitySlot
            {
                Start = current,
                End = current.AddMinutes(duration)
            });

            current = current.AddMinutes(30); // Intervalos cada 30 min
        }

        // 4. Traer citas existentes del barbero en ese día
        var appointments = await _appointmentRepo.GetAllForBarberAsync(barberId);

        appointments = appointments
            .Where(a => a.StartTime.Date == date.Date)
            .ToList();

        // 5. Eliminar slots que choquen con citas existentes
        foreach (var appt in appointments)
        {
            slots.RemoveAll(slot =>
                slot.Start < appt.EndTime &&
                slot.End > appt.StartTime
            );
        }

        // 6. Bloquear horas pasadas si es hoy
        if (date.Date == DateTime.Today)
        {
            slots = slots
                .Where(s => s.Start > DateTime.Now)
                .ToList();
        }

        return slots;
    }

    //1 validae cliente
    public async Task<AppointmentResponse> CreateAppointmentAsync(CreateAppointmentRequest request)
    {
        var client = await _userRepo.GetByIdAsync(request.ClientId);
        if (client == null) throw new Exception("El cliente no existe.");

        if (!client.IsActive) throw new Exception("El cliente no está activo.");

        //2 validar barbero

        var barber = await _userRepo.GetByIdAsync(request.BarberId);

        if (barber == null) throw new Exception("El Barber no existe.");

        if (barber.Role.ToString() != "Barber") throw new Exception("El usuaario seleccionado no es barbero");

        if (!barber.IsActive) throw new Exception("El barbero no está activo");

        // validar corte

        var haircut = await _haircutRepo.GetByIdAsync(request.HairCutId);

        if (haircut == null) throw new Exception("el corte no existe.");

        int duration = haircut.DurationMinutes;

        // validacion fechas

        if (request.StartTime < DateTime.Now) throw new Exception("no puedes viajar al pasado perro hpta");

        //calcular fin de corte

        DateTime endTime = request.StartTime.AddMinutes(duration);

        //validar que se dupliqeun horarios

        var appointments = await _appointmentRepo.GetAllForBarberAsync(request.BarberId);

        bool hasConflict = appointments.Any(a => a.StartTime <= endTime && a.EndTime > request.StartTime);
        
        if (hasConflict) throw new Exception("EL barbero ya tiene cita en ese horario");
        
        //crear cita
        
        var appointment = new Appointment
        {
            ClientId = request.ClientId,
            BarberId = request.BarberId,
            HairCutId = request.HairCutId,
            StartTime = request.StartTime,
            EndTime = endTime,
            Status = AppointmentStatus.Pending,
            PaymentStatus = PaymentStatus.Pending
        };
        

        if (client != null)
        {
            string subject = "Confirmación de tu cita en la barbería";
            string body = $@"
            <h2>Hola {client.FullName} 👋</h2>
            <p>Tu cita ha sido creada correctamente.</p>
            <p><b>Barbero:</b> {barber?.FullName}</p>
            <p><b>Fecha:</b> {appointment.StartTime:dd/MM/yyyy}</p>
            <p><b>Hora:</b> {appointment.StartTime:HH:mm}</p>
            <p><b>Estado:</b> {appointment.Status}</p>
            <br/>
            <p>Gracias por confiar en nuestra barbería 💈</p>";

            await _emailService.SendEmailAsync(client.Email, subject, body);
        }

        if (barber != null)
        {
            string subjectB = "Nueva cita asignada";
            string bodyB = $@"
            <h2>Hola {barber.FullName}</h2>
            <p>Tienes una nueva cita agendada.</p>
            <p><b>Cliente:</b> {client?.FullName}</p>
            <p><b>Fecha:</b> {appointment.StartTime:dd/MM/yyyy}</p>
            <p><b>Hora:</b> {appointment.StartTime:HH:mm}</p>";
            
            await _emailService.SendEmailAsync(barber.Email, subjectB, bodyB);
        }
        
        
        await _appointmentRepo.AddAsync(appointment);

        return new AppointmentResponse
        {
            Id = appointment.Id,

            ClientId = appointment.ClientId,
            ClientName = client.FullName,

            BarberId = appointment.BarberId,
            BarberName = barber.FullName,

            HairCutId = appointment.HairCutId,
            HairCutName = haircut.Name,

            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            Status = appointment.Status,
            PaymentStatus = appointment.PaymentStatus
        };

        
    }

    public async Task<AppointmentResponse> UpdateAppointmentAsync(int appointmentId, int userId, string userRole,
        UpdateAppointmentRequest  request) 
    {
        //obtener cita 
         var appointment = await _appointmentRepo.GetByIdAsync(appointmentId);
         if (appointment == null) throw new Exception("la cita no existe");
         
         //validar estado actual 
         if (appointment.Status == AppointmentStatus.Completed)
             throw new Exception("no puedes modificar una cita que ya fue completada");

         if (appointment.Status == AppointmentStatus.Canceled)
             throw new Exception("no puedes modificar una cita que esta canelada");
         
         //permisos segun role 

         if (userRole == "Client" && appointment.ClientId != userId)
             throw new Exception("no puedes modificar una cita que no es tuya.");

         if (userRole == "Barber" && appointment.BarberId != userId)
             throw new UnauthorizedAccessException("solo puedes modificar citas de tus clientes");
         
         //validar feha no pasada

         if (request.StartTime < DateTime.Now) throw new Exception("No puedes mover la cita a una hora pasada");
         
         //Obtener servicio para duracion 
         
         var haircut = await _haircutRepo.GetByIdAsync(appointment.HairCutId);
         
         if (haircut == null) throw new Exception("Corte no encontrado");
         
         int duration  = haircut.DurationMinutes;
         DateTime newEnd = request.StartTime.AddMinutes(duration);
         
         //validar hoques de horarios 
         
         var appointments = await _appointmentRepo.GetAllForBarberAsync(appointment.BarberId);
         
         bool  conflict = appointments.Any(a => a.Id != appointment.Id && a.StartTime < newEnd && a.EndTime > request.StartTime);

         if (conflict) throw new Exception("El barbero ya tiene otra cita en ese horario");
         
         //Actualizar cita 
         
         appointment.StartTime = request.StartTime;
         appointment.EndTime = newEnd;

         await _appointmentRepo.UpdateAsync(appointment);
         
         //Respuesta 

         return new AppointmentResponse
         {
             Id = appointment.Id,
             ClientId = appointment.ClientId,
             BarberId = appointment.BarberId,
             HairCutId = appointment.HairCutId,
             StartTime = appointment.StartTime,
             EndTime = appointment.EndTime,
             Status = appointment.Status,
             PaymentStatus = appointment.PaymentStatus
         };
    }

    public async Task<AppointmentResponse> UpdatePaymentStatusAsync(int userId, string userRole,
        UpdatePaymentStatusRequest request)
    {
        //obtener cita
        
        var appointment = await _appointmentRepo.GetByIdAsync(request.AppointmentId);
        
        if (appointment == null) throw new Exception("la cita no existe");
        
        //No permitir cambios si esta cancelada

        if (appointment.Status == AppointmentStatus.Canceled)
            throw new Exception("No puedes cambiar el estado de pago de una cita cancelada");
        
        // no permitir cambios si esta completada
        
        if (appointment.Status == AppointmentStatus.Completed)
            throw new Exception("No puedes cambiar el estado de pago de una cita completada");
        
        // validar permisos

        bool isOwner = appointment.ClientId == userId;

        if (userRole == "Client" && !isOwner)
            throw new Exception("No puedes cambiar el estado de una cita");
        
        //admin si puede cambiar estados
        
        //actucalizar estado de pago 
        
        appointment.PaymentStatus = request.PaymentStatus;
        
        await _appointmentRepo.UpdateAsync(appointment);
        
        //FALTA EMAIL
        
        //respuesta

        return new AppointmentResponse
        {
            Id = appointment.Id,
            ClientId = appointment.ClientId,
            BarberId = appointment.BarberId,
            HairCutId = appointment.HairCutId,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            Status = appointment.Status,
            PaymentStatus = appointment.PaymentStatus
        };
    }

    public async Task CancelAppointmentAsync(int appointmentId, int userId, string userRole)
    {
        //obetener cita
        
        var appointment = await _appointmentRepo.GetByIdAsync(appointmentId);
        
        if  (appointment == null) throw new Exception("La appointment no existe");
        
        //validar estado

        if (appointment.Status == AppointmentStatus.Completed)
            throw new Exception("No puedes cancelar una cita completada estupido");
        
        if (appointment.Status == AppointmentStatus.Canceled) return;
        
        //validar permisos
        
        bool isOwner = appointment.ClientId == userId;

        if (userRole == "Client" && !isOwner) throw new Exception("AH si con que cancelando citas de otro");
        
        // cancelar cita
        
        appointment.Status =  AppointmentStatus.Canceled;
        
        await _appointmentRepo.UpdateAsync(appointment);
        
        //FALTA EMAIL

      
    }
    
    public async Task<AppointmentResponse> CompleteAppointmentAsync(int appointmentId, int userId, string userRole)
    {
        //obtener cita
        
        var appointment = await _appointmentRepo.GetByIdAsync(appointmentId);
        
        if (appointment == null) throw new Exception("La appointment no existe");
        
        //validar estado

        if (appointment.Status == AppointmentStatus.Completed) throw new Exception("La cita ya esta compltado");

        if (appointment.Status == AppointmentStatus.Canceled)
            throw new Exception("No puedes completar una cita ya cancelada");
        
        bool isBarber =appointment.BarberId == userId;

        if (userRole == "Barber" && !isBarber)
            throw new UnauthorizedAccessException("No se meta en lo que no le importa");

        if (userRole == "Client") throw new UnauthorizedAccessException("Los clinetes no pueden completar citas");
        
        //completar cita
        
        appointment.Status =  AppointmentStatus.Completed;
        
        await _appointmentRepo.UpdateAsync(appointment);
        
        //FALTA EMAIL

        return new AppointmentResponse
        {
            Id = appointment.Id,
            ClientId = appointment.ClientId,
            BarberId = appointment.BarberId,
            HairCutId = appointment.HairCutId,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            Status = appointment.Status,
            PaymentStatus = appointment.PaymentStatus
        };
    }

    public async Task<IEnumerable<AppointmentResponse>> GetAllAppointmentsAsync(int userId, string userRole)
    {
        var appointments = await _appointmentRepo.GetAllAsync();

        if (userRole == "Client")
            appointments = appointments.Where(a => a.ClientId == userId).ToList();

        if (userRole == "Barber")
            appointments = appointments.Where(a => a.BarberId == userId).ToList();

        return await MapAppointmentsAsync(appointments);
    }



    public async Task<IEnumerable<AppointmentResponse>> FilterAppointmentsAsync(
        int userId, string userRole, AppointmentFilterRequest filter)
    {
        var appointments = await _appointmentRepo.GetAllAsync();

        if (userRole == "Client")
            appointments = appointments.Where(a => a.ClientId == userId).ToList();

        if (userRole == "Barber")
            appointments = appointments.Where(a => a.BarberId == userId).ToList();

        if (filter.BarberId.HasValue)
            appointments = appointments.Where(a => a.BarberId == filter.BarberId.Value).ToList();

        if (filter.ClientId.HasValue)
            appointments = appointments.Where(a => a.ClientId == filter.ClientId.Value).ToList();

        if (filter.HairCutId.HasValue)
            appointments = appointments.Where(a => a.HairCutId == filter.HairCutId.Value).ToList();

        if (filter.Date.HasValue)
            appointments = appointments.Where(a => a.StartTime.Date == filter.Date.Value.Date).ToList();

        if (filter.Status.HasValue)
            appointments = appointments.Where(a => a.Status == filter.Status.Value).ToList();

        if (filter.PaymentStatus.HasValue)
            appointments = appointments.Where(a => a.PaymentStatus == filter.PaymentStatus.Value).ToList();

        return await MapAppointmentsAsync(appointments);
    }

    
    public async Task<IEnumerable<AppointmentResponse>> GetAllPublicAsync()
    {
        var appointments = await _appointmentRepo.GetAllAsync();
        return await MapAppointmentsAsync(appointments);
    }

    
    public async Task<IEnumerable<AppointmentResponse>> FilterAppointmentsPublicAsync(
        AppointmentFilterRequest filter)
    {
        var appointments = await _appointmentRepo.GetAllAsync();

        if (filter.BarberId.HasValue)
            appointments = appointments.Where(a => a.BarberId == filter.BarberId.Value).ToList();

        if (filter.ClientId.HasValue)
            appointments = appointments.Where(a => a.ClientId == filter.ClientId.Value).ToList();

        if (filter.HairCutId.HasValue)
            appointments = appointments.Where(a => a.HairCutId == filter.HairCutId.Value).ToList();

        if (filter.Date.HasValue)
            appointments = appointments.Where(a => a.StartTime.Date == filter.Date.Value.Date).ToList();

        if (filter.Status.HasValue)
            appointments = appointments.Where(a => a.Status == filter.Status.Value).ToList();

        if (filter.PaymentStatus.HasValue)
            appointments = appointments.Where(a => a.PaymentStatus == filter.PaymentStatus.Value).ToList();

        return appointments.Select(a => new AppointmentResponse
        {
            Id = a.Id,
            ClientId = a.ClientId,
            BarberId = a.BarberId,
            HairCutId = a.HairCutId,
            StartTime = a.StartTime,
            EndTime = a.EndTime,
            Status = a.Status,
            PaymentStatus = a.PaymentStatus
        });
    }
    
    private async Task<List<AppointmentResponse>> MapAppointmentsAsync(IEnumerable<Appointment> appointments)
    {   
        var users = await _userRepo.GetAllAsync();
        var hairCuts = await _haircutRepo.GetAllAsync();

        return appointments.Select(a =>
        {
            var client = users.FirstOrDefault(u => u.Id == a.ClientId);
            var barber = users.FirstOrDefault(u => u.Id == a.BarberId);
            var hairCut = hairCuts.FirstOrDefault(h => h.Id == a.HairCutId);

            return new AppointmentResponse
            {
                Id = a.Id,

                ClientId = a
                    .ClientId,
                ClientName = client?.FullName,

                BarberId = a.BarberId,
                BarberName = barber?.FullName,

                HairCutId = a.HairCutId,
                HairCutName = hairCut?.Name,

                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Status = a.Status,
                PaymentStatus = a.PaymentStatus
            };
        }).ToList();
    }



}