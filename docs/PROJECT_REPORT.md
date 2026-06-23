# HomeControllerHUB - Project Report

> Relatório elaborado a partir da estrutura atual do repositório, dos projetos `.csproj`, do código-fonte, das migrations, das configurações, dos testes e dos documentos Markdown existentes.
>
> Neste documento, **fato observado** significa comportamento diretamente identificável no código ou na configuração. **Inferência** significa uma conclusão provável baseada na combinação entre nomes, estrutura e implementação. Quando a documentação existente diverge do código, o código atual é tratado como a fonte principal.

## 1. Visão geral do projeto

O **HomeControllerHUB** é um backend HTTP construído com ASP.NET Core para administrar usuários, estabelecimentos e recursos de automação/monitoramento por IoT.

O núcleo funcional implementado combina dois grupos de capacidades:

- administração de usuários, estabelecimentos, perfis, privilégios e menus;
- cadastro e monitoramento de locais e sensores, com recepção de leituras, atualização de status e geração de alertas.

O projeto não contém uma aplicação frontend. Ele expõe uma API REST versionada e inclui recursos voltados à integração de um frontend administrativo e de dispositivos IoT, como ESP32 ou Arduino.

### Problema que o sistema procura resolver

Com base nas entidades, endpoints e documentos `IoT_Implementation_Guide.md`, `ProjDoc.md` e `Frontend_API_Documentation.md`, o sistema procura centralizar:

- o cadastro de estabelecimentos que funcionam como unidades ou tenants;
- a organização física desses estabelecimentos em prédios, andares, salas, áreas ou equipamentos;
- o cadastro de sensores instalados nesses locais;
- a coleta de dados de sensores;
- a geração de alertas quando uma leitura viola limites mínimos ou máximos;
- o acompanhamento de bateria, firmware, conectividade e status dos sensores;
- o controle de acesso por usuários, perfis, domínios e privilégios;
- a retenção de dados conforme o plano de assinatura do estabelecimento.

### Funcionalidades efetivamente encontradas no código

- autenticação de usuários por login e senha;
- emissão de JWT e refresh token;
- confirmação de e-mail;
- recuperação e alteração de senha;
- CRUD de usuários;
- CRUD de estabelecimentos;
- CRUD de perfis e associação de privilégios;
- listagem de privilégios acessíveis ao usuário;
- composição de menus conforme os domínios autorizados;
- CRUD de locais hierárquicos;
- CRUD de sensores;
- envio individual e em lote de leituras de sensores;
- atualização de status de sensores;
- alertas de limite mínimo, limite máximo e bateria baixa;
- consulta paginada de sensores, leituras e alertas;
- limpeza periódica de leituras antigas e alertas reconhecidos;
- inicialização automática de dados básicos;
- localização de mensagens em português e inglês;
- documentação Swagger/OpenAPI em Development e Testing.

### Funcionalidades descritas, mas não integralmente expostas

Os documentos sugerem dashboards, relatórios, gestão completa de planos e limites comerciais. O código contém `SubscriptionPlan`, retenção por plano e campos como `MaxSensors`, `AlertsPerMonth`, `IncludesReporting` e `IncludesApiAccess`, mas:

- não existe controller ou fluxo CRUD para planos de assinatura;
- os limites de quantidade de sensores e alertas não são aplicados pelos handlers;
- não existem endpoints específicos de dashboard ou relatórios;
- existe suporte de persistência para reconhecimento de alertas, mas não foi encontrado endpoint para reconhecer um alerta.

Assim, a parte de assinatura e reporting deve ser entendida como um domínio parcialmente modelado.

## 2. Tecnologias utilizadas

### Backend

- **.NET 9 / C#**: todos os projetos usam `TargetFramework` `net9.0`.
- **ASP.NET Core Web API**: a aplicação executável está em `src/HomeControllerHUB.Api`.
- **ASP.NET Core MVC**: controllers, model binding, filtros de resposta e roteamento.
- **API Versioning**: os pacotes `Asp.Versioning.Mvc` e `Asp.Versioning.Mvc.ApiExplorer` implementam versão por segmento de URL, como `/api/v1/...`.
- **MediatR 12.4.1**: desacopla os controllers dos handlers de commands e queries.
- **AutoMapper 14.0.0**: converte entidades em DTOs e é usado com `ProjectTo` em consultas.
- **FluentValidation**: define regras para commands e queries.

### Banco de dados e persistência

- **Entity Framework Core 9**: ORM usado diretamente pelos handlers.
- **Npgsql.EntityFrameworkCore.PostgreSQL**: provider PostgreSQL.
- **PostgreSQL**: banco configurado nos `appsettings` e no `docker-compose.yml`.
- **Migrations Code First**: armazenadas em `src/HomeControllerHUB.Api/Migrations`.
- **JSONB**: usado para `Metadata` de leituras e atualizações de status.
- **System.Linq.Dynamic.Core**: usado para ordenação dinâmica na paginação genérica.

### Autenticação e autorização

- **ASP.NET Core Identity**: `ApplicationUser` herda de `IdentityUser<Guid>`, e `ApplicationDbContext` herda de `IdentityDbContext`.
- **JWT Bearer**: autenticação configurada em `src/HomeControllerHUB.Api/ConfigureServices.cs`.
- **JWT customizado**: tokens são gerados por `src/HomeControllerHUB.Infra/Services/JwtTokenService.cs`.
- **MediatR pipeline behavior**: a autorização por domínio e ação é aplicada por `AuthorizationBehaviour<TRequest,TResponse>`.
- **Perfis e privilégios próprios**: `Profile`, `Privilege`, `ProfilePrivilege` e `UserProfile` formam o modelo principal de autorização de negócio.

### Validações

- **FluentValidation 11**: validators ficam próximos dos commands/queries.
- **Validações assíncronas com EF Core**: por exemplo, os validators de local e sensor verificam existência e relacionamentos no banco.
- **Regras nos handlers**: regras que dependem do estado atual também aparecem nos handlers, como impedir a exclusão de um local com filhos ou sensores.

### Testes

- **xUnit 2.9.2**: framework de testes.
- **FluentAssertions 8.5.0**: asserções legíveis.
- **Moq 4.20.72**: mocks de serviços e dependências.
- **FluentValidation.TestHelper**: validação direta dos validators.
- **Testcontainers.PostgreSql 4.6.0**: sobe PostgreSQL real para os testes de Application.
- **coverlet.collector**: coleta de cobertura.
- **Microsoft.EntityFrameworkCore.InMemory**: está referenciado, mas o setup principal usa PostgreSQL em container.

### Documentação e OpenAPI

