using System.ComponentModel.DataAnnotations;

namespace Sensore.Models;

public class User
{
    public int UserId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(72)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<SensorFrame> Frames { get; set; } = new List<SensorFrame>();

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
