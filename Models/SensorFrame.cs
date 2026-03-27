namespace Sensore.Models;

public class SensorFrame
{
    public int FrameId { get; set; }

    public int UserId { get; set; }

    public DateTime Timestamp { get; set; }

    public string FrameData { get; set; } = string.Empty;

    public double Ppi { get; set; }

    public double ContactArea { get; set; }

    public bool IsFlagged { get; set; }

    public User? Patient { get; set; }

    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
