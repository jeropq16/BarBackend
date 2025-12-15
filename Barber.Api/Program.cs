using System.Text;
using Barber.Api.Middlewares;
using Barber.Api.Policies;
using Barber.Application.Interfaces;
using Barber.Application.Services.Appointments;
using Barber.Application.Services.Auth;
using Barber.Application.Services.Users;
using Barber.Application.Services.Services;
using Barber.Domain.Interfaces;
using Barber.Infrastructure.Config;
using Barber.Infrastructure.Data;
using Barber.Infrastructure.Repositories;
using Barber.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ──────────────────────────────────────────────
// DATABASE
// ──────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 43))));

// ──────────────────────────────────────────────
// APP SETTINGS
// ──────────────────────────────────────────────
builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("Cloudinary  "));
builder.Services.Configure<GoogleSettings>(builder.Configuration.GetSection("Google"));

// ──────────────────────────────────────────────
// JWT
// ──────────────────────────────────────────────
var jwtConfig = builder.Configuration.GetSection("Jwt").Get<JwtConfig>();
Console.WriteLine("JWT LOAD" + jwtConfig?.SecretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,

        ValidIssuer = jwtConfig.Issuer,
        ValidAudience = jwtConfig.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtConfig.SecretKey))
    };
});

// ──────────────────────────────────────────────
// AUTHORIZATION POLICIES
// ──────────────────────────────────────────────
builder.Services.AddScoped<IAuthorizationHandler, OwnerOrAdminHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ClientOnly", p => p.RequireRole("Client"));
    options.AddPolicy("BarberOnly", p => p.RequireRole("Barber"));
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));

    options.AddPolicy("OwnerOrAdmin",
        policy => policy.Requirements.Add(new OwnerOrAdminRequirement()));
});

// ──────────────────────────────────────────────
// CORS
// ──────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        p => p.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ──────────────────────────────────────────────
// REPOSITORIES
// ──────────────────────────────────────────────
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IHairCutRepository, HairCutRepository>();


// ──────────────────────────────────────────────
// SERVICES
// ──────────────────────────────────────────────
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AppointmentService>();
builder.Services.AddScoped<HairCutService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IFileStorageService, CloudinaryFileStorageService>();
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("Cloudinary"));


builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("Email"));
builder.Services.AddScoped<IEmailService, EmailService>();

// TODO: Email service

// ──────────────────────────────────────────────
// CONTROLLERS + SWAGGER
// ──────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Barber API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using Bearer. Ejemplo: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAsync(db);
}

// ──────────────────────────────────────────────
// MIDDLEWARES
// ──────────────────────────────────────────────
app.UseMiddleware<ErrorHandlerMiddleware>();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ──────────────────────────────────────────────
// SWAGGER
// ──────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI();

app.Run();
