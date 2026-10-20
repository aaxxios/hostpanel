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
import type { PortalProductGroup } from '~/types/portalproductgroup'
import type { PortalProductPricing } from '~/types/portalproductpricing'
import type { ProductDetail } from '~/types/productdetail'
import type { ProductFamily } from '~/types/productfamily'
import type { ProductPricingRow } from '~/types/productpricingrow'
import type { ProductFeature } from '~/types/productfeature'
import { familyConfig, familyKey, isDomainGroup, isFamilyGroup, nameToKey, parseDescription } from '~/utils/catalogGroups'

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
 * Turns a group's specification lines into card bullets: one "label: value" per distinct label.
 *
 * @param lines The group's `ProductFeature` lines, every plan's.
 * @returns The bullets, in display order, the first plan's value winning for a repeated label.
 */
export const specificationBullets = (lines: readonly ProductFeature[]): string[] => {
  const seen = new Set<string>()
  const bullets: string[] = []
  for (const line of [...lines].sort((a, b) => a.sortOrder - b.sortOrder)) {
    const label = line.label.trim()
    if (!label || seen.has(label)) {
      continue
    }
    seen.add(label)
    bullets.push(line.value.trim() ? `${label}: ${line.value.trim()}` : label)
  }
  return bullets
}

/**
 * The key a product answers to in an address: its slug, or its normalised name when it has none.
 *
 * @param product The catalogue product.
 * @returns The key.
 */
const productKeyOf = (product: PortalProduct): string => product.slug?.trim() || nameToKey(product.name)

/**
 * Groups the catalogue into one entry per product family.
 *
 * Each family group becomes a family named by its group and its parent product (the one whose slug
 * or name is the family key, when there is one) and priced by its other plans; a group whose only
 * product is the parent is priced by the parent alone, and a group with no products is left out.
 * The card's feature list is the unique feature lines of every plan's description; a group whose
 * descriptions hold none falls back to its specification lines, and the list is empty when neither
 * exists. A group that sells domain names is left out: the `leading` domains card stands for it, so
 * domains are shown once.
 *
 * @param groups The catalogue's product groups.
 * @param raw Every product the catalogue returned for the family groups.
 * @param format Formats an amount in the payer's currency.
 * @param payerCode The payer's currency code; empty while it is still resolving.
 * @param leading Entries that come before the catalogue families (the static domains card).
 * @param specifications The specification lines read for a group, by its id.
 * @returns The families, `leading` first.
 */
export const buildProductFamilies = (
  groups: PortalProductGroup[],
  raw: PortalProduct[],
  format: MoneyFormatter,
  payerCode: string | null,
  leading: ProductFamily[] = [],
  specifications: (groupId: number) => readonly ProductFeature[] = () => [],
): ProductFamily[] => {
  const items: ProductFamily[] = [...leading]
  for (const group of groups.filter(isFamilyGroup)) {
    const key = familyKey(group)
    const inGroup = raw.filter((product) => Number(product.groupId) === group.id)
    if (isDomainGroup(inGroup)) {
      continue
    }
    const parent = inGroup.find((product) => productKeyOf(product) === key)
    const subPlans = inGroup.filter((product) => product !== parent)
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
      gid: group.id,
      ...familyConfig(group, inGroup),
      // A family has no tagline: the catalogue endpoint sends none. Only the static domains card
      // carries one, and it comes from the locale files.
      tagline: '',
      description: parseDescription((parent ?? plans[0])?.description || '').summary,
      features: (featureSet.size > 0 ? Array.from(featureSet).filter(Boolean) : specificationBullets(specifications(group.id))).slice(0, FAMILY_FEATURE_LIMIT),
      pricing: buildPricingRows(plans, format, payerCode),
    })
  }
  return items
}

/**
 * Finds the product a detail page is about and shapes it.
 *
 * The product is matched by its slug (or its normalised name), then by the family key of a group;
 * its plans are the other products of the same group.
 *
 * @param groups The catalogue's product groups.
 * @param list The catalogue products of the family groups.
 * @param productId The route parameter, e.g. `alpha-suite`.
 * @returns The product, or null when no product matches.
 */
export const findProductDetail = (groups: PortalProductGroup[], list: PortalProduct[], productId: string): ProductDetail | null => {
  const families = groups.filter(isFamilyGroup)
  const inFamilies = list.filter((product) => families.some((group) => group.id === Number(product.groupId)))

  let found = inFamilies.find((product) => productKeyOf(product) === productId)
  if (!found) {
    const targetGroup = families.find((group) => familyKey(group) === productId)
    if (targetGroup) {
      // `ProductDto` has no featured flag, so the first product of the group is the one shown.
      found = inFamilies.find((product) => Number(product.groupId) === targetGroup.id)
    }
  }
  const group = found ? families.find((candidate) => candidate.id === Number(found?.groupId)) : undefined
  if (!found || !group) {
    return null
  }

  const key = familyKey(group)
  const config = familyConfig(group, inFamilies.filter((product) => Number(product.groupId) === group.id))
  const { summary, features } = parseDescription(found.description || '')

  const plans = inFamilies
    .filter((product) => Number(product.groupId) === group.id && productKeyOf(product) !== key)
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
