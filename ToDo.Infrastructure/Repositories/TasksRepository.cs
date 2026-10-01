using Microsoft.EntityFrameworkCore;
using ToDo.Core.Entities;
using ToDo.Core.Enums;
using ToDo.Core.Interfaces.Repositories;
using ToDo.Infrastructure.Database;

namespace ToDo.Infrastructure.Repositories;

public class TasksRepository(ToDoDbContext context) : ITasksRepository
{
    public async Task<TaskEntity> CreateAsync(TaskEntity task, CancellationToken cancellationToken = default)
    {
        context.Tasks.Add(task);
        await context.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task<TaskEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Tasks.FindAsync([id], cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<TaskEntity>> GetAllAsync(StatusTask? status = null, DateOnly? dueDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.Tasks.AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(task => task.Status == status.Value);
        }

        if (dueDate.HasValue)
        {
            query = query.Where(task => task.DueDate == dueDate.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(TaskEntity task, CancellationToken cancellationToken = default)
    {
        context.Tasks.Update(task);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(TaskEntity task, CancellationToken cancellationToken = default)
    {
        context.Tasks.Remove(task);
        await context.SaveChangesAsync(cancellationToken);
    }
}