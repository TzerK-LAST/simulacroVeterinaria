namespace VeterinaraApp.Models;

public enum AppointmentStatus
{
    Programada,
    Cancelada,
    Atendida,
    NoAsistio
}

public class Appointment
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Programada;
    public int PetId { get; set; }
    public Pet Pet { get; set; } = null!;
    public int OwnerId { get; set; }
    public Owner Owner { get; set; } = null!;
    public int VetId { get; set; }
    public Vet Vet { get; set; } = null!;
    public Treatment? Treatment { get; set; }
}
