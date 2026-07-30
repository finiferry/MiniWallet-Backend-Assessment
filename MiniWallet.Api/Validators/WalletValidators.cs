using FluentValidation;
using MiniWallet.Api.DTOs;

namespace MiniWallet.Api.Validators;

public class CreateWalletRequestValidator : AbstractValidator<CreateWalletRequest>
{
    public CreateWalletRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.MobileNumber)
            .NotEmpty().WithMessage("Mobile number is required.")
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Mobile number must be in a valid E.164 format (e.g. +1234567890).");

        RuleFor(x => x.InitialBalance)
            .GreaterThanOrEqualTo(0).WithMessage("Initial balance cannot be negative.");
    }
}

public class CreditWalletRequestValidator : AbstractValidator<CreditWalletRequest>
{
    public CreditWalletRequestValidator()
    {
        RuleFor(x => x.WalletId)
            .NotEmpty().WithMessage("WalletId is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Credit amount must be greater than zero.");

        RuleFor(x => x.ReferenceId)
            .NotEmpty().WithMessage("ReferenceId is required for idempotency tracking.");
    }
}

public class DebitWalletRequestValidator : AbstractValidator<DebitWalletRequest>
{
    public DebitWalletRequestValidator()
    {
        RuleFor(x => x.WalletId)
            .NotEmpty().WithMessage("WalletId is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Debit amount must be greater than zero.");

        RuleFor(x => x.ReferenceId)
            .NotEmpty().WithMessage("ReferenceId is required for idempotency tracking.");
    }
}

public class TransferRequestValidator : AbstractValidator<TransferRequest>
{
    public TransferRequestValidator()
    {
        RuleFor(x => x.FromWalletId)
            .NotEmpty().WithMessage("FromWalletId is required.");

        RuleFor(x => x.ToWalletId)
            .NotEmpty().WithMessage("ToWalletId is required.")
            .NotEqual(x => x.FromWalletId).WithMessage("Sender and receiver wallets cannot be the same.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Transfer amount must be greater than zero.");

        RuleFor(x => x.ReferenceId)
            .NotEmpty().WithMessage("ReferenceId is required for idempotency tracking.");
    }
}