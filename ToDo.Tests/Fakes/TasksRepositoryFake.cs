using ToDo.Core.Entities;
using ToDo.Core.Enums;
using ToDo.Core.Interfaces.Repositories;

namespace ToDo.Tests.Fakes;

internal sealed class TasksRepositoryFake : ITasksRepository
{
    public TaskEntity? ExistingTask { get; set; }
    public IReadOnlyList<TaskEntity> ListedTasks { get; set; } = [];
    public TaskEntity? CreatedTask { get; private set; }
    public TaskEntity? UpdatedTask { get; private set; }
    public TaskEntity? DeletedTask { get; private set; }
    public int Calls { get; private set; }
    public StatusTask? ReceivedStatus { get; private set; }
    public DateOnly? ReceivedDueDate { get; private set; }
    public CancellationToken ReceivedToken { get; private set; }
    public Exception? Failure { get; set; }

    private void Record(CancellationToken cancellationToken)
    {
        Calls++;
        ReceivedToken = cancellationToken;
        if (Failure is not null) throw Failure;
        cancellationToken.ThrowIfCancellationRequested();
    }

    public Task<TaskEntity> CreateAsync(TaskEntity task, CancellationToken cancellationToken = default)
    {
        Record(cancellationToken);
        CreatedTask = task;
        return Task.FromResult(task);
    }

    public Task<TaskEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Record(cancellationToken);
        return Task.FromResult(ExistingTask?.Id == id ? ExistingTask : null);
    }

    public Task<IReadOnlyList<TaskEntity>> GetAllAsync(StatusTask? status = null, DateOnly? dueDate = null, CancellationToken cancellationToken = default)
    {
        Record(cancellationToken);
        ReceivedStatus = status;
        ReceivedDueDate = dueDate;
        return Task.FromResult(ListedTasks);
    }

    public Task UpdateAsync(TaskEntity task, CancellationToken cancellationToken = default)
    {
        Record(cancellationToken);
        UpdatedTask = task;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(TaskEntity task, CancellationToken cancellationToken = default)
    {
        Record(cancellationToken);
        DeletedTask = task;
        return Task.CompletedTask;
    }
}
