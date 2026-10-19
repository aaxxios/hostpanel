using Innovayse.Hostpanel.Domain.Faqs;
using Innovayse.Hostpanel.Domain.Faqs.Enums;
using Innovayse.Hostpanel.Domain.Partners;
using Innovayse.Hostpanel.Domain.Projects;
using Innovayse.Hostpanel.Domain.Settings;
using Innovayse.Hostpanel.Domain.Testimonials;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>
/// Seeds the public storefront's own content for the demo: fictional customer testimonials, partners,
/// showcased projects and frequently asked questions, each in English, Russian and Armenian, plus the hero
/// figures and the office address. Everything is invented — the people, companies and addresses are made up
/// and every link is on a reserved <c>.example</c> domain. Each dataset is added only when its table is empty,
/// through the entity's factory and behaviour methods, with fixed counts, so a second run changes nothing.
/// </summary>
internal sealed class DemoStorefrontSeeder
{
    /// <summary>The locale codes every translated row carries, in the order the triples below are written.</summary>
    private static readonly string[] Locales = ["en", "ru", "hy"];

    /// <summary>The storefront settings the demo sets, by key.</summary>
    private static readonly (string Key, string Value)[] Settings =
    [
        (PortalSettingKeys.HeroBadge, "Hosting, domains and servers in one place"),
        (PortalSettingKeys.StatsClients, "500+"),
        (PortalSettingKeys.StatsUptime, "99.9%"),
        (PortalSettingKeys.StatsRegions, "Europe, North America"),
        (PortalSettingKeys.ContactAddress, "1 Sample Street, Sampletown"),
        (PortalSettingKeys.ContactMapUrl, "https://maps.example/embed?q=sample"),
    ];

    /// <summary>The testimonials: client name, company, rating, then the role and the quote in three languages.</summary>
    private static readonly (string Name, string Company, int Rating, string[] Role, string[] Quote)[] Testimonials =
    [
        ("Alex Morgan", "Northwind Example", 5,
            ["CEO", "Генеральный директор", "Գործադիր տնօրեն"],
            ["Moving our sites here took one evening, and nothing has gone down since.",
             "Перенос наших сайтов занял один вечер, и с тех пор ничего не падало.",
             "Մեր կայքերի տեղափոխումը տևեց մեկ երեկո, և այդ պահից ոչինչ չի խափանվել։"]),
        ("Mira Stone", "Bluebird Studio", 5,
            ["Creative director", "Креативный директор", "Ստեղծագործական տնօրեն"],
            ["Domains, hosting and billing in one account saves us hours every month.",
             "Домены, хостинг и счета в одном аккаунте экономят нам часы каждый месяц.",
             "Դոմեյնները, հոսթինգը և հաշիվները մեկ հաշվում ամեն ամիս խնայում են մեզ ժամեր։"]),
        ("Jon Park", "Harborline Logistics", 4,
            ["Operations manager", "Операционный менеджер", "Օպերացիոն մենեջեր"],
            ["Support answers fast and actually fixes the problem the first time.",
             "Поддержка отвечает быстро и решает проблему с первого раза.",
             "Աջակցությունը արագ է պատասխանում և խնդիրը լուծում է առաջին անգամից։"]),
        ("Sara Lind", "Fjordly Foods", 5,
            ["Founder", "Основатель", "Հիմնադիր"],
            ["Traffic tripled in peak season and the server held without a single interruption.",
             "Трафик утроился в пиковый сезон, а сервер выдержал без единого сбоя.",
             "Տրաֆիկը եռապատկվեց սեզոնի գագաթնակետին, իսկ սերվերը դիմացավ առանց մեկ խափանման։"]),
        ("Omar Reyes", "Granite Advisory", 5,
            ["Managing partner", "Управляющий партнёр", "Գլխավոր գործընկեր"],
            ["Invoices and renewals are clear, so nothing expires unnoticed.",
             "Счета и продления понятны, поэтому ничего не истекает незамеченным.",
             "Հաշիվները և երկարացումները հստակ են, ուստի ոչինչ աննկատ չի լրանում։"]),
        ("Lena Hart", "Emberwood Press", 4,
            ["Head of web", "Руководитель веб-направления", "Վեբ ուղղության ղեկավար"],
            ["A clean control panel and honest prices. Exactly what we wanted.",
             "Понятная панель управления и честные цены. Именно то, что мы искали.",
             "Մաքուր կառավարման վահանակ և ազնիվ գներ։ Հենց այն, ինչ փնտրում էինք։"]),
    ];

