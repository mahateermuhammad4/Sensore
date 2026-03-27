namespace Sensore.Models;

public class Comment
{
    public int CommentId { get; set; }

    public int FrameId { get; set; }

    public int AuthorId { get; set; }

    public int? ParentCommentId { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public User? Author { get; set; }

    public SensorFrame? Frame { get; set; }

    public Comment? Parent { get; set; }

    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
}
