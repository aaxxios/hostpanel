/**
 * Mount tests for MigrationView: history and selection, the setup instructions of a pending job,
 * the new-migration dialog (create sends the chosen groups), and the API's refusal sentence.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import MigrationView from '@/views/MigrationView.vue'
import { mountRouted } from '../helpers/mount'
import { stubRoutes } from '../helpers/routes'
import { json, problem } from '../helpers/fetch'
import { migrationJob } from '../helpers/migrationFixtures'

/**
 * Mounts the screen and waits for the history.
 *
 * @returns The wrapper.
 */
const mountMigration = async () => {
  const { wrapper } = await mountRouted(MigrationView, '/migration', '/migration')
  await flushPromises()
  return wrapper
}

describe('MigrationView', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('lists the jobs in the history and asks to pick one', async () => {
    stubRoutes({ 'GET /api/admin/migrations': () => json([migrationJob()]) })
    const wrapper = await mountMigration()

    expect(wrapper.get('[data-testid="migration-job-5"]').text()).toContain('Production')
    expect(wrapper.find('[data-testid="migration-none-selected"]').exists()).toBe(true)
  })

  it('shows the setup instructions and the secret key of a selected pending job', async () => {
    // The more specific line first: the stub matches by prefix, and the job read must not get the list.
    stubRoutes({
      'GET /api/admin/migrations/5': () => json(migrationJob()),
      'GET /api/admin/migrations': () => json([migrationJob()]),
    })
    const wrapper = await mountMigration()

    await wrapper.get('[data-testid="migration-select-5"]').trigger('click')
    await flushPromises()

    expect(wrapper.find('[data-testid="migration-progress-card"]').exists()).toBe(true)
    expect(wrapper.get('[data-testid="migration-secret-key"]').text()).toBe('secret-key-123')
    expect(wrapper.find('[data-testid="migration-download-plugin"]').exists()).toBe(true)
  })

  it('says so when there is no history yet', async () => {
    stubRoutes({ 'GET /api/admin/migrations': () => json([]) })
    const wrapper = await mountMigration()

    expect(wrapper.find('[data-testid="migration-history-empty"]').exists()).toBe(true)
  })

  it('creates a migration from the dialog with the chosen groups', async () => {
    const mock = stubRoutes({
      'GET /api/admin/migrations/9': () => json(migrationJob({ id: 9 })),
      'GET /api/admin/migrations': () => json([]),
      'POST /api/admin/migrations': () => json(migrationJob({ id: 9 })),
    })
    const wrapper = await mountMigration()

    await wrapper.get('[data-testid="migration-new"]').trigger('click')
    await wrapper.get('[data-testid="migration-source-url"]').setValue('https://old.example.com')
    await wrapper.get('[data-testid="migration-toggle-all"]').trigger('click')
    await wrapper.get('[data-testid="migration-create"]').trigger('click')
    await flushPromises()

    const post = mock.mock.calls.find(([url, init]) => String(url) === '/api/admin/migrations' && init?.method === 'POST')
    expect(post).toBeDefined()
    const body = JSON.parse(String(post?.[1]?.body))
    expect(body.sourceUrl).toBe('https://old.example.com')
    // "Select all" starts from all-selected, so the first click clears every group.
    expect(body.selection.clients).toBe(false)
  })

  it('shows the API sentence when the history cannot be loaded', async () => {
    stubRoutes({ 'GET /api/admin/migrations': () => problem('Migrations are unavailable.', 503) })
    const wrapper = await mountMigration()

    expect(wrapper.get('[data-testid="migration-error"]').text()).toContain('Migrations are unavailable.')
  })
})
