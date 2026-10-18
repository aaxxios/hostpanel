/**
 * Shapes the catalogue's flat product list into what the products pages show.
 *
 * The catalogue endpoint returns every plan of every group as one list; the storefront shows one
 * card per family (a group) on the products page and one detail page per family. The grouping,
 * the choice of the "parent" product that names a family, the first offered price of a plan and
 * the parsing of description copy live here as pure functions so they are tested without a page.
 *
 * @module utils/productFamilies
 */

import { PRICING_CYCLES } from '~/constants/pricingCycle'
import { PricingCycle } from '~/types/pricingcycle'
import type { PortalProduct } from '~/types/portalproduct'
import type { PortalProductPricing } from '~/types/portalproductpricing'
import type { ProductDetail } from '~/types/productdetail'
import type { ProductFamily } from '~/types/productfamily'
import type { ProductPricingRow } from '~/types/productpricingrow'
import { nameToKey, parseDescription, productConfig, productGidToKey, productGids } from '~/utils/catalogGroups'

/** Visual config of a family the config table does not know. */
const UNKNOWN_FAMILY_ICON = 'cube'

/** Accent of a family the config table does not know. */
const UNKNOWN_FAMILY_COLOR = '#6366f1'

/** How many feature lines a family card aggregates from its plans. */
const FAMILY_FEATURE_LIMIT = 6

/** Formats an amount in the payer's currency. */
export type MoneyFormatter = (amount: number) => string

/** A billing cycle together with the amount a plan is priced at for it. */
export interface OfferedPrice {
  /** The billing cycle. */
  cycle: PricingCycle
  /** The price for that cycle. */
  amount: number
}

/**
 * A plan's price for one billing cycle, in the caller's own currency, as the API sent it.
 *
 * @param pricing The plan's prices, or null/undefined when it has none.
 * @param cycle The billing cycle.
 * @returns The amount, or null when the plan carries no price for that cycle.
 */
export const cycleAmount = (pricing: PortalProductPricing | null | undefined, cycle: PricingCycle): number | null =>
  pricing?.[cycle] ?? null

/**
 * The first cycle a plan is priced for, and that price.
 *
 * @param pricing The plan's prices, or null/undefined when it has none.
 * @returns The cycle and amount, or null when the plan carries no price at all.
 */
export const firstOfferedPrice = (pricing: PortalProductPricing | null | undefined): OfferedPrice | null => {
  for (const cycle of PRICING_CYCLES) {
    const amount = cycleAmount(pricing, cycle)
    if (amount !== null) {
      return { cycle, amount }
    }
  }
  return null
}

/**
 * Maps the catalogue products of one family onto the pricing rows a card renders.
 *
 * @param plans Products belonging to one family, as the catalogue endpoint returned them.
 * @param format Formats an amount in the payer's currency.
 * @param payerCode The payer's currency code; empty while it is still resolving.
 * @returns One row per plan: id, tier name, first offered price and its billing cycle, and the
 * feature lines parsed out of the description.
 */
export const buildPricingRows = (plans: PortalProduct[], format: MoneyFormatter, payerCode: string | null): ProductPricingRow[] =>
  plans.map((plan) => {
    const offered = firstOfferedPrice(plan.pricing)
    const { features } = parseDescription(plan.description || '')
    return {
      // The proxy always adds `pid`; the type marks it optional.
      pid: plan.pid as number,
      tier: plan.name,
      slug: plan.slug || '',
      price: offered ? format(offered.amount) : '',
      amount: offered?.amount ?? 0,
      currency: payerCode ?? '',
      billingCycle: offered?.cycle ?? PricingCycle.Monthly,
      features,
    }
  })

