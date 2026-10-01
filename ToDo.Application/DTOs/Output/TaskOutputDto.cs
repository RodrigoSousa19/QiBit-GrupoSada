using ToDo.Core.Entities;
using ToDo.Core.Enums;

namespace ToDo.Application.DTOs.Output;

public class TaskOutputDto(Guid id, string title, string? description, DateOnly? dueDate,StatusTask status)
{
    public Guid Id { get; private set; } = id;
    public string Title { get; private set; } = title;
    public string? Description { get; private set; } = description;
    public DateOnly? DueDate { get; private set; } = dueDate;
    public StatusTask Status { get; private set; } = status;

    public static TaskOutputDto FromEntity(TaskEntity entity) => new(entity.Id, entity.Title, entity.Description, entity.DueDate, entity.Status);
}