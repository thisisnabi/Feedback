using FluentValidation;

namespace Feedback.APIs.Endpoints.Contracts;

public record CreateSubjectRequest(string Title, DateTime? ExpirationOn);
 
public class CreateSubjectRequestValidator : AbstractValidator<CreateSubjectRequest>
{
    public CreateSubjectRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(100);
    }
}