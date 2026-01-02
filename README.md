# 🍽 Restaurant Reservation System

API REST para gerenciamento de reservas de restaurante, com autenticação JWT, regras de horário configuráveis, validação de disponibilidade e execução via Docker.

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

### Opção A) Docker (recomendado para testar rapidamente)
1) Copie o arquivo de exemplo:
```sh
cp .env-example .env
```

2) Suba tudo com Docker Compose (na raiz do repositório):
```sh
docker compose up --build
```

3) Acesse:
- Swagger: http://localhost:5003/swagger

**Admin padrão (vem do .env):**
- Email: `admin@restaurant.com`
- Password: `Admin@123`

> Observação: no Docker, o hostname do Postgres deve ser o nome do serviço do compose (por padrão, `db`). Por isso o `.env-example` usa `POSTGRES_HOST=db`.

---

### Opção B) Local (Visual Studio / dotnet run)
Pré-requisitos:
- .NET SDK
- PostgreSQL rodando localmente (ex.: porta 5432)

1) Configuração
- Este repositório inclui um `appsettings.Development.json` com valores de desenvolvimento (incluindo `JwtOptions:SecurityKey`) para facilitar o teste local.

2) Migrações (Infra e Identity)
Na raiz do repositório:
```sh
dotnet ef database update -p src/RestaurantReservation.Infra -s src/RestaurantReservation.WebApi
dotnet ef database update -p src/RestaurantReservation.Identity -s src/RestaurantReservation.WebApi
```

3) Executar API
```sh
dotnet run --project src/RestaurantReservation.WebApi
```

4) Acesse:
- Swagger: https://localhost:<porta>/swagger  (ou veja a porta no console)
- Se você preferir padronizar a porta, execute com `--urls`.

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

## Notas para avaliadores
- Chaves e credenciais presentes em arquivos de desenvolvimento são **apenas para teste local**.
- Para produção, a configuração deve vir de um gerenciador de segredos/variáveis de ambiente e as credenciais devem ser substituídas.
