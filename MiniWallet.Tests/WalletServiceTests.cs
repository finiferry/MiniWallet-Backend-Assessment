using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MiniWallet.Api.Data;
using MiniWallet.Api.DTOs;
using MiniWallet.Api.Entities;
using MiniWallet.Api.Services;
using Xunit;

namespace MiniWallet.Tests
{
    public class WalletServiceTests
    {
        // Helper method to create an isolated WalletDbContext in memory for each test
        private WalletDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<WalletDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                // Suppresses the in-memory transaction warning so BeginTransactionAsync works smoothly in tests
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            return new WalletDbContext(options);
        }

        [Fact]
        public async Task DebitWalletAsync_ShouldThrowException_WhenBalanceIsInsufficient()
        {
            // Arrange - Set up a wallet with $100 balance
            var dbContext = GetInMemoryDbContext();
            var wallet = new Wallet
            {
                Id = Guid.NewGuid(),
                Name = "Alice",
                Email = "alice@test.com",
                MobileNumber = "+1234567890",
                Balance = 100.00m
            };
            dbContext.Wallets.Add(wallet);
            await dbContext.SaveChangesAsync();

            var service = new WalletService(dbContext);

            var debitRequest = new DebitWalletRequest(wallet.Id, 500.00m, "TEST-REF-OVERDRAFT");

            // Act & Assert - Calling DebitWalletAsync to match WalletService.cs
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.DebitWalletAsync(debitRequest)
            );

            Assert.Equal("Insufficient balance.", exception.Message);
        }

        [Fact]
        public async Task CreditWalletAsync_ShouldBeIdempotent_WhenSameReferenceIdIsSent()
        {
            // Arrange - Set up Bob's wallet with $200
            var dbContext = GetInMemoryDbContext();
            var wallet = new Wallet
            {
                Id = Guid.NewGuid(),
                Name = "Bob",
                Email = "bob@test.com",
                MobileNumber = "+1987654321",
                Balance = 200.00m
            };
            dbContext.Wallets.Add(wallet);
            await dbContext.SaveChangesAsync();

            var service = new WalletService(dbContext);

            var creditRequest = new CreditWalletRequest(wallet.Id, 100.00m, "TEST-REF-DUPLICATE");

            // Act - Send the same credit request twice using CreditWalletAsync
            var response1 = await service.CreditWalletAsync(creditRequest);
            var response2 = await service.CreditWalletAsync(creditRequest);

            // Fetch transaction history to verify that only 1 transaction was logged
            var recordedTransactions = await dbContext.Transactions
                .Where(t => t.ReferenceId == "TEST-REF-DUPLICATE")
                .ToListAsync();

            // Assert - Balance should be $300 (credited once), not $400
            Assert.Equal(300.00m, response1.Balance);
            Assert.Equal(300.00m, response2.Balance);
            Assert.Single(recordedTransactions); // Verifies idempotency by confirming only 1 transaction record exists
        }
    }
}