# TESTING GUIDE

Este documento define os padrões para implementação de testes no projeto **CafeSystem**.

O objetivo é garantir:

* qualidade de código
* consistência entre os membros do time
* facilidade de manutenção
* aprendizado de boas práticas

---

# Tipos de Teste

O projeto utiliza dois tipos principais de testes:

## Testes Unitários

Testam **unidades isoladas** do sistema:

* métodos
* classes
* regras de negócio

Sem acesso a:

* banco de dados
* API
* infraestrutura externa

---

## Testes de Integração

Testam a **integração real entre componentes**, incluindo:

* API
* banco de dados
* infraestrutura

Utilizam banco real via container.

---

# Bibliotecas Recomendadas

## Testes Unitários

* xUnit
* FluentAssertions
* Moq

---

## Testes de Integração

* xUnit
* Testcontainers for .NET
* WebApplicationFactory

---

# Testes Unitários

## Estrutura

Cada teste deve seguir o padrão:

```text
Arrange → Act → Assert
```

---

## Exemplo correto

```csharp
[Fact]
public void Should_Calculate_Order_Total_Correctly()
{
    // Arrange
    var order = new Order();

    var product = new Product("Café", 10);

    // Act
    order.AddItem(product, 2);

    // Assert
    order.Total.Should().Be(20);
}
```

---

## Exemplo incorreto (EVITAR)

```csharp
[Fact]
public void Test1()
{
    var order = new Order();
    order.AddItem(new Product("Café", 10), 2);

    Assert.Equal(20, order.Total);
}
```

Problemas:

* nome ruim
* sem separação de etapas
* difícil de entender

---

# Nomeação de Testes

Padrão recomendado:

```text
Should_<ExpectedBehavior>_When_<Condition>
```

Exemplo:

```text
Should_Add_Item_To_Order_When_Order_Is_Open
Should_Throw_Exception_When_Adding_Item_To_Closed_Order
```

---

# Uso de Mocks

Use mocks apenas quando necessário.

Exemplo:

```csharp
var repositoryMock = new Mock<IProductRepository>();

repositoryMock
    .Setup(x => x.GetById(It.IsAny<Guid>()))
    .Returns(product);
```

---

## Evitar

❌ Mockar tudo sem necessidade  
❌ Mockar entidades do domínio  
❌ Testar implementação de mocks  

---

# O que testar

Testar:

✔ regras de negócio  
✔ validações  
✔ cálculos  
✔ comportamento esperado  

---

## Não testar

❌ Entity Framework  
❌ getters/setters simples  
❌ código trivial  

---

# Testes de Integração

## Objetivo

Garantir que o sistema funciona **como um todo**.

---

## Características

* utilizam banco real (PostgreSQL)
* sobem a API real
* fazem chamadas HTTP reais

---

## Estrutura

```text
Test
 ↓
API (WebApplicationFactory)
 ↓
Application
 ↓
Infrastructure
 ↓
Database (Testcontainer)
```

---

# Exemplo de teste de integração

```csharp
[Fact]
public async Task Should_Create_Order_Successfully()
{
    // Arrange
    OrderRequest request = new()
    {
        userId = Guid.NewGuid()
    };

    // Act
    OrderResponse response = await _client.PostAsJsonAsync("/orders", request);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Created);
}
```

---

# Boas práticas

## Testes devem ser independentes

Cada teste deve poder rodar isoladamente.

---

## Evitar dependência entre testes

❌ Teste A depende do teste B  
✔ Cada teste prepara seus próprios dados  

---

## Banco de dados

Utilizar:

* Testcontainers
* banco PostgreSQL real

---

# O que NÃO fazer (EVITAR)

## 1. Testes frágeis

```csharp
response.Content.Should().Be("ok");
```

❌ Depende de string exata
✔ Prefira validar estrutura ou comportamento

---

## 2. Testar implementação ao invés de comportamento

```csharp
repositoryMock.Verify(x => x.Save(), Times.Once);
```

❌ Foco em implementação  
✔ Testar resultado final

---

## 3. Testes gigantes

❌ Muitos asserts  
❌ Muitas responsabilidades  

✔ Um comportamento por teste

---

## 4. Não usar Arrange/Act/Assert

Dificulta leitura e manutenção.

---

# Organização dos Testes

Estrutura recomendada:

```text
tests
 ├── Domain.UnitTests
 ├── Application.UnitTests
 ├── Infrastructure.UnitTests
 └── API.IntegrationTests
```

---

# Dicas importantes

* testes são parte do código
* devem ser legíveis
* devem ser simples
* devem explicar o comportamento do sistema

---

# Princípio mais importante

> Um bom teste deve ser fácil de entender por alguém que não escreveu o código.

---

# Resumo

✔ Use testes unitários para lógica  
✔ Use testes de integração para fluxos completos  
✔ Mantenha testes simples  
✔ Nomeie bem os testes  
✔ Evite complexidade desnecessária  

---
