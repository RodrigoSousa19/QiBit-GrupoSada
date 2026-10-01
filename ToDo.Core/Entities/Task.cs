using ToDo.Core.Enums;

namespace ToDo.Core.Entities;

public class TaskEntity(Guid id, string title, string? description, DateOnly? dueDate, StatusTask status)
{
    public Guid Id { get; private set; } = id;
    public string Title { get; private set; } = title;
    public string? Description { get; private set; } = description;
    public DateOnly? DueDate { get; private set; } = dueDate;
    public StatusTask Status { get; private set; } = status;

    public void Update(string title, string? description, DateOnly? dueDate, StatusTask status)
    {
        Title = title;
        Description = description;
        DueDate = dueDate;
        Status = status;
    }
}