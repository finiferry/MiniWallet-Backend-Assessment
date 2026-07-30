using Microsoft.AspNetCore.Mvc;
using MiniWallet.Api.DTOs;
using MiniWallet.Api.Services;
using Microsoft.AspNetCore.Authorization;

namespace MiniWallet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WalletsController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletsController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    /// <summary>
    /// Creates a new wallet account.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateWallet([FromBody] CreateWalletRequest request)
    {
        try
        {
            var result = await _walletService.CreateWalletAsync(request);
            return CreatedAtAction(nameof(GetWallet), new { id = result.WalletId }, result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves wallet details and current balance by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetWallet(Guid id)
    {
        var wallet = await _walletService.GetWalletAsync(id);
        if (wallet == null)
            return NotFound(new { error = "Wallet not found." });

        return Ok(wallet);
    }

    /// <summary>
    /// Credits money into a wallet account (Idempotent using ReferenceId).
    /// </summary>
    [HttpPost("credit")]
    public async Task<IActionResult> CreditWallet([FromBody] CreditWalletRequest request)
    {
        try
        {
            var result = await _walletService.CreditWalletAsync(request);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Debits money from a wallet account (Idempotent using ReferenceId).
    /// </summary>
    [HttpPost("debit")]
    public async Task<IActionResult> DebitWallet([FromBody] DebitWalletRequest request)
    {
        try
        {
            var result = await _walletService.DebitWalletAsync(request);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Transfers funds atomically between two wallets (Idempotent using ReferenceId).
    /// </summary>
    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer([FromBody] TransferRequest request)
    {
        try
        {
            var result = await _walletService.TransferAsync(request);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Fetches transaction history for a wallet with optional filtering and pagination.
    /// </summary>
    [HttpGet("{id:guid}/transactions")]
    public async Task<IActionResult> GetTransactionHistory(
        Guid id,
        [FromQuery] string? transactionType = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _walletService.GetTransactionHistoryAsync(
                id,
                transactionType,
                fromDate,
                toDate,
                pageNumber,
                pageSize);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}