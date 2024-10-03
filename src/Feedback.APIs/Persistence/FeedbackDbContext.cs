using Feedback.APIs.Models.Domain;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Feedback.APIs.Persistence;

public class FeedbackDbContext(DbContextOptions<FeedbackDbContext> dbContextOptions) 
    : DbContext(dbContextOptions)
{

    public const string ConnectionStringName = "SvcDbContext";

    public DbSet<Subject> Subjects { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

}
