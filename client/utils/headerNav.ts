/**
 * Builds the classic header navigation from the catalogue.
 *
 * Pure functions: the header passes in the translator, the locale prefixer and what the catalogue
 * store holds, and gets back the entries to draw. Hosting is a dropdown of the catalogue's plan
 * groups (shared hosting, VPS, dedicated servers and the other service groups it sells, each with a
 * short description), and Support groups the help pages. The top level stays short on purpose: it
 * leaves room for the controls on the right of the bar.
 *
 * @module utils/headerNav
 */

import { HOSTING_GROUP_ID } from '~/constants/catalogGroup'
import { SUPPORT_LINK_ICON, SupportLinkKey } from '~/constants/headerNavIcon'
import { productTypeIcon } from '~/constants/productTypeIcon'
import type { HeaderNavChild } from '~/types/headernavchild'
import type { HeaderNavItem } from '~/types/headernavitem'
import type { PortalProduct } from '~/types/portalproduct'
import type { PortalProductGroup } from '~/types/portalproductgroup'
import { ProductType } from '~/types/producttype'
import { dominantProductType, familyConfig, HOSTING_FAMILY_URL, isFamilyGroup, parseDescription } from '~/utils/catalogGroups'

/** Translates a message key. */
export type HeaderTranslate = (key: string) => string

/** Prefixes a route with the active locale. */
export type HeaderLocalePath = (path: string) => string

/** What {@link buildHeaderNav} reads. */
export interface HeaderNavInput {
  /** Translator for the active locale. */
  t: HeaderTranslate
  /** Locale prefixer. */
  localePath: HeaderLocalePath
  /** The catalogue's product groups; empty until loaded. */
  groups: readonly PortalProductGroup[]
  /** The products of the family groups; empty until loaded. */
  products: readonly PortalProduct[]
}

/** Short description line of a Hosting dropdown entry, by the service type its group sells. */
const HOSTING_MENU_DESC_KEYS: Readonly<Record<string, string>> = {
  [ProductType.SharedHosting]: 'nav.hostingDesc',
  [ProductType.Vps]: 'nav.vpsDesc',
  [ProductType.Dedicated]: 'nav.serversDesc',
  [ProductType.Email]: 'nav.emailDesc',
  [ProductType.Ssl]: 'nav.sslDesc',
  [ProductType.Backup]: 'nav.backupDesc',
}

/**
 * The description line under a Hosting dropdown entry.
 *
 * The group's own short description wins when the operator wrote one (its first paragraph, as
 * plain text); otherwise the line written for the service type the group sells, so the row never
 * just repeats its title.
 *
 * @param group - The catalogue group.
 * @param fallbackKey - Message key of the line for the group's service type.
 * @param t - Translator for the active locale.
 * @returns The description to show.
 */
export const hostingEntryDescription = (group: PortalProductGroup, fallbackKey: string, t: HeaderTranslate): string =>
  parseDescription(group.description ?? '').summary || t(fallbackKey)

/** Route of the domain search page. */
const DOMAINS_PATH = '/domains'

/** Route of the all-products page. */
const PRODUCTS_PATH = '/products'

/** Route of the knowledge base. */
const HELP_PATH = '/knowledgebase'

/** Route of the FAQ. */
const FAQ_PATH = '/faq'

/** Route of the contact page. */
const CONTACT_PATH = '/contact'

/**
 * Whether a navigation target is the page being viewed: its own path or a page below it.
 *
 * @param path - The current route path.
 * @param href - The link target.
 * @returns True for the current target.
 */
export const isHeaderPathActive = (path: string, href: string | undefined): boolean => {
  if (!href) {
    return false
  }
  return path === href || path.startsWith(`${href}/`)
}

/**
 * Whether an entry, or any member of its group, is the page being viewed.
 *
 * @param path - The current route path.
 * @param item - The navigation entry.
 * @returns True when the entry or one of its children is current.
 */
export const isHeaderItemActive = (path: string, item: HeaderNavItem): boolean =>
  isHeaderPathActive(path, item.to) || (item.children ?? []).some(child => isHeaderPathActive(path, child.to))

/**
 * Builds the header entries.
 *
 * @param input - Translator, locale prefixer and the catalogue state.
 * @returns The entries in display order: Hosting, Domains, Products, Support.
 */
export const buildHeaderNav = ({ t, localePath, groups, products }: HeaderNavInput): HeaderNavItem[] => {
  const families = groups.filter(isFamilyGroup)

  /**
   * The products a group holds.
   *
   * @param group - The catalogue group.
   * @returns Its products.
   */
  const productsOf = (group: PortalProductGroup): PortalProduct[] =>
    products.filter(product => product.groupId === group.id)

  /**
   * The service type a group sells.
   *
   * @param group - The catalogue group.
   * @returns The type, or an empty string when unknown.
   */
  const typeOf = (group: PortalProductGroup): string =>
    group.id === HOSTING_GROUP_ID ? ProductType.SharedHosting : dominantProductType(productsOf(group)) ?? ''

  /**
   * The localised page of a catalogue group.
   *
   * @param group - The catalogue group.
   * @returns The route of the group's family page.
   */
  const pageOf = (group: PortalProductGroup): string => localePath(familyConfig(group, productsOf(group)).learnMoreUrl)

  const hostingChildren: HeaderNavChild[] = [
    {
      key: 'hosting-all',
      label: t('nav.allHosting'),
      icon: productTypeIcon(ProductType.SharedHosting),
      desc: t('nav.hostingDesc'),
      to: localePath(HOSTING_FAMILY_URL),
    },
    ...families
      .flatMap((group) => {
        const descKey = HOSTING_MENU_DESC_KEYS[typeOf(group)]
        // A group of a type the menu has no sentence for, and the hosting group itself, get no entry.
        return group.id !== HOSTING_GROUP_ID && descKey
          ? [{
            key: `group-${group.id}`,
            label: group.name,
            icon: productTypeIcon(typeOf(group)),
            desc: hostingEntryDescription(group, descKey, t),
            to: pageOf(group),
          }]
          : []
      }),
  ]

  const items: HeaderNavItem[] = [
    hostingChildren.length > 1
      ? { key: 'hosting', label: t('nav.hosting'), children: hostingChildren }
      : { key: 'hosting', label: t('nav.hosting'), to: localePath(HOSTING_FAMILY_URL) },
    { key: 'domains', label: t('nav.domains'), to: localePath(DOMAINS_PATH) },
  ]


  items.push(
    { key: 'products', label: t('nav.products'), to: localePath(PRODUCTS_PATH) },
    {
      key: 'support',
      label: t('nav.support'),
      children: [
        { key: SupportLinkKey.Help, label: t('nav.help'), icon: SUPPORT_LINK_ICON[SupportLinkKey.Help], desc: t('nav.helpDesc'), to: localePath(HELP_PATH) },
        { key: SupportLinkKey.Faq, label: t('nav.faq'), icon: SUPPORT_LINK_ICON[SupportLinkKey.Faq], desc: t('nav.faqDesc'), to: localePath(FAQ_PATH) },
        { key: SupportLinkKey.Contact, label: t('nav.contact'), icon: SUPPORT_LINK_ICON[SupportLinkKey.Contact], desc: t('nav.contactDesc'), to: localePath(CONTACT_PATH) },
      ],
    },
  )
  return items
}
