using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace Korp.Estoque.API.Endpoints;

public static class IaEndpoints
{
    public static void MapIaEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/produtos/ia/sugerir-codigo", SugerirCodigo)
            .WithTags("IA");
    }

    private static async Task<IResult> SugerirCodigo(
        [FromBody] SugerirCodigoRequest request,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Descricao) || request.Descricao.Trim().Length < 4)
            return Results.BadRequest(new { error = "Descrição muito curta." });

        var apiKey = configuration["Gemini:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            return Results.Ok(new SugerirCodigoResponse(null));

        try
        {
            var client = httpClientFactory.CreateClient("gemini");
            var prompt = $"""
                Gere um código de produto curto, padronizado e em maiúsculas para o seguinte produto: "{request.Descricao}".
                O código deve ter no máximo 15 caracteres, usar apenas letras, números e hífens, sem espaços.
                Exemplos: "NTB-DELL-15", "CAD-ESC-PRE", "MNT-LG-27".
                Responda APENAS com o código, sem explicações.
                """;

            var body = JsonSerializer.Serialize(new
            {
                contents = new[] { new { parts = new[] { new { text = prompt } } } },
                generationConfig = new { maxOutputTokens = 500, temperature = 0.3 }
            });

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.6-flash:generateContent?key={apiKey}";
            using var content = new StringContent(body, Encoding.UTF8, "application/json");
            using var response = await client.PostAsync(url, content, ct);

            if (!response.IsSuccessStatusCode)
                return Results.Ok(new SugerirCodigoResponse(null));

            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);
            var raw = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString()?.Trim().ToUpperInvariant();

            var texto = raw is null ? null : (raw.Length > 20 ? raw[..20] : raw);

            return Results.Ok(new SugerirCodigoResponse(texto));
        }
        catch
        {
            return Results.Ok(new SugerirCodigoResponse(null));
        }
    }
}

public sealed record SugerirCodigoRequest(string Descricao);
public sealed record SugerirCodigoResponse(string? Codigo);
