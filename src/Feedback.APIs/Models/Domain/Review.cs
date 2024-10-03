namespace Feedback.APIs.Models.Domain;

public class Review
{

    public long Id { get; set; }

    public required string ReviewerName { get; set; }

    public required string Comment { get; set; }

    public int SubjectId { get; set; }

    public int Rate { get; set; }

    public DateTime Date { get; set; }
}
