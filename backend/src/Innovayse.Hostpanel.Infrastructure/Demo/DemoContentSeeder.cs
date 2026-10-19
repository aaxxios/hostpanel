using System.Text.Encodings.Web;
using System.Text.Json;
using Innovayse.Hostpanel.Domain.Slides;
using Innovayse.Hostpanel.Domain.Slides.Enums;
using Innovayse.Hostpanel.Domain.Support;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>
/// Seeds the storefront and client-area content: homepage slides with English, Russian and Armenian text,
/// and the downloads library with its categories. Everything is built through the entities' factories and
/// behaviour methods, added only when its table is empty.
/// </summary>
internal sealed class DemoContentSeeder
{
    /// <summary>Writes the feature list as JSON without turning Armenian and Russian letters into escapes.</summary>
    private static readonly JsonSerializerOptions FeatureJson = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    /// <summary>The call-to-action text per locale, in the order en, ru, hy.</summary>
    private static readonly string[] CallToAction = ["Get Started", "Начать", "Սկսել"];

    /// <summary>The locales every slide is translated into, in the order the tables below use.</summary>
    private static readonly string[] Locales = ["en", "ru", "hy"];

    /// <summary>The data context the rows are added to and saved through.</summary>
    private readonly AppDbContext _db;

    /// <summary>The anchor dates are measured from.</summary>
    private readonly DateTimeOffset _today;

    /// <summary>Creates the seeder.</summary>
    /// <param name="db">The data context the rows are added to and saved through.</param>
    /// <param name="today">The anchor dates are measured from.</param>
    public DemoContentSeeder(AppDbContext db, DateTimeOffset today)
    {
        _db = db;
        _today = today;
    }

    /// <summary>Seeds the slides and downloads that are not there yet.</summary>
    /// <param name="ct">Cancels the save.</param>
    /// <returns>A task that completes once the rows are saved.</returns>
    public async Task SeedAsync(CancellationToken ct)
    {
        if (!await _db.Slides.AnyAsync(ct))
        {
            await SeedSlidesAsync(ct);
        }

        if (!await _db.DownloadCategories.AnyAsync(ct))
        {
            await SeedDownloadsAsync(ct);
        }
    }

