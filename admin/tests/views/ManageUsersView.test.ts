/**
 * Mount tests for ManageUsersView: rows render with their actions, the search is sent to the API,
 * the password-reset action confirms with a success message, and a refused load shows the API's
 * sentence instead of the table.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import ManageUsersView from '@/views/ManageUsersView.vue'
import { mountRouted } from '../helpers/mount'
import { stubRoutes } from '../helpers/routes'
import { json, problem } from '../helpers/fetch'

/** One user as the list endpoint returns it. */
const user = {
  id: 'u1',
  clientId: 7,
  firstName: 'Ada',
  lastName: 'Lovelace',
  email: 'ada@example.com',
  language: null,
  lastLoginAt: null,
  createdAt: '2026-01-01T00:00:00Z',
}

/**
 * Mounts the screen and waits for the first load.
 *
 * @returns The wrapper.
 */
const mountUsers = async () => {
  const { wrapper } = await mountRouted(ManageUsersView, '/clients/users', '/clients/users')
  await flushPromises()
  return wrapper
}

describe('ManageUsersView', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('renders a row per user with the three actions', async () => {
    stubRoutes({ 'GET /api/admin/users': () => json({ items: [user], totalCount: 1 }) })
    const wrapper = await mountUsers()

    const row = wrapper.get('[data-testid="users-row-u1"]')
    expect(row.text()).toContain('Ada')
    expect(row.text()).toContain('ada@example.com')
    expect(row.text()).toContain('Never')
    expect(wrapper.find('[data-testid="users-manage-u1"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="users-reset-u1"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="users-password-u1"]').exists()).toBe(true)
  })

  it('sends the typed search to the API', async () => {
    const mock = stubRoutes({ 'GET /api/admin/users': () => json({ items: [user], totalCount: 1 }) })
    const wrapper = await mountUsers()

    await wrapper.get('[data-testid="users-search"]').setValue('ada')
    await wrapper.get('[data-testid="users-search-submit"]').trigger('click')
    await flushPromises()

    expect(mock.mock.calls.some(([url]) => String(url).includes('search=ada'))).toBe(true)
  })

  it('confirms a sent password reset email', async () => {
    stubRoutes({
      'GET /api/admin/users': () => json({ items: [user], totalCount: 1 }),
      'POST /api/admin/users/u1/reset-password': () => new Response(null, { status: 204 }),
    })
    const wrapper = await mountUsers()

    await wrapper.get('[data-testid="users-reset-u1"]').trigger('click')
    await flushPromises()

    expect(wrapper.get('[data-testid="users-action-success"]').text()).toContain('Password reset email sent.')
  })

  it('shows the API sentence instead of the table when the load is refused', async () => {
    stubRoutes({ 'GET /api/admin/users': () => problem('Users are unavailable.', 503) })
    const wrapper = await mountUsers()

    expect(wrapper.get('[data-testid="users-error"]').text()).toContain('Users are unavailable.')
    expect(wrapper.find('[data-testid="users-table"]').exists()).toBe(false)
  })

  it('shows the empty state when there are no users', async () => {
    stubRoutes({ 'GET /api/admin/users': () => json({ items: [], totalCount: 0 }) })
    const wrapper = await mountUsers()

    expect(wrapper.text()).toContain('No users found')
  })
})
