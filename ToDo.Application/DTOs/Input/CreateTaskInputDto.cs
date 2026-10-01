using ToDo.Core.Enums;

namespace ToDo.Application.DTOs.Input;

public class CreateTaskInputDto
{
    public required string Title { get; init; }
    public string? Description { get; init; }
    public DateOnly? DueDate { get; init; }
    public required StatusTask Status { get; init; }
}