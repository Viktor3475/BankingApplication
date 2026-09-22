# Banking Application

A prototype ASP.NET Core 10 API written in C# 14. It stores bank users and accounts in PostgreSQL through EF Core 10 and Npgsql. The starting data model came from `db model.txt`.

> This is a learning prototype, not a payment system. It has password authentication, owner checks, and immutable demo transfer records, but no email verification, MFA, double-entry accounting ledger, or real bank account issuance. Do not expose it publicly or use it for real customer data or transfers.

## Run locally

Prerequisites: .NET 10 SDK and a running PostgreSQL server.

For a disposable local PostgreSQL instance, the repository includes `compose.yaml`:

```powershell
$env:BANKING_DB_PASSWORD = '<choose a local password>'
docker compose up -d --wait
$env:ConnectionStrings__Banking = "Host=localhost;Port=55432;Database=banking_application;Username=banking_demo;Password=$env:BANKING_DB_PASSWORD"
```

Run `docker compose down` when finished. The Compose database has no persistent volume, so removing its container removes its data. The Docker password comes from your environment and is not stored in the repository.

1. If you are not using Compose, create an empty database named `banking_application` and a login allowed to create tables in it.
2. Set the connection string outside source control. For an existing server, in PowerShell:

   ```powershell
   $env:ConnectionStrings__Banking = 'Host=localhost;Port=5432;Database=banking_application;Username=postgres;Password=<your password>'
   ```

3. From the repository root, run:

   ```powershell
   dotnet run --project BankingApplication
   ```

The app applies EF Core migrations at startup. The migration in `Data/Migrations/` creates both the banking and Identity tables. **Use a fresh empty database if you ran the earlier `EnsureCreated` version:** that version has no migration history or stored credentials. Do not drop an existing database containing data you need; plan a separate data migration instead. To add future migrations, run `dotnet tool restore` and `dotnet tool run dotnet-ef migrations add <Name> --project BankingApplication`.

In Development, the OpenAPI document is available at `/openapi/v1.json`. The application uses HTTPS redirection when configured with an HTTPS endpoint.

## Server-rendered pages

Open `/` (or `/web`) after starting the application. The pages support registration, sign-in, viewing your profile and accounts, sending demo money, reviewing sent and received movements, opening another account, and sign-out. Registration and sign-in are also available directly at `/web/register` and `/web/login`. A new registration receives a 1,000.00 demo opening balance so the transfer flow can be exercised; additional accounts open with a zero balance.

The pages use an HTTP-only cookie session and antiforgery-protected forms. The JSON API continues to use Identity bearer tokens; a browser cookie does not authenticate API requests. Both interfaces call the same application services and enforce the same account ownership rules. The pages display demo IBANs only.

