using Cognitask.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cognitask.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
}