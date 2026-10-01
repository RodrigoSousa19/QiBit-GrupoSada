# Gestão de Tarefas

> Este README contou com o apoio de inteligência artificial para fins de revisão e melhor estruturação do conteúdo.

Este repositório é a minha implementação do teste técnico de gestão de tarefas. A ideia é ter uma API simples para criar, consultar, editar e excluir tarefas, com filtros por status e data de vencimento.

Decidi seguir o padrão Repository e separar o projeto em camadas para deixar claro o que cada parte faz. O projeto é somente backend; dá para testar todas as funcionalidades pelo Swagger, sem precisar criar um frontend.

## Estrutura que escolhi

```text
ToDo.API             Controllers, Swagger, tratamento de erros e logging
ToDo.Application     Services, contratos dos services, DTOs e validações
ToDo.Core            Entity, enum de status, contrato do repository e response
ToDo.Infrastructure  DbContext, EF Core InMemory e implementação do repository
ToDo.Tests           Testes dos services e do repository com xUnit
```

A controller recebe a requisição, chama o service e monta a response com o status HTTP correto. O service aplica as validações, chama o repository e devolve um DTO. O repository concentra o acesso aos dados e salva as alterações pelo EF Core.

Escolhi essa estrutura para evitar que a controller fique responsável por tudo e para conseguir testar as regras sem subir a API. O contrato do repository fica no Core; a implementação fica na Infrastructure. A Application depende desse contrato, sem precisar conhecer o DbContext.

A injeção de dependências fica nos módulos `ApplicationModule` e `InfrastructureModule`, chamados no `Program.cs`. Service, repository e contexto usam o tempo de vida Scoped.

O banco é InMemory, conforme o desafio. Não precisa instalar servidor de banco, configurar connection string ou executar migrations. Os dados são compartilhados durante a execução da API e são perdidos quando ela é reiniciada.

## Como executar

Você precisa do **SDK .NET 10** instalado. Rider, Visual Studio ou outro editor são opcionais.

Na raiz do repositório, execute:

```powershell
dotnet --version
dotnet restore ToDo.slnx
dotnet build ToDo.slnx
dotnet run --project ToDo.API/ToDo.API.csproj --launch-profile http
```

Com o perfil `http`:

- API: http://localhost:5297
- Swagger: http://localhost:5297/swagger
- Documento OpenAPI: http://localhost:5297/openapi/v1.json

O perfil configura o ambiente Development, onde o Swagger fica disponível. No Rider, selecione o projeto ToDo.API e o perfil correspondente para executar.

Se preferir HTTPS:

```powershell
dotnet dev-certs https --trust
dotnet run --project ToDo.API/ToDo.API.csproj --launch-profile https
```

Nesse perfil, o Swagger fica em https://localhost:7234/swagger. O perfil HTTP já permite testar o projeto sem configurar o certificado. Para encerrar a execução no terminal, use Ctrl+C.

Se o restore reclamar de uma fonte NuGet local que não existe, confira `dotnet nuget list source`. Corrija ou desabilite a fonte inválida na sua máquina e tente novamente. Os pacotes públicos deste projeto estão no nuget.org.

## Como executar os testes

```powershell
dotnet test ToDo.slnx
```

Para executar somente o projeto de testes:

```powershell
dotnet test ToDo.Tests/ToDo.Tests.csproj
```

Os testes usam xUnit e cobrem validações, geração de identificador, mapeamento para DTOs, consulta de tarefa inexistente, atualização, exclusão, cancelamento e propagação de falhas.

Nos testes de repository, cada caso recebe um banco isolado. A persistência é conferida com outro contexto, para não depender somente do tracking do contexto que gravou. Os filtros são verificados pelo conjunto exato de tarefas retornadas, incluindo quais precisam ficar de fora.

A suíte contém 24 casos na versão documentada aqui. Ela cobre services e repository; não inclui testes HTTP automatizados dos controllers nem testes automatizados do logging.

## Campos de uma tarefa

| Campo | Tipo | Como funciona |
| --- | --- | --- |
| `id` | UUID/Guid | Gerado pela aplicação; não é enviado na criação nem alterado na edição |
| `title` | string | Obrigatório; não pode estar vazio ou conter somente espaços |
| `description` | string ou null | Opcional |
| `dueDate` | data ou null | Opcional, no formato `yyyy-MM-dd` |
| `status` | número inteiro | Obrigatório na criação e na atualização |

| Status | Valor enviado no JSON |
| --- | --- |
| Pendente (`Pending`) | `0` |
| Em progresso (`InProgress`) | `1` |
| Concluída (`Completed`) | `2` |

