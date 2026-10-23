/**
 * Unit tests for ProductPlansTable: one column per plan with its price, one row per specification
 * line, and "Not included" for a plan that lacks a line.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import ProductPlansTable from '~/components/catalog/ProductPlansTable.vue'
import type { ComparisonRow } from '~/types/comparisonrow'
import type { ProductPlan } from '~/types/productplan'

/**
 * Builds a plan.
 *
 * @param pid Order id, also the product id.
 * @param tier Plan name.
 * @returns The plan.
 */
const plan = (pid: number, tier: string): ProductPlan => ({
  id: pid, pid, tier, pricing: { monthly: pid, annual: null }, description: '', features: [], isFeatured: false,
})

/** The two plans the table compares. */
const plans: ProductPlan[] = [plan(11, 'Starter'), plan(12, 'Pro')]

/** The rows aligned to the plans; the Pro plan has no "Backups" line. */
const rows: ComparisonRow[] = [
  { label: 'Disk', values: ['10 GB', '20 GB'] },
  { label: 'Backups', values: ['Daily', ''] },
]

/**
 * Mounts the table.
 *
 * @returns The wrapper.
 */
const mountTable = () =>
  mountWithKit(ProductPlansTable, { props: { plans, rows, priceFor: (item: ProductPlan) => `$${item.pid}` } })

describe('ProductPlansTable', () => {
  it('heads one column per plan with its name and price', () => {
    const headers = mountTable().findAll('th')

    expect(headers).toHaveLength(3)
    expect(headers[0]?.text()).toBe('hosting.comparison.feature')
    expect(headers[1]?.text()).toContain('Starter')
    expect(headers[1]?.text()).toContain('$11')
    expect(headers[2]?.text()).toContain('Pro')
  })

  it('draws one row per specification line with each plan value in its column', () => {
    const wrapper = mountTable()
    const disk = wrapper.find('[data-testid="product-compare-row-Disk"]').findAll('td')

    expect(wrapper.findAll('tbody tr')).toHaveLength(2)
    expect(disk.map((cell) => cell.text())).toEqual(['Disk', '10 GB', '20 GB'])
  })

  it('reads a missing value as "not included", not as an empty cell', () => {
    const backups = mountTable().find('[data-testid="product-compare-row-Backups"]').findAll('td')

    expect(backups[1]?.text()).toBe('Daily')
    expect(backups[2]?.text()).toBe('hosting.comparison.notIncluded')
  })

  it('names its scroll region so a keyboard reader can scroll a wide table', () => {
    const region = mountTable().find('[role="region"]')

    expect(region.attributes('aria-label')).toBe('hosting.comparison.title')
    expect(region.attributes('tabindex')).toBe('0')
  })
})
