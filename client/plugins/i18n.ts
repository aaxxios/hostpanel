/**
 * i18n Plugin
 * Dynamically loads all locale files
 */
import { APP_LOCALES } from '~/constants/appLocales'

export default defineNuxtPlugin(async ({ $i18n }) => {
  const locales = APP_LOCALES
  const modules = ['common', 'nav', 'hero', 'products', 'contact', 'faq', 'cta', 'whychooseus', 'team', 'partners', 'process', 'testimonials', 'footer', 'terms', 'privacy', 'whatsapp', 'error', 'cookieBanner', 'cookiePolicy', 'refundPolicy', 'aup', 'seo', 'domains', 'hosting', 'cart', 'checkout', 'client', 'configure', 'announcements', 'invoicePay', 'knowledgebase', 'trial', 'aurora', 'nova', 'paymentResult', 'demo']

  for (const locale of locales) {
    const messages: Record<string, unknown> = {}

    for (const module of modules) {
      try {
        const data = await import(`~/locales/${locale}/${module}.json`)
        Object.assign(messages, data.default || data)
      } catch {
        // Silently skip missing locale files
      }
    }

    $i18n.setLocaleMessage(locale, messages)
  }
})
