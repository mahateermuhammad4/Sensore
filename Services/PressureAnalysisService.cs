using System.Globalization;

namespace Sensore.Services;

public class PressureAnalysisService
{
    private const int GridSize = 32;
    private const int Threshold = 50;

    public int CalculatePpi(int[,] frame)
    {
        var visited = new bool[GridSize, GridSize];
        var maxCandidate = 0;

        for (var row = 0; row < GridSize; row++)
        {
            for (var col = 0; col < GridSize; col++)
            {
                if (visited[row, col])
                {
                    continue;
                }

                visited[row, col] = true;
                if (frame[row, col] <= Threshold)
                {
                    continue;
                }

                var queue = new Queue<(int r, int c)>();
                queue.Enqueue((row, col));

                var regionCount = 0;
                var regionMax = 0;

                while (queue.Count > 0)
                {
                    var (r, c) = queue.Dequeue();
                    regionCount++;
                    if (frame[r, c] > regionMax)
                    {
                        regionMax = frame[r, c];
                    }

                    foreach (var (nr, nc) in GetNeighbors(r, c))
                    {
                        if (visited[nr, nc])
                        {
                            continue;
                        }

                        visited[nr, nc] = true;
                        if (frame[nr, nc] > Threshold)
                        {
                            queue.Enqueue((nr, nc));
                        }
                    }
                }

                if (regionCount >= 10)
                {
                    maxCandidate = Math.Max(maxCandidate, regionMax);
                }
            }
        }

        return maxCandidate;
    }

    public double CalculateContactArea(int[,] frame)
    {
        var count = 0;
        for (var r = 0; r < GridSize; r++)
        {
            for (var c = 0; c < GridSize; c++)
            {
                if (frame[r, c] > Threshold)
                {
                    count++;
                }
            }
        }

        return (count / 1024d) * 100d;
    }

    public int[,] BuildFrame(IReadOnlyList<string> rows)
    {
        if (rows.Count != GridSize)
        {
            throw new ArgumentException("Exactly 32 rows are required to build one frame.", nameof(rows));
        }

        var frame = new int[GridSize, GridSize];
        for (var r = 0; r < GridSize; r++)
        {
            var tokens = rows[r].Split(',', StringSplitOptions.TrimEntries);
            if (tokens.Length != GridSize)
            {
                throw new FormatException($"Row {r + 1} does not contain 32 values.");
            }

            for (var c = 0; c < GridSize; c++)
            {
                if (!int.TryParse(tokens[c], NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
                {
                    throw new FormatException($"Invalid integer '{tokens[c]}' at row {r + 1}, column {c + 1}.");
                }

                frame[r, c] = value;
            }
        }

        return frame;
    }

    private static IEnumerable<(int r, int c)> GetNeighbors(int r, int c)
    {
        if (r > 0) yield return (r - 1, c);
        if (r < GridSize - 1) yield return (r + 1, c);
        if (c > 0) yield return (r, c - 1);
        if (c < GridSize - 1) yield return (r, c + 1);
    }
}
