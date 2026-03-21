# Instruções para IA — Módulo de Autenticação (Auth)

Este documento contém instruções objetivas e de boas práticas para a criação dos componentes do módulo de autenticação do projeto `CafeSystem`.

Observações gerais:
- O código produzido DEVE ser escrito em inglês (nomes de classes, métodos, variáveis, arquivos, rotas, mensagens de erro técnicas etc.).
- Documentação interna, comentários e textos explicativos devem ser escritos em português (pt-br) com acentuação correta.
- Siga a arquitetura do projeto (Monólito Modular, Clean Architecture + Vertical Slice) e concentre a lógica de casos de uso na camada `Application`.

Escopo mínimo que a IA deve implementar (apenas instruções — não realizar alterações agora):

1) Contract/DTOs (Application / Contracts)
- `LoginRequest` (properties: `Email`, `Password`).
- `LoginResponse` (properties: `AccessToken`, `RefreshToken` (optional), `ExpiresIn`, `UserId`, `UserName`, `Roles`).
- `RegisterRequest` (optional): `Email`, `Password`, `FullName`.
- `RefreshTokenRequest`: `RefreshToken`.

Comentários: as classes DTO devem ser simples POCOs em inglês. Documentar em pt-br o propósito de cada campo.

2) Commands / Handlers (Application / Identity)
- `LoginCommand` + `LoginHandler` — valida credenciais e retorna `LoginResponse`.
- `RegisterCommand` + `RegisterHandler` — cria usuário com senha hasheada.
- `RefreshTokenCommand` + `RefreshTokenHandler` (se usar refresh tokens).

Regras:
- Handlers implementam a lógica de caso de uso e dependem apenas de interfaces definidas na camada Application (ex.: `IUserRepository`, `ITokenService`).
- Validar entrada com FluentValidation e retornar erros claros.

3) Domain (Domain)
- Entidade `User` com propriedades: `Id (Guid)`, `Email`, `PasswordHash`, `FullName`, `IsActive`, `Roles`, `CreatedAt`, `UpdatedAt`.
- Método(s) de domínio mínimos: verificação de estado, marcação para exclusão lógica.

Comentários: manter regras de negócio aqui; o hash de senha e JWT ficam na infraestrutura/serviços.

4) Interfaces (Application Interfaces)
- `IUserRepository` — métodos: `GetByEmailAsync(string email)`, `GetByIdAsync(Guid id)`, `CreateAsync(User user)`, `UpdateAsync(User user)`.
- `ITokenService` — métodos: `GenerateAccessToken(User user)`, `GenerateRefreshToken()`, `ValidateAccessToken(string token)` (opcional).

5) Infraestrutura (Infrastructure)
- Implementação do `IUserRepository` usando Entity Framework Core (Postgres DbContext). Mapear `User` em uma tabela `users`.
- Implementação do `ITokenService`:
  - Gerar JWT usando configuração central (`Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpiresInMinutes`).
  - Usar HS256 com secret na configuração para desenvolvimento; documentar que em produção deve-se usar chaves assimétricas (RS256) e armazenar segredos em Key Vault/secret manager.
  - Incluir claims essenciais: `sub` (user id), `email`, `name`, `roles`.
  - Definir expiração curta para access token (ex.: 15-60 minutos) e utilizar refresh tokens para UX.
- Armazenamento de Refresh Tokens (se implementado): persistir em tabela `refresh_tokens` com `token`, `user_id`, `expires_at`, `is_revoked`, `created_at`, `replaced_by`.

Segurança de senhas:
- Nunca armazenar senhas em texto.
- Usar `Argon2` para hashear/verificar senhas.
- Usar técnica de Salt e Pepper para segurança adicional.
- Documentar em pt-br o processo de hashing e recomendações.

Acesso ao banco de dados:
- Utilizar entity framework com migrations para criar as tabelas necessárias.
- Banco de dados deve ser configurável via `appsettings.json` e variáveis de ambiente.
  - Documentar necessidade de usar conexões seguras (SSL) em produção.
- PostgreSQL é o banco recomendado, mas a implementação deve ser flexível para outros providers.
 
