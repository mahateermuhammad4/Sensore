namespace Sensore.Models;

public class Alert
{
    public int AlertId { get; set; }

    public int FrameId { get; set; }

    public string AlertType { get; set; } = "HighPressure";

    public DateTime CreatedAt { get; set; }

    public bool IsAcknowledged { get; set; }

    public SensorFrame? Frame { get; set; }
}
