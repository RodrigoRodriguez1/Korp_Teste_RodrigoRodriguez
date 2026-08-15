using Korp.Estoque.API.Endpoints;
using Korp.Estoque.API.Middlewares;
using Korp.Estoque.Application;
using Korp.Estoque.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()));

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseCors();
app.UseHttpsRedirection();

app.MapProdutoEndpoints();

// Aplica migrations automaticamente na inicialização
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<Korp.Estoque.Infrastructure.Persistence.EstoqueDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();

// Torna a classe Program acessível para WebApplicationFactory nos testes de integração
public partial class Program { }
