using System.Text;

namespace Innovayse.Providers.Inecobank;

/// <summary>Connection settings for the Inecobank payment gateway REST API.</summary>
/// <param name="BaseUrl">Gateway base URL, e.g. https://pg.inecoecom.am (no trailing path).</param>
/// <param name="UserName">Merchant API login.</param>
/// <param name="Password">Merchant API password.</param>
public sealed record InecobankClientOptions(string BaseUrl, string UserName, string Password)
{
    /// <summary>Prints the members a log line may carry; the password is never written out.</summary>
    /// <param name="builder">The builder the record's <c>ToString</c> fills.</param>
    /// <returns>Always true, since members were written.</returns>
    private bool PrintMembers(StringBuilder builder)
    {
        builder.Append("BaseUrl = ").Append(BaseUrl).Append(", UserName = ").Append(UserName);
        return true;
    }
}
