/**
 * Unit tests for AppLauncher: invisible without apps, a tile per app with a token gradient and no
 * inline style, and closing on a choice.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import AppLauncher from '~/templates/aurora/layout/AppLauncher.vue'
import type { PortalApp } from '~/types/portalapp'
import { PortalAppId } from '~/types/portalappid'
import { stubAuroraGlobals } from '../auroraGlobals'

/**
 * Builds one launcher entry.
 *
 * @param id App id.
 * @returns A ready-to-render entry; the composable's own gradient string is deliberately unused.
 */
const app = (id: PortalAppId): PortalApp => ({
  id,
  tag: id.slice(0, 2),
  tint: 'linear-gradient(135deg, #000000, #ffffff)',
  label: `App ${id}`,
  desc: `About ${id}`,
  url: `https://${id}.test`,
})

describe('AppLauncher', () => {
  it('renders nothing for a deployment with no apps', () => {
    stubAuroraGlobals({ apps: [] })
    const wrapper = mountWithKit(AppLauncher)

    expect(wrapper.find('[data-testid="app-launcher-trigger"]').exists()).toBe(false)
  })

  it('opens a panel with one link per app', async () => {
    stubAuroraGlobals({ apps: [app(PortalAppId.Tasks), app(PortalAppId.Erp)] })
    const wrapper = mountWithKit(AppLauncher)

    await wrapper.find('[data-testid="app-launcher-trigger"]').trigger('click')

    expect(wrapper.find('[data-testid="app-launcher-tasks"]').attributes('href')).toBe('https://tasks.test')
    expect(wrapper.find('[data-testid="app-launcher-erp"]').text()).toContain('App erp')
    expect(wrapper.find('[data-testid="app-launcher-erp"]').text()).toContain('About erp')
  })

  it('paints each tile from a gradient token keyed by app id, with no inline style', async () => {
    stubAuroraGlobals({ apps: [app(PortalAppId.Tasks), app(PortalAppId.Calendar)] })
    const wrapper = mountWithKit(AppLauncher)
    await wrapper.find('[data-testid="app-launcher-trigger"]').trigger('click')

    expect(wrapper.find('[data-testid="app-launcher-tasks"] .place-items-center').classes()).toContain('bg-tile-mint')
    expect(wrapper.find('[data-testid="app-launcher-calendar"] .place-items-center').classes()).toContain('bg-tile-rose')
    expect(wrapper.html()).not.toContain(' style=')
  })

  it('closes the panel when an app is chosen', async () => {
    stubAuroraGlobals({ apps: [app(PortalAppId.Tasks)] })
    const wrapper = mountWithKit(AppLauncher)
    await wrapper.find('[data-testid="app-launcher-trigger"]').trigger('click')

    await wrapper.find('[data-testid="app-launcher-tasks"]').trigger('click')

    expect(wrapper.find('[data-testid="ui-popover-panel"]').exists()).toBe(false)
  })
})
