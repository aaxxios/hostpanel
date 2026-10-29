using Innovayse.Hostpanel.Domain.Faqs.Enums;

namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>A demo FAQ entry; the translated fields carry one entry per demo locale.</summary>
/// <param name="Category">The FAQ category.</param>
/// <param name="Question">The question, per locale.</param>
/// <param name="Answer">The answer, per locale.</param>
internal sealed record DemoFaq(FaqCategory Category, string[] Question, string[] Answer);
