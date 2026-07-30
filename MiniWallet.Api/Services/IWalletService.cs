using MiniWallet.Api.DTOs;

namespace MiniWallet.Api.Services;

public interface IWalletService
{
    Task<WalletResponse> CreateWalletAsync(CreateWalletRequest request);
    Task<WalletResponse?> GetWalletAsync(Guid id);
    Task<WalletResponse> CreditWalletAsync(CreditWalletRequest request);
    Task<WalletResponse> DebitWalletAsync(DebitWalletRequest request);
    Task<TransferResponse> TransferAsync(TransferRequest request);

    // Updated method signature with 6 parameters
    Task<PagedTransactionHistoryResponse> GetTransactionHistoryAsync(
        Guid id,
        string? transactionType,
        DateTime? fromDate,
        DateTime? toDate,
        int pageNumber,
        int pageSize);
}