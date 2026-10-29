namespace Innovayse.Hostpanel.Infrastructure.Email.Settings;

/// <summary>
/// The sender identity stamped into the <c>From</c> header of every notification this product
/// sends. Bound from the "Smtp:From" subsection, which a deployment fills as
/// <c>Smtp__From__Address</c> and <c>Smtp__From__Name</c>.
/// </summary>
/// <remarks>
/// A nested subsection rather than a flat pair of keys, because the two values are one thing,
/// an address and the name shown beside it, and
/// neither is meaningful without the other.
/// </remarks>
public sealed class SmtpFromOptions
{
    /// <summary>
    /// Gets the sender email address. Carries no default: any built-in address would be the wrong
    /// one for whoever runs this product.
    /// </summary>
    public required string Address { get; init; }

    /// <summary>
    /// Gets the sender display name shown beside <see cref="Address"/> in a mail client. A
    /// human-readable brand name, not an address.
    /// </summary>
    public required string Name { get; init; }
}
