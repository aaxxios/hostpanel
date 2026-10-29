using Innovayse.Hostpanel.Domain.Faqs.Enums;

namespace Innovayse.Hostpanel.Application.Faqs.Common;

/// <summary>DTO representing a question for the storefront, with the translation flattened for the requested locale.</summary>
/// <param name="Id">Question identifier.</param>
/// <param name="Category">The topic the question belongs to; serialized as its name.</param>
/// <param name="Question">The question in the resolved locale.</param>
/// <param name="Answer">The answer in the resolved locale.</param>
/// <param name="SortOrder">Display order (ascending).</param>
public sealed record FaqPublicDto(
    int Id,
    FaqCategory Category,
    string Question,
    string Answer,
    int SortOrder);
