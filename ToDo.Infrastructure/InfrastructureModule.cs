using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ToDo.Infrastructure.Database;

namespace ToDo.Infrastructure;

public static class InfrastructureModule
{
    public static IServiceCollection  AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<ToDoDbContext>(options => options.UseInMemoryDatabase("ToDoDatabase"));
        
        services.AddRepository();
        return services;
    }

    private static IServiceCollection AddRepository(this IServiceCollection services)
    {
        return services;
    }
}