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

- Linguagem: C# / ASP.NET Core 8
- Banco de dados: SQL Server (Entity Framework Core)
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

## Como rodar

1. Configure a connection string em `KnowHowApi/appsettings.json` (`ConnectionStrings:DefaultConnection`) e a `Jwt:SecretKey`.
2. Restaure os pacotes e gere a primeira migração:
   ```
   dotnet restore
   dotnet ef migrations add InicialUsuarios --project KnowHowApi
   dotnet ef database update --project KnowHowApi
   ```
3. Rode a API:
   ```
   dotnet run --project KnowHowApi
   ```