O título é salvo sem espaços no começo e no fim. Datas passadas são aceitas. Não há regra para impedir títulos repetidos ou restringir a passagem entre os três status.

## Padrão de retorno

As respostas montadas pela API usam `success`, `message` e `data`:

```json
{
  "success": true,
  "message": "Consulta realizada com sucesso.",
  "data": {
    "id": "11111111-1111-1111-1111-111111111111",
    "title": "Finalizar desafio",
    "description": "Revisar os testes",
    "dueDate": "2026-10-05",
    "status": 0
  }
}
```

O identificador acima é só um exemplo. Nos próximos pedidos, use o ID que a criação realmente devolver.

As falhas de validação tratadas pelo service retornam **400**:

```json
{
  "success": false,
  "message": "O título é obrigatório.",
  "data": null
}
```

Existe uma diferença importante no comportamento atual: erros detectados pelo ASP.NET antes de chamar o service, como JSON malformado, campo obrigatório ausente ou data com formato inválido, retornam **400** no formato `ValidationProblemDetails`. O formato é semelhante a este; os textos e o conteúdo de `errors` dependem da entrada:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "DueDate": ["Mensagem de validação do campo."]
  },
  "traceId": "identificador-da-requisicao"
}
```

Se o ID informado tiver formato inválido, como `/api/Tasks/abc`, a restrição de rota rejeita o endereço com **404**. Esse caso não passa pela controller e não garante o corpo padrão da response. Um UUID válido, mas não cadastrado, retorna a response de tarefa não encontrada.

## Rotas

Todos os exemplos abaixo usam a base `http://localhost:5297`. POST e PUT recebem `Content-Type: application/json`. Não é necessário token de autenticação.

| Método | Rota | Sucesso | Falhas esperadas |
| --- | --- | --- | --- |
| POST | `/api/Tasks` | 201 | 400; 500 em falha inesperada |
| GET | `/api/Tasks` | 200 | 400; 500 em falha inesperada |
| GET | `/api/Tasks/{id}` | 200 | 400, 404; 500 em falha inesperada |
| PUT | `/api/Tasks/{id}` | 200 | 400, 404; 500 em falha inesperada |
| DELETE | `/api/Tasks/{id}` | 200 | 400, 404; 500 em falha inesperada |

### Criar uma tarefa

```http
POST /api/Tasks
Content-Type: application/json
```

Corpo:

```json
{
  "title": "Finalizar desafio",
  "description": "Revisar os testes",
  "dueDate": "2026-10-05",
  "status": 0
}
```

Descrição e vencimento podem ser omitidos ou enviados como null.

Quando der certo, retorna **201 Created**:

```json
{
  "success": true,
  "message": "Tarefa criada com sucesso.",
  "data": {
    "id": "11111111-1111-1111-1111-111111111111",
    "title": "Finalizar desafio",
    "description": "Revisar os testes",
    "dueDate": "2026-10-05",
    "status": 0
  }
}
```

O cabeçalho `Location` contém a URL para consultar essa tarefa.

Se enviar título vazio ou somente espaços, retorna **400** com `O título é obrigatório.`. Um status como `99` retorna **400** com `O status informado é inválido.`. JSON malformado ou ausência dos campos obrigatórios segue o formato de validação do ASP.NET descrito acima.

### Listar e filtrar tarefas

```http
GET /api/Tasks
```

Você pode chamar sem filtros, com um filtro ou com os dois:

```text
http://localhost:5297/api/Tasks
http://localhost:5297/api/Tasks?Status=0
http://localhost:5297/api/Tasks?DueDate=2026-10-05
http://localhost:5297/api/Tasks?Status=0&DueDate=2026-10-05
```

O filtro de vencimento compara a data exata. Quando os dois filtros são enviados, a tarefa precisa atender aos dois. Tarefas sem vencimento não aparecem quando uma data é informada. A ordem da listagem não é definida pelo contrato.

Quando der certo, retorna **200 OK**:

```json
{
  "success": true,
  "message": "Consulta realizada com sucesso.",
  "data": [
    {
      "id": "11111111-1111-1111-1111-111111111111",
      "title": "Finalizar desafio",
      "description": "Revisar os testes",
      "dueDate": "2026-10-05",
      "status": 0
    }
  ]
}
```

Se não houver resultados, continua retornando **200**, com `success: true` e `data: []`.

`Status=99` retorna **400** com `O status informado é inválido.`. Data inválida ou status sem um valor interpretável retorna **400** pelo mecanismo de validação do ASP.NET.

