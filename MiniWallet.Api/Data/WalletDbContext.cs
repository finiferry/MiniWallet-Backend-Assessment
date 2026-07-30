using Microsoft.EntityFrameworkCore;
using MiniWallet.Api.Entities;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace MiniWallet.Api.Data;

public class WalletDbContext : DbContext
{
    public WalletDbContext(DbContextOptions<WalletDbContext> options) : base(options)
    {
    }

    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Wallet configuration
        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.HasKey(w => w.Id);

            // Unique constraints for email and mobile number
            entity.HasIndex(w => w.Email).IsUnique();
            entity.HasIndex(w => w.MobileNumber).IsUnique();

            entity.Property(w => w.Balance)
                  .HasPrecision(18, 2);

            entity.Property(w => w.Name)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(w => w.Email)
                  .IsRequired()
                  .HasMaxLength(150);

            entity.Property(w => w.MobileNumber)
                  .IsRequired()
                  .HasMaxLength(20);
        });

        // Transaction configuration
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);

            // Unique constraint on ReferenceId guarantees duplicate transactions are prevented at DB level
            entity.HasIndex(t => t.ReferenceId).IsUnique();

            // Indexes for fast history query performance
            entity.HasIndex(t => new { t.WalletId, t.CreatedAt });

            entity.Property(t => t.Amount)
                  .HasPrecision(18, 2);

            entity.Property(t => t.BalanceBefore)
                  .HasPrecision(18, 2);

            entity.Property(t => t.BalanceAfter)
                  .HasPrecision(18, 2);

            entity.Property(t => t.ReferenceId)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.HasOne(t => t.Wallet)
                  .WithMany(w => w.Transactions)
                  .HasForeignKey(t => t.WalletId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}