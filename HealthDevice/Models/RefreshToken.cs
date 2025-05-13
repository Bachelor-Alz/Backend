using System.ComponentModel.DataAnnotations;

namespace HealthDevice.Models
{
    public class RefreshToken
    {

        [Key]
        public int Id { get; set; }

        public required string Email { get; set; }
        public required string TokenHash { get; set; }
        public DateTime Expiration { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime? RevokedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ReplacedByTokenHash { get; set; }
        public string? CreatedByIp { get; set; }
        public string? RevokedByIp { get; set; }
        public bool IsExpired => DateTime.UtcNow >= Expiration;
    }
}