- **Swashbuckle.AspNetCore 8**: geração de Swagger/OpenAPI.
- **Swashbuckle.AspNetCore.Filters**: filtros e exemplos.
- **Comentários XML**: habilitados no projeto API e incluídos no Swagger.
- **Filtros customizados**: remoção do parâmetro de versão, substituição da versão na rota, descrições automáticas e respostas 401/403.

### Globalização

- **Microsoft.Extensions.Localization**: localização por `IStringLocalizer`.
- **Arquivos `.resx`**: recursos em `src/HomeControllerHUB.Globalization/Resources`.
- **Culturas configuradas**: `pt-BR`, `en-US` e `es-ES`; porém só foram encontrados recursos específicos para português e inglês.

### Infraestrutura e integrações

- **Docker**: `Dockerfile` multi-stage para build e publicação da API.
- **Docker Compose**: define PostgreSQL; os serviços Redis e API estão comentados.
- **Mailgun**: `EmailService` envia e-mails pela API HTTP do Mailgun em builds não DEBUG.
- **HttpClientFactory**: usado para criar o cliente nomeado `Mailgun`.
- **BackgroundService**: `DataRetentionService` executa limpeza diária.

### Outros pacotes relevantes

- **Pluralize.NET**: usado na geração de descrições do Swagger.
- **Newtonsoft.Json**: usado pelo middleware de erro para serializar respostas.

## 3. Arquitetura e padrões

### Classificação geral

O projeto possui uma **arquitetura em camadas inspirada em DDD e Clean Architecture**, com CQRS e Mediator bem visíveis. Entretanto, não é uma Clean Architecture estrita.

Essa classificação é importante porque:

- o repositório e `src/Readme.md` declaram DDD, CQRS e Mediator;
- existem projetos separados para API, Application, Domain e Infra;
- porém `HomeControllerHUB.Application.csproj` referencia diretamente `HomeControllerHUB.Infra.csproj`;
- handlers de Application usam diretamente `ApplicationDbContext`, que pertence a Infra;
- Domain contém dependências de EF Core e Identity;
- não existem interfaces de repositório ou uma abstração de contexto definida em uma camada interna.

### DDD

Elementos compatíveis com uma abordagem DDD:

- entidades agrupadas em `src/HomeControllerHUB.Domain/Entities`;
- conceitos de negócio claros: estabelecimento, local, sensor, leitura, alerta, perfil e privilégio;
- constantes de domínios de autorização em `DomainNames`;
- separação de casos de uso na camada Application.

Limites da classificação:

- as entidades são predominantemente modelos de dados com propriedades;
- a maior parte das regras está nos handlers e validators;
- não foram encontrados aggregates explícitos, value objects, domain services ou domain events;
- EF Core e Identity fazem parte do projeto Domain.

Portanto, o projeto se aproxima de um **modelo de domínio anêmico organizado por conceitos de negócio**, e não de uma implementação completa de DDD tático.

### Clean Architecture

Há uma separação visual de responsabilidades:

- API: entrada HTTP;
- Application: casos de uso;
- Domain: entidades, contratos e modelos;
- Infra: banco, serviços externos e detalhes técnicos.

Contudo, a direção de dependências não segue integralmente o princípio de dependência para dentro. O principal exemplo é:

```text
API -> Application -> Infra -> Domain -> Shared
```

Em uma Clean Architecture estrita, Application normalmente dependeria de interfaces internas, e Infra implementaria essas interfaces sem ser referenciada por Application.

### CQRS

O CQRS é usado de forma estrutural:

- operações de escrita são `Command`;
- operações de leitura são `Query`;
- cada request tem um handler MediatR;
- controllers somente montam ou recebem o request e chamam `Mediator.Send`.

Exemplos:

- `CreateSensorCommand` e `CreateSensorCommandHandler`;
- `GetSensorsQuery` e `GetSensorsQueryHandler`;
- `SubmitSensorReadingCommand`;
- `GetSensorReadingsQuery`.

É um CQRS no nível da aplicação. Não há bancos, modelos de persistência ou pipelines separados para leitura e escrita.

### Mediator / MediatR

MediatR é registrado em `src/HomeControllerHUB.Application/ConfigureServices.cs`. Todos os controllers herdam de `ApiControllerBase`, que resolve um `ISender` e encaminha os requests.

O pipeline MediatR também aplica `AuthorizationBehaviour<TRequest,TResponse>`, responsável por interpretar o atributo customizado `HomeControllerHUB.Shared.Common.AuthorizeAttribute`.

### Repository Pattern

Não foram encontradas implementações de Repository Pattern.

Os projetos Domain e Infra possuem entradas de pasta `Repositories` nos `.csproj`, mas não existem classes de repositório no repositório atual. Os handlers usam `ApplicationDbContext` e `DbSet<T>` diretamente.

### Unit of Work

Não existe uma interface ou classe `IUnitOfWork`.

O `ApplicationDbContext` funciona implicitamente como unidade de trabalho:

- rastreia alterações;
- concentra inclusões, atualizações e exclusões;
- persiste as alterações com `SaveChangesAsync`.

Não foram encontrados usos explícitos de `BeginTransaction`, `Commit` ou `Rollback`. Fluxos com várias alterações dependem da transação implícita de cada chamada a `SaveChanges`.

### Dependency Injection

A configuração é distribuída por métodos de extensão:

- `src/HomeControllerHUB.Api/ConfigureServices.cs`;
- `src/HomeControllerHUB.Application/ConfigureServices.cs`;
- `src/HomeControllerHUB.Domain/ConfigureServices.cs`;
- `src/HomeControllerHUB.Infra/ConfigureServices.cs`;
- `src/HomeControllerHUB.Globalization/GlobalizationExtensions.cs`.

O ponto de composição é `src/HomeControllerHUB.Api/Program.cs`.

### DTOs e mapeamentos

DTOs ficam dentro das features da camada Application e normalmente implementam:

- `IMapFrom<TEntity>` para mapeamento AutoMapper;
- `IPaginatedDto` quando usados por `PaginatedRequest<T>` e `PaginateAsync`.

`MappingProfile` usa reflexão para encontrar implementações de `IMapFrom<>`.

Alguns DTOs sobrescrevem `Mapping` para propriedades derivadas, como:

- `UserListDto.EstablishmentName`;
- `GenericDto.Name`.

### Services

Serviços de infraestrutura ficam em `src/HomeControllerHUB.Infra/Services`:

- `JwtTokenService`;
- `EmailService`;
- `CurrentUserService`;
- `DateTimeService`;
- `DataRetentionService`;
- `ApiUserManager`.

