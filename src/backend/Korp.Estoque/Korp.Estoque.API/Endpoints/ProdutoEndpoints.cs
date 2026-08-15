using Korp.Estoque.Application.Products.Commands.CreateProduto;
using Korp.Estoque.Application.Products.Commands.DeleteProduto;
using Korp.Estoque.Application.Products.Commands.DescontarSaldo;
using Korp.Estoque.Application.Products.Commands.UpdateProduto;
using Korp.Estoque.Application.Products.DTOs;
using Korp.Estoque.Application.Products.Queries.GetAllProdutos;
using Korp.Estoque.Application.Products.Queries.GetProdutoById;
using Korp.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Korp.Estoque.API.Endpoints;

public static class ProdutoEndpoints
{
    public static void MapProdutoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/produtos").WithTags("Produtos");

        group.MapGet("/", GetAll);
        group.MapGet("/{id:guid}", GetById);
        group.MapPost("/", Create);
        group.MapPut("/{id:guid}", Update);
        group.MapDelete("/{id:guid}", Delete);

        // Endpoint interno chamado pelo Faturamento.API
        app.MapPost("/produtos/descontar-saldo", DescontarSaldo)
            .WithTags("Produtos (Interno)");
    }

    private static async Task<IResult> GetAll(ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetAllProdutosQuery(), ct);
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetById(Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetProdutoByIdQuery(id), ct);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : ToProblem(result.Error);
    }

    private static async Task<IResult> Create(
        [FromBody] CreateProdutoCommand command,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? Results.Created($"/produtos/{result.Value.Id}", result.Value)
            : ToProblem(result.Error);
    }

    private static async Task<IResult> Update(
        Guid id,
        [FromBody] UpdateProdutoRequest request,
        ISender sender,
        CancellationToken ct)
    {
        var command = new UpdateProdutoCommand(id, request.Codigo, request.Descricao, request.Saldo);
        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : ToProblem(result.Error);
    }

    private static async Task<IResult> Delete(Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteProdutoCommand(id), ct);
        return result.IsSuccess
            ? Results.NoContent()
            : ToProblem(result.Error);
    }

    private static async Task<IResult> DescontarSaldo(
        [FromBody] DescontarSaldoRequestDto request,
        ISender sender,
        CancellationToken ct)
    {
        var command = new DescontarSaldoCommand(request.Itens);
        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? Results.Ok(new { sucesso = true })
            : ToProblem(result.Error);
    }

    private static IResult ToProblem(Error error) => error.Type switch
    {
        ErrorType.NotFound => Results.NotFound(BuildProblem(404, "Recurso não encontrado", error.Description)),
        ErrorType.Conflict => Results.Conflict(BuildProblem(409, "Conflito", error.Description)),
        ErrorType.Validation => Results.BadRequest(BuildProblem(400, "Erro de validação", error.Description)),
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

public sealed record UpdateProdutoRequest(string Codigo, string Descricao, int Saldo);
