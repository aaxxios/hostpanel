namespace Innovayse.Hostpanel.Application.Migration.Common;

/// <summary>Where a migration job's legacy source listens and the key it accepts; every call to the source carries both.</summary>
/// <param name="Url">The address the source answers on, as configured on the job.</param>
/// <param name="AccessKey">The job's shared key; the source refuses a call that carries any other.</param>
public sealed record MigrationSourceEndpoint(string Url, string AccessKey);
