using System.Net.Http.Json;
using Korp.Faturamento.Application.Common.ExternalServices;

namespace Korp.Faturamento.Infrastructure.ExternalServices;

internal sealed class EstoqueHttpService : IEstoqueService
{
    private readonly HttpClient _httpClient;

    public EstoqueHttpService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(bool Sucesso, string? ErroDetalhe)> DescontarSaldoAsync(
        List<DescontarSaldoItem> itens,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            itens = itens.Select(i => new { produtoId = i.ProdutoId, quantidade = i.Quantidade })
        };

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsJsonAsync(
                "produtos/descontar-saldo", payload, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new EstoqueIndisponivelException(ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new EstoqueIndisponivelException(ex);
        }

        if (response.IsSuccessStatusCode)
            return (true, null);

        // 422 Unprocessable Entity = saldo insuficiente
        if (response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity ||
            response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetailResponse>(cancellationToken);
            return (false, problem?.Detail ?? "Saldo insuficiente para um ou mais produtos.");
        }

        // 503 / 5xx = serviço indisponível (não deveria chegar aqui se Polly circuit breaker atuou)
        throw new EstoqueIndisponivelException();
    }

    private sealed record ProblemDetailResponse(string? Detail);
}
