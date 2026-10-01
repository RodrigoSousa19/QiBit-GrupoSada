using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ToDo.Core.Interfaces.Repositories;
using ToDo.Infrastructure.Database;
using ToDo.Infrastructure.Repositories;

namespace ToDo.Infrastructure;

public static class InfrastructureModule
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddInfrastructure()
        {
            services.AddDbContext<ToDoDbContext>(options => options.UseInMemoryDatabase("ToDoDatabase"));

            services.AddRepository();
            return services;
        }

        private IServiceCollection AddRepository()
        {
            services.AddScoped<ITasksRepository, TasksRepository>();
            return services;
        }
    }
}