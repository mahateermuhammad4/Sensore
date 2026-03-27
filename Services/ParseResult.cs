namespace Sensore.Services;

public class ParseResult
{
    public int FramesSaved { get; set; }

    public List<string> Errors { get; } = new();

    public double LastPpi { get; set; }

    public double LastContactArea { get; set; }
}
