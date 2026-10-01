using ToDo.Core.Enums;

namespace ToDo.Application.DTOs.Input;

public class TaskFilterInputDto
{
    public StatusTask? Status { get; init; }
    public DateOnly? DueDate { get; init; }
}