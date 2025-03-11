namespace HealthDevice.Models;

public class Caregiver : User
{
    public List<Elder> elders { get; set; }
}