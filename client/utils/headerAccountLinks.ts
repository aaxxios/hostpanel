/**
 * The client-area destinations offered by the classic header's account menu and drawer.
 *
 * @module utils/headerAccountLinks
 */

import { FileText, Globe, LayoutDashboard, MessageSquare, Server, Settings } from 'lucide-vue-next'
import {
  CLIENT_ACCOUNT_PATH,
  CLIENT_DASHBOARD_PATH,
  CLIENT_DOMAINS_PATH,
  CLIENT_INVOICES_PATH,
  CLIENT_SERVICES_PATH,
  CLIENT_TICKETS_PATH,
} from '~/constants/clientRoutes'
import type { HeaderAccountLink } from '~/types/headeraccountlink'
import type { HeaderLocalePath, HeaderTranslate } from '~/utils/headerNav'

/**
 * Builds the signed-in account links, in the order the client-area navigation uses.
 *
 * @param t - Translator for the active locale.
 * @param localePath - Locale prefixer.
 * @returns The links.
 */
export const buildAccountLinks = (t: HeaderTranslate, localePath: HeaderLocalePath): HeaderAccountLink[] => [
  { key: 'dashboard', icon: LayoutDashboard, label: t('client.nav.dashboard'), to: localePath(CLIENT_DASHBOARD_PATH) },
  { key: 'services', icon: Server, label: t('client.nav.services'), to: localePath(CLIENT_SERVICES_PATH) },
  { key: 'domains', icon: Globe, label: t('client.nav.domains'), to: localePath(CLIENT_DOMAINS_PATH) },
  { key: 'invoices', icon: FileText, label: t('client.nav.invoices'), to: localePath(CLIENT_INVOICES_PATH) },
  { key: 'tickets', icon: MessageSquare, label: t('client.nav.support'), to: localePath(CLIENT_TICKETS_PATH) },
  { key: 'account', icon: Settings, label: t('client.nav.account'), to: localePath(CLIENT_ACCOUNT_PATH) },
]