Screenshots of the dashboard, sign-in, and registration pages are in the [repository README](../README.md#browser-interface).

## API

JSON enum values are strings. Requests and responses use DTOs; EF entities are never sent directly to clients.

| Method | Route | Purpose |
| --- | --- | --- |
| `POST` | `/api/auth/register` | Register with a password and create an initial demo-funded Current account. Public. |
| `POST` | `/api/auth/login` | Verify credentials and issue a bearer token. Public. |
| `GET` | `/api/bank-users/me` | Read the signed-in user's profile. |
| `POST` | `/api/accounts` | Open another zero-balance account for the signed-in user. |
| `GET` | `/api/accounts` | List the signed-in user's accounts. |
| `GET` | `/api/accounts/{id}` | Read an account owned by the signed-in user. |
| `POST` | `/api/accounts/{id}/transfers` | Send money from an owned account to a demo IBAN. |
| `GET` | `/api/money-movements` | List sent and received movements for the signed-in user. |

Register with `POST /api/auth/register`:

```json
{ "username": "Ada", "email": "ada@example.com", "password": "ExamplePass123!", "country": "BG" }
```

A successful response is HTTP 201, with a `Location` header for `/api/bank-users/me` and a body shaped like:

```json
{
  "user": { "id": "<guid>", "username": "Ada", "email": "ada@example.com", "country": "BG" },
  "account": { "id": "<guid>", "iban": "<demo IBAN>", "balance": 1000, "accountType": "Current", "bankUserId": "<same user guid>" }
}
```

Sign in using `POST /api/auth/login`:

```json
{ "email": "ada@example.com", "password": "ExamplePass123!" }
```

The response contains an `accessToken`. Send it on protected calls as `Authorization: Bearer <accessToken>`. This is an ASP.NET Core Identity bearer token, **not a JWT**. Passwords must be at least 12 characters and satisfy Identity's other default password rules. Login failures return HTTP 401. Passwords are hashed by Identity, and five failed attempts trigger a 15-minute lockout. Token refresh and logout endpoints are not implemented.

Open another account while signed in:

```json
{ "accountType": "Savings" }
```

The server chooses the owner from the bearer token, along with the IBAN and opening balance. Clients cannot choose those values. To send money, provide a recipient IBAN, a positive amount with at most two decimal places, and an optional description of up to 140 characters. The source account must belong to the signed-in user and have enough funds. Authentication is required by default; only registration, login, and the Development OpenAPI document are public. Protected routes return HTTP 401 without a valid token. An account ID owned by another user returns HTTP 404. Invalid inputs return HTTP 400; duplicate email or IBAN values return HTTP 409.

## How the code is organized

- `Controllers/` translates HTTP requests into service calls and service results into HTTP status codes. Controllers do not query EF or parse claims.
- `Views/` contains Razor templates for the browser interface. `WebController` handles page requests and forms through the existing services.
- `Dtos/` defines the request and response fields visible to clients. `CreateAccountDto` deliberately has no balance, IBAN, or owner ID field.
- `Services/` contains application rules and interfaces for persistence, authentication, and the current user. Services validate inputs, generate demo IBANs, and map entities to DTOs without depending on EF Core or Npgsql.
- `Data/` implements the persistence interfaces with EF Core and PostgreSQL. It owns queries, unique-constraint handling, and the registration transaction that spans Identity credentials, profile, and starting account.
- `Auth/` adapts ASP.NET Core Identity and the HTTP user principal to the application interfaces.
- `Models/` contains the EF entities and enums. A `BankUser` can own many `Account` rows; every account has one owner.
- `Data/BankingDbContext.cs` maps banking and Identity tables, unique indexes, enum strings, foreign keys, `numeric(18,2)` balance, and a nonnegative balance constraint.
- `MoneyMovementStore` changes both balances and inserts the immutable movement record in one serializable transaction. History queries are scoped to accounts owned by the signed-in user.
- `Program.cs` configures dependency injection, PostgreSQL, Identity bearer authentication, authorization, JSON enums, OpenAPI, and startup migrations.

The dependency flow is `Controllers → Services → persistence interfaces`, with `Data/` providing the EF Core implementations. The authenticated user ID comes from `ICurrentUser`, implemented in `Auth/`, and is used as the owner filter for account operations.

### Request flow

1. ASP.NET Core validates and binds a request DTO. `[Authorize]` and the fallback policy reject unauthenticated callers on protected routes.
2. A controller gets the current profile ID through `ICurrentUser` and calls an application service.
3. The service applies rules and maps between EF entities and response DTOs. It calls a store interface for persistence.
4. A `Data/` store performs owner-filtered EF queries or a transaction. The controller converts the service result to an HTTP response.

Registration is the one multi-save operation: `BankUserStore` coordinates Identity's credential write with the profile and initial account write. The bearer token itself is issued by the authentication handler after `IdentityAuthService` verifies a password.

### Generated database files

`Data/Migrations/*InitialIdentitySchema.cs` and `BankingDbContextModelSnapshot.cs` are generated by EF Core. `Data/Migrations/initial.sql` is a generated SQL rendering of that migration for inspection. The application applies the C# migration at startup; it does not execute `initial.sql` directly. Make schema changes in the model and `BankingDbContext`, then generate a new migration rather than editing the snapshot or SQL copy by hand.

## Writes and transactions

EF Core uses its default `AutoTransactionBehavior.WhenNeeded`: each `SaveChangesAsync` call is atomic, and EF creates an explicit transaction when needed. Registration uses an **outer transaction** because Identity saves the credential row before the service saves the profile and initial account. A money transfer uses a serializable transaction around both balance changes and its movement record. Either every part commits or none does. Separate `SaveChangesAsync` calls without an outer transaction remain separate transactions. Direct SQL writes are outside this EF behavior.

`CancellationToken` flows from each HTTP request through the services to EF Core. Cancellation can stop work when a request ends, but a canceled client request does not prove that a write was rolled back.

## Demo IBAN rules

`DemoIbanGenerator` creates identifiers with the country-specific structure and MOD-97 check digits for UK (`GB`), BG, RO, RU, and TR. It uses placeholder bank identifiers. These values are **not issued by a bank** and must not be used for payments. The PostgreSQL unique index on `Iban` prevents a generated duplicate from being saved; the API returns HTTP 409 if one occurs.

US has no registered IBAN format. Because registration always creates an initial account, a `POST /api/auth/register` request with country `US` returns HTTP 400. Supported country values remain `UK`, `US`, `BG`, `RO`, `RU`, and `TR` in the model.

The country structures are based on [Swift's IBAN registry](https://www.swift.com/standards/data-standards/iban-international-bank-account-number). A valid format and checksum do not establish that an account exists or that this application may issue identifiers for a bank.

## Demo walkthrough

Start the API in one PowerShell window with the connection string set:

```powershell
$env:ASPNETCORE_URLS = 'http://localhost:5080'
dotnet run --no-launch-profile --project BankingApplication
```

In a second PowerShell window, register, sign in, and read the new account:

```powershell
$base = 'http://localhost:5080'
$email = "ada+$([guid]::NewGuid().ToString('N'))@example.test"
$password = 'ExamplePass123!'
$registration = Invoke-RestMethod -Method Post -Uri "$base/api/auth/register" `
    -ContentType 'application/json' `
    -Body (@{ username = 'Ada'; email = $email; password = $password; country = 'BG' } | ConvertTo-Json)
$login = Invoke-RestMethod -Method Post -Uri "$base/api/auth/login" `
    -ContentType 'application/json' `
    -Body (@{ email = $email; password = $password } | ConvertTo-Json)
$headers = @{ Authorization = "Bearer $($login.accessToken)" }
$profile = Invoke-RestMethod -Uri "$base/api/bank-users/me" -Headers $headers
$accounts = Invoke-RestMethod -Uri "$base/api/accounts" -Headers $headers
$registration
$profile
$accounts
```

The registration response includes a demo-funded Current account and demo IBAN. The signed-in account list contains that same account. The values are for demonstration only.

## Verify and extend

Run `dotnet test BankingApplication.slnx` to compile and run the xUnit suite. It covers country formats and IBAN checksums, owner-filtered account queries, registration rollback, successful atomic transfers, and insufficient-funds rejection. The store tests use SQLite in memory so they run without Docker; PostgreSQL-specific behavior still needs an end-to-end check against the Compose database. Run `dotnet tool run dotnet-ef migrations has-pending-model-changes --project BankingApplication --no-build` after building to confirm that the EF model matches the latest migration.

For a new write operation, put its rules in a service, its EF work in a store, and expose only the necessary request and response fields through DTOs. If it makes more than one save that must succeed together, put an explicit transaction around the entire operation in the data layer. Keep generated IBANs out of any real payment flow.
