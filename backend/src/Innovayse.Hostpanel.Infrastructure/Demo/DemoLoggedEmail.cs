namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>A sent email the demo email log shows.</summary>
/// <param name="Subject">The subject line.</param>
/// <param name="Body">The message body.</param>
internal sealed record DemoLoggedEmail(string Subject, string Body);
