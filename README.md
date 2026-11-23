# Canada Immigration Tracker

[![CI](https://github.com/Dutra3/canada-immigration/actions/workflows/ci.yml/badge.svg)](https://github.com/Dutra3/canada-immigration/actions/workflows/ci.yml)

Sistema web sem login para consultar dados públicos de imigração canadense (Express Entry), calcular sua nota CRS e receber alertas por e-mail quando uma nova chamada (draw) for publicada.

Projeto pessoal de estudo e portfólio, feito para praticar .NET 8 / C# e Angular 22 com boas práticas — arquitetura em camadas, testes automatizados nas duas pontas e configuração via ambiente em vez de valores hardcoded. Dependências externas mínimas: no backend, EF Core/SQLite para persistência, MailKit para envio de e-mail e o pipeline de resiliência oficial da Microsoft (`Microsoft.Extensions.Http.Resilience`, baseado em Polly) para chamadas à API externa; no frontend, nenhuma biblioteca adicional de UI.

## O que o projeto faz

- **Express Entry Draws**: histórico paginado das chamadas (draws) do sistema Express Entry, com programa, data, convites emitidos e pontuação CRS mínima, com filtros por ano e por categoria. Traz também um destaque sempre atualizado com o draw mais recente, independente da página que você está navegando.
- **Distribuição do Pool CRS**: gráfico de barras mostrando quantos candidatos existem em cada faixa de pontuação CRS, com base no draw mais recente.
- **Tendência do corte CRS**: gráfico de linha mostrando a evolução da pontuação CRS mínima ao longo do tempo, com filtros por período (ano) e categoria.
- **Proof of Funds**: calculadora do valor mínimo em fundos exigido pelo IRCC (Immigration, Refugees and Citizenship Canada), de acordo com o número de familiares.
- **Calculadora CRS**: estima sua nota no Comprehensive Ranking System a partir de idade, educação, línguas oficiais (CLB/NCLC), experiências de trabalho, pontos adicionais e dados do cônjuge — com breakdown por fator. Usa as tabelas oficiais do IRCC (pontos de job offer foram removidos pelo IRCC em março/2025 e não entram no cálculo). Após calcular, o app também compara sua nota com o corte do draw mais recente de cada categoria ("você seria convidado?"), marcando as categorias em que você seria elegível, e mostra sua posição aproximada no pool atual.
- **Alertas por e-mail de novos draws**: na página "Receber E-mail" você cadastra seu e-mail e escolhe as categorias de draws que quer acompanhar (ou "Todas/Qualquer"). Um serviço em background no backend verifica periodicamente a API pública e, ao detectar um draw novo, envia um e-mail para os inscritos daquela categoria. Todo e-mail inclui um link para cancelar a inscrição.

Os dados de Express Entry e Pool vêm de uma **API pública community-driven** (não oficial do IRCC): `https://can-ee-draws.karanjit-sagun01.workers.dev/api/`. O cálculo de Proof of Funds usa a tabela oficial do IRCC, implementada localmente no backend (sem depender de API externa) — o mesmo vale para a Calculadora CRS, que segue o grid oficial de critérios do CRS publicado pelo IRCC. Já as inscrições de e-mail ficam armazenadas em um banco **SQLite local**, gerenciado via EF Core — nenhum dado é enviado para serviços de terceiros além do envio do próprio e-mail via SMTP.

## Stack e versões

| Camada     | Tecnologia          | Versão   |
|------------|----------------------|----------|
| Backend    | .NET / C#            | .NET 8   |
| Persistência (backend) | EF Core + SQLite | EF Core 8 |
| Frontend   | Angular (standalone components) | Angular 22 |
| Testes backend | xUnit (sem libs de mock de terceiros — fakes escritos à mão) | — |
| Testes frontend | Vitest (via `@angular/build:unit-test`) | — |
| CI | GitHub Actions (testes de backend + testes e build do frontend em `.github/workflows/ci.yml`) | — |
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
    └── src/app/features/               ← telas: draws, pool, trends, proof-of-funds, crs-calculator e subscribe
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
- As respostas da API externa ficam em **cache em memória por 15 minutos** (`IMemoryCache`), para não bater na API community a cada request — configurável via `ExternalApi:CacheMinutes`. Como o cache é em memória, ele **não é compartilhado entre instâncias**: se um dia o backend rodar em múltiplas réplicas, vale migrar para `IDistributedCache`/FusionCache.
- As chamadas à API externa passam por um **pipeline de resiliência** (retry com backoff exponencial + jitter, circuit breaker e timeout) via `Microsoft.Extensions.Http.Resilience`, para tolerar instabilidades da API community sem derrubar os endpoints.
- Na primeira execução, o backend cria automaticamente o banco SQLite `canadaimmigration.db` na pasta do projeto, aplicando as migrations de `Data/Migrations/` no startup. A connection string pode ser alterada via `ConnectionStrings:Default` no `appsettings.json`.
- **Limitação do SQLite em produção**: o arquivo do banco vive no disco da instância que está rodando. Em deploy com containers/PaaS, os dados (inscrições, estado do polling e logs de notificação) **não sobrevivem a um redeploy** e não são compartilhados entre réplicas. Para produção real, o caminho é migrar para Postgres (ou outro banco cliente-servidor) — o código já está isolado atrás do `AppDbContext`, então a troca é basicamente trocar o provider e gerar uma nova migration.
- Existe um endpoint de health check em **`GET /health`**, que verifica a conectividade com o SQLite e com a API externa do Express Entry — útil para monitoramento e probes de deploy.

### Configurando as notificações por e-mail (opcional)

O backend inclui um serviço em background (`ExpressEntryPollingService`) que verifica a API pública a cada **60 minutos** (configurável via `ExpressEntryPolling:IntervalMinutes`), somente entre **7h e 19h de Brasília, de segunda a sábado**. Ao detectar um draw novo, ele envia e-mail aos inscritos da categoria correspondente — cada envio é isolado em try/catch, então um endereço inválido não interrompe o lote, e cada tentativa fica registrada na tabela `NotificationLogs` (auditoria de "quem recebeu o alerta de qual draw"). O envio usa **MailKit** (SMTP moderno, com STARTTLS na porta 587 ou SSL direto na 465). Todo e-mail inclui um link para `/unsubscribe?token=...` no frontend, que chama `GET /api/Subscriptions/unsubscribe` e mostra uma página amigável de confirmação.

A chave `Email:SiteBaseUrl` do `appsettings.json` deve apontar para a **URL pública do frontend** (ex.: `http://localhost:4200` em dev), pois é ela que monta o link de cancelamento dentro do e-mail.

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

O endpoint `POST /api/Subscriptions` tem **rate limit de 5 requisições por minuto por IP** (retorna HTTP 429 acima disso), como proteção contra spam de cadastros — configurável via `RateLimiting:SubscribePermitLimit` e `RateLimiting:SubscribeWindowMinutes` no `appsettings.json`. Os endpoints de cálculo (`POST /api/CrsScore`, `GET /api/CrsScore/invitation` e `GET /api/ProofOfFunds`) têm um limite mais generoso de **60 requisições por minuto por IP** (`RateLimiting:ComputePermitLimit`), já que são computação local sem cache.

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
- Em produção (`environment.ts`), o `apiBaseUrl` é `/api` — ou seja, o build assume que frontend e API são servidos do **mesmo domínio** (ou atrás de um proxy que encaminhe `/api`). Se a API estiver em outro host, ajuste `apiBaseUrl` para a URL completa.
- **Importante**: o backend (`dotnet run`) precisa estar rodando em paralelo para as telas de Draws, Pool, Proof of Funds e o formulário de inscrição funcionarem — sem ele, o app carrega normalmente mas exibe mensagens de erro ao buscar dados.
- Todas as chamadas HTTP passam por um **interceptor** (`core/interceptors/api.interceptor.ts`) que aplica timeout de 20s e traduz erros para mensagens amigáveis em português (incluindo o 429 de rate limit). Cada rota define `title`/`description` no `app.routes.ts`, aplicados ao `<title>` e à meta description via `AppTitleStrategy`.

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