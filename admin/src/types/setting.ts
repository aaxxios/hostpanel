
/** Represents a system setting key-value pair. */
export interface Setting {
  /** Unique setting identifier. */
  id: number
  /** Setting key. */
  key: string
  /** Setting value. */
  value: string
  /** Optional human-readable description. */
  description?: string
}

/** Keys of the storefront settings the admin cards edit; the strings are the rows' `key` values. */
export const SettingKey = {
  /** The name the storefront calls itself. */
  SiteName: 'portal.site.name',
  /** One-line tagline, the default page description. */
  SiteTagline: 'portal.site.tagline',
  /** Clients figure in the hero stats. */
  StatsClients: 'portal.stats.clients',
  /** Uptime figure in the hero stats. */
  StatsUptime: 'portal.stats.uptime',
  /** Regions figure in the hero stats. */
  StatsRegions: 'portal.stats.regions',
  /** Short badge line above the hero heading. */
  HeroBadge: 'portal.hero.badge',
  /** Contact email. */
  ContactEmail: 'portal.contact.email',
  /** Contact phone. */
  ContactPhone: 'portal.contact.phone',
  /** WhatsApp number. */
  ContactWhatsapp: 'portal.contact.whatsapp',
  /** Telegram handle. */
  ContactTelegram: 'portal.contact.telegram',
  /** Postal address. */
  ContactAddress: 'portal.contact.address',
  /** Link to the address on a map. */
  ContactMapUrl: 'portal.contact.map_url',
  /** The storefront template. */
  Template: 'portal.template',
  /** Default colour mode of the storefront. */
  ThemeDefault: 'portal.theme.default',
  /** Whether visitors may switch the colour mode. */
  ThemeUserToggle: 'portal.theme.user_toggle',
  /** Logo drawn on light backgrounds. */
  Logo: 'portal.logo',
  /** Logo drawn on dark backgrounds. */
  LogoDark: 'portal.logo.dark',
  /** Square logo mark. */
  LogoMark: 'portal.logo.mark',
  /** Browser tab icon. */
  Favicon: 'portal.favicon',
  /** Primary brand colour. */
  BrandPrimary: 'portal.brand.primary',
  /** Accent brand colour. */
  BrandAccent: 'portal.brand.accent',
  /** Heading font. */
  BrandFontHeading: 'portal.brand.font_heading',
  /** Body font. */
  BrandFontBody: 'portal.brand.font_body',
  /** Tag manager container id. */
  AnalyticsGtmId: 'portal.analytics.gtm_id',
  /** Chat widget provider. */
  ChatProvider: 'portal.chat.provider',
  /** Chat server address. */
  ChatBaseUrl: 'portal.chat.base_url',
  /** Chat website token. */
  ChatWebsiteToken: 'portal.chat.website_token',
  /** Chat website token for Russian. */
  ChatWebsiteTokenRu: 'portal.chat.website_token.ru',
  /** Chat website token for Armenian. */
  ChatWebsiteTokenHy: 'portal.chat.website_token.hy',
} as const

/** One of the {@link SettingKey} values. */
export type SettingKey = (typeof SettingKey)[keyof typeof SettingKey]

/** Prefix of the environment variable a portal setting falls back to until its row exists. */
const SETTING_ENV_PREFIX = 'NUXT_PUBLIC_PORTAL_'

/** Environment variable the storefront falls back to for each setting, shown while the row is missing. */
export const SettingEnv: Record<SettingKey, string> = {
  [SettingKey.SiteName]: `${SETTING_ENV_PREFIX}SITE_NAME`,
  [SettingKey.SiteTagline]: `${SETTING_ENV_PREFIX}SITE_TAGLINE`,
  [SettingKey.StatsClients]: `${SETTING_ENV_PREFIX}STATS_CLIENTS`,
  [SettingKey.StatsUptime]: `${SETTING_ENV_PREFIX}STATS_UPTIME`,
  [SettingKey.StatsRegions]: `${SETTING_ENV_PREFIX}STATS_REGIONS`,
  [SettingKey.HeroBadge]: `${SETTING_ENV_PREFIX}HERO_BADGE`,
  [SettingKey.ContactEmail]: `${SETTING_ENV_PREFIX}CONTACT_EMAIL`,
  [SettingKey.ContactPhone]: `${SETTING_ENV_PREFIX}CONTACT_PHONE`,
  [SettingKey.ContactWhatsapp]: `${SETTING_ENV_PREFIX}CONTACT_WHATSAPP`,
  [SettingKey.ContactTelegram]: `${SETTING_ENV_PREFIX}CONTACT_TELEGRAM`,
  [SettingKey.ContactAddress]: `${SETTING_ENV_PREFIX}CONTACT_ADDRESS`,
  [SettingKey.ContactMapUrl]: `${SETTING_ENV_PREFIX}CONTACT_MAP_URL`,
  [SettingKey.Template]: `${SETTING_ENV_PREFIX}TEMPLATE`,
  [SettingKey.ThemeDefault]: `${SETTING_ENV_PREFIX}THEME_DEFAULT`,
  [SettingKey.ThemeUserToggle]: `${SETTING_ENV_PREFIX}THEME_USER_TOGGLE`,
  [SettingKey.Logo]: `${SETTING_ENV_PREFIX}LOGO`,
  [SettingKey.LogoDark]: `${SETTING_ENV_PREFIX}LOGO_DARK`,
  [SettingKey.LogoMark]: `${SETTING_ENV_PREFIX}LOGO_MARK`,
  [SettingKey.Favicon]: `${SETTING_ENV_PREFIX}FAVICON`,
  [SettingKey.BrandPrimary]: `${SETTING_ENV_PREFIX}BRAND_PRIMARY`,
  [SettingKey.BrandAccent]: `${SETTING_ENV_PREFIX}BRAND_ACCENT`,
  [SettingKey.BrandFontHeading]: `${SETTING_ENV_PREFIX}BRAND_FONT_HEADING`,
  [SettingKey.BrandFontBody]: `${SETTING_ENV_PREFIX}BRAND_FONT_BODY`,
  [SettingKey.AnalyticsGtmId]: `${SETTING_ENV_PREFIX}GTM_ID`,
  [SettingKey.ChatProvider]: `${SETTING_ENV_PREFIX}CHAT_PROVIDER`,
  [SettingKey.ChatBaseUrl]: `${SETTING_ENV_PREFIX}CHAT_BASE_URL`,
  [SettingKey.ChatWebsiteToken]: `${SETTING_ENV_PREFIX}CHAT_WEBSITE_TOKEN`,
  [SettingKey.ChatWebsiteTokenRu]: `${SETTING_ENV_PREFIX}CHAT_WEBSITE_TOKEN_RU`,
  [SettingKey.ChatWebsiteTokenHy]: `${SETTING_ENV_PREFIX}CHAT_WEBSITE_TOKEN_HY`,
}

/** Closed set of the strings a boolean setting or integration toggle is stored as. */
export const SettingToggle = {
  /** The switch is on. */
  On: 'true',
  /** The switch is off. */
  Off: 'false',
} as const

/** One of the {@link SettingToggle} values. */
export type SettingToggle = (typeof SettingToggle)[keyof typeof SettingToggle]
