using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Application.Domains.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Domains.Commands.UpdateEmailForwardingRule;

/// <summary>
/// Updates an existing email forwarding rule in the domain aggregate and syncs the change to the
/// registrar.
/// </summary>
public sealed class UpdateEmailForwardingRuleHandler
{
    /// <summary>The registrar provider the updated rule is synced to.</summary>
    private readonly IRegistrarProvider _registrar;

    /// <summary>The data context the domain aggregate is loaded, mutated and saved through.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Unit of work the aggregate is saved through.</summary>
    private readonly IUnitOfWork _uow;

    /// <summary>Initializes the handler.</summary>
    /// <param name="registrar">The registrar provider the updated rule is synced to.</param>
    /// <param name="context">The data context the domain aggregate is loaded, mutated and saved through.</param>
    /// <param name="uow">Unit of work the aggregate is saved through.</param>
    public UpdateEmailForwardingRuleHandler(IRegistrarProvider registrar, IHostpanelDbContext context, IUnitOfWork uow)
    {
        _registrar = registrar;
        _context = context;
        _uow = uow;
    }

    /// <summary>
    /// Handles <see cref="UpdateEmailForwardingRuleCommand"/>.
    /// </summary>
    /// <param name="cmd">The update email forwarding rule command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>DOMAIN_NOT_FOUND</c> when the id names no domain, or
    /// <c>EMAIL_FORWARDING_RULE_NOT_FOUND</c> when the rule id names no rule on that domain, or
    /// <c>EMAIL_FORWARDING_RULE_UPSTREAM_FAILED</c> when the registrar rejects the update.
    /// </returns>
    public async Task<Result> HandleAsync(UpdateEmailForwardingRuleCommand cmd, CancellationToken ct)
    {
        var domain = await _context.Domains
            .IncludeChildren()
            .FirstOrDefaultAsync(x => x.Id == cmd.DomainId!.Value, ct);
        if (domain is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.DOMAIN_NOT_FOUND);
        }

        if (domain.EmailForwardingRules.All(r => r.Id != cmd.RuleId!.Value))
        {
            return Result.FailureCode(HostpanelErrorCodes.EMAIL_FORWARDING_RULE_NOT_FOUND);
        }

        domain.UpdateForwardingRule(cmd.RuleId!.Value, cmd.Source, cmd.Destination, cmd.IsActive);
        await _uow.SaveChangesAsync(ct);

        var result = await _registrar.UpdateEmailForwardingRuleAsync(domain.Name, cmd.Source, cmd.Destination, ct);
        if (!result.Success)
        {
            return Result.Failure(
                $"Registrar failed to update email forwarding rule for '{domain.Name}': {result.ErrorMessage}",
                HostpanelErrorCodes.EMAIL_FORWARDING_RULE_UPSTREAM_FAILED);
        }

        return Result.Success();
    }
}
