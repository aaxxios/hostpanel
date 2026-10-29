using Innovayse.Hostpanel.Domain.Clients;

namespace Innovayse.Hostpanel.Application.Clients.Common;

/// <summary>
/// The outcome of writing a client record for a user at most once.
/// </summary>
/// <param name="Client">The saved candidate, or the record another writer saved first for the same user.</param>
/// <param name="Created">
/// <see langword="true"/> when this write saved the candidate; <see langword="false"/> when the
/// record already existed and <paramref name="Client"/> is that existing one.
/// </param>
public sealed record ClientRecordWriteResult(Client Client, bool Created);
