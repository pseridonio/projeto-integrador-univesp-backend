# Requisito Funcional: Cadastro e Gestão de Usuários

## 1. Objetivo

Este documento especifica os requisitos funcionais para o cadastro e gestão de usuários do sistema, contemplando:

- inclusão de novo usuário (registro);
- consulta de usuário por identificador;
- atualização de dados do usuário;
- troca de senha pelo usuário autenticado;
- exclusão lógica de usuário.

O documento padroniza entradas, respostas HTTP, validações e regras de negócio.

## 2. Escopo

O escopo inclui as operações de:

- Registro de usuário (`POST /api/users`);
- Troca de senha do próprio usuário autenticado (`PUT /api/users/password`);
- Consulta de usuário por `code` (`GET /api/users/{code}`);
- Atualização de dados do usuário (`PUT /api/users/{code}`);
- Exclusão lógica de usuário (`DELETE /api/users/{code}`).

Ficam fora do escopo:

- controle de permissões por perfil além da necessidade de autenticação básica;
- integração com provedores externos de identidade (SSO);
- listagem paginada de usuários;
- relacionamentos avançados e permissões administrativas.

## 3. Premissas e Restrições

- Todas as operações sensíveis exigem autenticação via header `Authorization: Bearer <token>` quando aplicável.
- O identificador público do usuário será um `GUID` referenciado no campo `code`.
- A exclusão de usuário é lógica: registros permanecem no banco com `deleted_at` preenchido.
- Senhas são armazenadas apenas como `password_hash` e `password_salt`.
- O algoritmo de hash utilizado é Argon2 com uso de `pepper` (segredo da aplicação).

## 4. Modelo de Dados

### 4.1 Entidade de Usuário

| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `code` | GUID | sim | Identificador público do usuário |
| `full_name` | texto | sim | Nome completo do usuário |
| `email` | texto | sim | Endereço de e-mail único |
| `birth_date` | data | não | Data de nascimento (yyyy-MM-dd) |
| `password_hash` | texto | sim | Hash da senha (Argon2) |
| `password_salt` | texto | sim | Salt único por usuário |
| `is_active` | booleano | sim | Indica se o usuário está ativo |
| `created_at` | datetime | sim | Data de criação do registro |
| `updated_at` | datetime | sim | Data da última atualização |
| `deleted_at` | datetime nula | não | Data de exclusão lógica, quando aplicável |

### 4.2 Regras de integridade

- O `email` deve ser único (índice único no banco);
- Usuários com `deleted_at != null` são considerados excluídos e não devem ser retornados em consultas operacionais.

## 5. Endpoints e Contratos

### 5.1 RF-01 — Criar usuário

- Método: `POST`
- Rota: `/api/users`
- Segurança: sem autenticação (registro público) — validações aplicadas.

Entrada (JSON):

```json
{
  "fullName": "Maria da Silva",
  "birthDate": "1990-01-10",
  "email": "maria.silva@email.com",
  "password": "senha123"
}
```

Respostas:

- Sucesso: `201 Created` + body `{ "code": "<user-id-guid>" }`;
- Validação/Negócio: `400 Bad Request` com `{ "message": "mensagem" }`.

Regras aplicáveis:

- Validar campos conforme seção 6;
- Verificar duplicidade de `email` — retornar `400` com `"E-mail já cadastrado"`;
- Gerar `password_salt`, combinar senha com `pepper` e gerar `password_hash` via Argon2;
- Persistir entidade sem armazenar `pepper`.

Critérios de aceite:

- Sem dados obrigatórios: `400`;
- `email` duplicado: `400` com mensagem específica;
- Dados válidos: `201` com `code`.

### 5.2 RF-02 — Trocar senha do usuário autenticado

- Método: `PUT`
- Rota: `/api/users/password`
- Segurança: `Authorization: Bearer <token>` obrigatório.

Entrada (JSON):

```json
{ "password": "novaSenha123" }
```

Respostas:

- Sucesso: `204 No Content`;
- Autenticação: `401 Unauthorized` quando token ausente, inválido ou expirado;
- Validação: `400 Bad Request` com mensagem de validação.

Regras aplicáveis:

- Somente o usuário identificado pelo token pode alterar sua senha;
- Validar nova senha (seção 6);
- Gerar novo `password_salt` e `password_hash` aplicando `pepper` + Argon2;
- Persistir alteração e atualizar `updated_at`.

### 5.3 RF-03 — Consultar usuário por código

- Método: `GET`
- Rota: `/api/users/{code}`
- Segurança: `Authorization: Bearer <token>` obrigatório.

Parâmetros:

- `code` (route) — GUID do usuário.

Respostas:

- Sucesso: `200 OK` com payload:

```json
{
  "code": "<user-id-guid>",
  "fullName": "Maria da Silva",
  "email": "maria@email.com",
  "birthDate": "1990-01-10"
}
```

- Erros:

  - `400 Bad Request` — `code` inválido (`"Código informado é inválido"`);
  - `401 Unauthorized` — token ausente/inválido;
  - `404 Not Found` — usuário inexistente ou excluído (`"Usuário não encontrado."`).

Validações:

- `code` deve ser GUID válido;
- Usuário deve estar `is_active = true` e `deleted_at = null`.

### 5.4 RF-04 — Atualizar usuário

- Método: `PUT`
- Rota: `/api/users/{code}`
- Segurança: `Authorization: Bearer <token>` obrigatório.

Entrada: campos atualizáveis (`fullName`, `birthDate`).

Respostas:

