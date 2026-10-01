namespace ToDo.Application.Exceptions;

public sealed class TaskValidationException(string message) : Exception(message)
{
}
