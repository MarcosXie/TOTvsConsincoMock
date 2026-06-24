using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TotvsConsincoMock.MockData;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "TOTVS Consinco Mock API", Version = "v1" });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TOTVS Consinco Mock API v1");
    c.RoutePrefix = string.Empty; // Swagger na raiz
});

// ===== SMProdutosAPI =====

// Helper para converter objetos anônimos em JsonElement para filtragem
JsonElement ToJson(object obj) => JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(obj));

List<object> FilterByCodigoAcessoAndId(List<object> data, string? codigoAcesso, int? idProduto)
{
    var result = data;
    if (!string.IsNullOrEmpty(codigoAcesso))
    {
        result = result.Where(item =>
        {
            var json = ToJson(item);
            // Busca no CodigoAcessoPrincipal
            if (json.TryGetProperty("CodigoAcessoPrincipal", out var cap) && cap.GetString() == codigoAcesso)
                return true;
            // Busca dentro do array CodigosAcessos
            if (json.TryGetProperty("CodigosAcessos", out var arr))
                return arr.EnumerateArray().Any(x => x.GetProperty("CodigoAcesso").GetString() == codigoAcesso);
            return false;
        }).ToList();
    }
    if (idProduto.HasValue)
    {
        result = result.Where(item =>
        {
            var json = ToJson(item);
            return json.TryGetProperty("IdProduto", out var id) && id.GetInt32() == idProduto.Value;
        }).ToList();
    }
    return result;
}

app.MapGet("/SMProdutosAPI/api/v4/produtos/precos-produtos", (
    [AsParameters] ProdutoQueryParams p) =>
{
    var data = FilterByCodigoAcessoAndId(PrecosProdutosMock.Get(), p.codigoAcesso, p.idProduto);
    return Results.Ok(data);
})
.WithName("GetPrecosProdutos")
.WithTags("SMProdutosAPI - Preços")
.AllowAnonymous();

app.MapGet("/SMProdutosAPI/api/v4/produtos/codigos-acesso-prod", (
    [AsParameters] ProdutoQueryParams p) =>
{
    var data = FilterByCodigoAcessoAndId(CodigosAcessoMock.Get(), p.codigoAcesso, p.idProduto);
    return Results.Ok(data);
})
.WithName("GetCodigosAcessoProd")
.WithTags("SMProdutosAPI - Códigos de Acesso")
.AllowAnonymous();

app.MapGet("/SMProdutosAPI/api/v4/produtos/dados-cadastrais", (
    [AsParameters] ProdutoQueryParams p) =>
{
    var data = FilterByCodigoAcessoAndId(DadosCadastraisMock.Get(), p.codigoAcesso, p.idProduto);
    if (!string.IsNullOrEmpty(p.descricao))
    {
        data = data.Where(item =>
        {
            var json = ToJson(item);
            if (json.TryGetProperty("DescricaoCompleta", out var desc))
                return desc.GetString()?.Contains(p.descricao, StringComparison.OrdinalIgnoreCase) == true;
            return false;
        }).ToList();
    }
    return Results.Ok(data);
})
.WithName("GetDadosCadastrais")
.WithTags("SMProdutosAPI - Dados Cadastrais")
.AllowAnonymous();

app.MapGet("/SMProdutosAPI/api/v4/produtos/tributacao-produtos", (
    [AsParameters] ProdutoQueryParams p) =>
{
    var data = FilterByCodigoAcessoAndId(TributacaoProdutosMock.Get(), p.codigoAcesso, p.idProduto);
    return Results.Ok(data);
})
.WithName("GetTributacaoProdutos")
.WithTags("SMProdutosAPI - Tributação")
.AllowAnonymous();

// ===== SMVendasAPI =====

app.MapGet("/SMVendasAPI/api/v1/vendas", () =>
{
    return Results.Ok(new[]
    {
        new
        {
            IdVenda = 9001,
            NumeroEmpresa = 1,
            NumeroPDV = 50,
            NumeroCupom = 123456,
            DataVenda = "2026-05-06T10:30:00Z",
            ValorTotal = 41.38m,
            StatusVenda = "F"
        }
    });
})
.WithName("GetVendas")
.WithTags("SMVendasAPI - Vendas")
.AllowAnonymous();

app.MapPost("/SMVendasAPI/api/v1/vendas", () =>
{
    return Results.Ok(new { IdVenda = Random.Shared.Next(10000, 99999), Mensagem = "Venda registrada com sucesso (mock)" });
})
.WithName("PostVendas")
.WithTags("SMVendasAPI - Vendas")
.AllowAnonymous();

app.Run();

public record ProdutoQueryParams(
    [FromQuery(Name = "modelo.idProduto")] int? idProduto = null,
    [FromQuery(Name = "modelo.codigoAcesso")] string? codigoAcesso = null,
    [FromQuery(Name = "modelo.descricao")] string? descricao = null,
    [FromQuery(Name = "modelo.descricaoProduto")] string? descricaoProduto = null,
    [FromQuery(Name = "modelo.CNPJEmpresa")] string? cnpjEmpresa = null,
    [FromQuery(Name = "modelo.nroEmpresa")] int? nroEmpresa = null,
    [FromQuery(Name = "modelo.numeroSegmento")] int? numeroSegmento = null,
    [FromQuery(Name = "modelo.statusVenda")] string? statusVenda = null,
    [FromQuery(Name = "modelo.tipoTributacao")] string? tipoTributacao = null,
    [FromQuery(Name = "modelo._pageNo")] int? pageNo = null,
    [FromQuery(Name = "modelo._pageSize")] int? pageSize = null
);
