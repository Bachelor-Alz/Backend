namespace HealthDevice.DTO;

public class Caregiver : User
{
    public List<Elder> elders { get; set; }
}