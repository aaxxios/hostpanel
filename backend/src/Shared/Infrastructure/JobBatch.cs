namespace Innovayse.Hostpanel.Shared.Infrastructure;

/// <summary>The size of the batches a scheduled job reads its work in, so no pass loads a whole table.</summary>
public static class JobBatch
{
    /// <summary>How many rows (or ids) one batch of a scheduled job reads.</summary>
    public const int Size = 200;
}
