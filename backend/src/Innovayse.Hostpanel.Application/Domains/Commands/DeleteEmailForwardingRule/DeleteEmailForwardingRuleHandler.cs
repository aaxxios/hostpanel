using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Application.Domains.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Domains.Commands.DeleteEmailForwardingRule;

/// <summary>
/// Removes an email forwarding rule from the domain aggregate and deletes it at the registrar.
/// The rule's source address is captured before removal so it can be sent to the registrar.
/// </summary>
public sealed class DeleteEmailForwardingRuleHandler
{
    /// <summary>The registrar provider the deletion is synced to.</summary>
    private readonly IRegistrarProvider _registrar;

    /// <summary>The data context the domain aggregate is loaded, mutated and saved through.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Unit of work the aggregate is saved through.</summary>
    private readonly IUnitOfWork _uow;

    /// <summary>Initializes the handler.</summary>
    /// <param name="registrar">The registrar provider the deletion is synced to.</param>
    /// <param name="context">The data context the domain aggregate is loaded, mutated and saved through.</param>
    /// <param name="uow">Unit of work the aggregate is saved through.</param>
    public DeleteEmailForwardingRuleHandler(IRegistrarProvider registrar, IHostpanelDbContext context, IUnitOfWork uow)
    {
        _registrar = registrar;
        _context = context;
        _uow = uow;
    }

    /// <summary>
    /// Handles <see cref="DeleteEmailForwardingRuleCommand"/>.
    /// </summary>
    /// <param name="cmd">The delete email forwarding rule command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>DOMAIN_NOT_FOUND</c> when the id names no domain, or
    /// <c>EMAIL_FORWARDING_RULE_NOT_FOUND</c> when the rule id names no rule on that domain, or
    /// <c>EMAIL_FORWARDING_RULE_UPSTREAM_FAILED</c> when the registrar rejects the deletion.
    /// </returns>
    public async Task<Result> HandleAsync(DeleteEmailForwardingRuleCommand cmd, CancellationToken ct)
    {
        var domain = await _context.Domains
            .IncludeChildren()
            .FirstOrDefaultAsync(x => x.Id == cmd.DomainId!.Value, ct);
        if (domain is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.DOMAIN_NOT_FOUND);
        }

        var rule = domain.EmailForwardingRules.FirstOrDefault(r => r.Id == cmd.RuleId!.Value);
        if (rule is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.EMAIL_FORWARDING_RULE_NOT_FOUND);
        }

        var source = rule.Source;

        domain.RemoveForwardingRule(cmd.RuleId!.Value);
        await _uow.SaveChangesAsync(ct);

        var result = await _registrar.DeleteEmailForwardingRuleAsync(domain.Name, source, ct);
        if (!result.Success)
        {
            return Result.Failure(
                $"Registrar failed to delete email forwarding rule for '{domain.Name}': {result.ErrorMessage}",
                HostpanelErrorCodes.EMAIL_FORWARDING_RULE_UPSTREAM_FAILED);
        }

        return Result.Success();
    }
}
