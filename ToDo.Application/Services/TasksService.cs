using ToDo.Application.DTOs.Input;
using ToDo.Application.DTOs.Output;
using ToDo.Application.Exceptions;
using ToDo.Application.Services.Interfaces;
using ToDo.Core.Entities;
using ToDo.Core.Enums;
using ToDo.Core.Interfaces.Repositories;

namespace ToDo.Application.Services;

public class TasksService(ITasksRepository repository) : ITasksService
{
    public async Task<TaskOutputDto> CreateAsync(CreateTaskInputDto input,
        CancellationToken cancellationToken = default)
    {
        if (input is null)
        {
            throw new TaskValidationException("Os dados da tarefa são obrigatórios.");
        }

        ValidateTask(input.Title, input.Status);

        var task = new TaskEntity(Guid.NewGuid(), input.Title.Trim(), input.Description, input.DueDate, input.Status);
        var createdTask = await repository.CreateAsync(task, cancellationToken);

        return TaskOutputDto.FromEntity(createdTask);
    }

    public async Task<TaskOutputDto?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        ValidateId(id);

        var task = await repository.GetByIdAsync(id, cancellationToken);

        return task is null ? null : TaskOutputDto.FromEntity(task);
    }

    public async Task<IReadOnlyList<TaskOutputDto>> GetAllAsync(TaskFilterInputDto input,
        CancellationToken cancellationToken = default)
    {
        if (input is null)
        {
            throw new TaskValidationException("Os parâmetros de consulta são obrigatórios.");
        }

        if (input.Status.HasValue && !Enum.IsDefined(input.Status.Value))
        {
            throw new TaskValidationException("O status informado é inválido.");
        }

        var tasks = await repository.GetAllAsync(input.Status, input.DueDate, cancellationToken);

        return tasks.Select(TaskOutputDto.FromEntity).ToList();
    }

    public async Task<TaskOutputDto?> UpdateAsync(Guid id, UpdateTaskInputDto input,
        CancellationToken cancellationToken = default)
    {
        ValidateId(id);

        if (input is null)
        {
            throw new TaskValidationException("Os dados da tarefa são obrigatórios.");
        }

        ValidateTask(input.Title, input.Status);

        var task = await repository.GetByIdAsync(id, cancellationToken);

        if (task is null)
        {
            return null;
        }

        task.Update(input.Title.Trim(), input.Description, input.DueDate, input.Status);
        await repository.UpdateAsync(task, cancellationToken);

        return TaskOutputDto.FromEntity(task);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        ValidateId(id);

        var task = await repository.GetByIdAsync(id, cancellationToken);

        if (task is null)
        {
            return false;
        }

        await repository.DeleteAsync(task, cancellationToken);

        return true;
    }

    private static void ValidateTask(string? title, StatusTask status)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new TaskValidationException("O título é obrigatório.");
        }

        if (!Enum.IsDefined(status))
        {
            throw new TaskValidationException("O status informado é inválido.");
        }
    }

    private static void ValidateId(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new TaskValidationException("O identificador da tarefa é inválido.");
        }
    }
}