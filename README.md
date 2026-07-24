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

## Banco de dados

O projeto usa **SQL Server** via Entity Framework Core. Para rodar localmente (Windows),
cada dev precisa ter uma instância do **SQL Server 2022 Developer Edition** — é gratuita,
sem limite de tamanho, licenciada para uso em desenvolvimento/teste.

### 1. Instalar o SQL Server

Abra o **PowerShell como Administrador** (clique direito no ícone > "Executar como
administrador" — não use o terminal integrado do VS Code, ele pode não ter permissão de
elevação) e rode:

```powershell
.\scripts\install-sqlserver-dev.ps1
```

O script baixa e instala o SQL Server Developer Edition com:
- Autenticação em modo misto habilitada (usuário `sa`)
- Senha do `sa` já igual à que está em `KnowHowApi/appsettings.json` (`@Senha123@`)
- TCP/IP habilitado na porta padrão (1433)

Demora de 10 a 15 minutos. No final deve aparecer `Concluído!` no console.

Se preferir instalar manualmente (ex: já tem SQL Server/Docker configurado de outro jeito),
só garanta que a connection string em `appsettings.json` bate com o seu ambiente.

### 2. Criar o banco e aplicar as migrations

Com o SQL Server rodando, na raiz do projeto:

```powershell
dotnet ef database update --project KnowHowApi
```

Isso cria o banco `KnowHowDb` e aplica as migrations já existentes (não precisa gerar
migration nova — ela já está versionada em `KnowHowApi/Migrations/`).

Se o comando `dotnet ef` não existir, instale a ferramenta global antes:
```powershell
dotnet tool install --global dotnet-ef
```

### Problemas comuns

- **Erro de conexão / "server not found"**: confirme que o serviço está rodando:
  `Get-Service MSSQLSERVER` deve mostrar `Running`. Se não existir, a instalação não
  terminou — rode o script de novo.
- **Erro ao extrair mídia / `MediaType` inválido**: só ocorre se você editar o script; o
  valor correto é `CAB` (não `Core`).
- Sempre feche e reabra o PowerShell **como administrador** antes de rodar o script — sem
  elevação, a instalação falha silenciosamente sem gerar log de erro.

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
