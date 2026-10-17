/**
 * Mount tests for MigrationProgressCard: state, percent and counts render from the job, a failed
 * job shows the recorded message, and the connection check and the import start go through the
 * store to the API.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises, mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import MigrationProgressCard from '@/components/migration/MigrationProgressCard.vue'
import { createTestI18n } from '../../helpers/i18n'
import { json, problem } from '../../helpers/fetch'
import { stubRoutes } from '../../helpers/routes'
import { migrationJob } from '../../helpers/migrationFixtures'
import type { MigrationJob } from '@/types/migration'

/**
 * Mounts the card for a job.
 *
 * @param job - The job to show.
 * @returns The wrapper.
 */
const mountCard = (job: MigrationJob) => {
  const pinia = createPinia()
  setActivePinia(pinia)
  return mount(MigrationProgressCard, { props: { job }, global: { plugins: [createTestI18n(), pinia] } })
}

describe('MigrationProgressCard', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('shows the label, state, percent and the selected groups only', () => {
    const wrapper = mountCard(migrationJob({ overallPercent: 40, status: 'InProgress' }))

    expect(wrapper.text()).toContain('Production')
    expect(wrapper.get('[data-testid="migration-job-status"]').text()).toBe('Running')
    expect(wrapper.get('[data-testid="migration-job-percent"]').text()).toBe('40%')
    expect(wrapper.find('[data-testid="migration-entity-clients"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="migration-entity-invoices"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="migration-entity-domains"]').exists()).toBe(false)
  })

  it('sums imported, skipped and failed records into the tiles', () => {
    const base = migrationJob({ status: 'Completed' })
    const job = migrationJob({
      status: 'Completed',
      progress: {
        ...base.progress,
        clients: { imported: 8, skipped: 1, total: 10, done: true },
      },
    })
    const wrapper = mountCard(job)

    expect(wrapper.get('[data-testid="migration-entity-clients"]').text()).toContain('9/10')
    // 10 total - 8 imported - 1 skipped = 1 failed, shown as its own tile.
    expect(wrapper.text()).toContain('Failed')
  })

  it('shows the recorded message of a failed job', () => {
    const wrapper = mountCard(migrationJob({ status: 'Failed', errorMessage: 'Source refused the key.' }))

    expect(wrapper.get('[data-testid="migration-job-error"]').text()).toContain('Source refused the key.')
    expect(wrapper.find('[data-testid="migration-check"]').exists()).toBe(false)
  })

  it('reports whether the plugin answered after a connection check', async () => {
    stubRoutes({ 'POST /api/admin/migrations/5/test-connection': () => json(migrationJob({ pluginConnected: true })) })
    const wrapper = mountCard(migrationJob())

    await wrapper.get('[data-testid="migration-check"]').trigger('click')
    await flushPromises()

    expect(wrapper.get('[data-testid="migration-check-result"]').text()).toBe('Connected')
  })

  it('reports an unreachable plugin when the check is refused', async () => {
    stubRoutes({ 'POST /api/admin/migrations/5/test-connection': () => problem('Source unreachable.', 502) })
    const wrapper = mountCard(migrationJob())

    await wrapper.get('[data-testid="migration-check"]').trigger('click')
    await flushPromises()

    expect(wrapper.get('[data-testid="migration-check-result"]').text()).toBe('Not reachable')
  })

  it('offers the import only once the plugin is connected', async () => {
    const pending = mountCard(migrationJob())
    expect(pending.find('[data-testid="migration-start"]').exists()).toBe(false)

    const connected = mountCard(migrationJob({ pluginConnected: true }))
    expect(connected.find('[data-testid="migration-start"]').exists()).toBe(true)
  })
})
