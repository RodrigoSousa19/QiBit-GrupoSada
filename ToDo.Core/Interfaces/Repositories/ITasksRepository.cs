using ToDo.Core.Entities;
using ToDo.Core.Enums;

namespace ToDo.Core.Interfaces.Repositories;

public interface ITasksRepository
{
    public Task<TaskEntity> CreateAsync(TaskEntity task,CancellationToken cancellationToken = default);
    public Task<TaskEntity?> GetByIdAsync(Guid id,CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaskEntity>> GetAllAsync(StatusTask? status = null,DateOnly? dueDate = null, CancellationToken cancellationToken = default);
    public Task UpdateAsync(TaskEntity task,CancellationToken cancellationToken = default);
    public Task DeleteAsync(TaskEntity task,CancellationToken cancellationToken = default);
}