using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Notifications;

namespace Innovayse.Hostpanel.Application.Notifications.Seeding;

/// <summary>
/// Seeds the staff alerts of the billing and provisioning jobs, in every supported language, the first
/// time each is needed (the pattern of <see cref="RenewalTemplatesSeeder"/>): a runtime seed lets an
/// operator edit the copy and never overwrites the edit.
/// </summary>
public static class StaffAlertTemplatesSeeder
{
    /// <summary>Slug of the staff alert about an automatic termination that could not remove the hosting account.</summary>
    public const string ServiceTerminationFailedSlug = "service-termination-failed-admin";

    /// <summary>Slug of the staff alert about a gateway payment that stays pending for an unusual time.</summary>
    public const string GatewayPaymentStalledSlug = "gateway-payment-stalled-admin";

    /// <summary>Adds whichever language version of the staff alerts is missing.</summary>
    /// <param name="context">Data context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes once every version exists.</returns>
    public static Task EnsureSeededAsync(IHostpanelDbContext context, CancellationToken ct)
    {
        return EmailTemplateSeeding.AddMissingAsync(
            context,
            [.. ServiceTerminationFailed(), .. GatewayPaymentStalled()],
            ct);
    }

    /// <summary>The staff alert about a termination that could not remove the hosting account, one version per language.</summary>
    /// <returns>The versions.</returns>
    private static IReadOnlyList<EmailTemplate> ServiceTerminationFailed() => EmailTemplateSeeding.Branded(
        ServiceTerminationFailedSlug,
        "Sent to staff when an automatic termination could not remove the hosting account; the service stays as it was and the next run retries it.",
        [
            new SeededEmailVersion("Service termination failed: service #{{ service.id }}", new SeededEmailCopy(
                "en",
                "Service termination failed",
                ["Service #{{ service.id }} could not be terminated automatically, so its hosting account was not removed. The next run retries it. Reason: {{ reason }}"],
                ButtonUrl: null,
                ButtonText: null,
                Note: null,
                EchoButtonUrl: false)),
            new SeededEmailVersion("Не удалось завершить услугу №{{ service.id }}", new SeededEmailCopy(
                "ru",
                "Не удалось завершить услугу",
                ["Услугу №{{ service.id }} не удалось завершить автоматически, поэтому её аккаунт хостинга не удалён. Следующий запуск повторит попытку. Причина: {{ reason }}"],
                ButtonUrl: null,
                ButtonText: null,
                Note: null,
                EchoButtonUrl: false)),
            new SeededEmailVersion("Ծառայության դադարեցումը ձախողվեց՝ №{{ service.id }}", new SeededEmailCopy(
                "hy",
                "Ծառայության դադարեցումը ձախողվեց",
                ["№{{ service.id }} ծառայությունը չհաջողվեց ավտոմատ դադարեցնել, ուստի նրա հոստինգ հաշիվը չհեռացվեց։ Հաջորդ գործարկումը կփորձի կրկին։ Պատճառ՝ {{ reason }}"],
                ButtonUrl: null,
                ButtonText: null,
                Note: null,
                EchoButtonUrl: false)),
        ]);

    /// <summary>The staff alert about a gateway payment pending for an unusual time, one version per language.</summary>
    /// <returns>The versions.</returns>
    private static IReadOnlyList<EmailTemplate> GatewayPaymentStalled() => EmailTemplateSeeding.Branded(
        GatewayPaymentStalledSlug,
        "Sent to staff once per payment session when a gateway payment is still pending after an unusual time.",
        [
            new SeededEmailVersion("Payment still pending: invoice #{{ invoice.id }}", new SeededEmailCopy(
                "en",
                "Payment still pending",
                ["The {{ module }} payment for invoice #{{ invoice.id }} (client #{{ client.id }}) has been pending for {{ pending_hours }} hours. The system keeps asking the gateway until it gives a final answer."],
                ButtonUrl: null,
                ButtonText: null,
                Note: null,
                EchoButtonUrl: false)),
            new SeededEmailVersion("Платёж всё ещё в обработке: счёт №{{ invoice.id }}", new SeededEmailCopy(
                "ru",
                "Платёж всё ещё в обработке",
                ["Платёж {{ module }} по счёту №{{ invoice.id }} (клиент №{{ client.id }}) в обработке уже {{ pending_hours }} ч. Система продолжает опрашивать шлюз, пока он не даст окончательный ответ."],
                ButtonUrl: null,
                ButtonText: null,
                Note: null,
                EchoButtonUrl: false)),
            new SeededEmailVersion("Վճարումը դեռ սպասման մեջ է՝ հաշիվ №{{ invoice.id }}", new SeededEmailCopy(
                "hy",
                "Վճարումը դեռ սպասման մեջ է",
                ["№{{ invoice.id }} հաշվի {{ module }} վճարումը (հաճախորդ №{{ client.id }}) սպասման մեջ է արդեն {{ pending_hours }} ժամ։ Համակարգը շարունակում է հարցում անել դարպասին, մինչև այն վերջնական պատասխան տա։"],
                ButtonUrl: null,
                ButtonText: null,
                Note: null,
                EchoButtonUrl: false)),
        ]);
}
