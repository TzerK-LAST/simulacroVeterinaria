namespace VeterinaraApp.Models;

public class Pet
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
    public string Breed { get; set; } = string.Empty;
    public int Age { get; set; }
    public double Weight { get; set; }
    public int OwnerId { get; set; }
    public Owner Owner { get; set; } = null!;
    public List<Appointment> Appointments { get; set; } = new();
}
