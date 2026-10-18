/**
 * Unit tests for ComparisonTable: hidden without rows, one column per plan, and a dash for a plan
 * that does not carry a line.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import ComparisonTable from '~/templates/aurora/sections/AuroraComparisonTable.vue'
import { stubAuroraGlobals, PLANS } from '../auroraGlobals'

describe('ComparisonTable', () => {
  it('renders nothing when no plan has a specification', () => {
    stubAuroraGlobals()
    const wrapper = mountWithKit(ComparisonTable, { props: { plans: PLANS, rows: [] } })

    expect(wrapper.find('[data-testid="comparison-table"]').exists()).toBe(false)
  })

  it('renders a header per plan and a row per specification line', () => {
    stubAuroraGlobals()
    const wrapper = mountWithKit(ComparisonTable, {
      props: { plans: PLANS.slice(0, 2), rows: [{ label: 'Disk', values: ['10 GB', '50 GB'] }] },
    })

    expect(wrapper.findAll('th').map((cell) => cell.text())).toEqual(['aurora.comparison.feature', 'Starter', 'Business'])
    expect(wrapper.find('[data-testid="comparison-row-Disk"]').text()).toContain('10 GB')
    expect(wrapper.find('[data-testid="comparison-row-Disk"]').text()).toContain('50 GB')
  })

  it('shows a dash where a plan has no such line', () => {
    stubAuroraGlobals()
    const wrapper = mountWithKit(ComparisonTable, {
      props: { plans: PLANS.slice(0, 2), rows: [{ label: 'Backups', values: ['Daily', ''] }] },
    })

    const cells = wrapper.find('[data-testid="comparison-row-Backups"]').findAll('td')
    expect(cells.map((cell) => cell.text())).toEqual(['Backups', 'Daily', '—'])
  })
})
