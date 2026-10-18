using Innovayse.Hostpanel.Application.Notifications.Commands.CreateEmailTemplate;
using Innovayse.Hostpanel.Application.Notifications.Commands.DeleteEmailTemplate;
using Innovayse.Hostpanel.Application.Notifications.Commands.ToggleEmailTemplate;
using Innovayse.Hostpanel.Application.Notifications.Commands.UpdateEmailTemplate;
using Innovayse.Hostpanel.Application.Notifications.Common;
using Innovayse.Hostpanel.Application.Notifications.Queries.GetEmailTemplate;
using Innovayse.Hostpanel.Application.Notifications.Queries.ListEmailTemplates;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>Admin endpoints for the stored email templates the platform renders its mail from.</summary>
[Route("api/admin/email-templates")]
[Authorize(Policy = Policies.Admin)]
public sealed class EmailTemplatesController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public EmailTemplatesController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Returns every registered email template.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the templates; 401; 403.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EmailTemplateDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<EmailTemplateDto>>>(new ListEmailTemplatesQuery(), ct);
        return HandleResult(outcome);
    }

    /// <summary>Returns a single email template.</summary>
    /// <param name="id">The template's primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the template; 404 with <c>EMAIL_TEMPLATE_NOT_FOUND</c>; 401; 403.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EmailTemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<EmailTemplateDto>>(new GetEmailTemplateQuery(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Creates a new email template.</summary>
    /// <param name="command">The template's slug, subject, body and description.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>201 with the new template's id; 400 when a field is refused; 401; 403.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateEmailTemplateCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<int>>(command, ct);
        return HandleResult(outcome, id => CreatedResult($"api/admin/email-templates/{id}", id));
    }

    /// <summary>Updates the content of an existing email template.</summary>
    /// <param name="id">Template primary key; wins over any id in the body.</param>
    /// <param name="command">The template's new subject, body and description.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 on success; 400 when a field is refused; 404 with <c>EMAIL_TEMPLATE_NOT_FOUND</c>; 401; 403.
    /// </returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(
        int id, [FromBody] UpdateEmailTemplateCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { Id = id }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Deletes an email template.</summary>
    /// <param name="id">The template's primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 on success; 404 with <c>EMAIL_TEMPLATE_NOT_FOUND</c>; 401; 403.</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new DeleteEmailTemplateCommand(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Activates or deactivates an email template.</summary>
    /// <param name="id">Template primary key; wins over any id in the body.</param>
    /// <param name="command">The desired active state.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 on success; 404 with <c>EMAIL_TEMPLATE_NOT_FOUND</c>; 401; 403.</returns>
    [HttpPut("{id:int}/toggle")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleAsync(
        int id, [FromBody] ToggleEmailTemplateCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { Id = id }, ct);
        return HandleResult(outcome);
    }
}
