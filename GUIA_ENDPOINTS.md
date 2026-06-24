# Guia de Endpoints - TOTVS Consinco Mock

## Overview: Fluxo do Self-Checkout

| Etapa | Ação | Endpoint |
|-------|------|----------|
| 1. Cliente escaneia produto | Buscar produto pelo código de barras | `GET /SMProdutosAPI/api/v4/produtos/codigos-acesso-prod` |
| 2. Exibir descrição do produto | Buscar dados cadastrais (nome, descrição) | `GET /SMProdutosAPI/api/v4/produtos/dados-cadastrais` |
| 3. Exibir preço do produto | Buscar preço atualizado e promoções | `GET /SMProdutosAPI/api/v4/produtos/precos-produtos` |
| 4. Calcular impostos (NFC-e/SAT) | Buscar tributação do produto | `GET /SMProdutosAPI/api/v4/produtos/tributacao-produtos` |
| 5. Finalizar venda (após pagamento TEF) | Registrar a venda no ERP | `POST /SMVendasAPI/api/v1/vendas` |

---

## Exemplos de Payload (Query String) por Endpoint

### 1. Códigos de Acesso dos Produtos

Buscar pelo código de barras escaneado:

```
GET /SMProdutosAPI/api/v4/produtos/codigos-acesso-prod?modelo.codigoAcesso=7893500020103
```

Buscar por ID do produto:

```
GET /SMProdutosAPI/api/v4/produtos/codigos-acesso-prod?modelo.idProduto=1001
```

Com paginação:

```
GET /SMProdutosAPI/api/v4/produtos/codigos-acesso-prod?modelo.codigoAcesso=7893500020103&modelo._pageNo=1&modelo._pageSize=10
```

---

### 2. Dados Cadastrais dos Produtos

Buscar pelo código de barras:

```
GET /SMProdutosAPI/api/v4/produtos/dados-cadastrais?modelo.codigoAcesso=7893500020103
```

Buscar por descrição:

```
GET /SMProdutosAPI/api/v4/produtos/dados-cadastrais?modelo.descricao=ARROZ
```

Buscar por ID do produto:

```
GET /SMProdutosAPI/api/v4/produtos/dados-cadastrais?modelo.idProduto=1001
```

Com paginação:

```
GET /SMProdutosAPI/api/v4/produtos/dados-cadastrais?modelo.codigoAcesso=7894900011517&modelo._pageNo=1&modelo._pageSize=10
```

---

### 3. Preços dos Produtos

Buscar preço pelo código de barras (caso mais comum no self-checkout):

```
GET /SMProdutosAPI/api/v4/produtos/precos-produtos?modelo.codigoAcesso=7893500020103
```

Buscar preço com empresa e segmento:

```
GET /SMProdutosAPI/api/v4/produtos/precos-produtos?modelo.nroEmpresa=1&modelo.codigoAcesso=7893500020103&modelo.numeroSegmento=1
```

Buscar por CNPJ da empresa:

```
GET /SMProdutosAPI/api/v4/produtos/precos-produtos?modelo.CNPJEmpresa=12345678000199&modelo.codigoAcesso=7896006700012
```

Buscar apenas produtos ativos:

```
GET /SMProdutosAPI/api/v4/produtos/precos-produtos?modelo.statusVenda=A&modelo.codigoAcesso=7894900011517
```

Buscar por ID do produto:

```
GET /SMProdutosAPI/api/v4/produtos/precos-produtos?modelo.idProduto=1003
```

Com paginação:

```
GET /SMProdutosAPI/api/v4/produtos/precos-produtos?modelo._pageNo=1&modelo._pageSize=20
```

---

### 4. Tributação dos Produtos

Buscar tributação pelo código de barras (saída para não contribuinte - padrão NFC-e):

```
GET /SMProdutosAPI/api/v4/produtos/tributacao-produtos?modelo.CNPJEmpresa=12345678000199&modelo.codigoAcesso=7893500020103&modelo.tipoTributacao=SN
```

Buscar tributação por ID do produto:

```
GET /SMProdutosAPI/api/v4/produtos/tributacao-produtos?modelo.CNPJEmpresa=12345678000199&modelo.idProduto=1001&modelo.tipoTributacao=SN
```

Com UF destino específica:

```
GET /SMProdutosAPI/api/v4/produtos/tributacao-produtos?modelo.CNPJEmpresa=12345678000199&modelo.codigoAcesso=7893500020103&modelo.tipoTributacao=SN&modelo.uFClienteFornecedor=SP
```

---

### 5. Vendas (POST)

```
POST /SMVendasAPI/api/v1/vendas
Content-Type: application/json

(Retorna 200 OK - mock apenas confirma recebimento)
```

---

## Códigos de Barras disponíveis no Mock

| Código de Barras | Produto | Preço | Peso (kg) |
|-----------------|---------|-------|-----------|
| 06632956 | CAIXA LEITOR DE CODIGO DE BARRAS | R$ 89,90 | 0,225 |
| 4005808555345 | PROTETOR SOLAR NIVEA SUN FPS50 200ML | R$ 65,90 (promo R$ 54,90) | 0,220 |
| 6452024011229 | CAMERA DIGITAL COMPACTA 20MP | R$ 1.299,00 | 0,520 |

> **Sobre o peso esperado por SKU.** O TOTVS Consinco SMProdutosAPI **não** tem um
> campo dedicado de "peso esperado por unidade para self-checkout". O que ele
> oferece, em `dados-cadastrais`, são `PesoLiquido` / `PesoBruto` (origem
> cadastral/logística — usados para fretes, paletização etc.). No mock estamos
> reaproveitando `PesoLiquido` como referência para a validação anti-fraude no
> gate, alinhado ao peso real medido. Em produção, recomenda-se manter uma
> tabela local de pesos calibrados por SKU (ou usar `RetornoCustomizados`) e
> tratar o campo do ERP apenas como fallback.

---

## Dica de uso no Self-Checkout

O fluxo típico ao escanear um produto seria:

1. Escaneia o código de barras → chama `codigos-acesso-prod?modelo.codigoAcesso={EAN}`
2. Com o `IdProduto` retornado → chama `dados-cadastrais?modelo.idProduto={id}` para exibir nome
3. Em paralelo → chama `precos-produtos?modelo.codigoAcesso={EAN}` para exibir preço
4. Na finalização → chama `tributacao-produtos` para cada item para montar o XML fiscal
5. Após pagamento → chama `POST vendas` para registrar no ERP

