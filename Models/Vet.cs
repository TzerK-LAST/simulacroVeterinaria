namespace VeterinaraApp.Models;

public class Vet
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public TimeSpan WorkStart { get; set; }
    public TimeSpan WorkEnd { get; set; }
    public List<Appointment> Appointments { get; set; } = new();
}
