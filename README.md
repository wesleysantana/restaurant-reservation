# 🍽 Restaurant Reservation System

API REST para gerenciamento de reservas de restaurante, com autenticação JWT, regras de horário configuráveis, validação de disponibilidade e execução via Docker.

> ⚠️ **Importante**
> Esta API **não possui Swagger/OpenAPI configurado**.
> Os testes devem ser realizados via **Postman** ou ferramenta equivalente.

## Principais funcionalidades
- Cadastro e autenticação de usuários (Identity + JWT).
- Usuário Admin criado automaticamente (via variáveis de ambiente).
- CRUD de mesas.
- Regras de horário de funcionamento (semanais e datas específicas/feriados).
- Criação/cancelamento de reservas com validações (conflito, capacidade, horário).

## Tecnologias
- ASP.NET Core
- Entity Framework Core + Identity
- PostgreSQL + Npgsql
- JWT Bearer
- NodaTime, Serilog
- Docker / Docker Compose
- xUnit + Moq

## Estrutura (alto nível)
```
src/
 ├── RestaurantReservation.WebApi
 ├── RestaurantReservation.Application
 ├── RestaurantReservation.Domain
 ├── RestaurantReservation.Infra
 └── RestaurantReservation.Identity
```

## Como executar

## Opção A — Docker Compose (recomendado)

```bash
cp .env-example .env
docker compose up --build
```

A API ficará disponível em:
```
http://localhost:5003
```

Admin padrão:
- Email: admin@restaurant.com
- Senha: Admin@123

---

### Opção B — Docker Run (API apenas)

Para quem já possui PostgreSQL local:

```env
ConnectionStrings__DefaultConnection=Host=host.docker.internal;Port=5432;Database=reservas;Username=postgres;Password=postgres
```

```bash
docker run -d --name restaurant-api -p 5003:8080 --env-file .env restaurant-api:dev
```

---

### Opção C — Execução Local

```bash
dotnet run --project src/RestaurantReservation.WebApi
```

---

## 🧪 Testes

Utilize Postman ou ferramenta equivalente.
Endpointa:
```
### LOGIN
POST /api/user/login
Content-Type: application/json

{
  "email": "teste@teste.com.br",
  "password": "Teste@1234"
}

###

### REGISTER
POST /api/user/register
Content-Type: application/json

{
  "email": "teste@teste.com.br",
  "password": "Teste@1234",
  "passwordConfirmation": "Teste@1234"
}

###

### REFRESH LOGIN
POST /api/user/refresh-login
Content-Type: application/json

{
  "refreshToken": "COLOQUE_O_REFRESH_TOKEN_AQUI"
}

###

### FAZER RESERVA
POST /api/reservations/make
Content-Type: application/json
Authorization: Bearer {{access_token}}

{
  "tableId": "00000000-0000-0000-0000-000000000001",
  "startsAt": "2025-04-01T19:00:00Z",
  "endsAt":   "2025-04-01T20:00:00Z",
  "numberOfGuests": 2
}

###

### LISTAR REGRAS DE HORÁRIO
GET /api/businesshours
Authorization: Bearer {{access_token}}

###

### CRIAR REGRA DE HORÁRIO (exemplo: regra semanal – terças de abril, 11h às 23h)
POST /api/businesshours
Content-Type: application/json
Authorization: Bearer {{access_token}}

{
  "startDate": "2025-04-01",
  "endDate": "2025-04-30",
  "specificDate": null,
  "weekDay": "Tuesday",
  "open": "11:00:00",
  "close": "23:00:00",
  "isClosed": false
}

###

### APAGAR REGRA DE HORÁRIO
DELETE /api/businesshours/{{businessHoursRuleId}}
Authorization: Bearer {{access_token}}

###

### OBTER UMA REGRA DE HORÁRIO POR ID
GET /api/businesshours/{{businessHoursRuleId}}
Authorization: Bearer {{access_token}}

###

### ATUALIZAR REGRA DE HORÁRIO
PUT /api/businesshours/{{businessHoursRuleId}}
Content-Type: application/json
Authorization: Bearer {{access_token}}

{
  "startDate": "2025-04-01",
  "endDate": "2025-04-30",
  "specificDate": null,
  "weekDay": "Saturday",
  "open": "12:00:00",
  "close": "23:59:00",
  "isClosed": false
}

###

### CANCELAR / DELETAR RESERVA
DELETE /api/reservation
Content-Type: application/json
Authorization: Bearer {{access_token}}

{
  "reservationId": "00000000-0000-0000-0000-000000000010"
}

###

### FAZER RESERVA
POST /api/reservation/make-reservation
Content-Type: application/json
Authorization: Bearer {{access_token}}

{
  "tableId": "00000000-0000-0000-0000-000000000001",
  "startsAt": "2025-04-01T19:00:00Z",
  "endsAt":   "2025-04-01T20:00:00Z",
  "numberOfGuests": 2
}

###

### LISTAR MESAS
GET /api/table
Authorization: Bearer {{access_token}}

###

### CRIAR MESA (apenas Admin)
POST /api/table
Content-Type: application/json
Authorization: Bearer {{access_token}}

{
  "name": "Mesa 01",
  "capacity": 4
}

###

### DELETAR MESA (apenas Admin)
DELETE /api/table/{{tableId}}
Authorization: Bearer {{access_token}}

###

### OBTER MESA POR ID
GET /api/table/{{tableId}}
Authorization: Bearer {{access_token}}

###

### ATUALIZAR MESA (apenas Admin)
PATCH /api/table/{{tableId}}
Content-Type: application/json
Authorization: Bearer {{access_token}}

{
  "name": "Mesa 01 - Atualizada",
  "capacity": 6,
  "status": "Disponivel"   // enum StatusTable: 'Disponivel','Reservada','Inativa'
}

###
```
---

## Variáveis de ambiente (Docker)
O arquivo `.env-example` mostra o conjunto mínimo para rodar via Docker Compose.
Destaques:
- Para configurar seções do ASP.NET Core via env var, use `__` (ex.: `JwtOptions__SecurityKey` vira `JwtOptions:SecurityKey`).
- O `IdentitySeeder` usa `ADMIN_EMAIL`, `ADMIN_PASSWORD`, `ADMIN_ROLE` para criar o Admin automaticamente.

## Testando com Postman
Fluxo típico:
1) `POST /api/user/register`
2) `POST /api/user/login` (use o admin do `.env`)
3) Use o `accessToken` como `Bearer Token` nas rotas protegidas.

## Modelagem (destaque)
Este projeto utiliza um constraint avançado no PostgreSQL para evitar reservas simultâneas, garantindo integridade no nível do banco.

---

## ✅ Observações 
- Chaves e credenciais presentes em arquivos de desenvolvimento são **apenas para teste local**.
- Para produção, a configuração deve vir de um gerenciador de segredos/variáveis de ambiente e as credenciais devem ser substituídas.
