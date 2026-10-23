/**
 * Shapes the catalogue into the groups the products-page comparison table can show.
 *
 * @module utils/comparisonGroups
 */

import { buildComparisonRows, isComparable } from '~/utils/comparisonRows'
import { isDomainGroup, isFamilyGroup } from '~/utils/catalogGroups'
import { comparePlans, firstOfferedPrice } from '~/utils/productFamilies'
import type { MoneyFormatter } from '~/utils/productFamilies'
import { PricingCycle } from '~/types/pricingcycle'
import type { ComparisonGroup } from '~/types/comparisongroup'
import type { PortalProduct } from '~/types/portalproduct'
import type { PortalProductGroup } from '~/types/portalproductgroup'
import type { ProductFeature } from '~/types/productfeature'

/**
 * Builds one comparison group per family group whose plans are comparable.
 *
 * A group that sells domains, has fewer than two plans, or whose specification lines barely
 * overlap (see {@link isComparable}) is left out, so the table never renders a wall of empty cells.
 * Plans keep the display order of the storefront and the featured flag is the operator's, with at
 * most one featured plan per group (the first in display order), like the plan cards.
 *
 * @param groups The catalogue's product groups.
 * @param raw Every product the catalogue returned for the family groups.
 * @param format Formats an amount in the payer's currency.
 * @param specifications The specification lines read for a group, by its id.
 * @returns The comparable groups, in the order of `groups`.
 */
export const buildComparisonGroups = (
  groups: readonly PortalProductGroup[],
  raw: readonly PortalProduct[],
  format: MoneyFormatter,
  specifications: (groupId: number) => readonly ProductFeature[],
): ComparisonGroup[] => {
  const result: ComparisonGroup[] = []
  for (const group of groups.filter(isFamilyGroup)) {
    const inGroup = raw.filter(product => Number(product.groupId) === group.id)
    if (inGroup.length < 2 || isDomainGroup(inGroup)) {
      continue
    }
    const ordered = [...inGroup].sort(comparePlans)
    const featuredId = ordered.find(plan => plan.isFeatured === true && plan.pricing?.monthly !== 0)?.id
    const rows = buildComparisonRows([...specifications(group.id)], ordered.map(plan => plan.id))
    if (!isComparable(rows)) {
      continue
    }
    result.push({
      id: group.id,
      name: group.name,
      plans: ordered.map((plan) => {
        const offered = firstOfferedPrice(plan.pricing)
        return {
          id: plan.id,
          name: plan.name,
          price: offered ? format(offered.amount) : '',
          billingCycle: offered?.cycle ?? PricingCycle.Monthly,
          isFeatured: plan.id === featuredId,
        }
      }),
      rows,
    })
  }
  return result
}
