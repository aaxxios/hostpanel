/**
 * Unit tests for ComparisonTable: one column per plan, ordered rows, and the absent marker.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import ComparisonTable from '~/templates/nova/sections/NovaComparisonTable.vue'
import type { PlanCard } from '~/types/plancard'

/**
 * Builds a plan card with a name.
 *
 * @param id Product id.
 * @param name Plan name shown as a column heading.
 * @returns A plan card.
 */
const plan = (id: number, name: string): PlanCard => ({
  id,
  name,
  description: '',
  features: [],
  isFree: false,
  priceMonthly: '$1.00',
  priceAnnual: '$1.00',
  href: `/configure/${id}`,
})

describe('ComparisonTable', () => {
  it('renders nothing when no row was specified', () => {
    const wrapper = mountWithKit(ComparisonTable, { props: { plans: [plan(1, 'A')], rows: [] } })

    expect(wrapper.find('table').exists()).toBe(false)
  })

  it('has a column heading per plan after the feature column', () => {
    const wrapper = mountWithKit(ComparisonTable, {
      props: { plans: [plan(1, 'Basic'), plan(2, 'Pro')], rows: [{ label: 'Storage', values: ['10 GB', '50 GB'] }] },
    })
    const heads = wrapper.findAll('thead th').map(cell => cell.text())

    expect(heads).toEqual(['nova.comparison.feature', 'Basic', 'Pro'])
  })

  it('puts the familiar lines in their preferred order and keeps unknown ones after', () => {
    const wrapper = mountWithKit(ComparisonTable, {
      props: {
        plans: [plan(1, 'Basic')],
        rows: [
          { label: 'Zebra line', values: ['x'] },
          { label: 'Bandwidth', values: ['1 TB'] },
          { label: 'Storage', values: ['10 GB'] },
        ],
      },
    })
    const labels = wrapper.findAll('tbody th').map(cell => cell.text())

    expect(labels).toEqual(['Storage', 'Bandwidth', 'Zebra line'])
  })

  it('marks a missing value as absent, never as zero', () => {
    const wrapper = mountWithKit(ComparisonTable, {
      props: { plans: [plan(1, 'Basic')], rows: [{ label: 'CDN', values: [''] }] },
    })

    expect(wrapper.find('tbody td').text()).toContain('nova.comparison.absent')
  })

  it('labels its scroll area so a keyboard can reach it', () => {
    const wrapper = mountWithKit(ComparisonTable, {
      props: { plans: [plan(1, 'Basic')], rows: [{ label: 'Storage', values: ['10 GB'] }] },
    })
    const scroller = wrapper.find('[role="region"]')

    expect(scroller.attributes('tabindex')).toBe('0')
    expect(scroller.attributes('aria-label')).toBe('nova.comparison.title')
  })
})
