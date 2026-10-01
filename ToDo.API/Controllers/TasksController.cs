using Microsoft.AspNetCore.Mvc;
using ToDo.API.Filters;
using ToDo.Application.DTOs.Input;
using ToDo.Application.DTOs.Output;
using ToDo.Application.Services.Interfaces;
using ToDo.Core.Responses;

namespace ToDo.API.Controllers;

[ApiController]
[TypeFilter(typeof(TaskValidationExceptionFilter))]
[Route("api/[controller]")]
public class TasksController(ITasksService service) : ControllerBase
{
    /// <summary>Cria uma tarefa e gera seu identificador único.</summary>
    /// <remarks>
    /// Título e status são obrigatórios. O título não pode conter somente espaços.
    /// Descrição e vencimento são opcionais. Datas passadas são aceitas.
    /// Exemplo de corpo: { "title": "Finalizar desafio", "description": "Revisar os testes", "dueDate": "2026-10-05", "status": 0 }
    /// O cabeçalho Location aponta para a consulta da tarefa criada.
    /// </remarks>
    /// <param name="input">Dados da nova tarefa. Status: 0 = Pending, 1 = InProgress, 2 = Completed.</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    /// <response code="201">Tarefa criada, com seus dados e identificador.</response>
    /// <response code="400">Corpo inválido, título ausente ou status inválido.</response>
    [HttpPost]
    [ProducesResponseType(typeof(Response<TaskOutputDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Response<TaskOutputDto>>> CreateAsync([FromBody] CreateTaskInputDto input, CancellationToken cancellationToken)
    {
        var task = await service.CreateAsync(input, cancellationToken);
        var response = Response<TaskOutputDto>.Ok(task, "Tarefa criada com sucesso.");

        return CreatedAtAction(nameof(GetById), new { id = task.Id }, response);
    }

    /// <summary>Consulta uma tarefa pelo identificador.</summary>
    /// <remarks>O identificador deve estar no formato UUID. Um valor fora desse formato não corresponde à rota e retorna 404.</remarks>
    /// <param name="id">Identificador único da tarefa.</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    /// <response code="200">Tarefa encontrada.</response>
    /// <response code="400">Identificador vazio (Guid.Empty).</response>
    /// <response code="404">Tarefa inexistente ou rota com identificador fora do formato UUID.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Response<TaskOutputDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Response<TaskOutputDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Response<TaskOutputDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var task = await service.GetAsync(id, cancellationToken);

        if (task is null)
        {
            return NotFound(Response<TaskOutputDto>.Error("Tarefa não encontrada."));
        }

        return Ok(Response<TaskOutputDto>.Ok(task, "Consulta realizada com sucesso."));
    }

    /// <summary>Lista tarefas com filtros opcionais por status e vencimento.</summary>
    /// <remarks>
    /// Sem filtros, retorna todas as tarefas. Filtros informados juntos são combinados com E.
    /// Status: 0 = Pending, 1 = InProgress, 2 = Completed.
    /// DueDate compara a data exata no formato yyyy-MM-dd e exclui tarefas sem vencimento.
    /// Exemplo: /api/Tasks?Status=0&amp;DueDate=2026-10-05
    /// Sem resultados, retorna sucesso com uma lista vazia.
    /// </remarks>
    /// <param name="input">Status e DueDate opcionais, aceitos separadamente ou juntos.</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    /// <response code="200">Lista de tarefas, possivelmente vazia.</response>
    /// <response code="400">Status ou formato da data inválido.</response>
    [HttpGet]
    [ProducesResponseType(typeof(Response<IReadOnlyList<TaskOutputDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Response<IReadOnlyList<TaskOutputDto>>>> GetAllAsync([FromQuery] TaskFilterInputDto input, CancellationToken cancellationToken)
    {
        var tasks = await service.GetAllAsync(input, cancellationToken);

        return Ok(Response<IReadOnlyList<TaskOutputDto>>.Ok(tasks, "Consulta realizada com sucesso."));
    }

    /// <summary>Atualiza todos os campos editáveis de uma tarefa.</summary>
    /// <remarks>
    /// Atualização completa: título e status são obrigatórios. Descrição e vencimento nulos ou omitidos removem os valores anteriores.
    /// O identificador é preservado. Exemplo: { "title": "Desafio revisado", "description": null, "dueDate": null, "status": 2 }
    /// </remarks>
    /// <param name="id">Identificador único da tarefa a atualizar.</param>
    /// <param name="input">Novos valores dos campos editáveis.</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    /// <response code="200">Tarefa atualizada.</response>
    /// <response code="400">Identificador vazio, corpo inválido, título ausente ou status inválido.</response>
    /// <response code="404">Tarefa inexistente ou identificador fora do formato UUID.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Response<TaskOutputDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Response<TaskOutputDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Response<TaskOutputDto>>> UpdateAsync(Guid id, [FromBody] UpdateTaskInputDto input, CancellationToken cancellationToken)
    {
        var task = await service.UpdateAsync(id, input, cancellationToken);

        if (task is null)
        {
            return NotFound(Response<TaskOutputDto>.Error("Tarefa não encontrada."));
        }

        return Ok(Response<TaskOutputDto>.Ok(task, "Tarefa atualizada com sucesso."));
    }

    /// <summary>Exclui uma tarefa pelo identificador.</summary>
    /// <remarks>A exclusão retorna uma response com mensagem e data nulo. Repetir a exclusão de uma tarefa removida retorna 404.</remarks>
    /// <param name="id">Identificador único da tarefa a excluir.</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    /// <response code="200">Tarefa excluída com sucesso.</response>
    /// <response code="400">Identificador vazio (Guid.Empty).</response>
    /// <response code="404">Tarefa inexistente ou identificador fora do formato UUID.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(Response<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Response<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Response<object>>> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await service.DeleteAsync(id, cancellationToken);

        if (!deleted)
        {
            return NotFound(Response<object>.Error("Tarefa não encontrada."));
        }

        return Ok(new Response<object>(true, "Tarefa excluída com sucesso.", null));
    }
}