Os contratos principais ficam em `src/HomeControllerHUB.Domain/Interfaces`.

### Validators

Validators ficam próximos do request correspondente e herdam de `AbstractValidator<T>`.

Exemplos:

- `CreateLocationCommandValidator`;
- `CreateSensorCommandValidator`;
- `GetSensorReadingsQueryValidator`;
- `PrivilegeSelectorQueryValidator`.

### Interceptors

Há dois interceptors de `SaveChanges`:

- `BaseEntityInterceptor`: atualiza `Created` e `Modified`;
- `NormalizedInterceptor`: preenche propriedades marcadas com `[Normalized]`.

## 4. Estrutura de projetos e pastas

### Solution principal

`HomeControllerHUB.sln` contém seis projetos de produção e três projetos de teste.

Existe também `src/src.sln`, uma solution secundária que contém cinco projetos de produção e não inclui Globalization nem os testes. A solution da raiz é a visão mais completa do repositório.

### `src/HomeControllerHUB.Api`

Responsabilidade: hospedagem ASP.NET Core e camada HTTP.

Conteúdo principal:

- `Program.cs`: composição da aplicação e pipeline HTTP;
- `ConfigureServices.cs`: banco, Identity, JWT e versionamento;
- `Controllers`: endpoints REST;
- `Middlewares/ErrorHandlingMiddleware.cs`: tratamento global de exceções;
- `Migrations`: histórico do schema;
- `appsettings*.json`: configurações por ambiente;
- `Properties/launchSettings.json`: perfil local;
- `HomeControllerHUB.Api.http`: arquivo de requisições manuais.

### `src/HomeControllerHUB.Application`

Responsabilidade: casos de uso.

Organização por feature:

- `Establishments`;
- `Generics`;
- `Locations`;
- `Menus`;
- `Privileges`;
- `Profiles`;
- `Sensors`;
- `SubscriptionPlans`;
- `Users`.

Dentro de uma feature são usados:

- `Commands/<Ação>`;
- `Queries/<Ação>`;
- DTOs na pasta `Queries` ou na subpasta específica.

Handlers e requests normalmente ficam no mesmo arquivo.

### `src/HomeControllerHUB.Domain`

Responsabilidade: modelos centrais, contratos, paginação e mapeamentos.

Pastas:

- `Entities`: entidades e enums;
- `Entities/Configuration`: mapeamentos EF Core;
- `Interfaces`: contratos de serviços;
- `Mappings`: infraestrutura de AutoMapper;
- `Models`: respostas, erros e paginação.

O diretório lógico `Repositories` aparece no `.csproj`, mas não contém implementação.

### `src/HomeControllerHUB.Infra`

Responsabilidade: detalhes técnicos e integrações.

Pastas:

- `DatabaseContext`: `ApplicationDbContext`;
- `DataInitializers`: seed de dados;
- `Interceptors`: auditoria, normalização e autorização MediatR;
- `Services`: JWT, e-mail, usuário atual, data/hora e retenção;
- `Settings`: classes de options;
- `Swagger`: filtros e configuração OpenAPI.

O diretório lógico `Repositories` também aparece no `.csproj`, mas está vazio.

### `src/HomeControllerHUB.Shared`

Responsabilidade: elementos reutilizados entre camadas.

Conteúdo:

- atributo customizado de autorização;
- constantes de domínios, privilégios e segurança;
- atributo de normalização;
- utilitários de string, tokens e asserts.

### `src/HomeControllerHUB.Globalization`

Responsabilidade: localização.

Conteúdo:

- `ISharedResource` e `SharedResource`;
- configuração de culturas;
- resources `pt-BR` e `en`;
- extensões para registrar e ativar localização.

### `tests/HomeControllerHUB.Application.Tests`

Responsabilidade: testes de handlers, validators e consultas.

Organização por feature semelhante à camada Application:

- Establishments;
- Generics;
- Locations;
- Menus;
- Privileges;
- Profiles;
- Sensors;
- Users.

`TestConfigs.cs` cria PostgreSQL com Testcontainers, configura interceptors e aplica migrations.

### `tests/HomeControllerHUB.Api.IntegrationTests`

Projeto preparado para testes de integração da API, mas atualmente contém apenas `UnitTest1.Test1`, sem comportamento testado.

### `tests/HomeControllerHUB.Domain.Tests`

Projeto preparado para testes do domínio, mas também contém somente um teste vazio.

### Documentação da raiz

- `ProjectReport.md`: relatório anterior, genérico e parcialmente desatualizado;
- `IoT_Implementation_Guide.md`: guia de implementação que mistura requisitos futuros e exemplos;
- `Frontend_API_Documentation.md`: documentação para frontend; arquivo não rastreado no estado analisado;
- `ProjDoc.md`: visão técnica adicional; arquivo não rastreado no estado analisado;
- `src/Readme.md`: regras e convenções do projeto.

## 5. Domínio da aplicação

### `ApplicationUser`

Caminho: `src/HomeControllerHUB.Domain/Entities/ApplicationUser.cs`.

Representa o usuário autenticável e herda de `IdentityUser<Guid>`.

Campos relevantes:

- `EstablishmentId`: estabelecimento principal;
- `Login`, `Email`, `Name`, `Document`, `Code`;
- `Enable`;
- `EmailConfirmationToken`;
- `PasswordConfirmationToken`;
- coleções `UserEstablishments` e `UserProfiles`.

Relacionamentos:

- um estabelecimento principal;
- vários estabelecimentos acessíveis;
- vários perfis.

### `Establishment`

Caminho: `src/HomeControllerHUB.Domain/Entities/Establishment.cs`.

Representa uma unidade administrativa ou tenant.

Campos relevantes:

- `Code`, gerado por sequence;
- `Name`, `SiteName`, `Document`;
- `Enable`, `IsMaster`;
- `SubscriptionPlanId`, `SubscriptionEndDate`.

Relacionamentos:

- usuários;
- associações usuário-estabelecimento;
- locais;
- sensores;
- plano de assinatura opcional.

### `Location`

Caminho: `src/HomeControllerHUB.Domain/Entities/Location.cs`.

Representa a estrutura física ou lógica de um estabelecimento.

Tipos:

- Building;
- Floor;
- Room;
- Area;
- Equipment.

Possui autorrelacionamento por `ParentLocationId`, permitindo uma árvore de locais.

### `Sensor`

Caminho: `src/HomeControllerHUB.Domain/Entities/Sensor.cs`.

Representa um dispositivo IoT.

Campos relevantes:

- estabelecimento e local;
- `DeviceId` único;
- tipo, modelo e firmware;
- `ApiKey`;
- limites mínimo e máximo;
- estado ativo;
- última comunicação;
- nível de bateria.

