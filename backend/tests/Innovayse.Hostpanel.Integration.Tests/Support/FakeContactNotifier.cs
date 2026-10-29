using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Support.Common;

namespace Innovayse.Hostpanel.Integration.Tests.Support;

/// <summary>A fake chat channel: records what it was asked to post, and can be made to fail.</summary>
internal sealed class FakeContactNotifier : IContactNotifier
{
    /// <summary>Every enquiry this fake was asked to post.</summary>
    public List<ContactEnquiry> Notified { get; } = [];

    /// <summary>When set, every call throws instead of recording the enquiry.</summary>
    public bool ThrowOnNotify { get; init; }

    /// <inheritdoc/>
    public Task NotifyAsync(ContactEnquiry enquiry, CancellationToken ct)
    {
        if (ThrowOnNotify)
        {
            throw new InvalidOperationException("The fake chat channel was told to fail this post.");
        }

        Notified.Add(enquiry);
        return Task.CompletedTask;
    }
}
