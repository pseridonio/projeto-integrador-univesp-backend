# CafeSystem

Sistema de gerenciamento de comandas para cafeterias/restaurantes simples.

Este projeto está sendo desenvolvido com foco **didático**, aplicando boas práticas modernas de arquitetura de software, mas mantendo a **complexidade adequada para um time iniciante**.

O objetivo é permitir que todos os membros do grupo aprendam conceitos importantes como:

* Modelagem de domínio
* Clean Architecture
* Organização por casos de uso
* Separação de responsabilidades
* Testes automatizados

---

# Objetivos do Projeto

O sistema permitirá:

* Autenticação de usuários
* Cadastro de usuários
* Cadastro de produtos
* Cadastro de categorias
* Abertura e gerenciamento de comandas
* Inclusão e alteração de itens em comandas
* Pagamento de comandas

Inicialmente **não haverá controle de autorização por perfil**, apenas **autenticação**.

---

# Arquitetura

O sistema utiliza um **Monólito Modular** com inspiração em **Clean Architecture**.

Essa abordagem foi escolhida por:

* Ser mais simples para um time iniciante
* Permitir evolução futura para microserviços
* Separar responsabilidades corretamente
* Facilitar testes

A estrutura geral será:

```
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
Database (PostgreSQL)
```

---

# Estrutura da Solução

A solução será dividida em múltiplos projetos `.csproj`.

```
CafeSystem.slnx
│
├── ./app/
│   ├── CafeSystem.API
│   ├── CafeSystem.Application
│   ├── CafeSystem.Domain
│   ├── CafeSystem.Infrastructure
│
├── ./tests/
│   ├── CafeSystem.API.UnitTests
│   ├── CafeSystem.Application.UnitTests
|   ├── CafeSystem.Domain.UnitTests
│   ├── CafeSystem.Infrastructure.UnitTests
│   │
│   └── CafeSystem.API.TestsEnd2End
```

---

# Descrição dos Projetos

## CafeSystem.API

Responsável por:

* Controllers
* Configuração da aplicação
* Autenticação
* Exposição dos endpoints HTTP

A API **não contém regras de negócio**.

Ela apenas:

* recebe requisições
* envia comandos ou queries para a camada Application

---

## CafeSystem.Application

Contém os **casos de uso do sistema**.

Aqui ficam:

* Commands
* Queries
* Handlers
* DTOs
* Interfaces de repositório

A organização interna segue **Vertical Slice Architecture**, agrupando código por funcionalidade.

Exemplo:

```
Application
│
├── Identity
│   └── Login
│       ├── LoginRequest
│       ├── LoginHandler
│       └── LoginResponse
│
├── Users
│   ├── Commands
│   │   ├── CreateUser
│   │   ├── UpdateUser
│   │   └── DeleteUser
│   │
│   └── Queries
│       ├── GetUser
│       └── ListUsers
│
├── Catalog
│   ├── Commands
│   │   ├── CreateProduct
│   │   ├── UpdateProduct
│   │   └── DeleteProduct
│   │
│   └── Queries
│       ├── GetProduct
│       ├── SearchProducts
│       └── ListCategories
│
└── Orders
    ├── Commands
    │   ├── OpenOrder
    │   ├── AddItemToOrder
    │   ├── RemoveItemFromOrder
    │   ├── CancelOrder
    │   └── CloseOrder
    │
    └── Queries
        ├── GetOrder
        └── ListOpenOrders
```

---

# Padrão de Organização

O projeto utiliza uma abordagem inspirada em **CQRS simplificado**.

Isso significa apenas separar **operações de leitura e escrita**.

### Commands

Alteram o estado do sistema.

Exemplos:

```
CreateUser
UpdateProduct
OpenOrder
AddItemToOrder
CancelOrder
CloseOrder
```

### Queries

Apenas consultam dados.

Exemplos:

```
GetUser
SearchProducts
ListCategories
GetOrder
ListOpenOrders
```

Essa separação melhora:

* organização do código
* clareza dos casos de uso
* manutenção futura

Importante: **não utilizamos CQRS completo**, apenas a separação conceitual.

---

# CafeSystem.Domain

Contém o **modelo de domínio do sistema**.

Aqui ficam:

* Entidades
* Value Objects
* Regras de negócio centrais
* Enums

Exemplo de entidades:

```
User
Product
Category
Order
OrderItem
Payment
```

A camada Domain **não depende de nenhuma outra camada**.

---

# CafeSystem.Infrastructure

Contém as implementações técnicas necessárias para o funcionamento do sistema.

Exemplos:

* Repositórios
* Integração com banco de dados
* ORM
* serviços externos

Também será responsável pelo:

* DbContext
* mapeamentos do banco

O banco de dados utilizado será **PostgreSQL**.

---

# Banco de Dados

O sistema utilizará **um único banco PostgreSQL**.

Inicialmente não será feita separação por múltiplos schemas.

Todas as tabelas ficarão no mesmo schema padrão.

As entidades serão mapeadas via **Entity Framework Core**.

---

# Autenticação

O sistema possui autenticação baseada em **token**.

Operações disponíveis:

### Login

Permite autenticar um usuário e gerar um token.

### Logoff

Invalida o token de autenticação.

### Validação de Token

Verifica se o token ainda é válido.

A única operação que **não exige autenticação** é:

```
Login
```

