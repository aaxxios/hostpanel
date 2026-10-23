using Innovayse.Hostpanel.Application.Admin.Commands.UpdateSetting;
using Innovayse.Hostpanel.Application.Admin.Commands.UploadBrandingImage;
using Innovayse.Hostpanel.Application.Admin.Common;
using Innovayse.Hostpanel.Application.Admin.Common.Enums;
using Innovayse.Hostpanel.Application.Admin.Queries.GetSetting;
using Innovayse.Hostpanel.Application.Admin.Queries.GetSettings;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Constants;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>Admin endpoints for managing system configuration settings and storefront branding.</summary>
[Route("api/admin/settings")]
[Authorize(Policy = Policies.Admin)]
public sealed class SettingsController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public SettingsController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Returns all configuration settings; a secret one carries no value.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with every setting; 401; 403.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SettingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<SettingDto>>>(new GetSettingsQuery(), ct);
        return HandleResult(outcome);
    }

    /// <summary>Returns a single setting by ID; a secret one carries no value.</summary>
    /// <param name="id">Setting primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the setting; 404 with <c>SETTING_NOT_FOUND</c>; 401; 403.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SettingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<SettingDto>>(new GetSettingQuery(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Updates the value of an existing setting.</summary>
    /// <param name="id">Setting primary key; wins over any id in the body.</param>
    /// <param name="command">
    /// The new value, or <c>clear</c> to remove a secret. An empty value for a secret keeps the
    /// stored one.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 on success; 404 with <c>SETTING_NOT_FOUND</c>; 400 with <c>INVALID_SETTING_VALUE</c>
    /// when the key has a fixed vocabulary or shape and the value does not match it; 401; 403.
    /// </returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(
        int id, [FromBody] UpdateSettingCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { Id = id }, ct);
        return HandleResult(outcome);
    }

    /// <summary>
    /// Uploads a storefront branding image and returns the URL to save into the setting.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The controller binds, caps and delegates. Deciding what the bytes are, rendering the icon
    /// set and writing the files all sit behind <c>UploadBrandingImageCommand</c>.
    /// </para>
    /// <para>
    /// <see cref="RequestSizeLimitAttribute"/> is the ceiling the request never gets past, and it
    /// is deliberately larger than the configured one. This is the framework refusing to buffer an
    /// enormous body at all; <c>Branding:MaxBytes</c> is the operator-facing limit, and it answers
    /// with a readable refusal rather than a connection reset.
    /// </para>
    /// </remarks>
    /// <param name="kind">
    /// Which branding image this is: <c>logo</c>, <c>logodark</c>, <c>logomark</c> or
    /// <c>favicon</c> — the <see cref="BrandingKind"/> member name, case-insensitive.
    /// </param>
    /// <param name="file">The image file to upload.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the primary URL and every file generated from the upload; 400 with
    /// <c>BRANDING_KIND_INVALID</c>-shaped validation, <c>BRANDING_IMAGE_INVALID</c>; 413 with
    /// <c>BRANDING_IMAGE_TOO_LARGE</c>; 401; 403.
    /// </returns>
    [HttpPost("branding/{kind}")]
    [RequestSizeLimit(BrandingUploadLimits.RequestSizeLimitBytes)]
    [ProducesResponseType(typeof(BrandingUploadResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status413PayloadTooLarge)]
    public async Task<IActionResult> UploadBrandingImageAsync(BrandingKind kind, IFormFile file, CancellationToken ct)
    {
        await using var content = file?.OpenReadStream() ?? Stream.Null;
        var command = new UploadBrandingImageCommand(
            kind, content, file?.Length ?? 0, file?.FileName ?? string.Empty);

        var outcome = await _bus.InvokeAsync<Result<BrandingUploadResultDto>>(command, ct);
        return HandleResult(outcome);
    }
}
