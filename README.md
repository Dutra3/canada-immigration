# Canada Immigration Tracker

Sistema web sem login para consultar dados públicos de imigração canadense (Express Entry), feito como projeto pessoal de estudo e portfólio.

## O que o projeto faz

- **Express Entry Draws**: histórico paginado das chamadas (draws) do sistema Express Entry, com programa, data, convites emitidos e pontuação CRS mínima. Traz também um destaque sempre atualizado com o draw mais recente, independente da página que você está navegando.
- **Distribuição do Pool CRS**: gráfico de barras mostrando quantos candidatos existem em cada faixa de pontuação CRS, com base no draw mais recente.
- **Proof of Funds**: calculadora do valor mínimo em fundos exigido pelo IRCC (Immigration, Refugees and Citizenship Canada), de acordo com o número de familiares.

Os dados de Express Entry e Pool vêm de uma **API pública community-driven** (não oficial do IRCC): `https://can-ee-draws.karanjit-sagun01.workers.dev/api/`. O cálculo de Proof of Funds usa a tabela oficial do IRCC, implementada localmente no backend (sem depender de API externa).

## Stack e versões

| Camada     | Tecnologia          | Versão   |
|------------|----------------------|----------|
| Backend    | .NET / C#            | .NET 8   |
| Frontend   | Angular (standalone components) | Angular 22 |
| Testes backend | xUnit (sem libs de mock de terceiros — fakes escritos à mão) | — |
| Testes frontend | Vitest (via `@angular/build:unit-test`) | — |
| Node.js (frontend) | ver `canada-immigration-web/.nvmrc` | 22.23.2 |

> Nota: o backend está fixado em **.NET 8 deliberadamente**, mesmo em máquinas com versões mais novas instaladas.

## Estrutura do repositório

```
canadaimmigrationtracker/
├── canadaimmigrationbackend/
│   ├── CanadaImmigration.Api/          ← projeto ASP.NET Core (Controllers, Services, Models)
│   └── CanadaImmigration.Api.Tests/    ← testes xUnit
└── canada-immigration-web/             ← projeto Angular (standalone components)
```

## Como rodar o backend

Pré-requisito: **.NET 8 SDK** instalado.

```bash
cd canadaimmigrationbackend/CanadaImmigration.Api
dotnet run
```

- A API sobe por padrão em `http://localhost:5088` (confirme no terminal ao rodar, ou em `Properties/launchSettings.json` — a porta pode variar entre máquinas).
- Documentação interativa (Swagger) disponível em `http://localhost:5088/swagger`.
- CORS e a URL da API externa são configurados via `appsettings.json` (chaves `Cors:AllowedOrigin` e `ExternalApi:BaseUrl`), não hardcoded no código.

## Como rodar o frontend

Pré-requisito: **Node.js 22.23.2+** (recomendado usar `nvm`, o repositório já tem um `.nvmrc`).

```bash
cd canada-immigration-web
nvm use          # garante a versão certa do Node
npm install
ng serve
```

- A aplicação sobe em `http://localhost:4200`.
- Por padrão (`ng serve`), o frontend já aponta para o backend local em `http://localhost:5088/api` (configurado em `src/environments/environment.development.ts`).
- **Importante**: o backend (`dotnet run`) precisa estar rodando em paralelo para as telas de Draws, Pool e Proof of Funds funcionarem — sem ele, o app carrega normalmente mas exibe mensagens de erro ao buscar dados.

## Como rodar os testes

**Backend (xUnit):**
```bash
cd canadaimmigrationbackend
dotnet test
```

**Frontend (Vitest):**
```bash
cd canada-immigration-web
ng test
```

## Deploy / Demo ao vivo

O frontend está publicado como página estática no GitHub Pages:

**🔗 https://dutra3.github.io/canada-immigration/**

Esse deploy é **somente do frontend** — o GitHub Pages não hospeda backends .NET (só arquivos estáticos). Por decisão de projeto, o backend não fica hospedado publicamente; ele é executado localmente (`dotnet run`) durante demonstrações. Por isso, ao acessar o link acima sem o backend rodando ao lado, as telas carregam normalmente mas exibem mensagens de erro ao tentar buscar dados — esse é o comportamento esperado.

Para gerar um novo deploy do frontend após alterações:
```bash
cd canada-immigration-web
ng deploy --base-href=/canada-immigration/
```

## Sobre o projeto

Este é um projeto pessoal, feito para praticar .NET 8 / C# e Angular 22 com boas práticas (arquitetura em camadas, testes automatizados em ambas as pontas, configuração via ambiente em vez de valores hardcoded), sem uso de bibliotecas de terceiros além das que já vêm nos scaffolds oficiais do `dotnet new` e `ng new`.