using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Support.Common;

namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>
/// Chat notification for contact-form enquiries on the public demo: does nothing. The enquiry's mail
/// still goes out through the configured mail relay; only the third-party chat channel is cut.
/// </summary>
public sealed class DemoContactNotifier : IContactNotifier
{
    /// <inheritdoc/>
    public Task NotifyAsync(ContactEnquiry enquiry, CancellationToken ct) => Task.CompletedTask;
}