Tipos suportados incluem temperatura, umidade, pressão, luz, movimento, porta, água, fumaça, gás, eletricidade e customizado.

### `SensorReading`

Caminho: `src/HomeControllerHUB.Domain/Entities/SensorReading.cs`.

Representa uma amostra temporal:

- sensor;
- timestamp;
- valor;
- unidade;
- dados brutos;
- metadados em JSONB.

### `SensorAlert`

Caminho: `src/HomeControllerHUB.Domain/Entities/SensorAlert.cs`.

Representa alertas de:

- limite máximo;
- limite mínimo;
- dispositivo offline;
- bateria baixa;
- erro.

Contém campos para reconhecimento por usuário, embora o fluxo de reconhecimento não esteja exposto na API atual.

### `SensorStatusUpdate`

Caminho: `src/HomeControllerHUB.Domain/Entities/SensorStatusUpdate.cs`.

Mantém o histórico de atualizações de status, bateria, intensidade de sinal e metadados.

### `SubscriptionPlan`

Caminho: `src/HomeControllerHUB.Domain/Entities/SubscriptionPlan.cs`.

Modela preço e limites:

- máximo de sensores;
- dias de retenção;
- alertas por mês;
- reporting;
- acesso à API.

Somente a retenção é utilizada por um serviço implementado.

### `Profile`, `Privilege` e `ProfilePrivilege`

Caminhos:

- `src/HomeControllerHUB.Domain/Entities/Profile.cs`;
- `src/HomeControllerHUB.Domain/Entities/Privilege.cs`;
- `src/HomeControllerHUB.Domain/Entities/ProfilePrivilege.cs`.

Um perfil pertence a um estabelecimento e agrega privilégios. Um privilégio liga:

- um domínio;
- uma ação (`Read`, `Create`, `Update`, `Delete` ou `All`);
- um nome funcional, como `sensor-read`.

### `UserProfile` e `UserEstablishment`

São tabelas de associação entre:

- usuário e perfil;
- usuário e estabelecimento.

### `ApplicationDomain` e `ApplicationMenu`

`ApplicationDomain` representa módulos usados pela autorização e menus.

`ApplicationMenu` forma uma árvore de navegação e pode apontar para um domínio. A query de menus filtra itens com base nos domínios autorizados.

### `Generic`

Entidade de lookup categorizada por `Identifier`, com código, valor, ordem e flag de habilitação.

## 6. Fluxos principais da aplicação

### 6.1 Autenticação e geração de token

1. `POST /api/v1/Users/Token` recebe `AccessTokenUserCommand`.
2. `AccessTokenUserCommandHandler` procura o usuário por `Login`.
3. O handler exige e-mail confirmado e senha válida.
4. `JwtTokenService.GenerateAsync` cria JWT com:
   - `NameIdentifier`;
   - `Name`;
   - `EstablishmentId`.
5. `AccessTokenEntry` gera um refresh token.
6. `ApiUserManager.SetAuthenticationTokenAsync` armazena o refresh token no store do Identity.
7. O retorno contém `access_token`, `refresh_token`, `token_type`, `expires_in` e o código sequencial do estabelecimento.

### 6.2 Refresh token

1. `POST /api/v1/Users/refresh-token` recebe login e refresh token.
2. `RefreshTokenCommandHandler` carrega usuário e estabelecimento.
3. Compara o token recebido com o token armazenado no Identity.
4. Também exige usuário habilitado e e-mail confirmado.
5. Gera novo JWT e substitui o refresh token armazenado.

### 6.3 Cadastro de usuário

1. `POST /api/v1/Users` envia `CreateUserCommand`.
2. O handler cria `ApplicationUser` com e-mail inicialmente não confirmado.
3. Cria associações `UserProfile` e `UserEstablishment`.
4. `UserManager.CreateAsync` persiste o usuário e aplica a senha.
5. `SaveChangesAsync` persiste associações.
6. `EmailService.SendEmailConfirmationAsync` envia o link de confirmação.
7. Retorna `BaseEntityResponse` com o ID.

### 6.4 Confirmação de e-mail e recuperação de senha

- `GET /api/v1/Users/confirm-email` procura o usuário pelo token customizado, marca `EmailConfirmed` e limpa o token.
- `POST /api/v1/Users/forgot-password` gera e armazena token customizado e envia e-mail.
- `POST /api/v1/Users/reset-password-with-token` encontra o usuário pelo token customizado, gera internamente um token Identity e redefine a senha.
- `POST /api/v1/Users/reset-password` altera a senha do próprio usuário autenticado.

### 6.5 Gestão de estabelecimentos

- criação: cria `Establishment` e associa usuários por `UserEstablishment`, incluindo o usuário autenticado;
- atualização: substitui as associações de usuários;
- exclusão: é lógica, alterando `Enable` para `false`, e remove associações `UserEstablishment`;
- consultas: lista paginada, selector e busca por ID.

### 6.6 Gestão de perfis

- criação: usa o estabelecimento da claim atual e cria `ProfilePrivilege`;
- atualização: substitui todos os privilégios;
- exclusão: remove `UserProfile`, `ProfilePrivilege` e o perfil;
- consultas: paginação, selector e busca por ID.

### 6.7 Gestão de locais

- criação: associa o local a um estabelecimento e opcionalmente a um local pai;
- atualização: impede somente a autorreferência direta;
- exclusão: bloqueia se houver filhos ou sensores;
- hierarquia: carrega os locais, cria lookup por ID e monta `Children` em memória;
- paginação: quando não há `ParentLocationId`, retorna somente locais raiz.

### 6.8 Cadastro de sensor

1. `POST /api/v1/Sensors` envia `CreateSensorCommand`.
2. O validator e o handler verificam estabelecimento, local e unicidade de `DeviceId`.
3. O handler confirma que o local pertence ao estabelecimento.
4. Gera uma API key aleatória de 32 bytes.
5. Cria o sensor ativo e define `LastCommunication`.
6. Retorna o ID.

### 6.9 Envio de leitura individual

1. `POST /api/v1/SensorData/readings` recebe `SubmitSensorReadingCommand`.
2. O endpoint é `[AllowAnonymous]` para JWT.
3. O handler localiza o sensor por `DeviceId`.
4. Se o sensor possui API key, compara com a chave recebida.
5. Sensores inativos têm a leitura ignorada silenciosamente.
6. Cria `SensorReading`.
7. Se o valor estiver fora do limite, cria `SensorAlert`.
8. Persiste leitura e possível alerta.

### 6.10 Envio de leituras em lote

