using Korp.Faturamento.Application.NotasFiscais.Commands.CreateNotaFiscal;
using Korp.Faturamento.Application.NotasFiscais.Commands.ImprimirNotaFiscal;
using Korp.Faturamento.Application.NotasFiscais.DTOs;
using Korp.Faturamento.Application.NotasFiscais.Queries.GetAllNotasFiscais;
using Korp.Faturamento.Application.NotasFiscais.Queries.GetNotaFiscalById;
using Korp.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Korp.Faturamento.API.Endpoints;

public static class NotaFiscalEndpoints
{
    public static void MapNotaFiscalEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/notas-fiscais").WithTags("Notas Fiscais");

        group.MapGet("/", GetAll);
        group.MapGet("/{id:guid}", GetById);
        group.MapPost("/", Create);
        group.MapPost("/{id:guid}/imprimir", Imprimir);

        // Endpoint de demo: força falha no circuit breaker do Estoque
        app.MapPost("/internal/simular-falha", SimularFalha)
            .WithTags("Demo / Testes");
    }

    private static async Task<IResult> GetAll(ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetAllNotasFiscaisQuery(), ct);
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetById(Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetNotaFiscalByIdQuery(id), ct);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : ToProblem(result.Error);
    }

    private static async Task<IResult> Create(
        [FromBody] CreateNotaFiscalRequest request,
        ISender sender,
        CancellationToken ct)
    {
        var command = new CreateNotaFiscalCommand(
            request.Itens.Select(i =>
                new CreateItemNotaCommand(i.ProdutoId, i.ProdutoCodigo, i.ProdutoDescricao, i.Quantidade))
            .ToList());

        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? Results.Created($"/notas-fiscais/{result.Value.Id}", result.Value)
            : ToProblem(result.Error);
    }

    private static async Task<IResult> Imprimir(
        Guid id,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        ISender sender,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            return Results.BadRequest(BuildProblem(400, "Cabeçalho obrigatório ausente",
                "O cabeçalho 'Idempotency-Key' é obrigatório."));

        var command = new ImprimirNotaFiscalCommand(id, idempotencyKey);
        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : ToProblem(result.Error);
    }

    private static IResult SimularFalha() =>
        Results.Problem(
            detail: "Endpoint de simulação de falha ativado para demo do circuit breaker.",
            statusCode: 503,
            title: "Serviço indisponível (simulado)");

    private static IResult ToProblem(Error error) => error.Type switch
    {
        ErrorType.NotFound => Results.NotFound(BuildProblem(404, "Recurso não encontrado", error.Description)),
        ErrorType.Conflict => Results.Conflict(BuildProblem(409, "Conflito", error.Description)),
        ErrorType.Validation => Results.BadRequest(BuildProblem(400, "Erro de validação", error.Description)),
        ErrorType.ServiceUnavailable => Results.Problem(
            detail: error.Description,
            statusCode: 503,
            title: "Serviço temporariamente indisponível"),
        _ => Results.Problem(detail: error.Description, statusCode: 500)
    };

    private static object BuildProblem(int status, string title, string detail) => new
    {
        type = "https://tools.ietf.org/html/rfc7807",
        title,
        status,
        detail
    };
}
