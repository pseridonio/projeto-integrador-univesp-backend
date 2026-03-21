# Contributing

Obrigado por contribuir com o **CafeSystem**.

Este projeto possui um objetivo educacional e busca seguir boas práticas de engenharia de software.

Este documento descreve como contribuir corretamente para o projeto.

---

# Objetivo Educacional

Este projeto foi estruturado para permitir que todos os participantes aprendam:

* arquitetura de software
* organização de projetos .NET
* modelagem de domínio
* testes automatizados
* trabalho colaborativo em equipe

Por isso, decisões de implementação devem sempre considerar **clareza e aprendizado**, não apenas velocidade.

---

# Fluxo de Desenvolvimento

O desenvolvimento deve seguir a seguinte ordem:

1. Modelagem do domínio
2. Definição dos casos de uso
3. Implementação da camada Application
4. Implementação da API
5. Implementação da infraestrutura
6. Implementação dos testes

Evite começar diretamente pela API.

---

# Padrões de Código

Algumas diretrizes importantes:

### Responsabilidade única

Cada classe deve ter **uma única responsabilidade clara**.

---

### Domínio isolado

A camada **Domain** não pode depender de:

* Application
* Infrastructure
* API

Ela deve ser completamente independente.

---

### API sem regra de negócio

Controllers **não devem conter lógica de negócio**.

Toda lógica deve estar na camada **Application** ou **Domain**.

---

### Commands e Queries

Operações que alteram o sistema devem ser **Commands**.

Operações de leitura devem ser **Queries**.

---

# Organização de Pastas

Cada funcionalidade da aplicação deve ser organizada por **Vertical Slice**.

Exemplo:

```
Application
 └── Users
     ├── Commands
     │   └── CreateUser
     │
     └── Queries
         └── GetUser
```

---

# Pull Requests

Para contribuir com código:

1. Crie uma branch a partir de `main`
2. Implemente a funcionalidade
3. Adicione testes
4. Abra um Pull Request

---

# Boas Práticas

Sempre que possível:

* escreva testes
* mantenha métodos pequenos
* use nomes claros
* evite duplicação de código

---

# Código Limpo

Algumas recomendações importantes:

* prefira nomes descritivos
* evite métodos muito longos
* evite classes com muitas responsabilidades
* mantenha o código simples

---

# Testes

Sempre que uma funcionalidade nova for implementada, deve haver:

* testes unitários
* validação do comportamento esperado

Testes ajudam a garantir que o sistema continue funcionando corretamente.

---

# Comunicação

Caso haja dúvidas sobre arquitetura ou implementação:

* discuta com o grupo
* evite decisões isoladas
* registre decisões importantes

---

# Princípio mais importante

Priorize sempre:

**clareza > complexidade**

O código deve ser fácil de entender para qualquer membro da equipe.
