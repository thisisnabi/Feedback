using Feedback.APIs.Models.Domain;
using Feedback.APIs.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Feedback.APIs.Services;

public class SubjectService(FeedbackDbContext feedbackDbContext)
{
    private readonly FeedbackDbContext _feedbackDbContext = feedbackDbContext;

    public async Task<int> Create(string title, int tenantId, DateTime? ExpirationOn)
    {
        if (_feedbackDbContext.Subjects.Any(x => x.TenantId == tenantId && x.Title == title))
            throw new Exception("");

        var subject = Subject.Create(title, tenantId, ExpirationOn);
        _feedbackDbContext.Subjects.Add(subject);
        await _feedbackDbContext.SaveChangesAsync();
        return subject.Id;
    }

    internal async Task AddReview(int subjectId, string reviewerName, string comment, int rate)
    {

        var subject = _feedbackDbContext.Subjects
                                        .Include(f => f.Reviews)
                                        .FirstOrDefault(x => x.Id == subjectId);
        ArgumentNullException.ThrowIfNull(subject);
        CheckLocked(subject);
        CheckExpiredOn(subject);

        subject.AddReview(rate, comment, reviewerName);
        await _feedbackDbContext.SaveChangesAsync();
    }

    internal async Task CheckSubjectForReview(int id)
    {
        var subject = await _feedbackDbContext.Subjects.FirstOrDefaultAsync(x => x.Id == id);
        ArgumentNullException.ThrowIfNull(subject);

        CheckLocked(subject);
        CheckExpiredOn(subject);
    }


    private static void CheckLocked(Subject? subject)
    {
        if (subject.Locked)
        {
            throw new Exception("Locked");
        }
    }

    private static void CheckExpiredOn(Subject? subject)
    {
        if (subject.ExpiredOn is not null && subject.ExpiredOn < DateTime.Now)
        {
            throw new Exception("Expired");
        }
    }
}
