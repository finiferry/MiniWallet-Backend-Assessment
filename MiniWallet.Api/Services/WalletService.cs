using Microsoft.EntityFrameworkCore;
using MiniWallet.Api.Data;
using MiniWallet.Api.DTOs;
using MiniWallet.Api.Entities;

namespace MiniWallet.Api.Services;

public class WalletService : IWalletService
{
    private readonly WalletDbContext _context;

    public WalletService(WalletDbContext context)
    {
        _context = context;
    }

    public async Task<WalletResponse> CreateWalletAsync(CreateWalletRequest request)
    {
        var existingEmail = await _context.Wallets.AnyAsync(w => w.Email == request.Email);
        if (existingEmail)
            throw new InvalidOperationException("A wallet with this email already exists.");

        var existingMobile = await _context.Wallets.AnyAsync(w => w.MobileNumber == request.MobileNumber);
        if (existingMobile)
            throw new InvalidOperationException("A wallet with this mobile number already exists.");

        if (request.InitialBalance < 0)
            throw new ArgumentException("Initial balance cannot be negative.");

        var wallet = new Wallet
        {
            Name = request.Name,
            Email = request.Email,
            MobileNumber = request.MobileNumber,
            Balance = request.InitialBalance
        };

        _context.Wallets.Add(wallet);

        if (request.InitialBalance > 0)
        {
            var initialTransaction = new Transaction
            {
                WalletId = wallet.Id,
                Type = TransactionType.Credit,
                Amount = request.InitialBalance,
                BalanceBefore = 0,
                BalanceAfter = request.InitialBalance,
                ReferenceId = $"INIT-{Guid.NewGuid()}",
                Status = TransactionStatus.Success
            };
            _context.Transactions.Add(initialTransaction);
        }

        await _context.SaveChangesAsync();

        return MapToWalletResponse(wallet);
    }

    public async Task<WalletResponse?> GetWalletAsync(Guid walletId)
    {
        var wallet = await _context.Wallets.FindAsync(walletId);
        return wallet == null ? null : MapToWalletResponse(wallet);
    }

    public async Task<WalletResponse> CreditWalletAsync(CreditWalletRequest request)
    {
        if (request.Amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.");

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var existingTransaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.ReferenceId == request.ReferenceId);

            var wallet = await _context.Wallets.FindAsync(request.WalletId)
                ?? throw new KeyNotFoundException("Wallet not found.");

            if (existingTransaction != null)
                return MapToWalletResponse(wallet);

            decimal balanceBefore = wallet.Balance;
            wallet.Balance += request.Amount;
            wallet.UpdatedAt = DateTime.UtcNow;

            var walletTx = new Transaction
            {
                WalletId = wallet.Id,
                Type = TransactionType.Credit,
                Amount = request.Amount,
                BalanceBefore = balanceBefore,
                BalanceAfter = wallet.Balance,
                ReferenceId = request.ReferenceId,
                Status = TransactionStatus.Success
            };

            _context.Transactions.Add(walletTx);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return MapToWalletResponse(wallet);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<WalletResponse> DebitWalletAsync(DebitWalletRequest request)
    {
        if (request.Amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.");

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var existingTransaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.ReferenceId == request.ReferenceId);

            var wallet = await _context.Wallets.FindAsync(request.WalletId)
                ?? throw new KeyNotFoundException("Wallet not found.");

            if (existingTransaction != null)
                return MapToWalletResponse(wallet);

            if (wallet.Balance < request.Amount)
                throw new InvalidOperationException("Insufficient balance.");

            decimal balanceBefore = wallet.Balance;
            wallet.Balance -= request.Amount;
            wallet.UpdatedAt = DateTime.UtcNow;

            var walletTx = new Transaction
            {
                WalletId = wallet.Id,
                Type = TransactionType.Debit,
                Amount = request.Amount,
                BalanceBefore = balanceBefore,
                BalanceAfter = wallet.Balance,
                ReferenceId = request.ReferenceId,
                Status = TransactionStatus.Success
            };

            _context.Transactions.Add(walletTx);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return MapToWalletResponse(wallet);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<TransferResponse> TransferAsync(TransferRequest request)
    {
        if (request.FromWalletId == request.ToWalletId)
            throw new InvalidOperationException("Cannot transfer funds to the same wallet.");

        if (request.Amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.");

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var sender = await _context.Wallets.FindAsync(request.FromWalletId)
                ?? throw new KeyNotFoundException("Sender wallet not found.");

            var receiver = await _context.Wallets.FindAsync(request.ToWalletId)
                ?? throw new KeyNotFoundException("Receiver wallet not found.");

            var existingTransaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.ReferenceId == request.ReferenceId);

            if (existingTransaction != null)
            {
                return new TransferResponse(
                    sender.Id,
                    receiver.Id,
                    request.Amount,
                    sender.Balance,
                    request.ReferenceId,
                    DateTime.UtcNow
                );
            }

            if (sender.Balance < request.Amount)
                throw new InvalidOperationException("Insufficient balance for transfer.");

            // Debit Sender
            decimal senderBalanceBefore = sender.Balance;
            sender.Balance -= request.Amount;
            sender.UpdatedAt = DateTime.UtcNow;

            var senderTx = new Transaction
            {
                WalletId = sender.Id,
                Type = TransactionType.Transfer,
                Amount = request.Amount,
                BalanceBefore = senderBalanceBefore,
                BalanceAfter = sender.Balance,
                ReferenceId = request.ReferenceId,
                Status = TransactionStatus.Success
            };

            // Credit Receiver
            decimal receiverBalanceBefore = receiver.Balance;
            receiver.Balance += request.Amount;
            receiver.UpdatedAt = DateTime.UtcNow;

            var receiverTx = new Transaction
            {
                WalletId = receiver.Id,
                Type = TransactionType.Transfer,
                Amount = request.Amount,
                BalanceBefore = receiverBalanceBefore,
                BalanceAfter = receiver.Balance,
                ReferenceId = $"{request.ReferenceId}-REC",
                Status = TransactionStatus.Success
            };

            _context.Transactions.AddRange(senderTx, receiverTx);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new TransferResponse(
                sender.Id,
                receiver.Id,
                request.Amount,
                sender.Balance,
                request.ReferenceId,
                DateTime.UtcNow
            );
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<PagedTransactionHistoryResponse> GetTransactionHistoryAsync(
        Guid walletId,
        string? transactionType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 10)
    {
        var walletExists = await _context.Wallets.AnyAsync(w => w.Id == walletId);
        if (!walletExists)
            throw new KeyNotFoundException("Wallet not found.");

        var query = _context.Transactions
            .Where(t => t.WalletId == walletId);

        if (!string.IsNullOrWhiteSpace(transactionType) && Enum.TryParse<TransactionType>(transactionType, true, out var parsedType))
        {
            query = query.Where(t => t.Type == parsedType);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= toDate.Value);
        }

        query = query.OrderByDescending(t => t.CreatedAt);

        int totalRecords = await query.CountAsync();

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(MapToTransactionResponse);

        return new PagedTransactionHistoryResponse(dtos, pageNumber, pageSize, totalRecords);
    }

    private static WalletResponse MapToWalletResponse(Wallet wallet) =>
        new(wallet.Id, wallet.Name, wallet.Email, wallet.MobileNumber, wallet.Balance, wallet.UpdatedAt);

    private static TransactionResponse MapToTransactionResponse(Transaction tx) =>
        new(tx.Id, tx.WalletId, tx.Type.ToString(), tx.Amount, tx.BalanceBefore, tx.BalanceAfter, tx.ReferenceId, tx.Status.ToString(), tx.CreatedAt);
}