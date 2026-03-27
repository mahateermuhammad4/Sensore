using Microsoft.EntityFrameworkCore;
using Sensore.Data;
using Sensore.Models;

namespace Sensore.Services;

public class AlertService
{
    private readonly AppDbContext _dbContext;
    private readonly Dictionary<int, int> _consecutiveHighCounts = new();

    public AlertService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public event EventHandler<AlertTriggeredEventArgs>? AlertTriggered;

    public Task EvaluateAsync(int userId, SensorFrame frame)
    {
        if (frame.Ppi > 200)
        {
            var current = _consecutiveHighCounts.TryGetValue(userId, out var existing) ? existing : 0;
            current++;
            _consecutiveHighCounts[userId] = current;

            if (current == 3)
            {
                var alert = new Alert
                {
                    Frame = frame,
                    AlertType = "HighPressure",
                    CreatedAt = DateTime.UtcNow,
                    IsAcknowledged = false
                };

                frame.IsFlagged = true;
                _dbContext.Alerts.Add(alert);

                AlertTriggered?.Invoke(this, new AlertTriggeredEventArgs
                {
                    UserId = userId,
                    FrameId = frame.FrameId == 0 ? null : frame.FrameId,
                    TriggeredAt = alert.CreatedAt,
                    Message = $"High pressure detected at {alert.CreatedAt:HH:mm}."
                });
            }
        }
        else
        {
            _consecutiveHighCounts[userId] = 0;
        }

        return Task.CompletedTask;
    }

    public async Task AcknowledgeLatestAsync(int userId)
    {
        var latestAlert = await _dbContext.Alerts
            .Include(a => a.Frame)
            .Where(a => a.Frame != null && a.Frame.UserId == userId && !a.IsAcknowledged)
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefaultAsync();

        if (latestAlert == null)
        {
            return;
        }

        latestAlert.IsAcknowledged = true;
        await _dbContext.SaveChangesAsync();
    }
}
