using Microsoft.Extensions.DependencyInjection;
using ToDo.Application.Services;
using ToDo.Application.Services.Interfaces;

namespace ToDo.Application;

public static class ApplicationModule
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddServices();
        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<ITasksService, TasksService>();
        return services;
    }
}