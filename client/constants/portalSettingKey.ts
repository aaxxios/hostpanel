/**
 * Keys of the operator-managed `portal.*` settings the storefront reads.
 *
 * @module constants/portalSettingKey
 */

/** Setting keys served by the public settings endpoint; kept in step with the backend list. */
export enum PortalSettingKey {
  /** Active storefront template. */
  Template = 'portal.template',
  /** Site name. */
  SiteName = 'portal.site.name',
  /** Site tagline. */
  SiteTagline = 'portal.site.tagline',
  /** Default colour theme. */
  ThemeDefault = 'portal.theme.default',
  /** Whether visitors may toggle the theme. */
  ThemeUserToggle = 'portal.theme.user_toggle',
  /** Logo. */
  Logo = 'portal.logo',
  /** Dark-mode logo. */
  LogoDark = 'portal.logo.dark',
  /** Logo mark. */
  LogoMark = 'portal.logo.mark',
  /** Favicon. */
  Favicon = 'portal.favicon',
  /** Primary brand colour. */
  BrandPrimary = 'portal.brand.primary',
  /** Accent brand colour. */
  BrandAccent = 'portal.brand.accent',
  /** Heading font. */
  BrandFontHeading = 'portal.brand.font_heading',
  /** Body font. */
  BrandFontBody = 'portal.brand.font_body',
  /** Tag manager id. */
  AnalyticsGtmId = 'portal.analytics.gtm_id',
  /** Live chat base URL. */
  ChatBaseUrl = 'portal.chat.base_url',
  /** Live chat website token. */
  ChatWebsiteToken = 'portal.chat.website_token',
  /** Live chat website token, Russian. */
  ChatWebsiteTokenRu = 'portal.chat.website_token.ru',
  /** Live chat website token, Armenian. */
  ChatWebsiteTokenHy = 'portal.chat.website_token.hy',
  /** Live chat provider. */
  ChatProvider = 'portal.chat.provider',
  /** WhatsApp number. */
  ContactWhatsapp = 'portal.contact.whatsapp',
  /** Telegram handle. */
  ContactTelegram = 'portal.contact.telegram',
  /** Contact email. */
  ContactEmail = 'portal.contact.email',
  /** Contact phone. */
  ContactPhone = 'portal.contact.phone',
  /** Office address. */
  ContactAddress = 'portal.contact.address',
  /** Opening hours text. */
  ContactHours = 'portal.contact.hours',
  /** Opening hours timezone. */
  ContactTimezone = 'portal.contact.timezone',
  /** Embedded map address. */
  ContactMapUrl = 'portal.contact.map_url',
  /** Newsletter form action. */
  NewsletterActionUrl = 'portal.newsletter.action_url',
  /** Facebook page. */
  SocialFacebook = 'portal.social.facebook',
  /** Instagram profile. */
  SocialInstagram = 'portal.social.instagram',
  /** Twitter profile. */
  SocialTwitter = 'portal.social.twitter',
  /** LinkedIn page. */
  SocialLinkedin = 'portal.social.linkedin',
  /** YouTube channel. */
  SocialYoutube = 'portal.social.youtube',
  /** Headline figure: clients. */
  StatsClients = 'portal.stats.clients',
  /** Headline figure: uptime. */
  StatsUptime = 'portal.stats.uptime',
  /** Headline figure: regions. */
  StatsRegions = 'portal.stats.regions',
  /** Hero badge line. */
  HeroBadge = 'portal.hero.badge',
  /** Tax id. */
  LegalTaxId = 'portal.legal.tax_id',
  /** Apps switch: all. */
  AppsEnabled = 'portal.apps.enabled',
  /** Apps switch: account. */
  AppsAccount = 'portal.apps.account',
  /** Apps switch: tasks. */
  AppsTasks = 'portal.apps.tasks',
  /** Apps switch: ERP. */
  AppsErp = 'portal.apps.erp',
  /** Apps switch: hostpanel. */
  AppsHostpanel = 'portal.apps.hostpanel',
  /** Apps switch: sheets. */
  AppsSheets = 'portal.apps.sheets',
  /** Apps switch: mail. */
  AppsMail = 'portal.apps.mail',
  /** Apps switch: docs. */
  AppsDocs = 'portal.apps.docs',
  /** Apps switch: calendar. */
  AppsCalendar = 'portal.apps.calendar',
  /** Backups feature switch. */
  FeaturesBackups = 'portal.features.backups',
  /** Storage feature switch. */
  FeaturesStorage = 'portal.features.storage',
  /** Caching feature switch. */
  FeaturesCaching = 'portal.features.caching',
  /** CDN feature switch. */
  FeaturesCdn = 'portal.features.cdn',
  /** Firewall feature switch. */
  FeaturesFirewall = 'portal.features.firewall',
  /** Malware scanning feature switch. */
  FeaturesMalware = 'portal.features.malware',
  /** Free migration offer switch. */
  MigrationFree = 'portal.migration.free',
  /** Company story on the About page, default language. */
  AboutText = 'portal.about.text',
  /** Company story for Russian visitors. */
  AboutTextRu = 'portal.about.text.ru',
  /** Company story for Armenian visitors. */
  AboutTextHy = 'portal.about.text.hy',
}
