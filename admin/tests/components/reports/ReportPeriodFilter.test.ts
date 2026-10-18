/**
 * Tests for ReportPeriodFilter: which dropdowns show, and the choices and generate it reports.
 */
import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import ReportPeriodFilter from '@/components/reports/ReportPeriodFilter.vue'
import { createTestI18n } from '../../helpers/i18n'

/**
 * Mounts the filter.
 *
 * @param props - Props on top of the year.
 * @returns The wrapper.
 */
const mountFilter = (props: Record<string, unknown> = {}) =>
  mount(ReportPeriodFilter, { props: { year: 2024, ...props }, global: { plugins: [createTestI18n()] } })

describe('ReportPeriodFilter', () => {
  it('shows only the year by default', () => {
    const wrapper = mountFilter()

    expect(wrapper.find('[data-testid="report-year"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="report-month"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="report-currency"]').exists()).toBe(false)
  })

  it('shows the month and currency on request', () => {
    const wrapper = mountFilter({ showMonth: true, showCurrency: true })

    expect(wrapper.find('[data-testid="report-month"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="report-currency"]').exists()).toBe(true)
  })

  it('shows the chosen year in its dropdown', () => {
    expect(mountFilter({ yearsBack: 5, yearsAhead: 5 }).get('[data-testid="report-year"]').text()).toContain('2024')
  })

  it('asks the screen to generate when the button is pressed', async () => {
    const wrapper = mountFilter()

    await wrapper.get('[data-testid="report-generate"]').trigger('click')

    expect(wrapper.emitted('generate')).toHaveLength(1)
  })
})
