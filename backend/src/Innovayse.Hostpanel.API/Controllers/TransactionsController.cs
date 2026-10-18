using Innovayse.Hostpanel.Application.Billing.Commands.CreateTransaction;
using Innovayse.Hostpanel.Application.Billing.Commands.DeleteTransaction;
using Innovayse.Hostpanel.Application.Billing.Commands.UpdateTransaction;
using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Application.Billing.Queries.GetTransactionById;
using Innovayse.Hostpanel.Application.Billing.Queries.ListTransactions;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>Admin and reseller endpoints for a client's ledger of financial transactions.</summary>
[Route("api/transactions")]
public sealed class TransactionsController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public TransactionsController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Returns a paginated list of all transactions.</summary>
    /// <param name="query">The page and page size to list by.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the page and summary totals; 401 without a session; 403 for a client caller.</returns>
    [HttpGet]
    [Authorize(Policy = Policies.ResellerOrAdmin)]
    [ProducesResponseType(typeof(TransactionsResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListAsync([FromQuery] ListTransactionsQuery query, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<TransactionsResultDto>>(query, ct);
        return HandleResult(outcome);
    }

    /// <summary>Returns a paginated list of transactions for a specific client.</summary>
    /// <param name="clientId">The client's primary key; wins over any client id in the query.</param>
    /// <param name="query">The page and page size to list by.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the page and summary totals; 401 without a session; 403 for a client caller.</returns>
    [HttpGet("client/{clientId:int}")]
    [Authorize(Policy = Policies.ResellerOrAdmin)]
    [ProducesResponseType(typeof(TransactionsResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListByClientAsync(
        int clientId, [FromQuery] ListTransactionsQuery query, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<TransactionsResultDto>>(
            query with { ClientId = clientId }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Returns a single transaction by ID.</summary>
    /// <param name="id">Transaction primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the transaction; 404 when the id names no transaction; 401; 403 for a client caller.
    /// </returns>
    [HttpGet("{id:int}")]
    [Authorize(Policy = Policies.ResellerOrAdmin)]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<TransactionDto>>(new GetTransactionByIdQuery(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Creates a new transaction, optionally adjusting the client's credit balance.</summary>
    /// <param name="command">The transaction to create.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 201 with the new transaction ID; 404 when a credit adjustment names a client that does not
    /// exist; 401; 403 for a caller who is not an admin.
    /// </returns>
    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateTransactionCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<int>>(command, ct);
        return HandleResult(outcome, id => CreatedResult($"/api/transactions/{id}", id));
    }

    /// <summary>Updates an existing transaction's editable fields.</summary>
    /// <param name="id">Transaction primary key; wins over any id in the body.</param>
    /// <param name="command">The fields to write.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 when written; 404 when the id names no transaction; 401; 403 for a caller who is not an admin.
    /// </returns>
    [HttpPut("{id:int}")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(
        int id, [FromBody] UpdateTransactionCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { Id = id }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Deletes a transaction and reverses any credit adjustments it made.</summary>
    /// <param name="id">Transaction primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 when deleted; 404 when the id names no transaction, or its credit reversal names a
    /// client that no longer exists; 401; 403 for a caller who is not an admin.
    /// </returns>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new DeleteTransactionCommand(id), ct);
        return HandleResult(outcome);
    }
}
