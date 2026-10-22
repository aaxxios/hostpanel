/**
 * Builds the classic header navigation from the catalogue.
 *
 * Pure functions: the header passes in the translator, the locale prefixer and what the catalogue
 * store holds, and gets back the entries to draw. Hosting is a dropdown of the catalogue's plan
 * groups, VPS and Servers appear only when the catalogue sells them, and Support groups the help
 * pages.
 *
 * @module utils/headerNav
 */

import { HOSTING_GROUP_ID } from '~/constants/catalogGroup'
import type { HeaderNavChild } from '~/types/headernavchild'
import type { HeaderNavItem } from '~/types/headernavitem'
import type { PortalProduct } from '~/types/portalproduct'
import type { PortalProductGroup } from '~/types/portalproductgroup'
import { ProductType } from '~/types/producttype'
import { dominantProductType, familyConfig, HOSTING_FAMILY_URL, isFamilyGroup } from '~/utils/catalogGroups'

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

/** Service types whose groups are listed in the Hosting dropdown beside the hosting plans. */
const HOSTING_MENU_TYPES: ReadonlySet<string> = new Set<string>([
  ProductType.SharedHosting,
  ProductType.Email,
  ProductType.Ssl,
  ProductType.Backup,
])

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
 * @returns The entries in display order: Hosting, Domains, VPS and Servers when sold, Products,
 * Support.
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
    { key: 'hosting-all', label: t('nav.allHosting'), desc: t('nav.hostingDesc'), to: localePath(HOSTING_FAMILY_URL) },
    ...families
      .filter(group => group.id !== HOSTING_GROUP_ID && HOSTING_MENU_TYPES.has(typeOf(group)))
      .map(group => ({ key: `group-${group.id}`, label: group.name, to: pageOf(group) })),
  ]

  const items: HeaderNavItem[] = [
    hostingChildren.length > 1
      ? { key: 'hosting', label: t('nav.hosting'), children: hostingChildren }
      : { key: 'hosting', label: t('nav.hosting'), to: localePath(HOSTING_FAMILY_URL) },
    { key: 'domains', label: t('nav.domains'), to: localePath(DOMAINS_PATH) },
  ]

  const vps = families.find(group => typeOf(group) === ProductType.Vps)
  if (vps) {
    items.push({ key: 'vps', label: t('nav.vps'), to: pageOf(vps) })
  }
  const servers = families.find(group => typeOf(group) === ProductType.Dedicated)
  if (servers) {
    items.push({ key: 'servers', label: t('nav.servers'), to: pageOf(servers) })
  }

  items.push(
    { key: 'products', label: t('nav.products'), to: localePath(PRODUCTS_PATH) },
    {
      key: 'support',
      label: t('nav.support'),
      children: [
        { key: 'help', label: t('nav.help'), desc: t('nav.helpDesc'), to: localePath(HELP_PATH) },
        { key: 'faq', label: t('nav.faq'), desc: t('nav.faqDesc'), to: localePath(FAQ_PATH) },
        { key: 'contact', label: t('nav.contact'), desc: t('nav.contactDesc'), to: localePath(CONTACT_PATH) },
      ],
    },
  )
  return items
}
