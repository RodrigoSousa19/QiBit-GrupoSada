using Serilog;
using Serilog.Events;
using ToDo.API.ExceptionHandlers;
using ToDo.Application;
using ToDo.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Services.AddSerilog((services, configuration) => configuration
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}")
    .WriteTo.File(Path.Combine(builder.Environment.ContentRootPath, "Logs", "errors-.log"), restrictedToMinimumLevel: LogEventLevel.Warning, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14, fileSizeLimitBytes: 10 * 1024 * 1024, rollOnFileSizeLimit: true, outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}"));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "Gestão de Tarefas API";
        document.Info.Version = "v1";
        document.Info.Description = "API REST para cadastrar, consultar, atualizar e excluir tarefas.";
        return Task.CompletedTask;
    });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

var app = builder.Build();

app.UseExceptionHandler(new ExceptionHandlerOptions { SuppressDiagnosticsCallback = _ => true });
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Gestão de Tarefas v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "Gestão de Tarefas | Swagger";
        options.DisplayRequestDuration();
        options.EnableDeepLinking();
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();