O fluxo é semelhante ao individual, mas recebe uma lista e usa `AddRangeAsync` para leituras e alertas.

### 6.11 Atualização de status

1. `POST /api/v1/SensorData/status` valida `DeviceId` e API key.
2. Atualiza `IsActive`, `LastCommunication` e, se informado, `FirmwareVersion`.
3. Cria `SensorStatusUpdate`.
4. Gera alerta de bateria baixa quando o valor é menor que 15%.

### 6.12 Retenção de dados

`DataRetentionService` é registrado como hosted service:

1. executa continuamente enquanto a aplicação estiver ativa;
2. processa todos os estabelecimentos;
3. usa 30 dias quando não há plano;
4. usa `SubscriptionPlan.DataRetentionDays` quando há plano;
5. remove leituras antigas com `ExecuteDeleteAsync`;
6. remove alertas reconhecidos com mais de três meses;
7. aguarda um dia para a próxima execução.

### 6.13 Inicialização da aplicação

Ao iniciar:

1. `Program.cs` chama `Database.Migrate()`;
2. `IntializeDatabase()` executa initializers quando `InitializeDataBase` não está definido ou está `true`;
3. são criados estabelecimento master, perfis, usuário admin, domínios, privilégios e menus.

## 7. API e endpoints

Todas as rotas herdam o prefixo:

```text
/api/v{version}/[controller]
```

A versão disponível é `v1`.

### `UsersController` — `/api/v1/Users`

| Método | Rota | Request | Responsabilidade |
|---|---|---|---|
| POST | `/` | `CreateUserCommand` | Criar usuário |
| POST | `/Token` | `AccessTokenUserCommand` | Autenticar e emitir tokens |
| GET | `/current` | `GetCurrentUserQuery` | Consultar usuário atual e privilégios |
| PUT | `/{id}` | `UpdateUserCommand` | Atualizar usuário |
| DELETE | `/{id}` | `DeleteUserCommand` | Excluir usuário |
| POST | `/refresh-token` | `RefreshTokenCommand` | Renovar tokens |
| POST | `/reset-password` | `ResetPasswordCommand` | Alterar senha autenticada |
| GET | `/confirm-email` | `ConfirmEmailCommand` | Confirmar e-mail |
| POST | `/forgot-password` | `GeneratePasswordResetCommand` | Gerar recuperação |
| POST | `/reset-password-with-token` | `ResetPasswordWithTokenCommand` | Redefinir senha por token |
| GET | `/list` | `GetUserListQuery` | Listar usuários |

### `EstablishmentController` — `/api/v1/Establishment`

| Método | Rota | Request |
|---|---|---|
| POST | `/` | `CreateEstablishmentCommand` |
| PUT | `/{id}` | `UpdateEstablishmentCommand` |
| DELETE | `/{id}` | `DeleteEstablishmentCommand` |
| GET | `/` | `GetAllEstablishmentPaginatedQuery` |
| GET | `/list` | `GetEstablishmentSelectorQuery` |
| GET | `/{id}` | `GetEstablishmentByIdQuery` |

### `ProfilesController` — `/api/v1/Profiles`

| Método | Rota | Request |
|---|---|---|
| POST | `/` | `CreateProfileCommand` |
| PUT | `/{id}` | `UpdateProfileCommand` |
| DELETE | `/{id}` | `DeleteProfileCommand` |
| GET | `/` | `GetAllProfilePaginatedQuery` |
| GET | `/list` | `GetProfileSelectorQuery` |
| GET | `/{id}` | `GetProfileByIdQuery` |

### `LocationsController` — `/api/v1/Locations`

| Método | Rota | Request |
|---|---|---|
| POST | `/` | `CreateLocationCommand` |
| PUT | `/` | `UpdateLocationCommand` |
| DELETE | `/` | `DeleteLocationCommand` no body |
| GET | `/{id}` | `GetLocationQuery` |
| GET | `/` | `GetLocationsQuery` |
| GET | `/list` | `GetLocationListQuery` |
| GET | `/hierarchical` | `GetLocationHierarchyQuery` |

### `SensorsController` — `/api/v1/Sensors`

| Método | Rota | Request |
|---|---|---|
| POST | `/` | `CreateSensorCommand` |
| PUT | `/` | `UpdateSensorCommand` |
| DELETE | `/` | `DeleteSensorCommand` no body |
| GET | `/{id}` | `GetSensorQuery` |
| GET | `/` | `GetSensorsQuery` |
| GET | `/list` | `GetSensorListQuery` |
| GET | `/{id}/readings` | `GetSensorReadingsQuery` |
| GET | `/{id}/alerts` | `GetSensorAlertsQuery` |

### `SensorDataController` — `/api/v1/SensorData`

| Método | Rota | Request | Autenticação funcional |
|---|---|---|---|
| POST | `/readings` | `SubmitSensorReadingCommand` | API key do sensor |
| POST | `/readings/batch` | `SubmitSensorReadingBatchCommand` | API key do sensor |
| POST | `/status` | `UpdateSensorStatusCommand` | API key do sensor |

### Controllers de consulta auxiliar

- `GET /api/v1/Menu/list` → `MenuSelectorQuery`;
- `GET /api/v1/Privilege/list` → `PrivilegeSelectorQuery`;
- `GET /api/v1/Generic/list` → `GenericSelectorQuery`.

## 8. Persistência e banco de dados

### DbContext

`ApplicationDbContext`, em `src/HomeControllerHUB.Infra/DatabaseContext/ApplicationDbContext.cs`, herda de:

```text
IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
```

Ele reúne os DbSets de Identity e do domínio.

### Banco

O provider configurado é PostgreSQL, com connection string chamada `Npgsql`.

O schema/search path configurado nos ambientes é `home-hubdb,public`.

### Configurações EF Core

As configurações estão em:

`src/HomeControllerHUB.Domain/Entities/Configuration`.

Elas definem:

- tamanho e obrigatoriedade de campos;
- índices;
- relacionamentos;
- comportamento de exclusão;
- conversão de `Dictionary<string,string>` para JSONB;
- sequences de código;
- unicidade de `Sensor.DeviceId` e `SubscriptionPlan.NormalizedName`.

### Migrations

As migrations ficam no projeto API:

- `20250323175156_V-InitialTables`;
- `20250405155947_V-AppUserFields`;
- `20250405173749_AddIoTEntities`;
- `20250405175429_AddIoTDomainsAndPrivileges`;
- `20250405184433_AddSensorStatusUpdateAndMetadata`.

A migration `AddIoTDomainsAndPrivileges` não altera o schema; os domínios e privilégios são criados por initializers.