Todas as outras operações exigem header:

```
Authorization: Bearer <token>
```

---

# Funcionalidades do Sistema

## Autenticação

* Realizar login
* Realizar logoff
* Validar token

---

## Usuários

* Buscar usuário
* Listar usuários
* Criar usuário
* Atualizar usuário
* Excluir usuário (exclusão lógica)

---

## Categorias

* Criar categoria
* Atualizar categoria
* Excluir categoria
* Listar categorias

---

## Produtos

* Buscar produto
* Listar produtos
* Criar produto
* Atualizar produto
* Excluir produto (exclusão lógica)

---

## Comandas (Orders)

* Abrir comanda
* Adicionar item à comanda
* Alterar item da comanda
* Remover item da comanda
* Cancelar comanda
* Consultar comanda
* Listar comandas abertas
* Realizar pagamento da comanda
* Fechar comanda

---

# Testes

O projeto possui múltiplos níveis de testes.

## Testes Unitários

Separados por camada:

```
CafeSystem.API.UnitTests
CafeSystem.Application.UnitTests
CafeSystem.Domain.UnitTests
CafeSystem.Infrastructure.UnitTests
```

---

## Testes End-to-End

```
CafeSystem.API.TestsEnd2End
```

Esses testes simulam o comportamento completo da aplicação através da API.

---

# Objetivo Educacional

Este projeto foi estruturado para permitir que todos os participantes aprendam:

* Clean Architecture
* Domain Driven Design (DDD)
* Vertical Slice Architecture
* CQRS simplificado
* Testes automatizados
* Organização profissional de projetos .NET

---

# Evoluções Futuras

Possíveis melhorias futuras:

* Controle de acesso por perfil (RBAC)
* Pagamentos com múltiplos métodos

---

## Testcontainers (executando testes de integração)

Os testes de integração do projeto utilizam `Testcontainers for .NET` para subir um container PostgreSQL isolado durante a execução dos testes.

Pontos importantes:

- Requisito: Docker instalado e funcionando. Em Windows, recomenda-se usar Docker Desktop com integração WSL2 ou expor o daemon do WSL em `tcp://localhost:2375` quando necessário.
- O projeto de testes `tests/CafeSystem.API.IntegrationTests` já contém um `CustomWebApplicationFactory` que inicia um container PostgreSQL, aplica as migrations e injeta a `ConnectionStrings:DefaultConnection` para a aplicação sob teste.
- Variáveis de ambiente úteis para ajustar o endpoint do Docker:
  - `DOCKER_HOST` — por exemplo `tcp://localhost:2375`.
  - `TESTCONTAINERS_HOST_OVERRIDE` — por exemplo `localhost`.

Como executar os testes de integração localmente:

1. Garanta que o Docker esteja disponível (localmente ou via WSL). Testcontainers precisa se conectar ao daemon Docker.
2. Execute os testes de integração (PowerShell):

```powershell
dotnet test tests\CafeSystem.API.IntegrationTests -v d
```

Observações de segurança:

- Expor o daemon Docker via TCP sem TLS é inseguro; faça apenas em ambientes de desenvolvimento local.
- Em CI prefira runners com Docker local ou configure Testcontainers para usar o endpoint seguro do provedor de CI.

---

## Testcontainers (executando testes de integração)

Os testes de integração do projeto utilizam `Testcontainers for .NET` para subir um container PostgreSQL isolado durante a execução dos testes.

Pontos importantes:

- Requisito: Docker instalado e funcionando. Em Windows, recomenda-se usar Docker Desktop com integração WSL2 ou expor o daemon do WSL em `tcp://localhost:2375` quando necessário.
- O projeto de testes `tests/CafeSystem.API.IntegrationTests` já contém um `CustomWebApplicationFactory` que inicia um container PostgreSQL, aplica as migrations e injeta a `ConnectionStrings:DefaultConnection` para a aplicação sob teste.
- Variáveis de ambiente úteis para ajustar o endpoint do Docker:
  - `DOCKER_HOST` — por exemplo `tcp://localhost:2375`.
  - `TESTCONTAINERS_HOST_OVERRIDE` — por exemplo `localhost`.

Como executar os testes de integração localmente:

1. Garanta que o Docker esteja disponível (localmente ou via WSL). Testcontainers precisa se conectar ao daemon Docker.
2. Execute os testes de integração:

```powershell
dotnet test tests\CafeSystem.API.IntegrationTests -v d
```

Observações de segurança:

- Expor o daemon Docker via TCP sem TLS é inseguro; faça apenas em ambientes de desenvolvimento local.
- Em CI prefira runners com Docker local ou configure Testcontainers para usar o endpoint seguro do provedor de CI.

* Relatórios de vendas
* Dashboard administrativo
* Evolução para microserviços

---

# Tecnologias

* .NET
* C#
* ASP.NET Core
* Entity Framework Core
* PostgreSQL
* xUnit / NUnit (testes)

---

# Filosofia de Desenvolvimento

Antes de implementar APIs ou infraestrutura, o desenvolvimento seguirá a ordem:

1. Modelagem do domínio
2. Definição dos casos de uso
3. Implementação da camada Application
4. Implementação da API
5. Implementação da infraestrutura
6. Testes

Essa abordagem ajuda a evitar código acoplado à tecnologia e mantém o foco na regra de negócio.

---
