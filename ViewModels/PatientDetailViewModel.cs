using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using Sensore.Infrastructure;
using Sensore.Models;
using Sensore.Services;

namespace Sensore.ViewModels;

public class PatientDetailViewModel : ViewModelBase
{
    private readonly MainViewModel _mainViewModel;
    private readonly PatientFrameQueryService _patientFrameQueryService;
    private readonly ReportService _reportService;

    private ObservableCollection<HeatmapCell> _heatmapCells = BuildDefaultHeatmap();
    private ObservableCollection<CommentDisplayItem> _commentThread = new();
    private FlaggedFrameDisplayItem? _selectedFlaggedFrame;
    private CommentDisplayItem? _selectedComment;
    private string _replyText = string.Empty;
    private string _statusMessage = string.Empty;
    private DateTime? _reportDateFrom = DateTime.Today.AddDays(-1);
    private DateTime? _reportDateTo = DateTime.Today;

    public PatientDetailViewModel(
        MainViewModel mainViewModel,
        User clinicianUser,
        ClinicianPatientSummary selectedPatient,
        PatientFrameQueryService patientFrameQueryService,
        ReportService reportService)
    {
        _mainViewModel = mainViewModel;
        _patientFrameQueryService = patientFrameQueryService;
        _reportService = reportService;
        ClinicianUser = clinicianUser;
        SelectedPatient = selectedPatient;

        BackCommand = new RelayCommand(_ => _mainViewModel.NavigateToDashboard(ClinicianUser));
        ReplyToCommentCommand = new RelayCommand(async _ => await ReplyToCommentAsync());
        GenerateReportCommand = new RelayCommand(async _ => await GenerateReportAsync());

        _ = LoadAsync();
    }

    public User ClinicianUser { get; }

    public ClinicianPatientSummary SelectedPatient { get; }

