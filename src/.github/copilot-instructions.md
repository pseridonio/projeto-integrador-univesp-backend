# Instruções para GitHub Copilot — Desenvolvimento em C#

## Recomendações gerais

- Seguir as decisões arquiteturais definidas em `.docs/Architecture.md` (Monólito Modular, DDD, Vertical Slice, separação de camadas).
- Sempre usar Dependency Injection e depender de abstrações (interfaces) — nunca injetar implementações concretas diretamente.
- Habilitar e respeitar Nullable Reference Types; tratar `null` explicitamente.
- Validar entradas na camada de API; manter regras de negócio na camada Application/Domain.
- Implementações técnicas (ex.: EF Core, repositórios) devem ficar na pasta `Infrastructure`.
- Nomes (identificadores) do código devem ser escritos em inglês. Comentários, documentação XML e descrições de OpenAPI devem ser em pt-BR com acentuação adequada.
- Rotas de API e especificações OpenAPI devem estar em inglês e seguir OpenAPI 3.0.
- Adicionar testes unitários para novas regras de negócio e handlers.

Programando em C#
------------------
- Convenções de nomenclatura:
  - `PascalCase` para classes e métodos.
  - `camelCase` para parâmetros.
  - `_camelCase` para campos privados.
  - Interfaces começam com `I` (ex.: `IUserRepository`).

- Tipos explícitos: usar tipos explícitos sempre que possível. `var` só é permitido quando não existe opção prática (p.ex. tipos anônimos).
  - Exemplo (correto):

```csharp
UserDto userDto = await _userService.GetByIdAsync(id, cancellationToken);
```

  - Exemplo (permitido apenas quando necessário):

```csharp
var projection = new { user.Id, user.Name };
```

- Async/await e CancellationToken: preferir `async/await` para operações I/O e expor `CancellationToken` em handlers e métodos assíncronos.

- SOLID — detalhe e exemplos pequenos:
  - Single Responsibility Principle (SRP): uma classe deve ter apenas uma razão para mudar.

```csharp
// Ruim: mistura lógica de persistência com validação
public class UserManager
{
    public void Save(User user)
    {
        if (string.IsNullOrEmpty(user.Email)) throw new ArgumentException();
        // grava no banco
    }
}

// Bom: responsabilidade separada
public class UserValidator { public void Validate(User user) { /*...*/ } }
public class UserRepository { public void Save(User user) { /*...*/ } }
```

  - Open/Closed Principle (OCP): entidades abertas para extensão, fechadas para modificação.

```csharp
public interface IDiscount { decimal Apply(decimal price); }
public class SeasonalDiscount : IDiscount { public decimal Apply(decimal price) => price * 0.9m; }
```

  - Liskov Substitution Principle (LSP): subclasses devem ser substituíveis pelas suas bases.

```csharp
public abstract class Bird { public abstract void Fly(); }
public class Sparrow : Bird { public override void Fly() { /*...*/ } }
```

  - Interface Segregation Principle (ISP): preferir várias interfaces específicas a uma única interface gordurosa.

```csharp
public interface IReader { string Read(); }
public interface IWriter { void Write(string s); }
```

  - Dependency Inversion Principle (DIP): depender de abstrações, não de concretos.

```csharp
public class OrderService
{
    private readonly IOrderRepository _repo;
    public OrderService(IOrderRepository repo) { _repo = repo; }
}
```

- Clean Code — conceito e exemplos:
  - Clean Code prioriza clareza, nomes significativos, funções pequenas e baixo acoplamento.
  - Evite métodos longos e múltiplas responsabilidades.

Exemplo ruim:

```csharp
public void Process(User user)
{
    if (user == null) throw new Exception();
    // valida
    // atualiza banco
    // envia email
}
```

Exemplo limpo:

```csharp
public void Process(User user)
{
    Validate(user);
    Persist(user);
    Notify(user);
}

private void Validate(User user) { /*...*/ }
private void Persist(User user) { /*...*/ }
private void Notify(User user) { /*...*/ }
```

Realizando code Review
----------------------
- Priorizar legibilidade e manutenção acima de cleverness.
- Verificar aderência às decisões arquiteturais em `.docs/Architecture.md`.
- Verificar se as alterações:
  - respeitam SOLID e Clean Code;
  - usam tipos explícitos quando aplicável;
  - usam `async/await` e propagam `CancellationToken` quando necessário;
  - têm testes unitários adequados para lógica nova ou alterada;
  - mantêm identificadores em inglês;
  - mantêm comentários e documentação em pt-BR com acentuação.

- Em code reviews, quando encontrar `var` onde um tipo explícito é aplicável, sugerir substituição pelo tipo explícito.

Trabalhando com Entity Framework
-------------------------------
- Estrutura: para cada agregação persistida criar uma entidade (ex.: `UserEntity`) e uma configuração separada (ex.: `UserEntityConfiguration`). Colocar implementações em `Infrastructure`.
- Sempre mapear explicitamente nomes de tabela e colunas seguindo convenções do PostgreSQL: lower case e snake_case (ex.: `users`, `first_name`, `created_at`).

Exemplo mínimo:

```csharp
public class UserEntity
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
}

public class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("users");
        builder.HasKey(x => x.Id).HasName("pk_users_id");
        builder.Property(x => x.FirstName).HasColumnName("first_name").IsRequired();
        builder.Property(x => x.LastName).HasColumnName("last_name").IsRequired();
    }
}
```

- Considerar PostgreSQL como banco padrão do projeto: usar tipos e convenções compatíveis (schema, snake_case, constraints nomeadas de forma legível).

Observações finais
------------------
- Sempre priorizar clareza e manutenibilidade.
- Evitar mudanças que contrariem o que está em `.docs/Architecture.md`.
- Este arquivo é um guia curto para autocompletes e sugestões — mantê-lo atualizado.

