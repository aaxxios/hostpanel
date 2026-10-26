using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// <c>GET /api/billing/{id}/pdf</c> against Postgres: staff download any invoice as a PDF attachment,
/// drafts included and stamped, and the internal notes never appear on it.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetInvoicePdfTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>An admin downloads the invoice as a PDF attachment named after it.</summary>
    [Fact]
    public async Task GetPdf_ReturnsPdfAttachmentForAdmin()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);
        var clientId = await BillingTestSeeds.SeedClientAsync(factory);
        var invoiceId = await BillingTestSeeds.CreateInvoiceAsync(admin, clientId, 12.50m);

        var response = await admin.GetAsync($"/api/billing/{invoiceId}/pdf");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
        response.Content.Headers.ContentDisposition!.DispositionType.Should().Be("attachment");
        response.Content.Headers.ContentDisposition.FileName.Should().Be($"invoice-{invoiceId}.pdf");
        var bytes = await response.Content.ReadAsByteArrayAsync();
        System.Text.Encoding.ASCII.GetString(bytes, 0, 5).Should().Be("%PDF-");
        var text = await PdfText.CompactAsync(response);
        text.Should().Contain($"Invoice#{invoiceId}");
        text.Should().Contain("Hosting");
        text.Should().Contain("AdaLovelace");
    }

    /// <summary>An id that names no invoice answers 404 with the <c>INVOICE_NOT_FOUND</c> code.</summary>
    [Fact]
    public async Task GetPdf_UnknownId_Returns404()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);

        var response = await admin.GetAsync($"/api/billing/{BillingTestSeeds.NoSuchId}/pdf");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await BillingTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.INVOICE_NOT_FOUND);
    }

    /// <summary>Downloading an invoice is not for anonymous callers.</summary>
    [Fact]
    public async Task GetPdf_Anonymous_Returns401()
    {
        var response = await factory.CreateClient().GetAsync("/api/billing/1/pdf");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A client cannot use the staff route, even for their own invoice.</summary>
    [Fact]
    public async Task GetPdf_ClientToken_Returns403()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);
        var (client, clientId) = await BillingTestSeeds.RegisterClientAsync(factory, null, "pdf-staff-route");
        var invoiceId = await BillingTestSeeds.CreateInvoiceAsync(admin, clientId, 12.50m);

        var response = await client.GetAsync($"/api/billing/{invoiceId}/pdf");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>Staff can download a draft, and it is stamped so nobody mistakes it for an issued invoice.</summary>
    [Fact]
    public async Task GetPdf_DraftForAdmin_IsStamped()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);
        var clientId = await BillingTestSeeds.SeedClientAsync(factory);
        var invoiceId = await BillingTestSeeds.CreateInvoiceAsync(admin, clientId, 12.50m, isDraft: true);

        var response = await admin.GetAsync($"/api/billing/{invoiceId}/pdf");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await PdfText.CompactAsync(response)).Should().Contain("DRAFT");
    }

    /// <summary>The admins' internal notes stay off the PDF, which gets forwarded to customers.</summary>
    [Fact]
    public async Task GetPdf_LeavesOutInternalNotes()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);
        var clientId = await BillingTestSeeds.SeedClientAsync(factory);
        var invoiceId = await BillingTestSeeds.CreateInvoiceAsync(admin, clientId, 12.50m);
        var notes = await admin.PutAsJsonAsync($"/api/billing/{invoiceId}/notes", new { invoiceId, notes = "INTERNAL-ONLY-7731" });
        notes.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var response = await admin.GetAsync($"/api/billing/{invoiceId}/pdf");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await PdfText.CompactAsync(response)).Should().NotContain("INTERNAL-ONLY-7731");
    }

    /// <summary>An invoice in a currency with no row still renders, with the code written after each amount.</summary>
    [Fact]
    public async Task GetInvoicePdf_FallsBackWhenCurrencyRowMissing()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);
        var clientId = await BillingTestSeeds.SeedClientAsync(factory);
        var invoiceId = await BillingTestSeeds.CreateInvoiceAsync(admin, clientId, 12.50m);
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
            await context.Invoices
                .Where(i => i.Id == invoiceId)
                .ExecuteUpdateAsync(s => s.SetProperty(i => i.Currency, "XTS"));
        }

        var response = await admin.GetAsync($"/api/billing/{invoiceId}/pdf");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await PdfText.CompactAsync(response)).Should().Contain("12.50XTS");
    }
}
