namespace Feedback.APIs.Models.Domain;

public class Subject
{
    public int Id { get; set; }

    public DateTime CreatedOn { get; set; }

    public bool Locked { get; set; }

    public DateTime? ExpiredOn { get; set; }

    public required string Title { get; set; }

    public required int TenantId { get; set; }

    public ICollection<Review> Reviews { get; set; }

    public static Subject Create(string title, int tenantId,DateTime? expiredOn) => new Subject
    {
        ExpiredOn = expiredOn,
        Title = title,
        TenantId = tenantId
    };

    internal void AddReview(int rate, string comment, string reviewerName)
    {
        Reviews ??= new List<Review>();
        Reviews.Add(new Review
        {
            Comment = comment,
            ReviewerName = reviewerName,
            Rate = rate
        });
    }
}
