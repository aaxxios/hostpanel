namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>The subject, opening message and handling department of a demo ticket.</summary>
/// <param name="Subject">The ticket subject.</param>
/// <param name="Message">The opening message.</param>
/// <param name="Department">The name of the handling department.</param>
internal sealed record DemoTicketTopic(string Subject, string Message, string Department);