- Sucesso: `204 No Content`;
- `400 Bad Request` para validação;
- `404 Not Found` quando usuário inexistente ou excluído.

Regras:

- Apenas o próprio usuário (identificado pelo token) ou roles administrativos (quando houver) podem atualizar;
- Não é possível alterar `email` via este endpoint (padrão — pode ser alterado por fluxo específico com verificação);
- Aplicar validações conforme seção 6.

### 5.5 RF-05 — Excluir usuário (lógica)

- Método: `DELETE`
- Rota: `/api/users/{code}`
- Segurança: `Authorization: Bearer <token>` obrigatório.

Comportamento:

- Se usuário não existir: `404 Not Found`;
- Se usuário já excluído logicamente: `204 No Content` (idempotência);
- Se existir e ativo: marcar `deleted_at` e retornar `204 No Content`.

Regras:

- Operação protegida — apenas o próprio usuário (ou administrador, futuro escopo) pode excluir;
- Exclusão é lógica: preservar histórico.

## 6. Regras de Validação

### 6.1 Nome (`fullName`)

- Obrigatório;
- Nulo, vazio ou apenas espaços → `"Campo nome é obrigatório"`;
- Menos de 5 caracteres → `"Nome deve conter 5 ou mais caracteres"`;
- Mais de 250 caracteres → `"Nome deve conter no máximo 250 caracteres"`.

### 6.2 E-mail (`email`)

- Obrigatório;
- Nulo, vazio ou apenas espaços → `"Campo e-mail é obrigatório"`;
- Formato inválido → `"Campo e-mail está em um formato inválido"`;
- E-mail já existente → `"E-mail já cadastrado"`.

### 6.3 Senha (`password`)

- Obrigatória;
- Nulo, vazio ou apenas espaços → `"O campo senha é obrigatório"`;
- Menos de 5 caracteres → `"Senha deve conter 5 ou mais caracteres"`;
- Mais de 20 caracteres → `"Senha deve conter no máximo 20 caracteres"`.

### 6.4 Data de nascimento (`birthDate`)

- Opcional;
- Quando informada, deve ser data válida e menor que a data atual;
- Formato esperado: `yyyy-MM-dd`;
- Valor inválido → `"Data de nascimento inválida"`.

Exemplos inválidos: `2023-02-29`, data futura, formatos diferentes de `yyyy-MM-dd`.

## 7. Segurança de Senha

- Uso do algoritmo Argon2 para hashing irreversível;
- Geração de `salt` aleatório por usuário (único e não previsível);
- Uso de `pepper` (segredo da aplicação) combinado com a senha antes do hash;
- Persistência somente de `password_hash` e `password_salt` no banco;
- `pepper` não deve ser persistido no banco e deve ficar em secret store/config segura.

## 8. Fluxos e Diagrama de Alto Nível

Fluxo principal (criação):

1. Receber `POST /api/users`;
2. Validar payload na API;
3. Verificar duplicidade de e-mail;
4. Gerar `salt`, aplicar `pepper` e gerar hash (Argon2);
5. Persistir usuário;
6. Retornar `201 Created` com `code`.

Fluxo principal (troca de senha):

1. Receber `PUT /api/users/password` com token;
2. Validar token e identificar usuário;
3. Validar nova senha;
4. Gerar novo `salt` e `password_hash` e persistir;
5. Retornar `204 No Content`.

## 9. Respostas HTTP Padronizadas (resumo)

| Operação | Sucesso | Token inválido | Não encontrado | Validação inválida |
| --- | --- | --- | --- | --- |
| Criar usuário | `201 Created` | - | - | `400 Bad Request` |
| Trocar senha | `204 No Content` | `401 Unauthorized` | - | `400 Bad Request` |
| Consultar usuário | `200 OK` | `401 Unauthorized` | `404 Not Found` | `400 Bad Request` |
| Atualizar usuário | `204 No Content` | `401 Unauthorized` | `404 Not Found` | `400 Bad Request` |
| Excluir usuário | `204 No Content` | `401 Unauthorized` | `404 Not Found` | - |

## 10. Cenários de Teste

- Incluir sem token (quando aplicável) — `401`;
- Incluir com campos obrigatórios ausentes — `400`;
- Incluir com e-mail duplicado — `400`;
- Incluir com data de nascimento inválida — `400`;
- Trocar senha sem token — `401`;
- Trocar senha com validações falhando — `400`;
- Consultar usuário inexistente — `404`;
- Excluir usuário já excluído — `204` (idempotência).

## 11. Testes de Mutação (sugestões)

- Remover validação do token;
- Remover verificação de duplicidade do e-mail;
- Alterar armazenamento da senha para texto simples;
- Remover validação mínima de caracteres do nome.

## 12. Requisitos Não Funcionais

- A API deve seguir a convenção REST e usar status HTTP adequados;
- Validações devem ocorrer na camada API; regras de negócio na Application/Domain;
- Segredos (pepper) devem ser armazenados de forma segura (secret manager);
- Implementações devem ser compatíveis com PostgreSQL (snake_case nas colunas);
- Operações assíncronas devem expor `CancellationToken` em handlers.

## 13. Observações de Arquitetura

- Seguir o modelo de monólito modular e separação por responsabilidades;
- Persistência e configurações específicas (EF Core mappings) devem ficar em `Infrastructure`;
- Nomes de rotas e contratos devem estar em inglês; mensagens de validação em pt-BR.

---

Este documento serve como especificação para desenvolvimento, validação e testes do fluxo de cadastro e gestão de usuários. Alterações que afetem contratos públicos ou segurança devem atualizar este artefato.
