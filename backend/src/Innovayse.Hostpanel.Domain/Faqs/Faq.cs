using Innovayse.Hostpanel.Domain.Faqs.Enums;
using Innovayse.Hostpanel.SharedKernel.Domain;
using Innovayse.Hostpanel.SharedKernel.Domain.Interfaces;

namespace Innovayse.Hostpanel.Domain.Faqs;

/// <summary>
/// A frequently asked question shown on the storefront, with per-locale translations.
/// Stored in the <c>faqs</c> table.
/// </summary>
public sealed class Faq : BaseEntity, IAggregateRoot
{
    /// <summary>Gets the topic the question belongs to.</summary>
    public FaqCategory Category { get; private set; }

    /// <summary>Gets the display order (ascending).</summary>
    public int SortOrder { get; private set; }

    /// <summary>Gets whether the question is currently shown on the storefront.</summary>
    public bool IsActive { get; private set; }

    /// <summary>Gets the UTC creation timestamp.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Backing field for translations.</summary>
    private readonly List<FaqTranslation> _translations = [];

    /// <summary>Gets the per-locale translations.</summary>
    public IReadOnlyList<FaqTranslation> Translations => _translations.AsReadOnly();

    /// <summary>EF Core parameterless constructor — do not call directly.</summary>
    private Faq() { }

    /// <summary>
    /// Creates a new question with no translations yet.
    /// </summary>
    /// <param name="category">The topic the question belongs to.</param>
    /// <param name="sortOrder">Display order (ascending).</param>
    /// <param name="isActive">Whether the question is shown on the storefront.</param>
    /// <returns>A new <see cref="Faq"/>.</returns>
    /// <exception cref="DomainRuleException">The sort order is negative.</exception>
    public static Faq Create(FaqCategory category, int sortOrder, bool isActive)
    {
        EnsureSortOrder(sortOrder);

        return new Faq
        {
            Category = category,
            SortOrder = sortOrder,
            IsActive = isActive,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Updates every non-translation field.
    /// </summary>
    /// <param name="category">The topic the question belongs to.</param>
    /// <param name="sortOrder">New display order.</param>
    /// <param name="isActive">New active state.</param>
    /// <exception cref="DomainRuleException">The sort order is negative.</exception>
    public void Update(FaqCategory category, int sortOrder, bool isActive)
    {
        EnsureSortOrder(sortOrder);

        Category = category;
        SortOrder = sortOrder;
        IsActive = isActive;
    }

    /// <summary>Shows the question on the storefront.</summary>
    public void Activate() => IsActive = true;

    /// <summary>Hides the question from the storefront.</summary>
    public void Deactivate() => IsActive = false;

    /// <summary>Moves the question to a new position in the display order.</summary>
    /// <param name="sortOrder">The new display order (ascending).</param>
    /// <exception cref="DomainRuleException">The sort order is negative.</exception>
    public void SetSortOrder(int sortOrder)
    {
        EnsureSortOrder(sortOrder);
        SortOrder = sortOrder;
    }

    /// <summary>
    /// Adds or updates the translation for the given locale.
    /// </summary>
    /// <param name="locale">BCP-47 locale code.</param>
    /// <param name="question">The question in this locale.</param>
    /// <param name="answer">The answer in this locale.</param>
    public void SetTranslation(string locale, string question, string answer)
    {
        var existing = _translations.FirstOrDefault(t => t.Locale == locale);
        if (existing is not null)
        {
            existing.Update(question, answer);
        }
        else
        {
            _translations.Add(FaqTranslation.Create(locale, question, answer));
        }
    }

    /// <summary>
    /// Removes the translation for the given locale, if there is one.
    /// </summary>
    /// <param name="locale">BCP-47 locale code of the translation to remove.</param>
    public void RemoveTranslation(string locale)
    {
        var existing = _translations.FirstOrDefault(t => t.Locale == locale);
        if (existing is not null)
        {
            _translations.Remove(existing);
        }
    }

    /// <summary>Refuses a negative sort order.</summary>
    /// <param name="sortOrder">The display order to check.</param>
    /// <exception cref="DomainRuleException">The sort order is negative.</exception>
    private static void EnsureSortOrder(int sortOrder)
    {
        if (sortOrder < 0)
        {
            throw new DomainRuleException(FaqErrors.SortOrderNegative, "A sort order cannot be negative.");
        }
    }
}
