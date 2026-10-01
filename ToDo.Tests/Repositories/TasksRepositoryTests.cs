using Microsoft.EntityFrameworkCore;
using ToDo.Core.Entities;
using ToDo.Core.Enums;
using ToDo.Infrastructure.Database;
using ToDo.Infrastructure.Repositories;
using Xunit;

namespace ToDo.Tests.Repositories;

public sealed class TasksRepositoryTests
{
    private static DbContextOptions<ToDoDbContext> CreateOptions() => new DbContextOptionsBuilder<ToDoDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

    [Fact]
    public async Task Create_PersisteTodosOsCampos_ParaOutroContextoLer()
    {
        var options = CreateOptions();
        var task = new TaskEntity(Guid.NewGuid(), "Título", "Descrição", new DateOnly(2026, 10, 5), StatusTask.InProgress);
        await using (var context = new ToDoDbContext(options))
        {
            await new TasksRepository(context).CreateAsync(task, cancellationToken: TestContext.Current.CancellationToken);
        }

        await using var verification = new ToDoDbContext(options);
        var persisted = await new TasksRepository(verification).GetByIdAsync(task.Id, cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(persisted);
        Assert.Equal(task.Id, persisted.Id);
        Assert.Equal(task.Title, persisted.Title);
        Assert.Equal(task.Description, persisted.Description);
        Assert.Equal(task.DueDate, persisted.DueDate);
        Assert.Equal(task.Status, persisted.Status);
    }

    [Fact]
    public async Task Update_PersisteAlteracoes_EDeleteRemoveDoBanco()
    {
        var options = CreateOptions();
        var id = Guid.NewGuid();
        await using (var seed = new ToDoDbContext(options))
        {
            seed.Tasks.Add(new TaskEntity(id, "Antigo", "Descrição", new DateOnly(2026, 10, 5), StatusTask.Pending));
            await seed.SaveChangesAsync(cancellationToken: TestContext.Current.CancellationToken);
        }

        await using (var context = new ToDoDbContext(options))
        {
            var repository = new TasksRepository(context);
            var task = await repository.GetByIdAsync(id, cancellationToken: TestContext.Current.CancellationToken);
            Assert.NotNull(task);
            task.Update("Novo", null, null, StatusTask.Completed);
            await repository.UpdateAsync(task, cancellationToken: TestContext.Current.CancellationToken);
        }

        await using (var verification = new ToDoDbContext(options))
        {
            var persisted = await verification.Tasks.SingleAsync(cancellationToken: TestContext.Current.CancellationToken);
            Assert.Equal(id, persisted.Id);
            Assert.Equal("Novo", persisted.Title);
            Assert.Null(persisted.Description);
            Assert.Null(persisted.DueDate);
            Assert.Equal(StatusTask.Completed, persisted.Status);
            await new TasksRepository(verification).DeleteAsync(persisted, cancellationToken: TestContext.Current.CancellationToken);
        }

        await using var afterDelete = new ToDoDbContext(options);
        Assert.Empty(await afterDelete.Tasks.ToListAsync(cancellationToken: TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(null, null, new[] { "A", "B", "C", "D" })]
    [InlineData(StatusTask.Pending, null, new[] { "A", "B", "D" })]
    [InlineData(null, 5, new[] { "A", "C" })]
    [InlineData(StatusTask.Pending, 5, new[] { "A" })]
    [InlineData(StatusTask.Completed, 5, new string[] { })]
    public async Task GetAll_AplicaFiltrosSeparadosECombinados_SemIncluirTarefasIndevidas(StatusTask? status, int? day, string[] expectedTitles)
    {
        var options = CreateOptions();
        await using (var seed = new ToDoDbContext(options))
        {
            seed.Tasks.AddRange(
                new TaskEntity(Guid.NewGuid(), "A", null, new DateOnly(2026, 10, 5), StatusTask.Pending),
                new TaskEntity(Guid.NewGuid(), "B", null, new DateOnly(2026, 10, 6), StatusTask.Pending),
                new TaskEntity(Guid.NewGuid(), "C", null, new DateOnly(2026, 10, 5), StatusTask.InProgress),
                new TaskEntity(Guid.NewGuid(), "D", null, null, StatusTask.Pending));
            await seed.SaveChangesAsync(cancellationToken: TestContext.Current.CancellationToken);
        }

        await using var context = new ToDoDbContext(options);
        DateOnly? dueDate = day.HasValue ? new DateOnly(2026, 10, day.Value) : null;

        var result = await new TasksRepository(context).GetAllAsync(status, dueDate, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expectedTitles, result.Select(task => task.Title).OrderBy(title => title).ToArray());
        Assert.Empty(context.ChangeTracker.Entries());
    }
}
