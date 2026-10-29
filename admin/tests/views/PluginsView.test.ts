/**
 * Mount tests for PluginsView: the installed list, the empty state, the restart banner after an
 * install, a refusal shown as the API's sentence, the retry of a refused load and the danger
 * confirmation before a removal.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import PluginsView from '@/views/PluginsView.vue'
import { mountRouted } from '../helpers/mount'
import { stubRoutes } from '../helpers/routes'
import { json, problem } from '../helpers/fetch'

/** One installed plugin as the list endpoint returns it. */
const plugin = {
  id: 'ineco',
  name: 'Ineco Pay',
  version: '1.2.0',
  author: 'Hostpanel',
  description: 'Card payments',
  type: 'payment',
  category: 'Payments',
  color: '#123456',
  isLoaded: true,
  isBuiltIn: false,
}

/**
 * Mounts the screen and waits for the first load.
 *
 * @returns The wrapper.
 */
const mountPlugins = async () => {
  const { wrapper } = await mountRouted(PluginsView, '/plugins', '/plugins')
  await flushPromises()
  return wrapper
}

describe('PluginsView', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('lists the installed plugins', async () => {
    stubRoutes({ 'GET /api/admin/plugins': () => json([plugin]) })
    const wrapper = await mountPlugins()

    expect(wrapper.get('[data-testid="plugins-list"]').text()).toContain('Ineco Pay')
    expect(wrapper.find('[data-testid="plugins-empty"]').exists()).toBe(false)
  })

  it('shows the empty state when nothing is installed', async () => {
    stubRoutes({ 'GET /api/admin/plugins': () => json([]) })
    const wrapper = await mountPlugins()

    expect(wrapper.find('[data-testid="plugins-empty"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="plugins-restart-banner"]').exists()).toBe(false)
  })

  it('shows the API sentence when the load is refused', async () => {
    stubRoutes({ 'GET /api/admin/plugins': () => problem('Plugins are unavailable.', 503) })
    const wrapper = await mountPlugins()

    expect(wrapper.get('[data-testid="plugins-error"]').text()).toContain('Plugins are unavailable.')
  })

  it('loads again from the retry of a refused load', async () => {
    let calls = 0
    stubRoutes({
      'GET /api/admin/plugins': () => {
        calls += 1
        return calls === 1 ? problem('Plugins are unavailable.', 503) : json([plugin])
      },
    })
    const wrapper = await mountPlugins()

    await wrapper.get('[data-testid="plugins-error-retry"]').trigger('click')
    await flushPromises()

    expect(wrapper.get('[data-testid="plugins-list"]').text()).toContain('Ineco Pay')
  })

  it('removes a plugin and reloads the list', async () => {
    const mock = stubRoutes({
      'GET /api/admin/plugins': () => json([plugin]),
      'DELETE /api/admin/plugins/ineco': () => json({ requiresRestart: true }),
    })
    const wrapper = await mountPlugins()

    await wrapper.get('[data-testid="plugin-remove-ineco"]').trigger('click')
    await flushPromises()
    expect(mock.mock.calls.some(([, init]) => init?.method === 'DELETE')).toBe(false)
    await wrapper.get('[data-testid="dialog-confirm"]').trigger('click')
    await flushPromises()

    expect(mock.mock.calls.some(([url, init]) => String(url) === '/api/admin/plugins/ineco' && init?.method === 'DELETE')).toBe(true)
    expect(wrapper.get('[data-testid="plugins-feedback"]').text()).toContain('Plugin removed.')
  })

  it('shows the API sentence when the removal is refused', async () => {
    stubRoutes({
      'GET /api/admin/plugins': () => json([plugin]),
      'DELETE /api/admin/plugins/ineco': () => problem('Plugin is in use.', 409),
    })
    const wrapper = await mountPlugins()

    await wrapper.get('[data-testid="plugin-remove-ineco"]').trigger('click')
    await wrapper.get('[data-testid="dialog-confirm"]').trigger('click')
    await flushPromises()

    expect(wrapper.get('[data-testid="plugins-feedback"]').text()).toContain('Plugin is in use.')
    expect(wrapper.get('[data-testid="plugins-list"]').text()).toContain('Ineco Pay')
  })

  it('keeps the plugin when the removal confirmation is dismissed', async () => {
    const mock = stubRoutes({ 'GET /api/admin/plugins': () => json([plugin]) })
    const wrapper = await mountPlugins()

    await wrapper.get('[data-testid="plugin-remove-ineco"]').trigger('click')
    await flushPromises()
    await wrapper.get('[data-testid="dialog-cancel"]').trigger('click')
    await flushPromises()

    expect(mock.mock.calls.some(([, init]) => init?.method === 'DELETE')).toBe(false)
    expect(wrapper.find('[data-testid="dialog-confirm"]').exists()).toBe(false)
  })
})
