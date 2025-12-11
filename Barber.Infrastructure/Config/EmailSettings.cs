namespace Barber.Infrastructure.Config;

public class EmailSettings
{
    public string From { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string SmtpServer { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
}  