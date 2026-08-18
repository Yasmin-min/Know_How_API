# Know How API

Plataforma web (PWA) para contratação e agendamento de aulas on-line, conectando professores e alunos.

## Tipos de usuário

- **Professor**: cria perfil, informa matérias/cursos oferecidos, qualificações e faixa de valor das aulas.
- **Aluno**: pesquisa professores (por curso, matéria ou valor), visualiza perfis e inicia negociação.

## Fluxo principal

1. Aluno filtra e escolhe um professor.
2. Aluno inicia uma conversa via chat integrado com o professor.
3. Professor e aluno negociam valor, data e horário da aula pelo chat.

## Tecnologias

- Linguagem: C# / ASP.NET Core 9
- Banco de dados: PostgreSQL (Entity Framework Core)
- Autenticação: JWT (login por e-mail/senha)
- Comunicação em tempo real: SignalR (chat professor-aluno)
- Arquitetura: DDD em camadas (Controllers → Services → Repositories → Data)

## Arquitetura

```
KnowHowApi/
  Controllers/        # Endpoints da API
  Services/            # Regras de negócio
    Interfaces/
    Utils/             # Cryptography (hash de senha)
  Data/
    Repositories/       # Acesso a dados (padrão Repository)
    Maps/                # Configuração EF Core (Fluent API)
    Context.cs
  Domain/
    Models/              # Entidades
    DTOs/                # Contratos de entrada/saída
    Enum/
    Interfaces/           # Contratos dos repositórios
    Configurations/        # Settings tipadas (ex: JWTSettings)
  Hubs/                    # Hubs SignalR (ex: ChatHub)
KnowHowApi.Tests/          # Testes (xUnit + Moq)
```

## Status atual

Este é o esqueleto base do projeto: autenticação (registro/login) e estrutura em camadas já
funcionam. Os domínios de **Perfil de Professor**, **Busca/Filtro**, **Chat/Conversa** e
**Agendamento** ainda serão modelados e implementados.

## Banco de dados

O projeto usa **PostgreSQL** via Entity Framework Core. Para rodar localmente (Windows),
instale o PostgreSQL 17 com o winget:

```powershell
winget install --id PostgreSQL.PostgreSQL.17 -e --source winget --override "--mode unattended --unattendedmodeui minimal --superpassword postgres --serverport 5432 --disable-components stackbuilder"
```

Isso instala o Postgres como serviço do Windows (inicia sozinho com a máquina) já com o
usuário `postgres`/senha `postgres` na porta `5432` — igual à connection string padrão de
`KnowHowApi/appsettings.json`
(`Host=localhost;Port=5432;Database=knowhowdb;Username=postgres;Password=postgres`).

Se preferir Docker em vez de instalar localmente:
```powershell
docker run --name knowhow-postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=knowhowdb -p 5432:5432 -d postgres:16
```

Qualquer que seja a opção escolhida, só garanta que a connection string bate com o seu
ambiente.

### Aplicar as migrations

Com o Postgres rodando, na raiz do projeto:

```powershell
dotnet ef database update --project KnowHowApi
```

Isso cria o schema e já popula as áreas de interesse padrão (não precisa gerar migration
nova — ela já está versionada em `KnowHowApi/Migrations/`).

Se o comando `dotnet ef` não existir, instale a ferramenta global antes:
```powershell
dotnet tool install --global dotnet-ef
```

## Como rodar

1. Confirme que o banco está criado (seção **Banco de dados** acima).
2. Restaure os pacotes:
   ```
   dotnet restore
   ```
3. Rode a API:
   ```
   dotnet run --project KnowHowApi
   ```
4. Acesse `http://localhost:5046/swagger` para explorar e testar os endpoints (Swagger UI
   só fica habilitado em ambiente de desenvolvimento).

## Deploy (free tier, sem cartão de crédito)

- **Banco**: [Neon](https://neon.tech) — Postgres free, sem expiração. Copie a connection
  string do painel e converta para o formato Npgsql, ex.:
  `Host=<host>;Port=5432;Database=<db>;Username=<user>;Password=<senha>;Ssl Mode=Require`.
- **API**: [Render](https://render.com) — Web Service free, builda a partir do `Dockerfile`
  na raiz do repo. Configure como variáveis de ambiente do serviço (nunca no
  `appsettings.json` commitado):
  - `ConnectionStrings__DefaultConnection` — a connection string do Neon
  - `Jwt__SecretKey`, `Jwt__Issuer`, `Jwt__Audience`
  - `ASPNETCORE_ENVIRONMENT=Production`

  O Render injeta a porta a ser usada via variável `PORT`; o `Program.cs` já lê essa
  variável e escuta nela quando presente (sem afetar o `dotnet run` local, que continua
  usando `launchSettings.json`).
- Depois do primeiro deploy, rode `dotnet ef database update --project KnowHowApi` uma vez
  a partir da sua máquina apontando para a connection string do Neon, para criar o schema
  no banco de produção.