/**
 * Groups the catalogue into one entry per product family.
 *
 * Each group in `productGids` becomes a family named by its parent product (the one whose slug is
 * the family key) and priced by its sub-plans; a group with no sub-plans is priced by the parent
 * alone, and a group with no products is left out. The card's feature list is the unique feature
 * lines of every plan in the group.
 *
 * @param raw Every product the catalogue returned for the family groups.
 * @param format Formats an amount in the payer's currency.
 * @param payerCode The payer's currency code; empty while it is still resolving.
 * @param leading Entries that come before the catalogue families (the static domains card).
 * @returns The families, `leading` first.
 */
export const buildProductFamilies = (
  raw: PortalProduct[],
  format: MoneyFormatter,
  payerCode: string | null,
  leading: ProductFamily[] = [],
): ProductFamily[] => {
  const items: ProductFamily[] = [...leading]
  for (const gid of productGids) {
    const key = productGidToKey[gid] ?? ''
    const config = productConfig[key] ?? { name: key, icon: UNKNOWN_FAMILY_ICON, color: UNKNOWN_FAMILY_COLOR, learnMoreUrl: '' }
    const inGroup = raw.filter((product) => Number(product.groupId) === gid)
    const parent = inGroup.find((product) => product.slug?.trim() === key)
    const subPlans = inGroup.filter((product) => product.slug?.trim() !== key)
    // With no sub-plans the parent product is the single plan.
    const plans = subPlans.length > 0 ? subPlans : (parent ? [parent] : [])
    if (plans.length === 0) {
      continue
    }

    const featureSet = new Set<string>()
    for (const product of inGroup) {
      for (const feature of parseDescription(product.description || '').features) {
        featureSet.add(feature)
      }
    }

    items.push({
      id: key,
      gid,
      ...config,
      // A family has no tagline: the catalogue endpoint sends none. Only the static domains card
      // carries one, and it comes from the locale files.
      tagline: '',
      description: parseDescription((parent ?? plans[0])?.description || '').summary,
      features: Array.from(featureSet).filter(Boolean).slice(0, FAMILY_FEATURE_LIMIT),
      pricing: buildPricingRows(plans, format, payerCode),
    })
  }
  return items
}

/**
 * Finds the product a detail page is about and shapes it.
 *
 * The product is matched by its slug (or its normalised name), then by the family key of its
 * group; its plans are the other products of the same group.
 *
 * @param list The catalogue products of the family groups.
 * @param productId The route parameter, e.g. `smartlearn-system`.
 * @returns The product, or null when no product matches.
 */
export const findProductDetail = (list: PortalProduct[], productId: string): ProductDetail | null => {
  const inFamilies = list.filter((product) => productGids.includes(Number(product.groupId)))

  let found = inFamilies.find((product) => (product.slug?.trim() || nameToKey(product.name)) === productId)
  if (!found) {
    const targetGid = Number(Object.entries(productGidToKey).find(([, value]) => value === productId)?.[0] ?? 0)
    if (targetGid) {
      // `ProductDto` has no featured flag, so the first product of the group is the one shown.
      found = inFamilies.find((product) => Number(product.groupId) === targetGid)
    }
  }
  if (!found) {
    return null
  }

  const key = productGidToKey[Number(found.groupId)] ?? found.slug?.trim() ?? nameToKey(found.name)
  const config = productConfig[key] ?? { name: key, icon: UNKNOWN_FAMILY_ICON, color: UNKNOWN_FAMILY_COLOR, demoUrl: '', learnMoreUrl: '' }
  const { summary, features } = parseDescription(found.description || '')

  const plans = inFamilies
    .filter((product) => Number(product.groupId) === Number(found.groupId) && product.slug?.trim() !== key)
    .map((plan) => ({
      pid: plan.pid as number,
      tier: plan.name,
      pricing: plan.pricing ?? null,
      features: parseDescription(plan.description || '').features,
    }))

  return {
    id: key,
    pid: found.pid,
    name: found.name,
    icon: config.icon,
    color: config.color,
    demoUrl: config.demoUrl,
    learnMoreUrl: config.learnMoreUrl,
    description: summary,
    features,
    plans,
  }
}