    /// <summary>Seeds seven slides, six live (one only for visitors who are not signed in, one with a visibility window) and one switched off.</summary>
    /// <param name="ct">Cancels the save.</param>
    /// <returns>A task that completes once the rows are saved.</returns>
    private async Task SeedSlidesAsync(CancellationToken ct)
    {
        // The image is left empty on purpose: the storefront hides a slide's picture when there is none, and the
        // demo ships no stock photos that could be mistaken for real customers' material.
        (string Icon, string Color, string Image, string LearnMore, bool Active, SlideAudience Audience, bool Windowed, (string Title, string Tagline, string Description, string[] Features)[] Text)[] slides =
        [
            ("cloud-check", "#0ea5e9", "", "/hosting", true, SlideAudience.All, false,
            [
                ("Web Hosting", "Fast and reliable hosting", "Launch your website on high-performance servers with free SSL and daily backups.", ["Free SSL certificate", "99.9% uptime", "Daily backups", "24/7 support"]),
                ("Веб-хостинг", "Быстрый и надёжный хостинг", "Разместите сайт на производительных серверах с бесплатным SSL и ежедневными резервными копиями.", ["Бесплатный SSL", "99,9% аптайм", "Ежедневные копии", "Поддержка 24/7"]),
                ("Վեբ հոսթինգ", "Արագ և հուսալի հոսթինգ", "Տեղադրեք ձեր կայքը բարձր արտադրողականությամբ սերվերների վրա՝ անվճար SSL-ով և ամենօրյա պահուստավորմամբ։", ["Անվճար SSL", "99,9% աշխատանք", "Ամենօրյա պահուստ", "Աջակցություն 24/7"]),
            ]),
            ("earth", "#3b82f6", "", "/domains", true, SlideAudience.All, false,
            [
                ("Domain Registration", "Find and register your domain", "Search and register your domain name at competitive prices with instant activation.", ["Free DNS management", "WHOIS privacy", "Auto-renewal"]),
                ("Регистрация доменов", "Найдите и зарегистрируйте домен", "Ищите и регистрируйте домен по конкурентным ценам с мгновенной активацией.", ["Бесплатное управление DNS", "Скрытие данных WHOIS", "Автопродление"]),
                ("Դոմեյնների գրանցում", "Գտեք և գրանցեք ձեր դոմեյնը", "Որոնեք և գրանցեք ձեր դոմեյնը մրցունակ գներով՝ անմիջական ակտիվացմամբ։", ["Անվճար DNS կառավարում", "WHOIS գաղտնիություն", "Ավտոմատ երկարաձգում"]),
            ]),
            ("server", "#6366f1", "", "/vps", true, SlideAudience.All, false,
            [
                ("Cloud VPS", "Full control, instant setup", "Isolated virtual servers with root access and NVMe storage, ready in minutes.", ["Root access", "NVMe storage", "Instant setup", "Snapshots"]),
                ("Облачный VPS", "Полный контроль, мгновенный запуск", "Изолированные виртуальные серверы с root-доступом и NVMe-накопителями, готовые за минуты.", ["Root-доступ", "NVMe-диски", "Мгновенный запуск", "Снимки"]),
                ("Ամպային VPS", "Լիարժեք վերահսկողություն", "Մեկուսացված վիրտուալ սերվերներ root հասանելիությամբ և NVMe կրիչներով՝ պատրաստ րոպեների ընթացքում։", ["Root հասանելիություն", "NVMe կրիչ", "Արագ գործարկում", "Պատկերներ"]),
            ]),
            ("shield-check", "#10b981", "", "/ssl", true, SlideAudience.Guest, false,
            [
                ("SSL Certificates", "Protect your site", "Certificates that encrypt the connection and build trust with your visitors.", ["Domain and organisation validation", "Wildcard option", "Warranty included"]),
                ("SSL-сертификаты", "Защитите свой сайт", "Сертификаты, которые шифруют соединение и повышают доверие посетителей.", ["Проверка домена и организации", "Wildcard-вариант", "Гарантия включена"]),
                ("SSL վկայագրեր", "Պաշտպանեք ձեր կայքը", "Վկայագրեր, որոնք գաղտնագրում են կապը և մեծացնում այցելուների վստահությունը։", ["Դոմեյնի և կազմակերպության ստուգում", "Wildcard տարբերակ", "Երաշխիք"]),
            ]),
            ("email", "#f59e0b", "", "/email", true, SlideAudience.All, true,
            [
                ("Business Email", "Mail on your own domain", "Professional mailboxes with spam protection and shared calendars.", ["Spam filter", "Shared calendars", "Mobile sync"]),
                ("Деловая почта", "Почта на вашем домене", "Профессиональные почтовые ящики с защитой от спама и общими календарями.", ["Защита от спама", "Общие календари", "Синхронизация с телефоном"]),
                ("Բիզնես փոստ", "Փոստ ձեր դոմեյնում", "Մասնագիտական փոստարկղեր սպամի պաշտպանությամբ և ընդհանուր օրացույցներով։", ["Սպամի զտիչ", "Ընդհանուր օրացույցներ", "Համաժամացում հեռախոսի հետ"]),
            ]),
            ("server-network", "#ef4444", "", "/dedicated", true, SlideAudience.All, false,
            [
                ("Dedicated Servers", "Maximum power", "Single-tenant hardware for demanding workloads, with no noisy neighbours.", ["Up to 24 cores", "NVMe arrays", "Private network"]),
                ("Выделенные серверы", "Максимальная мощность", "Отдельное оборудование для требовательных проектов без соседей.", ["До 24 ядер", "Массивы NVMe", "Частная сеть"]),
                ("Նվիրված սերվերներ", "Առավելագույն հզորություն", "Առանձին սարքավորում պահանջկոտ ծրագրերի համար՝ առանց այլ օգտատերերի հետ կիսելու։", ["Մինչև 24 միջուկ", "NVMe զանգվածներ", "Մասնավոր ցանց"]),
            ]),
            ("tag", "#8b5cf6", "", "/offers", false, SlideAudience.All, false,
            [
                ("Seasonal Offer", "Switched off", "A seasonal offer that is prepared but not shown to visitors yet.", ["Prepared in advance"]),
                ("Сезонное предложение", "Отключено", "Сезонное предложение, которое подготовлено, но пока не показывается посетителям.", ["Подготовлено заранее"]),
                ("Սեզոնային առաջարկ", "Անջատված է", "Սեզոնային առաջարկ, որը պատրաստ է, բայց դեռ չի ցուցադրվում այցելուներին։", ["Նախապես պատրաստված"]),
            ]),
        ];

        for (var sortOrder = 0; sortOrder < slides.Length; sortOrder++)
        {
            var (icon, color, image, learnMore, active, audience, windowed, text) = slides[sortOrder];
            var slide = Slide.Create(
                icon, color, image, null, learnMore, null, sortOrder, active, audience,
                windowed ? _today.AddDays(-30) : null, windowed ? _today.AddDays(120) : null);

            for (var l = 0; l < Locales.Length; l++)
            {
                var (title, tagline, description, features) = text[l];
                slide.SetTranslation(
                    Locales[l], title, tagline, description, JsonSerializer.Serialize(features, FeatureJson), CallToAction[l], learnMore);
            }

            _db.Slides.Add(slide);
        }

        await _db.SaveChangesAsync(ct);
    }

