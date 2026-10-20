/**
 * Shared Catalog helpers — auto-imported by Nuxt from utils/
 *
 * @module utils/catalogGroups
 */

import { HOSTING_GROUP_ID } from '~/constants/catalogGroup'
import { DEFAULT_PRODUCT_ICON, productTypeIcon } from '~/constants/productTypeIcon'
import type { PortalProduct } from '~/types/portalproduct'
import type { PortalProductGroup } from '~/types/portalproductgroup'
import { ProductType } from '~/types/producttype'

/** Visual and meta config of one product family. */
export interface ProductFamilyConfig {
  /** Display name. */
  name: string
  /** Full icon name, e.g. `mdi:server`. */
  icon: string
  /** Accent colour, `#rrggbb`. */
  color: string
  /** Demo address, when the family has one. */
  demoUrl?: string
  /** Address of the family's detail page. */
  learnMoreUrl: string
}

/** Key of the hosting family, which has its own page rather than a `/products/<key>` one. */
export const HOSTING_FAMILY_KEY = 'web-hosting'

/** Icon of a family whose type is unknown; the catalogue stores no icon, so it is derived from the type. */
export const FAMILY_ICON = DEFAULT_PRODUCT_ICON

/** Accent of the hosting family. */
export const HOSTING_FAMILY_COLOR = '#0ea5e9'

/** Page of the hosting family. */
export const HOSTING_FAMILY_URL = '/hosting'

/** Accents a catalogue family is painted in; the catalogue stores no colour, so one is picked by key. */
export const FAMILY_COLORS: readonly string[] = ['#8b5cf6', '#06b6d4', '#f59e0b', '#ec4899', '#ef4444', '#10b981']

/**
 * Normalize a name to a slug key: "Alpha Suite" → "alpha-suite".
 *
 * @param name The display name.
 * @returns The slug.
 */
export const nameToKey = (name: string): string => {
  return name.toLowerCase().replace(/\s+/g, '-').replace(/[^a-z0-9-]/g, '')
}

/**
 * Whether the storefront shows a group as a product family: it is active and holds a product.
 *
 * @param group The catalogue group.
 * @returns True when it is a family.
 */
export const isFamilyGroup = (group: PortalProductGroup): boolean => group.isActive && group.productCount > 0

/**
 * The key (slug) of a family: the hosting family has a fixed one, every other family is named by
 * its group, so a group the operator adds gets a page with no code change.
 *
 * @param group The catalogue group.
 * @returns The key, used in `/products/<key>`.
 */
export const familyKey = (group: PortalProductGroup): string => {
  if (group.id === HOSTING_GROUP_ID) {
    return HOSTING_FAMILY_KEY
  }
  return nameToKey(group.name) || `group-${group.id}`
}

/**
 * Picks a family's accent from {@link FAMILY_COLORS}, the same one on every render.
 *
 * @param key The family key.
 * @returns A `#rrggbb` colour.
 */
export const familyColor = (key: string): string => {
  let sum = 0
  for (const char of key) {
    sum += char.charCodeAt(0)
  }
  return FAMILY_COLORS[sum % FAMILY_COLORS.length] ?? HOSTING_FAMILY_COLOR
}

/**
 * The service type most of a group's products sell.
 *
 * The group itself carries no type, so it is read from its products; a tie goes to the type met first.
 *
 * @param products The group's products.
 * @returns The dominant type, or null when there is no product or none has a type.
 */
export const dominantProductType = (products: readonly PortalProduct[]): string | null => {
  const counts = new Map<string, number>()
  let best: string | null = null
  for (const product of products) {
    if (!product.type) {
      continue
    }
    const count = (counts.get(product.type) ?? 0) + 1
    counts.set(product.type, count)
    if (best === null || count > (counts.get(best) ?? 0)) {
      best = product.type
    }
  }
  return best
}

/**
 * Whether a group sells domain names, which the static domains card stands for.
 *
 * @param products The group's products.
 * @returns True when the group's dominant type is {@link ProductType.Domain}.
 */
export const isDomainGroup = (products: readonly PortalProduct[]): boolean => dominantProductType(products) === ProductType.Domain

/**
 * The look and links of a family, derived from its group: the icon from the type of its products,
 * and only hosting has a colour and address of its own.
 *
 * @param group The catalogue group.
 * @param products The group's products, whose type picks the icon; omit when unknown.
 * @returns The family's config.
 */
export const familyConfig = (group: PortalProductGroup, products: readonly PortalProduct[] = []): ProductFamilyConfig => {
  const key = familyKey(group)
  const type = group.id === HOSTING_GROUP_ID ? ProductType.SharedHosting : dominantProductType(products)
  const icon = productTypeIcon(type)
  if (group.id === HOSTING_GROUP_ID) {
    return { name: group.name, icon, color: HOSTING_FAMILY_COLOR, learnMoreUrl: HOSTING_FAMILY_URL }
  }
  return { name: group.name, icon, color: familyColor(key), learnMoreUrl: `/products/${key}` }
}

