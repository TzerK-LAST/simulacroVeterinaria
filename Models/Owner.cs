namespace VeterinaraApp.Models;

public class Owner
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Document { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<Pet> Pets { get; set; } = new();
}
