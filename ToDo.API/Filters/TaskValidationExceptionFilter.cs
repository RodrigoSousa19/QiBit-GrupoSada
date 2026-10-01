using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ToDo.Application.Exceptions;
using ToDo.Core.Responses;

namespace ToDo.API.Filters;

public sealed class TaskValidationExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not TaskValidationException exception)
        {
            return;
        }

        context.Result = new BadRequestObjectResult(Response<object>.Error(exception.Message));
        context.ExceptionHandled = true;
    }
}
