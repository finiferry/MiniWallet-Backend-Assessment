# Mini Wallet Backend System

A high-performance, secure, and atomic Mini Wallet Backend API built with .NET 8, ASP.NET Core, and Entity Framework Core with SQLite.

---

## Features
- **Wallet Management**: Create user wallets with unique email/mobile checks and initial balance setup.
- **Atomic Operations**: Credit, Debit, and Wallet-to-Wallet transfer endpoints protected inside database transactions.
- **Idempotency & Safety**: Duplicate transactions are prevented using unique `referenceId` records.
- **Negative Balance Prevention**: Strict verification and database transaction locks ensure balances never drop below 0.
- **Transaction History**: Paginated, filterable audit log tracking `balanceBefore` and `balanceAfter` for every wallet operation.
- **Authentication & Security**: Protected with JWT Bearer Token authorization.
- **Documentation**: Swagger UI with JWT Bearer scheme enabled.

---

## How to Run Locally

### Option 1: Docker (Recommended)
1. Build the Docker image:
   ```bash
   docker build -t miniwallet-api .