### Repositórios

Não existem repositórios customizados. `DbSet<T>` é usado diretamente.

### Transações

Não há gerenciamento explícito de transações no código analisado.

### Auditoria e normalização

- `BaseEntityInterceptor` mantém `Created` e `Modified` em UTC.
- `NormalizedInterceptor` preenche campos normalizados antes do save.

Observação: o interceptor de normalização percorre entidades que implementam `IBaseEntity`. `ApplicationDomain` e `ApplicationMenu` possuem atributos `[Normalized]`, mas não implementam `IBaseEntity`; os initializers preenchem esses campos manualmente.

### Exclusão

O padrão não é uniforme:

- estabelecimento: desabilitação lógica;
- perfil, usuário, local e sensor: exclusão física;
- sensor e local possuem bloqueios antes da exclusão;
- relacionamentos de leituras e alertas possuem cascade no banco.

## 9. Autenticação e autorização

### Identity

Identity é registrado com:

- `ApplicationUser`;
- `IdentityRole<Guid>`;
- stores do EF Core;
- token providers padrão.

Roles fazem parte do schema de Identity, mas o controle de negócio observado usa perfis e privilégios próprios.

### JWT

O JWT é assinado com HMAC SHA-256.

Claims incluídas:

- `ClaimTypes.NameIdentifier`;
- `ClaimTypes.Name`;
- `EstablishmentId`.

Não há privilégios dentro do JWT. A autorização consulta o banco.

### Refresh token

O refresh token:

- é criado por `AccessTokenEntry`;
- é armazenado em `AspNetUserTokens`;
- é comparado pelo handler de refresh;
- é substituído após uso bem-sucedido.

### Autorização customizada

Commands e queries usam `HomeControllerHUB.Shared.Common.AuthorizeAttribute`, não o atributo ASP.NET Core de mesmo propósito.

`AuthorizationBehaviour`:

1. obtém os atributos do request;
2. recupera usuário e domínio;
3. procura um privilégio do perfil com domínio e ação correspondentes;
4. também aceita `platform-all`.

### Autorização HTTP

Não foi encontrado `[Microsoft.AspNetCore.Authorization.Authorize]` nos controllers. Portanto, o bloqueio principal das operações protegidas ocorre no pipeline MediatR, e não no middleware MVC.

Requests com `[Authorize]` customizado sem domínio retornam diretamente ao handler. Esse é o caso de `MenuSelectorQuery` e `GenericSelectorQuery`.

### Policies

Não foram encontradas policies ASP.NET Core.

### API key de sensores

Os endpoints de `SensorDataController` são anônimos para JWT e validam a API key no handler.

A API key é armazenada em texto na entidade `Sensor`. Se a API key armazenada for nula ou vazia, o handler não exige uma chave.

## 10. Configurações da aplicação

### `appsettings.json`

Contém:

- logging;
- configurações de Mailgun;
- URL do frontend;
- remetente;
- `AllowedHosts`.

### `appsettings.Development.json`

Contém:

- connection string local PostgreSQL;
- JWT;
- URLs de host e identity;
- inicialização de banco;
- Mailgun;
- Kestrel HTTP em `6001`;
- endpoint HTTP/2 em `7001`.

### `appsettings.Testing.json`

Contém:

- conexão com o container PostgreSQL;
- JWT;
- inicialização de banco;
- Kestrel HTTP em `80`;
- HTTP/2 em `8080`.

### Classes de settings

Em `src/HomeControllerHUB.Infra/Settings`:

- `ApplicationSettings`;
- `JwtSettings`;
- `HostSettings`;
- `SwaggerSettings`;
- `EmailSettings`.

### Swagger

`SwaggerConfigurationExtensions` configura:

- documento v1;
- XML comments;
- OAuth2 password flow apontando para `/api/v1/Users/Token`;
- filtros customizados;
- schema IDs com namespace;
- UI em `/swagger`.

Swagger é ativado somente em Development ou Testing.

### Docker

`Dockerfile`:

- usa runtime e SDK .NET 9;
- restaura projetos;
- publica a API;
- expõe a porta 6001.

`docker-compose.yml`:

- ativa somente PostgreSQL;
- expõe a porta local 15432;
- usa volume persistente;
- exige uma rede externa chamada `home-controller-hub-network`;
- contém definições comentadas de Redis e API.

### Inicialização e migrations

`Program.cs` executa migrations automaticamente em qualquer ambiente e depois executa os data initializers conforme `InitializeDataBase`.

### Variáveis de ambiente

Não há arquivo `.env` no repositório. O ambiente é selecionado por `ASPNETCORE_ENVIRONMENT`, como em `launchSettings.json` e no compose comentado.

## 11. Testes

### Projetos

- `HomeControllerHUB.Application.Tests`;
- `HomeControllerHUB.Api.IntegrationTests`;
- `HomeControllerHUB.Domain.Tests`.

Foram encontrados 37 arquivos C# de teste e 92 métodos marcados com `[Fact]` ou `[Theory]`. Desses, 90 estão no projeto Application Tests; os outros dois são placeholders vazios.

### Estratégia do projeto Application Tests

Apesar do nome “Application.Tests”, os testes não são exclusivamente unitários:

- `TestConfigs` inicia PostgreSQL real com Testcontainers;
- aplica as migrations reais;
- usa `ApplicationDbContext`;
- instancia handlers diretamente;
- mocks são usados para serviços como `ISharedResource`, `ICurrentUserService`, `IMapper` ou token service.

O resultado é uma combinação de teste de unidade de handler com teste de integração de persistência.

### Áreas cobertas

- autenticação básica;
- estabelecimentos;
- perfis;
- locais;
- sensores;
- envio de leituras;
- alertas;
- atualização de status;
- menus;
- privilégios;
- generics;
- paginação e filtros;
- validators.

Exemplos:

- `AccessTokenUserCommandTest`;
- `CreateEstablishmentCommandTest`;
- `GetAllProfilePaginatedQueryTest`;
- `GetLocationHierarchyQueryTest`;
- `SubmitSensorReadingCommandTest`;
- `UpdateSensorStatusCommandTest`;
- `GetSensorReadingsQueryTest`.

### Mocks

Moq é utilizado para:

- recursos de localização;
- usuário atual;
- AutoMapper em alguns handlers;
- JWT;
- MediatR;
- `ApiUserManager`.

### Lacunas

Os projetos de Domain e integração HTTP ainda não possuem testes reais.

Também não foram encontrados testes específicos para:

