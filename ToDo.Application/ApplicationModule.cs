using Microsoft.Extensions.DependencyInjection;

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
        return services;
    }
}