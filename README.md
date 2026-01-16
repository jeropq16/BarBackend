# Documentación General del Backend

## Índice
- [Barber.Api](#barberapi)
- [Barber.Application](#barberapplication)
- [Barber.Domain](#barberdomain)
- [Barber.Infrastructure](#barberinfrastructure)

---

## Barber.Api
### Controladores
- **AppointmentController**: Controlador para gestionar las citas (crear, actualizar, listar, cancelar, etc.).
- **AuthController**: Controlador para autenticación y registro de usuarios.
- **HairCutsController**: Controlador para operaciones sobre cortes de cabello.
- **UsersController**: Controlador para gestión de usuarios.

### Middlewares
- **ErrorHandlerMiddleware**: Middleware para manejo global de errores en las peticiones HTTP.

### Policies
- **OnlyAdminAttribute**: Atributo para restringir acceso solo a administradores.
- **OwnerOrAdminHandler**: Handler para verificar si el usuario es dueño del recurso o administrador.
- **OwnerOrAdminRequirement**: Requisito de autorización personalizado.

---

## Barber.Application
### Servicios
- **UserService**: Lógica de negocio para usuarios.
- **HairCutService**: Lógica de negocio para cortes de cabello.
- **AuthService**: Lógica de autenticación y registro.
- **AppointmentService**: Lógica de negocio para citas.

### DTOs (Data Transfer Objects)
- **UsersProfileResponse, UpdateUserRequest, CreateUserRequest, CreateBarberRequest**: Modelos para transferir datos de usuario.
- **HairCutResponse**: Modelo para transferir datos de cortes de cabello.
- **LoginRequest, RegisterRequest, LoginResponse, GoogleLoginResponse, GoogleLoginRequest, RegisterResponse**: Modelos para autenticación.
- **UpdatePaymentStatusRequest, UpdateAppointmentRequest, CreateAppointmentRequest, BarberAvailabilitySlot, BarberAvailabilityRequest, AppointmentResponse**: Modelos para citas y disponibilidad.

### Interfaces
- **IEmailService**: Contrato para servicios de email.
- **IFileStorageService**: Contrato para almacenamiento de archivos.
- **IJwtService**: Contrato para manejo de JWT.
- **IPasswordHasher**: Contrato para hasheo de contraseñas.

---

## Barber.Domain
### Entidades
- **User**: Entidad de usuario.
- **HairCut**: Entidad de corte de cabello.
- **Appointment**: Entidad de cita.

### Interfaces
- **IUserRepository**: Contrato para repositorio de usuarios.
- **IHairCutRepository**: Contrato para repositorio de cortes de cabello.
- **IAppointmentRepository**: Contrato para repositorio de citas.

### Enums
- **UserRole, AppointmentStatus, PaymentStatus**: Enumeraciones para roles, estado de citas y pagos.

---

## Barber.Infrastructure
### Servicios
- **PasswordHasher**: Implementación de hasheo de contraseñas.
- **JwtService**: Implementación de manejo de JWT.
- **EmailService**: Implementación de envío de emails.
- **CloudinaryFileStorageService**: Implementación de almacenamiento de archivos en Cloudinary.

### Repositorios
- **UserRepository**: Implementación de repositorio de usuarios.
- **HairCutRepository**: Implementación de repositorio de cortes de cabello.
- **AppointmentRepository**: Implementación de repositorio de citas.

### Configuración y Data
- **AppDbContext**: Contexto principal de Entity Framework.
- **DbSeeder**: Inicialización de datos.
- **AppointmentConfig, HairCutConfig, UserConfig**: Configuración de entidades para EF.
- **CloudinarySettings, EmailSettings, GoogleSettings, JwtConfig**: Configuración de servicios externos.

### Migrations
- **Clases de migración y snapshot**: Control de versiones de la base de datos.

---