/*
 * `getGt` and `getGroupFeatures` used to live here.
 *
 * `getGt(p, field)` read `p.group_translations.translated_<field>`, then
 * `p.translated_<field>`, then `p.<field>`; `getGroupFeatures(p)` read
 * `p.group_features[].translated_feature`. Neither field is on `ProductDto`, and
 * `GET /api/portal/public/products` — verified against a live backend — sends only
 * `ProductDto`'s own fields, so the first two branches of `getGt` never resolved and
 * `getGroupFeatures` always returned `[]`. Callers now read the base field directly.
 *
 * Structured, per-product specification lines do exist, but as their own resource:
 * `GET /api/portal/public/product-features` (`ProductFeature` — label/value/sortOrder),
 * reached through `useCatalogStore().loadProductFeatures(groupId)`. That is what a caller
 * wanting `group_features` wants today.
 *
 * Localised product copy does not exist anywhere in the backend: there is no product
 * translations table and no `lang` handling in `ProductsController`. `Slide` is the only
 * entity with a `*Translation` sibling, and `loadSlides(lang)` is the one place a `lang`
 * parameter still means something. Making product copy translatable is a backend change —
 * a `ProductTranslation` entity alongside `SlideTranslation` — not a frontend one.
 */

/** The HTML entities a legacy description may contain, by name or code. */
const ENTITIES: Record<string, string> = {
  amp: '&', lt: '<', gt: '>', quot: '"', apos: "'", nbsp: ' ', '#39': "'", '#x27': "'",
}

/**
 * Flatten a legacy description into plain text lines.
 *
 * The legacy system let each product's description be edited as HTML, so the same field
 * arrives either as newline-separated plain text or as one long line with
 * <br /> between items — the two are indistinguishable to whoever typed them,
 * and both are in use across the catalogue. Line-based parsing sees the HTML
 * variant as a single unbroken line, which is how raw "<strong>…<br />" ended
 * up printed on the product cards. Convert the block-level tags back into the
 * newlines they stand for, drop the rest, and the parser below works on either.
 *
 * @param text The raw description.
 * @returns The description as plain text with real newlines.
 */
const htmlToLines = (text: string): string => {
  return text
    .replace(/<br\s*\/?>/gi, '\n')
    .replace(/<\/(p|div|li|ul|ol|h[1-6])\s*>/gi, '\n')
    .replace(/<li\b[^>]*>/gi, '\n• ')
    .replace(/<[^>]+>/g, '')
    .replace(/&(#x?[0-9a-f]+|[a-z]+);/gi, (m, e: string) => ENTITIES[e.toLowerCase()] ?? m)
}

/**
 * Bullet markers seen across the catalogue: •, -, * and both check marks.
 *
 * U+2713 ✓ and U+2714 ✔ sit one codepoint apart and look identical at body size.
 * Only the first was listed, and the catalogue is written with the second, so no
 * feature line was ever recognised as one: every item fell through to the summary,
 * and a card printed one long paragraph with check marks running through it.
 */
const BULLET = /^[\u2022\u2713\u2714\-*]\s*/

/** The two parts a legacy description splits into. */
export interface ParsedDescription {
  /** The opening paragraph. */
  summary: string
  /** The bullet lines, prefix stripped. */
  features: string[]
}

/**
 * Parse a legacy description text into a summary paragraph and a features list.
 * - Summary  = lines before the first blank line or section header
 * - Features = all subsequent non-empty lines (bullet prefix stripped)
 * - Lines ending with ":" are treated as section headers in any language and skipped
 *
 * @param text The raw description.
 * @returns The summary and the feature lines.
 */
export const parseDescription = (text: string): ParsedDescription => {
  const summaryLines: string[] = []
  const features: string[] = []
  let pastSummary = false

  for (const line of htmlToLines(text).split(/\r?\n/)) {
    const t = line.trim()
    const isHeader = /:\s*$/.test(t)

    if (!pastSummary) {
      if (!t) {
        if (summaryLines.length) {
          pastSummary = true
        }
      } else if (isHeader) {
        pastSummary = true
      } else if (BULLET.test(t)) {
        pastSummary = true
        features.push(t.replace(BULLET, '').trim())
      } else {
        summaryLines.push(t)
      }
    } else {
      if (!t || isHeader) {
        continue
      }
      features.push(t.replace(BULLET, '').trim())
    }
  }

  return { summary: summaryLines.join(' '), features }
}