    /// <summary>The partners: the name shown, in three languages.</summary>
    private static readonly string[][] Partners =
    [
        ["Alpha Example", "Альфа Пример", "Ալֆա Օրինակ"],
        ["Beacon Labs", "Бикон Лабс", "Բիկոն Լաբս"],
        ["Cobalt Works", "Кобальт Воркс", "Կոբալտ Ուորքս"],
        ["Delta Studio", "Дельта Студия", "Դելտա Ստուդիա"],
        ["Ember Foods", "Эмбер Фудс", "Էմբեր Ֆուդս"],
        ["Fjord Logistics", "Фьорд Логистика", "Ֆյորդ Լոգիստիկա"],
        ["Granite Bank", "Гранит Банк", "Գրանիտ Բանկ"],
        ["Harbor Media", "Харбор Медиа", "Հարբոր Մեդիա"],
    ];

    /// <summary>The projects: slug, then the title and the one-line summary in three languages.</summary>
    private static readonly (string Slug, string[] Title, string[] Summary)[] Projects =
    [
        ("northwind-portal", ["Northwind Portal", "Портал Northwind", "Northwind պորտալ"],
            ["A customer portal with online payments and self-service requests.",
             "Клиентский портал с онлайн-оплатой и самообслуживанием.",
             "Հաճախորդների պորտալ առցանց վճարումներով և ինքնասպասարկմամբ։"]),
        ("bluebird-shop", ["Bluebird Shop", "Магазин Bluebird", "Bluebird խանութ"],
            ["An online store with inventory sync and a fast checkout.",
             "Интернет-магазин с синхронизацией склада и быстрым оформлением заказа.",
             "Առցանց խանութ պահեստի համաժամեցմամբ և արագ վճարմամբ։"]),
        ("harborline-tracking", ["Harborline Tracking", "Трекинг Harborline", "Harborline հետևում"],
            ["A shipment tracking dashboard for dispatchers and clients.",
             "Панель отслеживания грузов для диспетчеров и клиентов.",
             "Բեռների հետևման վահանակ դիսպետչերների և հաճախորդների համար։"]),
        ("fjordly-menu", ["Fjordly Menu", "Меню Fjordly", "Fjordly մենյու"],
            ["A restaurant site with a live menu and table booking.",
             "Сайт ресторана с живым меню и бронированием столиков.",
             "Ռեստորանի կայք կենդանի մենյուով և սեղանների ամրագրմամբ։"]),
        ("granite-library", ["Granite Library", "Библиотека Granite", "Granite գրադարան"],
            ["A searchable document library with access control.",
             "Библиотека документов с поиском и разграничением доступа.",
             "Փաստաթղթերի գրադարան որոնմամբ և հասանելիության վերահսկմամբ։"]),
        ("emberwood-magazine", ["Emberwood Magazine", "Журнал Emberwood", "Emberwood ամսագիր"],
            ["A publishing site with subscriptions and a reader archive.",
             "Издательский сайт с подписками и архивом для читателей.",
             "Հրատարակչական կայք բաժանորդագրություններով և ընթերցողների արխիվով։"]),
    ];

