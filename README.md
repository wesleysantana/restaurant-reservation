# 🍽 Restaurant Reservation System  
API REST completa para gerenciamento de reservas de restaurante, com autenticação JWT, regras de horário configuráveis, validação de disponibilidade e infraestrutura Docker.

## 📘 Descrição Geral
Este projeto implementa um sistema robusto de reservas com:
- Cadastro e autenticação de usuários (com função Admin).
- CRUD de mesas.
- Configuração de horários de funcionamento.
- Criação e cancelamento de reservas.
- Validação contra conflitos e horários inválidos.
- Execução simplificada via Docker.
- Uso de coleção Postman para testar os endpoints.

## 🛠 Tecnologias Utilizadas
- ASP.NET Core 10
- Entity Framework Core
- Identity + JWT Bearer
- PostgreSQL
- NodaTime
- FluentResults
- Serilog
- Docker + docker-compose
- xUnit + Moq

## 🧱 Arquitetura
Clean Architecture / DDD-lite:

```
src/
 ├── RestaurantReservation.WebApi
 ├── RestaurantReservation.Application
 ├── RestaurantReservation.Domain
 ├── RestaurantReservation.Infra
 └── RestaurantReservation.Identity
```

## 🗄 Modelagem do Banco
- Mesas (`tables`)
- Reservas (`reservations`)
- Regras de horário (`business_hours_rules`)

Destaque: constraint avançada do PostgreSQL que impede reservas simultâneas:

```sql
EXCLUDE USING gist (
  table_id WITH =,
  time_range WITH &&
)
WHERE (status = 'Ativo');
```

## 📜 Regras de Negócio
- Reserva só pode ocorrer:
  - se o horário estiver dentro do expediente,
  - se não houver conflito,
  - se a mesa suportar o número de convidados.
- Cancelamento apenas de reservas ativas.
- Regras de funcionamento configuráveis:
  - por dia da semana,
  - por período,
  - por dias específicos/feriados.

## 🔐 Autenticação
- JWT Bearer Tokens.
- Admin gerado automaticamente via variáveis de ambiente (ou user-secrets em desenvolvimento).

## 🕒 Configuração de Horários
Exemplo de regra semanal:
```json
{
  "startDate": "2025-03-01",
  "endDate": "2025-03-31",
  "weekDay": 1,
  "open": "09:00",
  "close": "18:00",
  "isClosed": false
}
```

Exemplo de feriado:
```json
{ "specificDate": "2025-03-21", "isClosed": true }
```

## ▶ Como Executar o Projeto

### 🔹 Via Docker
Na raiz da solução:

```sh
docker-compose up --build
```

API:
```
http://localhost:5003
```

Admin criado automaticamente (via variáveis/arquivo `.env`):

```
ADMIN_EMAIL=admin@restaurant.com
ADMIN_PASSWORD=Admin@123
ADMIN_ROLE=Admin
```

### 🔹 Localmente (sem Docker)

Configurar user-secrets (na pasta `src/RestaurantReservation.WebApi`):
```sh
dotnet user-secrets init
dotnet user-secrets set "ADMIN_EMAIL" "admin@restaurant.com"
dotnet user-secrets set "ADMIN_PASSWORD" "Admin@123"
dotnet user-secrets set "ADMIN_ROLE" "Admin"
```

Rodar migrations (banco principal):
```sh
dotnet ef database update -p src/RestaurantReservation.Infra -s src/RestaurantReservation.WebApi
```

Rodar migrations do Identity
```sh
dotnet ef database update -p src/RestaurantReservation.Identity -s src/RestaurantReservation.WebApi
```

Rodar API:
```sh
dotnet run --project src/RestaurantReservation.WebApi
```

---

## 🧪 Testes
- Testes de unidade para Services (Reservation, Table, Identity, BusinessHours).
- Testes de unidade para Controllers.
- Cenários cobrindo:
  - conflitos de reservas,
  - capacidade de mesa,
  - horário de funcionamento,
  - autenticação e autorização.

---

## 🌐 Testando a API com Postman

Você pode testar a API usando o Postman (ou qualquer cliente HTTP).

### 1. URL base

Se estiver rodando via Docker ou localmente conforme configuração acima:

```text
http://localhost:5003
```

### 2. Autenticação

#### 2.1. Registrar usuário

`POST /api/user/register`

Body (JSON):
```json
{
  "email": "user@teste.com",
  "password": "Teste@1234",
  "passwordConfirmation": "Teste@1234"
}
```

#### 2.2. Login

`POST /api/user/login`

Body (JSON):
```json
{
  "email": "admin@restaurant.com",
  "password": "Admin@123"
}
```

Resposta (200) retorna um objeto com o `accessToken` (JWT).  
No Postman, configure:

- Aba **Authorization**
- Type: `Bearer Token`
- Token: cole o valor do `accessToken`.

A partir daí, use esse token nas rotas protegidas.

---

### 3. Endpoints principais

#### 3.1. Mesas

**Criar mesa (Admin)**  
`POST /api/table`

Body:
```json
{
  "name": "Mesa 01",
  "capacity": 4
}
```

**Listar mesas**  
`GET /api/table`

**Obter mesa por id**  
`GET /api/table/{id}`

**Atualizar mesa (Admin)**  
`PATCH /api/table/{id}`

Body:
```json
{
  "name": "Mesa 01 - Atualizada",
  "capacity": 6,
  "status": "Disponivel"
}
```

**Excluir mesa (Admin)**  
`DELETE /api/table/{id}`

---

#### 3.2. Regras de horário (BusinessHours)

**Listar regras**  
`GET /api/businesshours`

**Criar regra**  
`POST /api/businesshours`

Body (exemplo regra semanal):
```json
{
  "startDate": "2025-04-01",
  "endDate": "2025-04-30",
  "weekDay": 1,
  "specificDate": null,
  "open": "11:00:00",
  "close": "23:00:00",
  "isClosed": false
}
```

**Obter regra por id**  
`GET /api/businesshours/{id}`

**Atualizar regra**  
`PUT /api/businesshours/{id}`

**Excluir regra**  
`DELETE /api/businesshours/{id}`

---

#### 3.3. Reservas

**Criar reserva**  
`POST /api/reservations/make`

Body:
```json
{
  "tableId": "GUID_DA_MESA",
  "startsAt": "2025-04-10T19:00:00Z",
  "endsAt":   "2025-04-10T20:00:00Z",
  "numberOfGuests": 2
}
```

**Cancelar reserva**  
Dependendo da sua API, pode ser via `DELETE /api/reservation` com body, ou rota específica.  
Exemplo com body:

`DELETE /api/reservation`

```json
{
  "reservationId": "GUID_DA_RESERVA"
}
```

---

## 🧠 Decisões Técnicas Importantes
- Uso de NodaTime para precisão temporal (Instant, LocalDateTime, etc.).
- GIST + `tstzrange` para garantir integridade e evitar sobreposição de reservas.
- FluentResults para padronização de sucesso/erro e integração com ProblemDetails.
- Arquitetura limpa com separação forte entre camadas (Domain, Application, Infra, WebApi, Identity).
- Execução altamente reprodutível via Docker e `docker-compose`.

## ⭐ O que Destacar no Portfólio
- Projeto realista com regras complexas de negócio.
- Uso de recursos avançados do PostgreSQL para consistência forte.
- Solução completa: API + autenticação + banco + Docker + testes.
- Código bem estruturado, expansível e testável.
- Documentação de uso via Postman, com fluxo completo (auth → mesas → horários → reservas).
