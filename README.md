# Mini Wallet Backend System

A high-performance, secure, and atomic Mini Wallet Backend API built with .NET 8, ASP.NET Core, Entity Framework Core, and SQLite.

---

## Technical Overview & Assessment Answers

### 1. How to run the project locally
* **Option 1 (.NET CLI):** Open your terminal, navigate to the project directory, and run `dotnet restore` followed by `dotnet run --project MiniWallet.Api`.
* **Option 2 (Docker):** Build the image using `docker build -t miniwallet-api .` and run it using `docker run -p 8080:8080 miniwallet-api`.

### 2. Database setup steps
* Entity Framework Core automatically creates and initializes the local SQLite database (`miniwallet.db`) when you launch the application for the first time. No manual database setup or SQL script execution is required.

### 3. API list
* `POST /api/auth/login` – Generate JWT token for authorized API access.
* `POST /api/wallet/create` – Create a new wallet account.
* `GET /api/wallet/balance/{walletId}` – Retrieve current account balance.
* `POST /api/wallet/credit` – Add funds to a wallet using a unique `referenceId`.
* `POST /api/wallet/debit` – Withdraw funds from a wallet using a unique `referenceId`.
* `POST /api/wallet/transfer` – Atomically transfer funds between two wallets.
* `GET /api/wallet/transactions/{walletId}` – Retrieve paginated audit history for a wallet.

### 4. How duplicate transactions are handled
* Every transaction (Credit, Debit, Transfer) requires a client-supplied `referenceId`.
* Before performing any operation, the system queries the `Transactions` table for the given `referenceId`.
* If a record with that ID already exists, the API rejects the request immediately with an **HTTP 409 Conflict** error, preventing duplicate processing.

### 5. How concurrent debit or transfer requests are handled
* Operations are wrapped inside explicit Entity Framework Core database transactions (`IDbContextTransaction`).
* Database-level locking during the active transaction guarantees that parallel requests are executed sequentially rather than simultaneously, preventing race conditions.

### 6. How negative balance is prevented
* Before deducting funds, the system fetches the wallet record within the transaction and checks:
  `if (wallet.Balance < amount) throw new InvalidOperationException("Insufficient funds");`
* Combined with transaction locks, this guarantees that two concurrent requests cannot overspend or drive the account balance below zero.

### 7. What performance optimizations were applied
* **Database Indexing:** Added indexes on frequently queried fields like `WalletId`, `ReferenceId`, and `CreatedAt` in SQLite.
* **Asynchronous Execution:** Used `async`/`await` across all database I/O operations to optimize thread utilization.
* **Minimal DTO Payloads:** Used clean Data Transfer Objects to avoid returning unnecessary database entities or circular JSON references.

### 8. What can be improved with more time
* **Database Scaling:** Upgrade from SQLite to PostgreSQL or SQL Server for higher write concurrency.
* **Distributed Locking & Caching:** Integrate Redis for distributed locking and caching account balance lookups.
* **Rate Limiting:** Implement ASP.NET Core Rate Limiting middleware to block brute-force or spam requests.
* **Integration Testing:** Expand test coverage with `WebApplicationFactory` integration tests.
