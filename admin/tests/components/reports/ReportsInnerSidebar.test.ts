/**
 * Tests for ReportsInnerSidebar: one link per report, the open one marked.
 */
import { describe, expect, it } from 'vitest'
import { REPORTS } from '@/lib/reports'
import ReportsInnerSidebar from '@/components/reports/ReportsInnerSidebar.vue'
import { mountView } from '../../helpers/mount'

describe('ReportsInnerSidebar', () => {
  it('lists every report as a link to its slug', async () => {
    const wrapper = await mountView(ReportsInnerSidebar, { path: '/reports/clients' })
    const links = wrapper.findAll('a')

    expect(links).toHaveLength(REPORTS.length)
    expect(wrapper.get('[data-testid="reports-nav-domains"]').attributes('href')).toBe('/reports/domains')
  })

  it('marks only the open report as the current page', async () => {
    const wrapper = await mountView(ReportsInnerSidebar, { path: '/reports/clients' })

    expect(wrapper.get('[data-testid="reports-nav-clients"]').attributes('aria-current')).toBe('page')
    expect(wrapper.get('[data-testid="reports-nav-domains"]').attributes('aria-current')).toBeUndefined()
  })

  it('marks nothing outside a report', async () => {
    const wrapper = await mountView(ReportsInnerSidebar, { path: '/dashboard' })

    expect(wrapper.find('[aria-current="page"]').exists()).toBe(false)
  })
})
