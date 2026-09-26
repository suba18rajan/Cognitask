using Cognitask.Api.Data;
using Cognitask.Api.Entities;
using Cognitask.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Cognitask.Api.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly ApplicationDbContext _context;

    public TaskRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Project?> GetProjectForUserAsync(
        Guid projectId,
        Guid userId)
    {
        return await _context.Projects
            .FirstOrDefaultAsync(p =>
                p.Id == projectId &&
                p.UserId == userId);
    }

    public async Task<TaskItem?> GetByIdAsync(
        Guid taskId,
        Guid userId)
    {
        return await _context.Tasks
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t =>
                t.Id == taskId &&
                t.Project.UserId == userId);
    }

    public async Task<(List<TaskItem> Items, int TotalCount)> GetByProjectAsync(
        Guid projectId,
        Guid userId,
        int pageNumber,
        int pageSize)
    {
        var query = _context.Tasks
            .Where(t =>
                t.ProjectId == projectId &&
                t.Project.UserId == userId)
            .OrderByDescending(t => t.CreatedAt);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task AddAsync(TaskItem task)
    {
        await _context.Tasks.AddAsync(task);
    }

    public Task UpdateAsync(TaskItem task)
    {
        _context.Tasks.Update(task);

        return Task.CompletedTask;
    }

    public Task DeleteAsync(TaskItem task)
    {
        _context.Tasks.Remove(task);

        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}