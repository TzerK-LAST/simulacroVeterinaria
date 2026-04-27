namespace VeterinaraApp.Models;

public class Medication
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Dose { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public int Stock { get; set; }
}

public class Treatment
{
    public int Id { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string Observations { get; set; } = string.Empty;
    public int AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;
    public List<TreatmentMedication> TreatmentMedications { get; set; } = new();
}

public class TreatmentMedication
{
    public int Id { get; set; }
    public int TreatmentId { get; set; }
    public Treatment Treatment { get; set; } = null!;
    public int MedicationId { get; set; }
    public Medication Medication { get; set; } = null!;
}