    /// <summary>The questions: category, then the question and the answer in three languages.</summary>
    private static readonly (FaqCategory Category, string[] Question, string[] Answer)[] Faqs =
    [
        (FaqCategory.General, ["What does the platform offer?", "Что предлагает платформа?", "Ի՞նչ է առաջարկում հարթակը"],
            ["Hosting, domains, servers and a client portal for ordering, billing and support.",
             "Хостинг, домены, серверы и клиентский портал для заказов, оплаты и поддержки.",
             "Հոսթինգ, դոմեյններ, սերվերներ և հաճախորդի պորտալ պատվերների, վճարումների և աջակցության համար։"]),
        (FaqCategory.General, ["How do I get started?", "Как начать?", "Ինչպե՞ս սկսել"],
            ["Choose a plan, create an account and pay the first invoice. Most services activate automatically.",
             "Выберите тариф, создайте аккаунт и оплатите первый счёт. Большинство услуг включается автоматически.",
             "Ընտրեք փաթեթ, ստեղծեք հաշիվ և վճարեք առաջին հաշիվը։ Ծառայությունների մեծ մասն ակտիվանում է ինքնաբերաբար։"]),
        (FaqCategory.General, ["Which payment methods do you accept?", "Какие способы оплаты вы принимаете?", "Ի՞նչ վճարման եղանակներ եք ընդունում"],
            ["Cards and the other methods enabled at checkout.",
             "Карты и другие способы, доступные при оформлении заказа.",
             "Քարտեր և վճարման այլ եղանակներ, որոնք հասանելի են պատվերի ժամանակ։"]),
        (FaqCategory.General, ["How does billing work?", "Как устроено выставление счетов?", "Ինչպե՞ս են ձևավորվում հաշիվները"],
            ["Services are billed per cycle. Invoices are issued ahead of the due date and reminders are emailed.",
             "Услуги оплачиваются по циклам. Счета выставляются заранее, напоминания приходят по почте.",
             "Ծառայությունները վճարվում են ցիկլերով։ Հաշիվները ձևավորվում են նախօրոք, հիշեցումները՝ էլ. փոստով։"]),
        (FaqCategory.General, ["How do I contact support?", "Как связаться с поддержкой?", "Ինչպե՞ս կապվել աջակցության հետ"],
            ["Open a ticket from the client portal or use the contact page.",
             "Откройте тикет в клиентском портале или воспользуйтесь страницей контактов.",
             "Բացեք հարցում հաճախորդի պորտալից կամ օգտվեք կապի էջից։"]),
        (FaqCategory.General, ["Can I cancel a service?", "Можно ли отменить услугу?", "Կարո՞ղ եմ չեղարկել ծառայությունը"],
            ["Yes. Request cancellation from the service page and it ends at the close of the paid period.",
             "Да. Запросите отмену на странице услуги, она завершится в конце оплаченного периода.",
             "Այո։ Չեղարկումը պահանջեք ծառայության էջից, այն կավարտվի վճարված ժամանակահատվածի վերջում։"]),
        (FaqCategory.Products, ["Can I move my existing site?", "Можно ли перенести существующий сайт?", "Կարո՞ղ եմ տեղափոխել առկա կայքը"],
            ["Yes. Migration is free on annual plans, including databases and mail.",
             "Да. Перенос бесплатен на годовых тарифах, включая базы данных и почту.",
             "Այո։ Տարեկան փաթեթների դեպքում տեղափոխումն անվճար է՝ տվյալների բազաների և փոստի հետ միասին։"]),
        (FaqCategory.Products, ["How do I register or transfer a domain?", "Как зарегистрировать или перенести домен?", "Ինչպե՞ս գրանցել կամ տեղափոխել դոմեյն"],
            ["Search for the name on the domains page and add it to your cart. To transfer, provide the authorization code.",
             "Найдите имя на странице доменов и добавьте в корзину. Для переноса укажите код авторизации.",
             "Փնտրեք անունը դոմեյնների էջում և ավելացրեք զամբյուղ։ Տեղափոխման համար նշեք թույլտվության կոդը։"]),
        (FaqCategory.Products, ["Is an SSL certificate included?", "Входит ли SSL-сертификат?", "Ներառվա՞ծ է SSL վկայագիրը"],
            ["Yes, a free SSL certificate is included in every hosting plan.",
             "Да, бесплатный SSL-сертификат входит в каждый тариф хостинга.",
             "Այո, անվճար SSL վկայագիրը ներառված է հոսթինգի յուրաքանչյուր փաթեթում։"]),
        (FaqCategory.Products, ["Can I upgrade my plan later?", "Можно ли сменить тариф позже?", "Կարո՞ղ եմ հետո փոխել փաթեթը"],
            ["Yes. Upgrade a service from the client portal at any time.",
             "Да. Повысить тариф можно в клиентском портале в любое время.",
             "Այո։ Ծառայությունը կարելի է թարմացնել հաճախորդի պորտալից ցանկացած պահի։"]),
        (FaqCategory.Products, ["Are backups included?", "Входят ли резервные копии?", "Ներառվա՞ծ են պահուստային պատճենները"],
            ["Daily backups are included in every hosting plan.",
             "Ежедневные резервные копии входят в каждый тариф хостинга.",
             "Օրական պահուստային պատճենները ներառված են հոսթինգի յուրաքանչյուր փաթեթում։"]),
        (FaqCategory.Products, ["Do you offer dedicated servers?", "Есть ли выделенные серверы?", "Առաջարկո՞ւմ եք առանձնացված սերվերներ"],
            ["Yes, virtual and dedicated servers with full root access and monitoring.",
             "Да, виртуальные и выделенные серверы с полным root-доступом и мониторингом.",
             "Այո, վիրտուալ և առանձնացված սերվերներ՝ լիարժեք root հասանելիությամբ և մոնիթորինգով։"]),
    ];

