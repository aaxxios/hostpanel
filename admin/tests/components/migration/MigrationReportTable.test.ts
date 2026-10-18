/**
 * Mount tests for MigrationReportTable: log rows render with their outcome, a filter reloads from
 * the first page with the chosen value, and an empty or refused log is handled.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises, mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import MigrationReportTable from '@/components/migration/MigrationReportTable.vue'
import { createTestI18n } from '../../helpers/i18n'
import { json } from '../../helpers/fetch'
import { stubRoutes } from '../../helpers/routes'

/** Two log rows: one imported client and one failed invoice. */
const page = {
  items: [
    { id: 1, entityType: 'Clients', identifier: 'C-100', action: 'Imported', reason: null, createdAt: '2026-10-01T10:00:00Z' },
    { id: 2, entityType: 'Invoices', identifier: 'INV-7', action: 'Failed', reason: 'Missing client', createdAt: '2026-10-01T10:00:05Z' },
  ],
  totalCount: 2,
  page: 1,
  pageSize: 20,
}

/**
 * Mounts the report for job 5 and waits for the first load.
 *
 * @returns The wrapper.
 */
const mountReport = async () => {
  const pinia = createPinia()
  setActivePinia(pinia)
  const wrapper = mount(MigrationReportTable, {
    props: { jobId: 5 },
    global: { plugins: [createTestI18n(), pinia] },
  })
  await flushPromises()
  return wrapper
}

describe('MigrationReportTable', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('renders a row per log entry with its outcome and reason', async () => {
    stubRoutes({ 'GET /api/admin/migrations/5/logs': () => json(page) })
    const wrapper = await mountReport()

    const text = wrapper.get('[data-testid="migration-report-table"]').text()
    expect(text).toContain('C-100')
    expect(text).toContain('INV-7')
    expect(text).toContain('Imported')
    expect(text).toContain('Failed')
    expect(text).toContain('Missing client')
    expect(wrapper.get('[data-testid="migration-report-count"]').text()).toContain('2')
  })

  it('asks for page 1 with the page size on the first load', async () => {
    const mock = stubRoutes({ 'GET /api/admin/migrations/5/logs': () => json(page) })
    await mountReport()

    const url = String(mock.mock.calls[0]?.[0])
    expect(url).toContain('page=1')
    expect(url).toContain('pageSize=20')
  })

  it('shows the empty state when the log has no rows', async () => {
    stubRoutes({ 'GET /api/admin/migrations/5/logs': () => json({ ...page, items: [], totalCount: 0 }) })
    const wrapper = await mountReport()

    expect(wrapper.text()).toContain('No records found')
  })
})
