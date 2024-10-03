using Feedback.APIs.Endpoints.Contracts;
using Feedback.APIs.Models.Domain;
using Feedback.APIs.Persistence;
using Feedback.APIs.Services;
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
     
    public static async Task<Results<ValidationProblem,Created>> CreateSubject(SubjectService service,
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

        var subjecId = await service.Create(request.Title, userPrincipal.TenantId, request.ExpirationOn);
        var longUrl = $"{configuration["BaseUrl"]}/subject/{subjecId}/ckeck";
        // shortener url
        return TypedResults.Created(longUrl);
    }

    public static async Task<Results<ValidationProblem, Ok, NotFound, BadRequest<string>>>
        CreateReview(SubjectService service,
        IValidator<CreateReviewRequest> Validator,
        IConfiguration configuration,
        CreateReviewRequest request)

    {

        var validate = Validator.Validate(request);
        if (!validate.IsValid)
        {
            return TypedResults.ValidationProblem(validate.ToDictionary());
        }


        try
        {
            await service.AddReview(request.SubjectId, request.ReviewerName, request.Comment, request.Rate);
            return TypedResults.Ok();
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }

    }

    public static async Task<Results<NotFound, Ok, BadRequest<string>>> CheckSubjectForReview(SubjectService service,
            IConfiguration configuration,
            [FromRoute] int id)
    {
        try
        {
            await service.CheckSubjectForReview(id);
            return TypedResults.Ok();
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
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