    public ObservableCollection<HeatmapCell> HeatmapCells
    {
        get => _heatmapCells;
        private set
        {
            _heatmapCells = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<FlaggedFrameDisplayItem> FlaggedFrames { get; } = new();

    public ObservableCollection<CommentDisplayItem> CommentThread
    {
        get => _commentThread;
        private set
        {
            _commentThread = value;
            OnPropertyChanged();
        }
    }

    public FlaggedFrameDisplayItem? SelectedFlaggedFrame
    {
        get => _selectedFlaggedFrame;
        set
        {
            _selectedFlaggedFrame = value;
            OnPropertyChanged();
            _ = LoadSelectedFrameAsync();
        }
    }

    public CommentDisplayItem? SelectedComment
    {
        get => _selectedComment;
        set
        {
            _selectedComment = value;
            OnPropertyChanged();
        }
    }

    public string ReplyText
    {
        get => _replyText;
        set
        {
            _replyText = value;
            OnPropertyChanged();
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public DateTime? ReportDateFrom
    {
        get => _reportDateFrom;
        set
        {
            _reportDateFrom = value;
            OnPropertyChanged();
        }
    }

    public DateTime? ReportDateTo
    {
        get => _reportDateTo;
        set
        {
            _reportDateTo = value;
            OnPropertyChanged();
        }
    }

    public ICommand BackCommand { get; }

    public ICommand ReplyToCommentCommand { get; }

    public ICommand GenerateReportCommand { get; }

    private async Task LoadAsync()
    {
        try
        {
            var frames = await _patientFrameQueryService.GetFlaggedFramesForClinicianAsync(ClinicianUser.UserId, SelectedPatient.PatientId);
            FlaggedFrames.Clear();

            foreach (var frame in frames)
            {
                FlaggedFrames.Add(new FlaggedFrameDisplayItem
                {
                    FrameId = frame.FrameId,
                    Timestamp = frame.Timestamp,
                    Ppi = frame.Ppi,
                    ContactArea = frame.ContactArea
                });
            }

            if (FlaggedFrames.Count > 0)
            {
                SelectedFlaggedFrame = FlaggedFrames[0];
            }
            else
            {
                StatusMessage = "No flagged frames available for this patient.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
    }

    private async Task LoadSelectedFrameAsync()
    {
        try
        {
            if (SelectedFlaggedFrame == null)
            {
                return;
            }

            var frame = await _patientFrameQueryService.GetFrameForClinicianAsync(ClinicianUser.UserId, SelectedFlaggedFrame.FrameId);
            if (frame == null)
            {
                StatusMessage = "Frame not found.";
                return;
            }

            HeatmapCells = BuildHeatmap(frame.FrameData);

            var comments = await _patientFrameQueryService.GetCommentsForPatientForClinicianAsync(ClinicianUser.UserId, SelectedPatient.PatientId);
            CommentThread = new ObservableCollection<CommentDisplayItem>(FlattenComments(comments));
            StatusMessage = comments.Count == 0
                ? $"Loaded frame {frame.FrameId}. No comments found yet."
                : $"Loaded frame {frame.FrameId} with {comments.Count} comment(s).";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Unable to load frame/comments: {ex.Message}";
        }
    }

    private async Task ReplyToCommentAsync()
    {
        if (SelectedComment == null)
        {
            StatusMessage = "Select a comment first.";
            return;
        }

        if (string.IsNullOrWhiteSpace(ReplyText))
        {
            StatusMessage = "Enter a reply before submitting.";
            return;
        }

        await _patientFrameQueryService.AddClinicianReplyAsync(
            ClinicianUser.UserId,
            SelectedComment.FrameId,
            SelectedComment.CommentId,
            ReplyText.Trim());

        ReplyText = string.Empty;
        await LoadSelectedFrameAsync();
    }

    private async Task GenerateReportAsync()
    {
        var from = ReportDateFrom?.Date;
        var to = ReportDateTo?.Date.AddDays(1).AddTicks(-1);

        if (!from.HasValue || !to.HasValue)
        {
            StatusMessage = "Select report date range.";
            return;
        }

        var filePath = await _reportService.GeneratePatientSummaryReportAsync(
            ClinicianUser.UserId,
            SelectedPatient.PatientId,
            DateTime.SpecifyKind(from.Value, DateTimeKind.Utc),
            DateTime.SpecifyKind(to.Value, DateTimeKind.Utc));

        StatusMessage = $"Report generated: {filePath}";
    }

    private static IEnumerable<CommentDisplayItem> FlattenComments(IReadOnlyCollection<Comment> comments)
    {
        var roots = comments
            .Where(c => c.ParentCommentId == null)
            .OrderBy(c => c.CreatedAt)
            .ToList();

        var byParent = comments
            .Where(c => c.ParentCommentId.HasValue)
            .GroupBy(c => c.ParentCommentId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderBy(x => x.CreatedAt).ToList());

        foreach (var root in roots)
        {
            yield return ToDisplay(root, 0);

            foreach (var nested in Traverse(root.CommentId, 1))
            {
                yield return nested;
            }
        }

        IEnumerable<CommentDisplayItem> Traverse(int parentId, int level)
        {
            if (!byParent.TryGetValue(parentId, out var children))
            {
                yield break;
            }

            foreach (var c in children)
            {
                yield return ToDisplay(c, level);

                foreach (var nested in Traverse(c.CommentId, level + 1))
                {
                    yield return nested;
                }
            }
        }

        static CommentDisplayItem ToDisplay(Comment c, int level)
        {
            return new CommentDisplayItem
            {
                CommentId = c.CommentId,
                FrameId = c.FrameId,
                AuthorName = c.Author?.FullName ?? "Unknown",
                CreatedAt = c.CreatedAt,
                Content = c.Content,
                Level = level
            };
        }
    }

    private static ObservableCollection<HeatmapCell> BuildDefaultHeatmap()
    {
        return new ObservableCollection<HeatmapCell>(
            Enumerable.Range(0, 1024).Select(_ => new HeatmapCell { Color = new SolidColorBrush(Color.FromRgb(0, 0, 255)) }));
    }

    private static ObservableCollection<HeatmapCell> BuildHeatmap(string frameData)
    {
        var values = frameData
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(token => int.TryParse(token, out var v) ? v : 1)
            .Take(1024)
            .ToList();

        if (values.Count < 1024)
        {
            values.AddRange(Enumerable.Repeat(1, 1024 - values.Count));
        }

        return new ObservableCollection<HeatmapCell>(
            values.Select(v => new HeatmapCell { Color = new SolidColorBrush(MapColor(v)) }));
    }

    private static Color MapColor(int value)
    {
        var clamped = Math.Clamp(value, 1, 255);
        if (clamped <= 128)
        {
            var t = (clamped - 1) / 127d;
            var r = (byte)(255 * t);
            var g = (byte)(255 * t);
            return Color.FromRgb(r, g, 255);
        }

        var u = (clamped - 128) / 127d;
        var green = (byte)(255 * (1 - u));
        return Color.FromRgb(255, green, 0);
    }
}
