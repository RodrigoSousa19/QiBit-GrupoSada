using ToDo.Application.DTOs.Input;
using ToDo.Application.DTOs.Output;

namespace ToDo.Application.Services.Interfaces;

public interface ITasksService
{
    Task<TaskOutputDto> CreateAsync(CreateTaskInputDto input, CancellationToken cancellationToken = default);
    Task<TaskOutputDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaskOutputDto>> GetAllAsync(TaskFilterInputDto input, CancellationToken cancellationToken = default);
    Task<TaskOutputDto?> UpdateAsync(Guid id, UpdateTaskInputDto input, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
