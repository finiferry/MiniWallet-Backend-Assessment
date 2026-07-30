namespace MiniWallet.Api.Entities;

public enum TransactionType
{
    Credit = 1,
    Debit = 2,
    Transfer = 3
}

public enum TransactionStatus
{
    Success = 1,
    Failed = 2
}