6) API (API Project)
- Controller: `AuthController` com rotas (todas em inglês):
  - `POST /api/auth/login` -> `Login` (consome `LoginRequest`, retorna `LoginResponse`).
  - `POST /api/auth/register` -> `Register` (opcional).
  - `POST /api/auth/refresh` -> `Refresh` (se usar refresh tokens).
  - `POST /api/auth/logout` -> `Logout` (revoga refresh token).

Regras de controller:
- Controllers apenas validam model binding básico e delegam aos handlers/mediator.
- Não colocar lógica de autenticação no controller.

7) Configuração e Secrets
- Configurar no `appsettings.json` chaves sob `Jwt` (Key, Issuer, Audience, ExpiresInMinutes) — valores de desenvolvimento apenas.
- Documentar variáveis de ambiente a usar em produção: `JWT__KEY`, `JWT__ISSUER`, `JWT__AUDIENCE`, `CONNECTION_STR`.
- Reforçar uso de secret manager / Azure Key Vault em produção.

8) Middleware e Startup
- Configurar autenticação JWT usando `AddAuthentication().AddJwtBearer(...)`.
- Configurar `Authorization` global e aplicar `[AllowAnonymous]` nos endpoints públicos (`/api/auth/login`, `/api/auth/register`).
- Garantir que `ITokenService` e `IUserRepository` sejam registrados no DI.

9) Testes
- Unit tests para:
  - `LoginHandler` (credenciais válidas/invalidas, usuário inativo).
  - `RegisterHandler` (email duplicado).
  - `TokenService` (formato do token, claims, expiração).
- Integration tests end-to-end simulando chamadas a `/api/auth/login` com um banco em memória ou dockerized postgres.

10) Observações de segurança e boas práticas (obrigatórias)
- Rate limit e proteção contra brute-force: documentar necessidade (ex.: bloqueio temporário após N tentativas).
- Logging: não logar senhas nem tokens inteiros. Logar eventos (login success/fail) com user id/email mascarados se necessário.
- CSRF não é aplicável para APIs stateless com Bearer tokens, mas documentar riscos para cookies.
- CORS: configurar apenas origens necessárias.

11) Convenções de código
- Nomes em inglês (ex.: `LoginHandler`, `IUserRepository`, `AuthController`).
- Comentários e documentação em pt-br.
- Usar CancellationToken em métodos assíncronos.
- Métodos assíncronos devem terminar com `Async`.

12) Checklist para revisão de PR (quando a IA gerar código)
- [ ] Código builda sob .NET 10
- [ ] Testes unitários relevantes presentes e passando
- [ ] Segredos removidos do código e configuráveis via environment
- [ ] Senhas devidamente hasheadas
- [ ] JWT configurado e validado
- [ ] Endpoints públicos marcados com `[AllowAnonymous]`
- [ ] Documentação em pt-br atualizada (README ou docs específicos)

Exemplo de estrutura de arquivos (sugestão mínima)

- src/
  - CafeSystem.API/
    - Controllers/
      - `AuthController.cs`
    - appsettings.json (dev)
  - CafeSystem.Application/
    - Identity/
      - Commands/
        - `LoginCommand.cs`
        - `RegisterCommand.cs`
      - Handlers/
        - `LoginHandler.cs`
        - `RegisterHandler.cs`
      - DTOs/
        - `LoginRequest.cs`
        - `LoginResponse.cs`
      - Interfaces/
        - `IUserRepository.cs`
        - `ITokenService.cs`
  - CafeSystem.Domain/
    - Entities/
      - `User.cs`
  - CafeSystem.Infrastructure/
    - Persistence/
      - `AppDbContext.cs`
      - `UserRepository.cs`
    - Security/
      - `JwtTokenService.cs`
    - Migrations/

Notas finais (pt-br):
- Produza código limpo, testável e desacoplado. Separe responsabilidades e use injeção de dependência.
- Comente trechos complexos em pt-br para facilitar revisão por membros do time.
- Se houver dúvida de design (ex.: usar ASP.NET Identity vs implementação custom), implemente uma solução simples e documente as desvantagens/benefícios em pt-br no PR.

---

Fim das instruções. Use este documento como checklist ao implementar o módulo de autenticação.