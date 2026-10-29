using System.Net;
using System.Reflection;
using Innovayse.Hostpanel.API.Controllers;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Shared.Constants;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// <c>GET /api/me/invoices/{id}/pdf</c> against Postgres: a client downloads their own published
/// invoices, and a draft, a stranger's invoice and an unknown id all answer the same 404.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetMyInvoicePdfTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>The owner downloads their published invoice as a PDF attachment.</summary>
    [Fact]
    public async Task GetMyPdf_Owner_GetsPublishedInvoice()
    {
        var (owner, ownerClientId) = await BillingTestSeeds.RegisterClientAsync(factory, null, "pdf-owner");
        var admin = await BillingTestSeeds.AdminClientAsync(factory);
        var invoiceId = await BillingTestSeeds.CreateInvoiceAsync(admin, ownerClientId, 10m);

        var response = await owner.GetAsync($"/api/me/invoices/{invoiceId}/pdf");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
        response.Content.Headers.ContentDisposition!.FileName.Should().Be($"invoice-{invoiceId}.pdf");
        var bytes = await response.Content.ReadAsByteArrayAsync();
        System.Text.Encoding.ASCII.GetString(bytes, 0, 5).Should().Be("%PDF-");
    }

    /// <summary>The client's download is not kept by the browser or a proxy either.</summary>
    [Fact]
    public async Task GetMyPdf_IsNeverCached()
    {
        var (owner, ownerClientId) = await BillingTestSeeds.RegisterClientAsync(factory, null, "pdf-no-store");
        var admin = await BillingTestSeeds.AdminClientAsync(factory);
        var invoiceId = await BillingTestSeeds.CreateInvoiceAsync(admin, ownerClientId, 10m);

        var response = await owner.GetAsync($"/api/me/invoices/{invoiceId}/pdf");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.CacheControl!.Private.Should().BeTrue();
        response.Headers.CacheControl.NoStore.Should().BeTrue();
    }

    /// <summary>
    /// Rendering holds a thread and a few megabytes, so a client may only have a few in flight.
    /// Limiting is switched off in the Testing environment, hence the attribute is checked rather
    /// than provoked.
    /// </summary>
    [Fact]
    public void GetMyPdf_IsLimitedByConcurrency()
    {
        var action = typeof(MyBillingController).GetMethod(nameof(MyBillingController.GetPdfAsync))!;

        action.GetCustomAttribute<EnableRateLimitingAttribute>()!.PolicyName.Should().Be(RateLimitPolicies.Concurrent);
    }

    /// <summary>
    /// The action documents every status it answers itself, with the error body on each refusal, so the
    /// generated API description matches what a client can actually receive; the 429 is declared once
    /// on the controller.
    /// </summary>
    [Fact]
    public void GetMyPdf_DeclaresEveryStatus()
    {
        var action = typeof(MyBillingController).GetMethod(nameof(MyBillingController.GetPdfAsync))!;

        var declared = action.GetCustomAttributes<ProducesResponseTypeAttribute>()
            .Select(a => (a.StatusCode, a.Type));

        declared.Should().BeEquivalentTo(new[]
        {
            (StatusCodes.Status200OK, typeof(FileContentResult)),
            (StatusCodes.Status400BadRequest, typeof(ErrorResponse)),
            (StatusCodes.Status401Unauthorized, typeof(ErrorResponse)),
            (StatusCodes.Status403Forbidden, typeof(ErrorResponse)),
            (StatusCodes.Status404NotFound, typeof(ErrorResponse)),
        });
    }

    /// <summary>
    /// The owner's own draft is not theirs to see yet: it answers exactly like an id that names no
    /// invoice, so a client cannot tell that a draft is being prepared.
    /// </summary>
    [Fact]
    public async Task GetMyPdf_RefusesOwnDraftLikeUnknownId()
    {
        var (owner, ownerClientId) = await BillingTestSeeds.RegisterClientAsync(factory, null, "pdf-draft");
        var admin = await BillingTestSeeds.AdminClientAsync(factory);
        var draftId = await BillingTestSeeds.CreateInvoiceAsync(admin, ownerClientId, 10m, isDraft: true);

        var response = await owner.GetAsync($"/api/me/invoices/{draftId}/pdf");
        var absent = await owner.GetAsync($"/api/me/invoices/{BillingTestSeeds.NoSuchId}/pdf");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await BillingTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.INVOICE_NOT_FOUND);
        (await absent.Content.ReadAsStringAsync()).Should().Be(await response.Content.ReadAsStringAsync());
    }

    /// <summary>A stranger's invoice answers exactly like an id that names no invoice.</summary>
    [Fact]
    public async Task GetMyPdf_Stranger_Gets404LikeUnknownId()
    {
        var (_, ownerClientId) = await BillingTestSeeds.RegisterClientAsync(factory, null, "pdf-owner-of-other");
        var admin = await BillingTestSeeds.AdminClientAsync(factory);
        var invoiceId = await BillingTestSeeds.CreateInvoiceAsync(admin, ownerClientId, 10m);
        var stranger = await BillingTestSeeds.RegisterStrangerAsync(factory, null, "pdf-stranger");

        var response = await stranger.GetAsync($"/api/me/invoices/{invoiceId}/pdf");
        var absent = await stranger.GetAsync($"/api/me/invoices/{BillingTestSeeds.NoSuchId}/pdf");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await BillingTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.INVOICE_NOT_FOUND);
        (await absent.Content.ReadAsStringAsync()).Should().Be(await response.Content.ReadAsStringAsync());
    }

    /// <summary>Downloading an invoice is not for anonymous callers.</summary>
    [Fact]
    public async Task GetMyPdf_Anonymous_Returns401()
    {
        var response = await factory.CreateClient().GetAsync("/api/me/invoices/1/pdf");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
