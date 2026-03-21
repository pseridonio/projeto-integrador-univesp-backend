# Architecture

Este documento descreve as decisões arquiteturais do projeto **CafeSystem**.

O objetivo desta arquitetura é equilibrar:

* boas práticas modernas
* simplicidade de implementação
* valor educacional para desenvolvedores iniciantes

---

# Visão Geral

O sistema utiliza uma arquitetura baseada em **Monólito Modular**, inspirada em **Clean Architecture** e **Domain Driven Design (DDD)**.

Além disso, os casos de uso são organizados utilizando **Vertical Slice Architecture** dentro da camada de aplicação.

Essa combinação permite:

* separação clara de responsabilidades
* código fácil de navegar
* evolução futura para microserviços
* facilidade de testes

---

# Diagrama de Camadas

```text
Client
   ↓
API
   ↓
Application
   ↓
Domain
   ↑
Infrastructure
   ↓
Database
```

Cada camada possui responsabilidades bem definidas.

---

# Domain

A camada **Domain** representa o coração do sistema.

Ela contém:

* entidades
* regras de negócio
* value objects
* enums

A camada Domain **não depende de nenhuma outra camada**.

Ela representa apenas **o modelo de negócio do sistema**.

Exemplo de possíveis entidades:

```
User
Product
Category
Order
OrderItem
Payment
```

---

# Application

A camada **Application** implementa os **casos de uso do sistema**.

Ela contém:

* Commands
* Queries
* Handlers
* DTOs
* Interfaces de repositório

Essa camada **coordena as regras do domínio**, mas não implementa detalhes técnicos como banco de dados.

---

# Vertical Slice Architecture

Dentro da camada Application utilizamos **Vertical Slice Architecture**.

Isso significa que o código é organizado por **funcionalidade**, e não por tipo técnico.

Exemplo:

```
Application
 ├── Users
 │   ├── Commands
 │   └── Queries
 │
 ├── Catalog
 │   ├── Commands
 │   └── Queries
 │
 └── Orders
     ├── Commands
     └── Queries
```

Cada funcionalidade possui seus próprios:

* Request
* Handler
* Response

Essa organização facilita:

* entendimento do fluxo do sistema
* manutenção
* leitura do código

---

# CQRS Simplificado

O projeto utiliza um modelo inspirado em **CQRS (Command Query Responsibility Segregation)**.

Isso significa apenas separar operações de:

### Escrita

Chamadas **Commands**, que modificam o estado do sistema.

Exemplos:

```
CreateUser
UpdateProduct
OpenOrder
AddItemToOrder
CancelOrder
```

### Leitura

Chamadas **Queries**, que apenas consultam dados.

Exemplos:

```
GetUser
SearchProducts
ListCategories
GetOrder
ListOpenOrders
```

Importante:

O sistema **não utiliza CQRS completo**, apenas a separação conceitual.

Não existem:

* bancos separados
* event sourcing
* message brokers

---

# API

A camada **API** expõe o sistema através de **endpoints HTTP**.

Ela contém:

* Controllers
* configuração da aplicação
* autenticação
* middlewares

A API **não contém regra de negócio**.

Ela apenas:

1. recebe requisições
2. valida dados básicos
3. encaminha para a camada Application

---

# Infrastructure

A camada **Infrastructure** contém implementações técnicas necessárias para o sistema funcionar.

Exemplos:

* repositórios
* acesso a banco de dados
* ORM
* integrações externas

Também é responsável por:

* DbContext
* configurações do Entity Framework

---

# Banco de Dados

O sistema utiliza:

**PostgreSQL**

A comunicação com o banco será feita através de:

**Entity Framework Core**

Inicialmente o sistema utilizará **um único banco de dados**.

---

# Autenticação

O sistema possui autenticação baseada em **token**.

Fluxo de autenticação:

1. Usuário realiza login
2. O sistema gera um token
3. O token é enviado nas requisições seguintes

Exemplo de header:

```
Authorization: Bearer <token>
```

A única operação pública do sistema é:

```
Login
```

Todas as demais exigem autenticação.

---

# Testes

O sistema possui múltiplos níveis de testes.

### Testes Unitários

Cada camada possui seu próprio projeto de testes.

```
CafeSystem.API.UnitTests
CafeSystem.Application.UnitTests
CafeSystem.Domain.UnitTests
CafeSystem.Infrastructure.UnitTests
```

### Testes End-to-End

Simulam o funcionamento completo do sistema através da API.

```
CafeSystem.API.TestsEnd2End
```

---

# Evolução Arquitetural

O sistema foi projetado para permitir evolução futura.

Possíveis evoluções:

* controle de acesso por perfil
* múltiplos métodos de pagamento
* relatórios
* dashboards administrativos
* divisão em microserviços

---

# Filosofia de Desenvolvimento

O desenvolvimento deve seguir a seguinte ordem:

1. Modelagem do domínio
2. Definição dos casos de uso
3. Implementação da camada Application
4. Implementação da API
5. Implementação da infraestrutura
6. Testes

Essa abordagem garante que o sistema seja construído **a partir da regra de negócio**, e não da tecnologia.
