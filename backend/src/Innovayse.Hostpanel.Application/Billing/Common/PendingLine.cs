using Innovayse.Hostpanel.Domain.Billing;

namespace Innovayse.Hostpanel.Application.Billing.Common;

/// <summary>A line waiting to be added: its period start (for the due date) and how to add it.</summary>
/// <param name="Start">Period start.</param>
/// <param name="AddTo">Adds the line to an invoice.</param>
internal sealed record PendingLine(DateTimeOffset Start, Action<Invoice> AddTo);