- refresh token;
- criação, atualização e exclusão completa de usuário;
- confirmação de e-mail;
- recuperação de senha;
- `AuthorizationBehaviour`;
- `DataRetentionService`;
- `ErrorHandlingMiddleware`;
- controllers e model binding;
- Swagger;
- initializers;
- isolamento entre estabelecimentos;
- execução ponta a ponta da API.

### Execução

Os testes exigem Docker disponível para o Testcontainers. Eles não foram executados durante esta análise, pois a tarefa solicitou somente leitura e criação do relatório.

## 12. Padrões de implementação observados

### Criar um novo Command

1. Criar a pasta:
   `src/HomeControllerHUB.Application/<Feature>/Commands/<Ação>`.
2. Criar um request `IRequest<TResponse>` ou `IRequest`.
3. Aplicar o atributo customizado `[Authorize]` quando necessário.
4. Criar o handler no mesmo arquivo.
5. Injetar `ApplicationDbContext` e serviços necessários.
6. Persistir com `SaveChangesAsync`.
7. Para criação, retornar ID ou `BaseEntityResponse`.
8. Criar um `AbstractValidator<TCommand>`.

### Criar uma nova Query

1. Criar a pasta:
   `src/HomeControllerHUB.Application/<Feature>/Queries/<Ação>`.
2. Definir filtros e retorno.
3. Usar `AsNoTracking` ou `ProjectTo` quando apropriado.
4. Aplicar paginação com `PaginatedList.CreateAsync` ou `PaginateAsync`.
5. Aplicar autorização customizada.
6. Criar validator se houver parâmetros com restrições.

### Criar um DTO

1. Colocar o DTO na feature correspondente.
2. Implementar `IMapFrom<TEntity>`.
3. Implementar `IPaginatedDto` se usado pela paginação genérica.
4. Sobrescrever `Mapping` quando o nome da propriedade ou a projeção não for direta.

### Criar um endpoint

1. Adicionar método ao controller correspondente.
2. Usar atributos HTTP.
3. Declarar tipos de resposta.
4. Encaminhar o command/query ao `Mediator`.
5. Evitar regras de negócio no controller.
6. Validar consistência entre ID de rota e ID do body, quando ambos existirem.

### Criar uma entidade

1. Criar em `src/HomeControllerHUB.Domain/Entities`.
2. Normalmente herdar de `Base`.
3. Adicionar atributos `[Normalized]` quando necessário.
4. Criar configuração em `Entities/Configuration`.
5. Adicionar `DbSet` e `ApplyConfiguration` no contexto.
6. Criar migration no projeto API.

### Criar um serviço

1. Definir contrato em `Domain/Interfaces` quando aplicável.
2. Implementar em `Infra/Services`.
3. Registrar em `Infra/ConfigureServices.cs`.

### Criar testes de handler

1. Herdar de `TestConfigs`.
2. Criar dados reais no PostgreSQL do Testcontainers.
3. Instanciar o handler diretamente.
4. Mockar somente serviços externos ou contexto HTTP.
5. Verificar retorno e estado persistido.
6. Para validators, usar `TestValidate` ou `TestValidateAsync`.

## 13. Pontos fortes do projeto

- **Separação explícita por projetos e features**: facilita localizar controllers, casos de uso, entidades e infraestrutura.
- **Uso consistente de MediatR nos controllers**: reduz lógica na camada HTTP.
- **Commands e queries claramente separados**: os casos de uso são fáceis de seguir.
- **Testes com PostgreSQL real**: reduz diferenças entre comportamento de teste e produção.
- **Boa cobertura do núcleo IoT**: cadastro, leitura individual, lote, status, alertas e consultas possuem testes.
- **Mapeamentos EF Core explícitos**: índices, relacionamentos e JSONB estão documentados no código.
- **Normalização automática**: melhora buscas textuais e consistência.
- **Localização centralizada**: `ISharedResource` evita mensagens de negócio espalhadas.
- **Versionamento e Swagger estruturados**: existe suporte explícito para evolução da API.
- **Retenção de dados implementada**: não é apenas um campo de plano; há serviço em background.
- **Menus derivados das permissões**: o backend produz navegação compatível com os domínios acessíveis.
- **API key por sensor**: há uma credencial própria para ingestão IoT, separada do JWT de usuário.

## 14. Pontos de atenção / melhorias possíveis

### 14.1 Dependências entre camadas

`Application` referencia `Infra` e usa `ApplicationDbContext` diretamente. Se o objetivo for Clean Architecture estrita, seria necessário introduzir contratos internos e inverter essa dependência.

### 14.2 Autorização dos sensores

Há uma inconsistência concreta:

- os privilégios de sensores são criados no domínio `Sensor`;
- várias queries e commands de sensor exigem o domínio `IoT`;
- não são criados privilégios para o domínio `IoT`;
- somente `CreateSensorCommand` usa `DomainNames.Sensor`.

Com a implementação atual, operações que exigem `DomainNames.IoT` tendem a ser liberadas apenas para `platform-all`.

### 14.3 Privilégios com ação `All`

`AuthorizationBehaviour` compara a ação do privilégio diretamente com a ação solicitada. Um privilégio como `sensor-all`, cuja ação é `All`, não satisfaz automaticamente uma solicitação `Read`, `Create`, `Update` ou `Delete`. Apenas `platform-all` possui tratamento especial.

### 14.4 Registro dos validators

`Program.cs` registra validators usando `RegisterValidatorsFromAssemblyContaining<Program>()`, mas os validators estão no projeto Application. Não foi encontrado `AddValidatorsFromAssembly` para o assembly de Application. Como os testes instanciam os validators manualmente, eles não comprovam que a validação automática HTTP esteja ativa para todos os requests.

### 14.5 Isolamento multiestabelecimento

Alguns fluxos usam o estabelecimento da claim, como criação e paginação de perfis. Outros aceitam qualquer `EstablishmentId` ou não filtram por tenant:

- consultas de sensores;
- consultas de locais;
- lista de usuários;
- lista de estabelecimentos;
- selectors.

Se o sistema for realmente multi-tenant, o escopo de dados deve ser validado de forma uniforme.

### 14.6 Configurações sensíveis

Há segredos, senhas e credenciais padrão em arquivos versionados:

- JWT secret e encrypt key;
- senha do PostgreSQL;
- senha do usuário admin inicial.

Esses valores deveriam ser fornecidos por secret manager ou variáveis de ambiente fora do repositório.

### 14.7 Logging e CORS

- `EnableSensitiveDataLogging()` é ativado sem condicionar ao ambiente.
- CORS permite qualquer origem, método e header.
- eventos JWT escrevem detalhes no console.

Essas opções são úteis localmente, mas devem ser restritas em produção.

