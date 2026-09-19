# Canada Immigration Tracker

Sistema web sem login para consultar dados públicos de imigração canadense (Express Entry) e receber alertas por e-mail quando uma nova chamada (draw) for publicada, feito como projeto pessoal de estudo e portfólio.

## O que o projeto faz

- **Express Entry Draws**: histórico paginado das chamadas (draws) do sistema Express Entry, com programa, data, convites emitidos e pontuação CRS mínima, com filtros por ano e por categoria. Traz também um destaque sempre atualizado com o draw mais recente, independente da página que você está navegando.
- **Distribuição do Pool CRS**: gráfico de barras mostrando quantos candidatos existem em cada faixa de pontuação CRS, com base no draw mais recente.
- **Tendência do corte CRS**: gráfico de linha mostrando a evolução da pontuação CRS mínima ao longo do tempo, com filtros por período (ano) e categoria.
- **Proof of Funds**: calculadora do valor mínimo em fundos exigido pelo IRCC (Immigration, Refugees and Citizenship Canada), de acordo com o número de familiares.
- **Alertas por e-mail de novos draws**: na página "Receber E-mail" você cadastra seu e-mail e escolhe as categorias de draws que quer acompanhar (ou "Todas/Qualquer"). Um serviço em background no backend verifica periodicamente a API pública e, ao detectar um draw novo, envia um e-mail para os inscritos daquela categoria. Todo e-mail inclui um link para cancelar a inscrição.

Os dados de Express Entry e Pool vêm de uma **API pública community-driven** (não oficial do IRCC): `https://can-ee-draws.karanjit-sagun01.workers.dev/api/`. O cálculo de Proof of Funds usa a tabela oficial do IRCC, implementada localmente no backend (sem depender de API externa). Já as inscrições de e-mail ficam armazenadas em um banco **SQLite local**, gerenciado via EF Core — nenhum dado é enviado para serviços de terceiros além do envio do próprio e-mail via SMTP.

## Stack e versões

| Camada     | Tecnologia          | Versão   |
|------------|----------------------|----------|
| Backend    | .NET / C#            | .NET 8   |
| Persistência (backend) | EF Core + SQLite | EF Core 8 |
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
│   │   └── Data/                       ← AppDbContext (EF Core) e migrations do SQLite
│   └── CanadaImmigration.Api.Tests/    ← testes xUnit
└── canada-immigration-web/             ← projeto Angular (standalone components)
    └── src/app/features/               ← telas: draws, pool, trends, proof-of-funds e subscribe
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
- Na primeira execução, o backend cria automaticamente o banco SQLite `canadaimmigration.db` na pasta do projeto, aplicando as migrations de `Data/Migrations/` no startup. A connection string pode ser alterada via `ConnectionStrings:Default` no `appsettings.json`.

### Configurando as notificações por e-mail (opcional)

O backend inclui um serviço em background (`ExpressEntryPollingService`) que verifica a API pública a cada **60 minutos** (configurável via `ExpressEntryPolling:IntervalMinutes`), somente entre **7h e 19h de Brasília, de segunda a sábado**. Ao detectar um draw novo, ele envia e-mail aos inscritos da categoria correspondente.

Para o envio funcionar, é preciso preencher a seção `Email` do `appsettings.json` (servidor SMTP, remetente e senha). Como envolve credencial, o recomendado é usar **user-secrets** em desenvolvimento ou variáveis de ambiente em produção:

```bash
cd canadaimmigrationbackend/CanadaImmigration.Api
dotnet user-secrets set "Email:SmtpServer" "smtp.gmail.com"
dotnet user-secrets set "Email:SmtpPort" "587"
dotnet user-secrets set "Email:SenderEmail" "seu-email@gmail.com"
dotnet user-secrets set "Email:SenderPassword" "sua-app-password"
```

> Para Gmail, use uma [senha de app](https://support.google.com/accounts/answer/185833) em vez da senha normal da conta.

Sem SMTP configurado, o resto do sistema funciona normalmente (draws, pool, proof of funds e até o cadastro de inscrições) — apenas os e-mails não são enviados, e o erro é registrado nos logs do backend.

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
- **Importante**: o backend (`dotnet run`) precisa estar rodando em paralelo para as telas de Draws, Pool, Proof of Funds e o formulário de inscrição funcionarem — sem ele, o app carrega normalmente mas exibe mensagens de erro ao buscar dados.

## Como rodar os testes

**Backend (xUnit):**
```bash
dotnet test canadaimmigrationtracker.slnx
```

**Frontend (Vitest):**
```bash
cd canada-immigration-web
ng test
```

## Deploy / Demo ao vivo

O frontend está publicado como página estática no GitHub Pages:

**🔗 https://dutra3.github.io/canada-immigration/**

Esse deploy é **somente do frontend** — o GitHub Pages não hospeda backends .NET (só arquivos estáticos). Por decisão de projeto, o backend não fica hospedado publicamente; ele é executado localmente (`dotnet run`) durante demonstrações. Por isso, ao acessar o link acima sem o backend rodando ao lado, as telas carregam normalmente mas exibem mensagens de erro ao tentar buscar dados ou enviar o formulário de inscrição — esse é o comportamento esperado.

Para gerar um novo deploy do frontend após alterações:
```bash
cd canada-immigration-web
ng deploy --base-href=/canada-immigration/
```

## Sobre o projeto

Este é um projeto pessoal, feito para praticar .NET 8 / C# e Angular 22 com boas práticas (arquitetura em camadas, testes automatizados em ambas as pontas, configuração via ambiente em vez de valores hardcoded), com poucas dependências além dos scaffolds oficiais do `dotnet new` e `ng new` — no backend, apenas EF Core/SQLite para persistência; no frontend, nenhuma biblioteca adicional de UI.