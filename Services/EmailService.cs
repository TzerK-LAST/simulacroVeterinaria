using System.Net;
using System.Net.Mail;

namespace VeterinaraApp.Services;

public class EmailService
{
    private const string FromEmail = "kevinyu332@gmail.com";
    private const string SmtpHost  = "smtp.gmail.com";
    private const int    SmtpPort  = 587;
    private const string Password  = "ncwusitwmhzxabdm";

    public void SendAppointmentCreated(string toEmail, string petName, DateTime date, TimeSpan start)
        => Send(toEmail,
            "Cita creada - Veterinaria",
            $"Se ha programado una cita para {petName} el {date:dd/MM/yyyy} a las {start:hh\\:mm}.");

    public void SendAppointmentCancelled(string toEmail, string petName, DateTime date)
        => Send(toEmail,
            "Cita cancelada - Veterinaria",
            $"La cita de {petName} del {date:dd/MM/yyyy} ha sido cancelada.");

    public void SendTreatmentAssigned(string toEmail, string petName, string diagnosis)
        => Send(toEmail,
            "Tratamiento asignado - Veterinaria",
            $"Se ha registrado un tratamiento para {petName}. Diagnóstico: {diagnosis}.");

    private void Send(string to, string subject, string body)
    {
        try
        {
            var client = new SmtpClient(SmtpHost, SmtpPort)
            {
                Credentials = new NetworkCredential(FromEmail, Password),
                EnableSsl    = true
            };
            client.Send(FromEmail, to, subject, body);
            Console.WriteLine($"[Email] Enviado a {to}: {subject}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Email] Error al enviar correo: {ex.Message}");
        }
    }
}