    /// <summary>The data context the storefront rows will be added to and saved through.</summary>
    private readonly AppDbContext _db;

    /// <summary>Creates the seeder.</summary>
    /// <param name="db">The data context the storefront rows will be added to and saved through.</param>
    public DemoStorefrontSeeder(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Seeds the storefront content that is not there yet and sets the hero and office settings.</summary>
    /// <param name="ct">Cancels the save.</param>
    /// <returns>A task that completes once the rows are saved.</returns>
    public async Task SeedAsync(CancellationToken ct)
    {
        await SeedTestimonialsAsync(ct);
        await SeedPartnersAsync(ct);
        await SeedProjectsAsync(ct);
        await SeedFaqsAsync(ct);
        await SeedSettingsAsync(ct);
    }

    /// <summary>Adds the testimonials when there are none.</summary>
    /// <param name="ct">Cancels the save.</param>
    /// <returns>A task that completes once the rows are saved.</returns>
    private async Task SeedTestimonialsAsync(CancellationToken ct)
    {
        if (await _db.Testimonials.AnyAsync(ct))
        {
            return;
        }

        for (var i = 0; i < Testimonials.Length; i++)
        {
            var (name, company, rating, role, quote) = Testimonials[i];
            var row = Testimonial.Create(name, company, null, rating, i, true);
            for (var l = 0; l < Locales.Length; l++)
            {
                row.SetTranslation(Locales[l], role[l], quote[l]);
            }

            _db.Testimonials.Add(row);
        }

        await _db.SaveChangesAsync(ct);
    }

    /// <summary>Adds the partners when there are none; they carry no logo file, only a name and a link.</summary>
    /// <param name="ct">Cancels the save.</param>
    /// <returns>A task that completes once the rows are saved.</returns>
    private async Task SeedPartnersAsync(CancellationToken ct)
    {
        if (await _db.Partners.AnyAsync(ct))
        {
            return;
        }

        for (var i = 0; i < Partners.Length; i++)
        {
            var row = Partner.Create(null, $"https://partner-{i + 1}.example", i, true);
            for (var l = 0; l < Locales.Length; l++)
            {
                row.SetTranslation(Locales[l], Partners[i][l]);
            }

            _db.Partners.Add(row);
        }

        await _db.SaveChangesAsync(ct);
    }

    /// <summary>Adds the projects when there are none; they carry no pictures, only text and a link.</summary>
    /// <param name="ct">Cancels the save.</param>
    /// <returns>A task that completes once the rows are saved.</returns>
    private async Task SeedProjectsAsync(CancellationToken ct)
    {
        if (await _db.Projects.AnyAsync(ct))
        {
            return;
        }

        for (var i = 0; i < Projects.Length; i++)
        {
            var (slug, title, summary) = Projects[i];
            var row = Project.Create(slug, $"https://{slug}.example", [], i, true);
            for (var l = 0; l < Locales.Length; l++)
            {
                row.SetTranslation(Locales[l], title[l], summary[l], null);
            }

            _db.Projects.Add(row);
        }

        await _db.SaveChangesAsync(ct);
    }

    /// <summary>Adds the frequently asked questions when there are none.</summary>
    /// <param name="ct">Cancels the save.</param>
    /// <returns>A task that completes once the rows are saved.</returns>
    private async Task SeedFaqsAsync(CancellationToken ct)
    {
        if (await _db.Faqs.AnyAsync(ct))
        {
            return;
        }

        for (var i = 0; i < Faqs.Length; i++)
        {
            var (category, question, answer) = Faqs[i];
            var row = Faq.Create(category, i, true);
            for (var l = 0; l < Locales.Length; l++)
            {
                row.SetTranslation(Locales[l], question[l], answer[l]);
            }

            _db.Faqs.Add(row);
        }

        await _db.SaveChangesAsync(ct);
    }

    /// <summary>Sets the hero figures and the office address on the rows the shared settings seeder created.</summary>
    /// <param name="ct">Cancels the save.</param>
    /// <returns>A task that completes once the settings are saved.</returns>
    private async Task SeedSettingsAsync(CancellationToken ct)
    {
        var keys = Settings.Select(s => s.Key).ToList();
        var rows = await _db.Settings.Where(s => keys.Contains(s.Key)).ToDictionaryAsync(s => s.Key, ct);

        foreach (var (key, value) in Settings)
        {
            if (rows.TryGetValue(key, out var row) && row.Value != value)
            {
                row.UpdateValue(value);
            }
        }

        await _db.SaveChangesAsync(ct);
    }
}
