using Cognitask.Api.Data;
using Cognitask.Api.Entities;
using Cognitask.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Cognitask.Api.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly ApplicationDbContext _context;

    public ProjectRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Project?> GetByIdAsync(
        Guid id,
        Guid userId)
    {
        return await _context.Projects
            .FirstOrDefaultAsync(p =>
                p.Id == id &&
                p.UserId == userId);
    }

    public async Task<List<Project>> GetAllAsync(
        Guid userId)
    {
        return await _context.Projects
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(Project project)
    {
        await _context.Projects.AddAsync(project);
    }

    public Task UpdateAsync(Project project)
    {
        _context.Projects.Update(project);

        return Task.CompletedTask;
    }

    public Task DeleteAsync(Project project)
    {
        _context.Projects.Remove(project);

        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}