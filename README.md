# HomeControllerHUB

API backend para gerenciamento de ambientes, sensores IoT, usuários, estabelecimentos e dados de monitoramento operacional.

## Visão geral

O **HomeControllerHUB** é uma API REST desenvolvida em .NET para apoiar a administração de uma plataforma de automação residencial e monitoramento IoT.

O projeto combina recursos administrativos, como gestão de usuários, perfis, privilégios e estabelecimentos, com funcionalidades voltadas ao contexto IoT, incluindo cadastro de locais, sensores, ingestão de leituras, atualização de status e suporte a alertas.

Este repositório contém apenas o backend da solução. A API foi estruturada para ser consumida por aplicações frontend, sistemas administrativos e dispositivos IoT.

## Principais recursos

- Autenticação com ASP.NET Core Identity e JWT.
- Controle de acesso baseado em perfis, privilégios, domínios e ações.
- Gestão de usuários, perfis e estabelecimentos.
- Cadastro de locais hierárquicos e sensores IoT.
- Ingestão de leituras individuais e em lote.
- Registro de atualizações de status dos sensores.
- Suporte à geração de alertas por thresholds e bateria baixa.
- Persistência em PostgreSQL com Entity Framework Core.
- Documentação de API via Swagger/OpenAPI.
- Suporte a globalização de mensagens.
- Testes automatizados com xUnit e Testcontainers.
- Suporte a Docker Compose para ambiente local.

## Tecnologias utilizadas

| Categoria | Tecnologias |
| --- | --- |
| Backend | .NET 9, ASP.NET Core Web API |
| Banco de dados | PostgreSQL, Entity Framework Core, Npgsql |
| Autenticação | ASP.NET Core Identity, JWT Bearer |
| Arquitetura | MediatR, CQRS, FluentValidation, AutoMapper |
| Documentação | Swagger / OpenAPI |
| Testes | xUnit, Moq, FluentAssertions, Testcontainers |
| Infraestrutura | Docker, Docker Compose |
| Integrações | Mailgun via HttpClient |

## Arquitetura

O projeto segue uma organização em camadas, separando responsabilidades de entrada HTTP, casos de uso, domínio, infraestrutura e recursos compartilhados.

Fluxo principal:

```text
Controller -> Command/Query -> Handler -> Services/DbContext -> PostgreSQL
```

Camadas principais:

| Projeto | Responsabilidade |
| --- | --- |
| `HomeControllerHUB.Api` | Camada de entrada da API, com controllers, middlewares, Swagger, versionamento e inicialização. |
| `HomeControllerHUB.Application` | Casos de uso organizados em commands, queries, handlers, DTOs e validações. |
| `HomeControllerHUB.Domain` | Entidades, modelos, interfaces, mapeamentos e configurações do domínio. |
| `HomeControllerHUB.Infra` | Persistência, serviços, interceptors, inicializadores de dados e integrações externas. |
| `HomeControllerHUB.Shared` | Constantes, atributos e utilitários compartilhados. |
| `HomeControllerHUB.Globalization` | Recursos e serviços de localização. |
| `tests` | Projetos de testes automatizados. |

## Estrutura do repositório

```text
.
|-- src/
|   |-- HomeControllerHUB.Api/
|   |-- HomeControllerHUB.Application/
|   |-- HomeControllerHUB.Domain/
|   |-- HomeControllerHUB.Globalization/
|   |-- HomeControllerHUB.Infra/
|   `-- HomeControllerHUB.Shared/
|-- tests/
|-- docs/
|-- Dockerfile
|-- docker-compose.yml
`-- HomeControllerHUB.sln
```

## Pré-requisitos

- .NET SDK 9.0.
- Docker Desktop ou Docker Engine.
- PostgreSQL local ou PostgreSQL via Docker Compose.
- `dotnet-ef`, caso queira gerenciar migrations manualmente.

Instalação opcional do Entity Framework CLI:

```bash
dotnet tool install --global dotnet-ef
```

## Configuração local

As principais configurações da aplicação ficam em:

```text
src/HomeControllerHUB.Api/appsettings.json
src/HomeControllerHUB.Api/appsettings.Development.json
src/HomeControllerHUB.Api/appsettings.Testing.json
```

Para desenvolvimento local, o projeto utiliza PostgreSQL na porta `15432`, conforme definido no `docker-compose.yml`.

Como o compose usa uma rede externa, crie a rede antes da primeira execução:

```bash
docker network create home-controller-hub-network
```

Em ambientes reais, utilize variáveis de ambiente, Secret Manager ou outro mecanismo seguro para connection strings, chaves JWT, credenciais de banco e configurações de e-mail.

## Como executar

Suba o PostgreSQL com Docker Compose:

```bash
docker compose up -d home-controller-hub-postgres
```

Restaure as dependências:

```bash
dotnet restore HomeControllerHUB.sln
```

Compile a solução:

```bash
dotnet build HomeControllerHUB.sln
```

Execute a API:

```bash
dotnet run --project src/HomeControllerHUB.Api/HomeControllerHUB.Api.csproj --launch-profile http
```

Por padrão, o launch profile `http` executa a API em:

```text
http://localhost:6001
```

## Banco de dados e migrations

O projeto usa Entity Framework Core Migrations. As migrations ficam no projeto `HomeControllerHUB.Api` e o `ApplicationDbContext` está na camada `HomeControllerHUB.Infra`.

Durante a inicialização da aplicação, o projeto aplica migrations automaticamente conforme a configuração atual:

```csharp
dbContext.Database.Migrate();
```

Para aplicar migrations manualmente:

```bash
dotnet ef database update --project src/HomeControllerHUB.Api/HomeControllerHUB.Api.csproj --startup-project src/HomeControllerHUB.Api/HomeControllerHUB.Api.csproj --context ApplicationDbContext
```

O projeto também possui inicialização de dados de desenvolvimento quando a configuração `ApplicationSettings:InitializeDataBase` está habilitada.

## Testes

Execute todos os testes:

```bash
dotnet test HomeControllerHUB.sln
```

Alguns testes utilizam Testcontainers com PostgreSQL. Para executar essa suíte, o Docker precisa estar ativo e acessível no ambiente local.

## Documentação da API

Com a API em execução nos ambientes `Development` ou `Testing`, a documentação Swagger fica disponível em:

```text
http://localhost:6001/swagger
```

O README mantém apenas uma visão geral do projeto. Detalhes de endpoints, contratos e integração ficam nos documentos complementares.

## Documentação complementar

A pasta `docs/` contém materiais mais detalhados sobre o projeto:

- `docs/Frontend_API_Documentation.md`: guia de integração para frontend.
- `docs/IoT_Implementation_Guide.md`: guia relacionado ao domínio IoT.
- `docs/ProjectReport.md`: relatório geral do projeto.
- `docs/ProjDoc.md`: documentação de arquitetura, entidades e execução.
- `docs/PROJECT_REPORT.md`: análise detalhada da estrutura, fluxos e pontos técnicos.

## Status do projeto

Projeto em evolução, utilizado para estudos e desenvolvimento de uma solução backend para automação residencial, administração de ambientes e monitoramento IoT.

## Autor

**Wilhelm Henrique Zimmermann**

GitHub: [Wilhelm-Zimmermann](https://github.com/Wilhelm-Zimmermann)