    /// <summary>Seeds download categories (one nested, one hidden) and nine downloads with download counts.</summary>
    /// <param name="ct">Cancels the save.</param>
    /// <returns>A task that completes once the rows are saved.</returns>
    private async Task SeedDownloadsAsync(CancellationToken ct)
    {
        var guides = DownloadCategory.Create("Guides", "Step-by-step manuals for our services.", false);
        var software = DownloadCategory.Create("Client Software", "Tools that make working with your services easier.", false);
        var internalDocs = DownloadCategory.Create("Internal Documents", "Documents kept for staff only.", true);
        _db.DownloadCategories.Add(guides);
        _db.DownloadCategories.Add(software);
        _db.DownloadCategories.Add(internalDocs);
        await _db.SaveChangesAsync(ct);

        var mail = DownloadCategory.Create("Mail Clients", "Ready-made settings for popular mail programs.", false, software.Id);
        _db.DownloadCategories.Add(mail);
        await _db.SaveChangesAsync(ct);

        (string Title, string Description, string Type, string File, int Category, bool ClientsOnly, bool Hidden, int Downloads)[] downloads =
        [
            ("Getting started guide", "A short tour of the client area and your first service.", "PDF Document", "getting-started.pdf", guides.Id, false, false, 128),
            ("Hosting control panel manual", "How to manage sites, databases and backups.", "PDF Document", "hosting-panel-manual.pdf", guides.Id, true, false, 76),
            ("DNS records cheat sheet", "What each record type does, with examples.", "PDF Document", "dns-cheat-sheet.pdf", guides.Id, false, false, 214),
            ("VPS security checklist", "Twelve steps to harden a new virtual server.", "PDF Document", "vps-security-checklist.pdf", guides.Id, true, false, 53),
            ("File transfer client settings", "Connection profile for the common transfer tools.", "ZIP File", "transfer-profile.zip", software.Id, true, false, 91),
            ("Mail client setup pack", "Account settings for desktop and mobile mail programs.", "ZIP File", "mail-setup-pack.zip", mail.Id, false, false, 167),
            ("Command line toolkit", "Scripts for backups and log rotation on your server.", "ZIP File", "cli-toolkit.zip", software.Id, true, false, 38),
            ("Service level agreement", "The uptime and support commitments in full.", "PDF Document", "service-level-agreement.pdf", guides.Id, false, false, 245),
            ("Staff onboarding notes", "Internal notes, not shown to customers.", "PDF Document", "staff-onboarding.pdf", internalDocs.Id, true, true, 4),
        ];

        foreach (var (title, description, type, file, category, clientsOnly, hidden, count) in downloads)
        {
            var download = Download.Create(title, description, type, file, category, clientsOnly, false, hidden);
            for (var i = 0; i < count; i++)
            {
                download.IncrementDownloadCount();
            }

            _db.Downloads.Add(download);
        }

        await _db.SaveChangesAsync(ct);
    }
}
