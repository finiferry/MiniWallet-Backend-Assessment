namespace MiniWallet.Api.DTOs;

public record CreateWalletRequest(
    string Name,
    string Email,
    string MobileNumber,
    decimal InitialBalance = 0
);

public record WalletResponse(
    Guid WalletId,
    string UserName,
    string Email,
    string MobileNumber,
    decimal Balance,
    DateTime LastUpdated
);

public record CreditWalletRequest(
    Guid WalletId,
    decimal Amount,
    string ReferenceId
);

public record DebitWalletRequest(
    Guid WalletId,
    decimal Amount,
    string ReferenceId
);

public record TransferRequest(
    Guid FromWalletId,
    Guid ToWalletId,
    decimal Amount,
    string ReferenceId
);

public record TransferResponse(
    Guid FromWalletId,
    Guid ToWalletId,
    decimal Amount,
    decimal FromWalletBalance,
    string ReferenceId,
    DateTime Timestamp
);

public record TransactionResponse(
    Guid TransactionId,
    Guid WalletId,
    string Type,
    decimal Amount,
    decimal BalanceBefore,
    decimal BalanceAfter,
    string ReferenceId,
    string Status,
    DateTime CreatedAt
);

public record PagedResponse<T>(
    IEnumerable<T> Items,
    int PageNumber,
    int PageSize,
    int TotalRecords
);

// Alias to match IWalletService interface expectations
public record PagedTransactionHistoryResponse(
    IEnumerable<TransactionResponse> Items,
    int PageNumber,
    int PageSize,
    int TotalRecords
);