using ToDo.Application.DTOs.Input;
using ToDo.Application.Exceptions;
using ToDo.Application.Services;
using ToDo.Core.Entities;
using ToDo.Core.Enums;
using ToDo.Tests.Fakes;
using Xunit;

namespace ToDo.Tests.Services;

public sealed class TasksServiceTests
{
    [Fact]
    public async Task Create_GeraIdUnico_NormalizaTitulo_EMapeiaTodosOsCampos()
    {
        var repository = new TasksRepositoryFake();
        var service = new TasksService(repository);
        var dueDate = new DateOnly(2026, 10, 5);
        var input = new CreateTaskInputDto { Title = "  Revisar desafio  ", Description = "Verificar filtros", DueDate = dueDate, Status = StatusTask.InProgress };

        var first = await service.CreateAsync(input, cancellationToken: TestContext.Current.CancellationToken);
        var second = await service.CreateAsync(input, cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotEqual(Guid.Empty, first.Id);
        Assert.NotEqual(first.Id, second.Id);
        Assert.Equal("Revisar desafio", first.Title);
        Assert.Equal(input.Description, first.Description);
        Assert.Equal(dueDate, first.DueDate);
        Assert.Equal(StatusTask.InProgress, first.Status);
        Assert.Equal(second.Id, repository.CreatedTask!.Id);
        Assert.Equal(2, repository.Calls);
    }

    [Fact]
    public async Task Create_AceitaDescricaoEVencimentoAusentes()
    {
        var repository = new TasksRepositoryFake();
        var service = new TasksService(repository);

        var result = await service.CreateAsync(new CreateTaskInputDto { Title = "Título", Status = StatusTask.Pending }, TestContext.Current.CancellationToken);

        Assert.Null(result.Description);
        Assert.Null(result.DueDate);
        Assert.NotNull(repository.CreatedTask);
        Assert.Null(repository.CreatedTask.Description);
        Assert.Null(repository.CreatedTask.DueDate);
    }

    [Fact]
    public async Task Update_SubstituiDescricaoEVencimentoComNovosValores()
    {
        var entity = new TaskEntity(Guid.NewGuid(), "Antigo", "Descrição antiga", new DateOnly(2026, 10, 5), StatusTask.Pending);
        var repository = new TasksRepositoryFake { ExistingTask = entity };
        var service = new TasksService(repository);
        var newDate = new DateOnly(2026, 10, 8);

        var result = await service.UpdateAsync(entity.Id, new UpdateTaskInputDto { Title = "Novo", Description = "Descrição nova", DueDate = newDate, Status = StatusTask.InProgress }, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("Descrição nova", result.Description);
        Assert.Equal(newDate, result.DueDate);
        Assert.Equal(StatusTask.InProgress, result.Status);
        Assert.Same(entity, repository.UpdatedTask);
        Assert.Equal("Descrição nova", entity.Description);
        Assert.Equal(newDate, entity.DueDate);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Create_EUpdate_RejeitamTituloInvalido_SemAcessarRepositorio(string? title)
    {
        var repository = new TasksRepositoryFake();
        var service = new TasksService(repository);

        await Assert.ThrowsAsync<TaskValidationException>(() => service.CreateAsync(new CreateTaskInputDto { Title = title!, Status = StatusTask.Pending }, cancellationToken: TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<TaskValidationException>(() => service.UpdateAsync(Guid.NewGuid(), new UpdateTaskInputDto { Title = title!, Status = StatusTask.Pending }, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(0, repository.Calls);
    }

    [Fact]
    public async Task StatusInvalido_ERejeitadoNaCriacaoAtualizacaoEConsulta()
    {
        var repository = new TasksRepositoryFake();
        var service = new TasksService(repository);
        var invalidStatus = (StatusTask)999;

        await Assert.ThrowsAsync<TaskValidationException>(() => service.CreateAsync(new CreateTaskInputDto { Title = "Titulo", Status = invalidStatus }, cancellationToken: TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<TaskValidationException>(() => service.UpdateAsync(Guid.NewGuid(), new UpdateTaskInputDto { Title = "Titulo", Status = invalidStatus }, cancellationToken: TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<TaskValidationException>(() => service.GetAllAsync(new TaskFilterInputDto { Status = invalidStatus }, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(0, repository.Calls);
    }

    [Fact]
    public async Task InputsNulos_SaoRejeitadosAntesDeAcessarRepositorio()
    {
        var repository = new TasksRepositoryFake();
        var service = new TasksService(repository);

        await Assert.ThrowsAsync<TaskValidationException>(() => service.CreateAsync(null!, cancellationToken: TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<TaskValidationException>(() => service.UpdateAsync(Guid.NewGuid(), null!, cancellationToken: TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<TaskValidationException>(() => service.GetAllAsync(null!, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(0, repository.Calls);
    }

    [Fact]
    public async Task IdVazio_ERejeitadoNaConsultaAtualizacaoEExclusao()
    {
        var repository = new TasksRepositoryFake();
        var service = new TasksService(repository);

        await Assert.ThrowsAsync<TaskValidationException>(() => service.GetAsync(Guid.Empty, cancellationToken: TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<TaskValidationException>(() => service.UpdateAsync(Guid.Empty, new UpdateTaskInputDto { Title = "Titulo", Status = StatusTask.Pending }, cancellationToken: TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<TaskValidationException>(() => service.DeleteAsync(Guid.Empty, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(0, repository.Calls);
    }

    [Fact]
    public async Task TarefaInexistente_RetornaAusencia_SemAtualizarOuExcluir()
    {
        var repository = new TasksRepositoryFake();
        var service = new TasksService(repository);
        var id = Guid.NewGuid();

        Assert.Null(await service.GetAsync(id, cancellationToken: TestContext.Current.CancellationToken));
        Assert.Null(await service.UpdateAsync(id, new UpdateTaskInputDto { Title = "Titulo", Status = StatusTask.Completed }, cancellationToken: TestContext.Current.CancellationToken));
        Assert.False(await service.DeleteAsync(id, cancellationToken: TestContext.Current.CancellationToken));
        Assert.Null(repository.UpdatedTask);
        Assert.Null(repository.DeletedTask);
        Assert.Equal(3, repository.Calls);
    }

    [Fact]
    public async Task Get_MapeiaTodosOsCamposDaTarefaEncontrada()
    {
        var entity = new TaskEntity(Guid.NewGuid(), "Titulo", "Descrição", new DateOnly(2026, 10, 5), StatusTask.Completed);
        var service = new TasksService(new TasksRepositoryFake { ExistingTask = entity });

        var result = await service.GetAsync(entity.Id, cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.Title, result.Title);
        Assert.Equal(entity.Description, result.Description);
        Assert.Equal(entity.DueDate, result.DueDate);
        Assert.Equal(entity.Status, result.Status);
    }

    [Fact]
    public async Task Update_PreservaId_AlteraTituloEStatus_ERemoveCamposOpcionais()
    {
        var entity = new TaskEntity(Guid.NewGuid(), "Antigo", "Descrição", new DateOnly(2026, 10, 5), StatusTask.Pending);
        var repository = new TasksRepositoryFake { ExistingTask = entity };
        var service = new TasksService(repository);

        var result = await service.UpdateAsync(entity.Id, new UpdateTaskInputDto { Title = "  Novo  ", Description = null, DueDate = null, Status = StatusTask.Completed }, cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
        Assert.Equal("Novo", result.Title);
        Assert.Equal(StatusTask.Completed, result.Status);
        Assert.Null(result.Description);
        Assert.Null(result.DueDate);
        Assert.Same(entity, repository.UpdatedTask);
        Assert.Equal(2, repository.Calls);
    }

    [Fact]
    public async Task Delete_ExcluiATarefaEncontrada()
    {
        var entity = new TaskEntity(Guid.NewGuid(), "Titulo", null, null, StatusTask.Pending);
        var repository = new TasksRepositoryFake { ExistingTask = entity };

        var result = await new TasksService(repository).DeleteAsync(entity.Id, cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result);
        Assert.Same(entity, repository.DeletedTask);
        Assert.Equal(2, repository.Calls);
    }

    [Fact]
    public async Task GetAll_EncaminhaFiltrosECancelamento_ERetornaDtos()
    {
        var entity = new TaskEntity(Guid.NewGuid(), "Titulo", "Descrição", new DateOnly(2026, 10, 5), StatusTask.InProgress);
        var repository = new TasksRepositoryFake { ListedTasks = [entity] };
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);

        var result = await new TasksService(repository).GetAllAsync(new TaskFilterInputDto { Status = entity.Status, DueDate = entity.DueDate }, cancellation.Token);

        var output = Assert.Single(result);
        Assert.Equal(entity.Id, output.Id);
        Assert.Equal(entity.Title, output.Title);
        Assert.Equal(entity.Description, output.Description);
        Assert.Equal(entity.DueDate, output.DueDate);
        Assert.Equal(entity.Status, output.Status);
        Assert.Equal(entity.Status, repository.ReceivedStatus);
        Assert.Equal(entity.DueDate, repository.ReceivedDueDate);
        Assert.Equal(cancellation.Token, repository.ReceivedToken);
    }

    [Fact]
    public async Task GetAll_SemResultados_RetornaListaVazia()
    {
        var result = await new TasksService(new TasksRepositoryFake()).GetAllAsync(new TaskFilterInputDto(), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Empty(result);
    }

    [Fact]
    public async Task FalhaDePersistencia_EPropagadaSemSerConvertidaEmValidacao()
    {
        var failure = new InvalidOperationException("Falha de persistência");
        var service = new TasksService(new TasksRepositoryFake { Failure = failure });

        var result = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(new CreateTaskInputDto { Title = "Titulo", Status = StatusTask.Pending }, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Same(failure, result);
    }

    [Fact]
    public async Task Cancelamento_EPropagadoAoRepositorio()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancellation.Cancel();
        var service = new TasksService(new TasksRepositoryFake());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.CreateAsync(new CreateTaskInputDto { Title = "Titulo", Status = StatusTask.Pending }, cancellation.Token));
    }
}
