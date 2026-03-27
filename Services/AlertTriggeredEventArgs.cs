namespace Sensore.Services;

public class AlertTriggeredEventArgs : EventArgs
{
    public int UserId { get; init; }

    public int? FrameId { get; init; }

    public DateTime TriggeredAt { get; init; }

    public string Message { get; init; } = string.Empty;
}
