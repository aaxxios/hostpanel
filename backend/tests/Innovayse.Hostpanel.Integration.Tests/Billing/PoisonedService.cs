namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>Names the one service whose suspension save <see cref="PoisonedServiceSaveInterceptor"/> refuses.</summary>
internal sealed class PoisonedService
{
    /// <summary>The poisoned service id.</summary>
    public int ServiceId { get; set; }
}