### Consultar uma tarefa pelo ID

```http
GET /api/Tasks/11111111-1111-1111-1111-111111111111
```

Quando a tarefa existir, retorna **200 OK**, com `success: true`, mensagem `Consulta realizada com sucesso.` e o objeto da tarefa em `data`, no formato mostrado em Padrão de retorno.

Quando o UUID for válido, mas a tarefa não existir, retorna **404 Not Found**:

```json
{
  "success": false,
  "message": "Tarefa não encontrada.",
  "data": null
}
```

O ID `00000000-0000-0000-0000-000000000000` retorna **400** com `O identificador da tarefa é inválido.`. Um ID fora do formato UUID retorna **404** por não corresponder à rota.

### Atualizar uma tarefa

```http
PUT /api/Tasks/11111111-1111-1111-1111-111111111111
Content-Type: application/json
```

Corpo:

```json
{
  "title": "Desafio finalizado",
  "description": null,
  "dueDate": null,
  "status": 2
}
```

A atualização é completa. Você precisa enviar título e status. Enviar descrição ou vencimento como null, ou omitir esses campos, remove os valores anteriores. O ID vem da rota e é preservado.

Quando der certo, retorna **200 OK**:

```json
{
  "success": true,
  "message": "Tarefa atualizada com sucesso.",
  "data": {
    "id": "11111111-1111-1111-1111-111111111111",
    "title": "Desafio finalizado",
    "description": null,
    "dueDate": null,
    "status": 2
  }
}
```

Com dados válidos e tarefa inexistente, retorna **404** com `Tarefa não encontrada.`. ID vazio, título inválido ou status inválido retornam **400** com as mensagens correspondentes já descritas. As validações do input acontecem antes de consultar a existência da tarefa. Erros de JSON e formatos seguem a validação do ASP.NET.

### Excluir uma tarefa

```http
DELETE /api/Tasks/11111111-1111-1111-1111-111111111111
```

Não precisa enviar corpo. Quando der certo, retorna **200 OK**:

```json
{
  "success": true,
  "message": "Tarefa excluída com sucesso.",
  "data": null
}
```

Se a tarefa não existir, retorna **404** com `Tarefa não encontrada.`. Isso também acontece ao excluir novamente uma tarefa já removida. ID vazio retorna **400** com `O identificador da tarefa é inválido.`; ID fora do formato UUID retorna **404** por não corresponder à rota.

## Como testar pelo Swagger

Com a API executando, abra `/swagger` e siga este fluxo:

1. Use POST para criar uma tarefa e copie o ID retornado.
2. Use GET por ID para consultar a tarefa criada.
3. Use GET da listagem para testar sem filtros, por status, por vencimento e com ambos.
4. Use PUT para mudar os campos e conferir a atualização.
5. Use DELETE para excluir a tarefa.
6. Consulte o mesmo ID novamente e confira o 404.
7. Faça uma criação com título somente de espaços e confira o 400.

Clique em `Try it out`, preencha os parâmetros ou o JSON e clique em `Execute`. O Swagger mostra o código HTTP, o corpo e os cabeçalhos da resposta.

## Falhas inesperadas e logs

Uma falha inesperada em qualquer operação retorna **500 Internal Server Error** com uma mensagem genérica:

```json
{
  "success": false,
  "message": "Ocorreu um erro inesperado. Tente novamente mais tarde.",
  "data": null
}
```

O retorno para o cliente e o console não exibem stack trace. O handler adiciona `X-Trace-Id` à resposta, e esse mesmo identificador aparece no log para ajudar a localizar a falha.

O Serilog registra mensagens no console e avisos/erros em `ToDo.API/Logs/errors-*.log`. No arquivo, as falhas inesperadas incluem a exceção completa, com stack trace. Há rotação diária, limite de 10 MiB por arquivo e retenção de até 14 arquivos. A pasta é relativa ao content root da API. Os arquivos `.log` estão ignorados no Git.

As validações também são registradas como avisos, com mensagem e identificador da requisição. Falhas de persistência não são convertidas em erros de validação.

## Decisões e limites do projeto

Mantive o escopo do desafio: backend, CRUD de tarefas, filtros, validações, Swagger, testes e logging. A busca é feita pelo identificador e pelos filtros de status/vencimento. Não há busca textual, paginação, autenticação ou frontend.

O InMemory atende ao ambiente do teste, mas não oferece persistência entre reinicializações nem reproduz todas as restrições de um banco relacional. Os testes de repository validam o provider usado aqui.