### 14.8 Expiração de tokens

- `JwtTokenService` usa expiração fixa de uma hora e não usa `JwtSettings.ExpirationMinutes`.
- os tokens customizados de confirmação de e-mail e recuperação de senha não possuem timestamp ou validação de expiração;
- a documentação afirma expiração de 24 horas, mas isso não é aplicado no código.

### 14.9 Status e bateria do sensor

`UpdateSensorStatusCommandHandler` cria um `SensorStatusUpdate` com bateria, mas não atualiza `Sensor.BatteryLevel`. Como `SensorDto.Status` usa `Sensor.BatteryLevel`, o resumo do sensor pode não refletir a última bateria recebida.

### 14.10 Planos parcialmente aplicados

Os limites comerciais do plano não são aplicados ao criar sensores ou alertas. Apenas retenção de leituras utiliza o plano.

### 14.11 Contrato de erro

Os controllers declaram `ProblemDetails`, e a documentação menciona RFC 7807. Entretanto, `ErrorHandlingMiddleware` retorna:

```json
{ "Error": "...", "Description": "..." }
```

Para exceções não `AppError`, retorna apenas:

```json
{ "error": "..." }
```

O contrato real não é uniforme com a documentação.

### 14.12 Inicialização automática

Migrations e seed executam no startup. Isso simplifica desenvolvimento, mas pode:

- aumentar tempo de inicialização;
- causar concorrência entre instâncias;
- criar usuário e senha padrão;
- misturar deploy da aplicação com alteração de schema.

### 14.13 Transações de fluxos compostos

Criação de usuário, associações e envio de e-mail não formam uma transação de negócio única. Uma falha no e-mail pode ocorrer após persistência parcial. Estratégias como transação explícita e outbox podem ser consideradas.

### 14.14 Documentação desatualizada

Exemplos concretos:

- `Frontend_API_Documentation.md` usa formatos de token diferentes do modelo real;
- algumas rotas de establishment/profile são descritas sem `/{id}`;
- o documento menciona propriedades que não existem no DTO atual;
- `HomeControllerHUB.Api.http` ainda referencia `weatherforecast` e porta 5195;
- `ProjDoc.md` afirma dependências de Clean Architecture que não correspondem aos `.csproj`.

### 14.15 Cobertura de testes

Os casos de uso de Application têm boa base, mas faltam testes reais de API, domínio, segurança, retenção, e-mail e multi-tenancy.

### 14.16 Hierarquia de locais

A atualização impede um local de ser pai de si próprio, mas não verifica ciclos indiretos, como A → B → C → A.

### 14.17 API key opcional

O handler só exige API key quando o sensor possui uma chave armazenada. Como `UpdateSensorCommand` permite definir `ApiKey` como nula, é possível tornar a ingestão daquele sensor sem chave.

## 15. Guia rápido para novos desenvolvedores

### Ordem recomendada de leitura

1. `src/Readme.md` para convenções declaradas.
2. `src/HomeControllerHUB.Api/Program.cs` para entender o startup.
3. `src/HomeControllerHUB.Api/ConfigureServices.cs` para banco, Identity e JWT.
4. `src/HomeControllerHUB.Infra/DatabaseContext/ApplicationDbContext.cs`.
5. `src/HomeControllerHUB.Domain/Entities`.
6. Um fluxo completo em Application.
7. O controller correspondente.
8. Os testes da mesma feature.

### Fluxo de referência administrativo

Para entender um CRUD tradicional, seguir:

```text
ProfilesController
  -> CreateProfileCommand
  -> CreateProfileCommandHandler
  -> Profile / ProfilePrivilege
  -> ApplicationDbContext
  -> CreateProfileCommandTest
```

### Fluxo de referência IoT

Para entender ingestão:

```text
SensorDataController
  -> SubmitSensorReadingCommand
  -> SubmitSensorReadingCommandHandler
  -> SensorReading / SensorAlert
  -> SubmitSensorReadingCommandTest
```

### Onde procurar

- endpoints: `src/HomeControllerHUB.Api/Controllers`;
- regras de caso de uso: `src/HomeControllerHUB.Application`;
- entidades e enums: `src/HomeControllerHUB.Domain/Entities`;
- schema e relacionamentos: `Entities/Configuration` e migrations;
- autenticação: `Users/Commands`, `JwtTokenService` e configuração JWT;
- autorização: `AuthorizeAttribute`, `AuthorizationBehaviour`, perfis e privilégios;
- integrações externas: `src/HomeControllerHUB.Infra/Services`;
- traduções: `src/HomeControllerHUB.Globalization/Resources`;
- testes: `tests/HomeControllerHUB.Application.Tests`.

### Como entender uma feature

1. Localizar o controller e listar seus requests.
2. Abrir o command/query.
3. Ler primeiro o request e o atributo de autorização.
4. Ler o handler e identificar DbSets afetados.
5. Ler o validator.
6. Ler os DTOs e mapeamentos.
7. Conferir configuração EF da entidade.
8. Conferir testes.

### Pré-requisitos operacionais

- .NET 9 SDK;
- Docker para o PostgreSQL e os testes;
- rede Docker externa `home-controller-hub-network`, se o compose for usado como está;
- PostgreSQL acessível na porta configurada;
- Mailgun configurado para envio real fora de DEBUG.

## 16. Resumo final

O HomeControllerHUB é uma API .NET 9 para administração de estabelecimentos e usuários e para coleta e monitoramento de dados IoT.

A organização usa projetos em camadas, features, CQRS e MediatR. Ela é inspirada em DDD e Clean Architecture, mas mantém acoplamento direto entre Application e Infra e usa EF Core diretamente nos handlers.

As tecnologias principais são ASP.NET Core, PostgreSQL, Entity Framework Core, Identity, JWT, MediatR, AutoMapper, FluentValidation, Swagger, Docker, xUnit e Testcontainers.

O projeto apresenta boa separação visual, fluxos IoT relevantes já implementados e uma suíte consistente de testes de Application com banco real. Para contribuir com segurança, um novo desenvolvedor precisa compreender principalmente:

- o pipeline controller → MediatR → handler → DbContext;
- o modelo de estabelecimento como escopo de dados;
- o modelo de perfis, privilégios, domínios e ações;
- os fluxos de ingestão por API key;
- os mapeamentos EF Core e interceptors;
- as diferenças entre a intenção descrita nos `.md` e o comportamento atual do código.

As prioridades técnicas mais claras são alinhar a autorização de sensores, garantir o registro efetivo dos validators, reforçar isolamento multi-tenant, remover segredos do repositório e criar testes reais de integração HTTP e segurança.
