using Feedback.APIs.Endpoints.Contracts;
using Feedback.APIs.Models.Domain;
using Feedback.APIs.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;

namespace Feedback.APIs.Endpoints;

public static class SubjectEndpoints
{

    public static void MapSubjectEndpoint(this IEndpointRouteBuilder endpoint)
    {
        var group = endpoint.MapGroup("/subject");

        group.MapPost("/", CreateSubject);
        group.MapGet("/{id}/check", CheckSubjectForReview);
        group.MapPost("/review/", CreateReview);
        group.MapGet("/review/ranking/{id}", GetRanking);
        group.MapGet("/{id}/review/", GetAllReviews);
    }
     
    public static async Task<Results<ValidationProblem,Created>> CreateSubject(FeedbackDbContext dbContext,
            IValidator<CreateSubjectRequest> Validator,
            IUserPrincipal userPrincipal,
            IConfiguration configuration,
            CreateSubjectRequest request)

    {
        var validate = Validator.Validate(request);
        if (!validate.IsValid)
        {
            return TypedResults.ValidationProblem(validate.ToDictionary());
        }

        var subject = Subject.Create(request.Title, userPrincipal.TenantId, request.ExpirationOn);
        dbContext.Subjects.Add(subject);
        await dbContext.SaveChangesAsync();

        var longUrl = $"{configuration["BaseUrl"]}/subject/{subject.Id}/ckeck";
        // shortener url

        return TypedResults.Created(longUrl);
    }

    public static async Task<Results<ValidationProblem, Ok, NotFound, BadRequest<string>>> CreateReview(FeedbackDbContext dbContext,
        IValidator<CreateReviewRequest> Validator,
        IConfiguration configuration,
        CreateReviewRequest request)

    {
        var validate = Validator.Validate(request);
        if (!validate.IsValid)
        {
            return TypedResults.ValidationProblem(validate.ToDictionary());
        }

        var subject = dbContext.Subjects
                               .Include(f => f.Reviews)
                               .FirstOrDefault(x => x.Id == request.SubjectId);
        if (subject is null)
        {
            return TypedResults.NotFound();
        }

        if (subject.Locked)
        {
            return TypedResults.BadRequest("your subject locked");
        }

        if (subject.ExpiredOn is not null && subject.ExpiredOn < DateTime.Now)
        {
            return TypedResults.BadRequest("your subject expired");
        }

        subject.AddReview(request.Rate, request.Comment, request.ReviewerName);
        await dbContext.SaveChangesAsync();

        return TypedResults.Ok();
    }

    public static async Task<Results<NotFound, Ok, BadRequest<string>>> CheckSubjectForReview(FeedbackDbContext dbContext,
            IConfiguration configuration,
            [FromRoute] int id)
    {
        var subject = dbContext.Subjects.FirstOrDefault(x => x.Id == id);

        if (subject is null)
        {
            return TypedResults.NotFound();
        }

        if(subject.Locked)
        {
            return TypedResults.BadRequest("your subject locked");
        }

        if (subject.ExpiredOn is not null && subject.ExpiredOn < DateTime.Now)
        {
            return TypedResults.BadRequest("your subject expired");
        }

        return TypedResults.Ok();
    }

    public static async Task<Results<NotFound, Ok<double>>> GetRanking(FeedbackDbContext dbContext,
        [FromRoute] int id)
    {
        var subject = dbContext.Subjects.Include(d => d.Reviews).FirstOrDefault(x => x.Id == id);
        if (subject is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(subject.Reviews.Average(f => f.Rate));
    }   
    
    public static async Task<Results<NotFound, Ok<IEnumerable<ReviewResponse>>>> GetAllReviews(FeedbackDbContext dbContext,
        [FromRoute] int id)
    {
        var subject = dbContext.Subjects.Include(d => d.Reviews).FirstOrDefault(x => x.Id == id);
        if (subject is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(subject.Reviews.Select(f => new ReviewResponse { 
            Comment = f.Comment,
        }));
    }
}


public class ReviewResponse
{
    public string Comment { get; set; }

}


