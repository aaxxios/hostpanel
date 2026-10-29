namespace Innovayse.Hostpanel.Application.Clients.Common;

/// <summary>
/// A section of a client data export that has no natural ceiling and so carries at most
/// <paramref name="Limit"/> newest rows. Whether rows were left out is stated in the payload, never
/// left for the reader to guess from the length.
/// </summary>
/// <typeparam name="T">The row type of the section.</typeparam>
/// <param name="Items">The newest rows, at most <paramref name="Limit"/>.</param>
/// <param name="Truncated"><see langword="true"/> when older rows exist that this section does not carry.</param>
/// <param name="Limit">The most rows this section carries.</param>
public sealed record ClientExportLimitedDto<T>(
    IReadOnlyList<T> Items,
    bool Truncated,
    int Limit);
