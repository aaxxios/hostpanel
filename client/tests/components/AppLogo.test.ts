/**
 * Unit tests for AppLogo: the bundled wordmark pair, an operator upload with and without a dark
 * variant, and the always-dark surface.
 */
import { beforeEach, describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import { stubTemplateGlobals } from '~/tests/helpers/templateGlobals'
import AppLogo from '~/components/AppLogo.vue'
import { LogoSurface } from '~/utils/appLogo'

describe('AppLogo', () => {
  beforeEach(() => {
    stubTemplateGlobals()
  })

  it('renders both bundled wordmarks, swapped by the dark variant, when nothing is uploaded', () => {
    const wrapper = mountWithKit(AppLogo)

    expect(wrapper.find('[data-testid="app-logo-light"]').classes()).toContain('dark:hidden')
    expect(wrapper.find('[data-testid="app-logo-dark"]').classes()).toContain('dark:block')
  })

  it('points the light theme at logo.svg and the dark theme at logo-dark.svg by default', () => {
    const wrapper = mountWithKit(AppLogo)

    expect(wrapper.get('[data-testid="app-logo-light"]').attributes('src')).toContain('/logo.svg')
    expect(wrapper.get('[data-testid="app-logo-dark"]').attributes('src')).toContain('/logo-dark.svg')
    expect(wrapper.get('[data-testid="app-logo-dark"]').classes()).toEqual(expect.arrayContaining(['hidden', 'dark:block']))
  })

  it('renders the uploads as plain images swapped by the dark variant', () => {
    stubTemplateGlobals({ logo: '/api/uploads/light.png', logoDark: '/api/uploads/dark.png' })
    const wrapper = mountWithKit(AppLogo)

    expect(wrapper.find('[data-testid="app-logo-light"]').attributes('src')).toBe('/api/uploads/light.png')
    expect(wrapper.find('[data-testid="app-logo-light"]').classes()).toContain('dark:hidden')
    expect(wrapper.find('[data-testid="app-logo-dark"]').attributes('src')).toBe('/api/uploads/dark.png')
  })

  it('lays a light plate behind a lone upload in dark mode, and not behind a dark variant', () => {
    stubTemplateGlobals({ logo: '/api/uploads/light.png' })
    const lone = mountWithKit(AppLogo)
    expect(lone.get('[data-testid="app-logo-light"]').classes()).toEqual(expect.arrayContaining(['dark:bg-white/90']))

    stubTemplateGlobals({ logo: '/api/uploads/light.png', logoDark: '/api/uploads/dark.png' })
    const pair = mountWithKit(AppLogo)
    expect(pair.get('[data-testid="app-logo-light"]').classes()).not.toContain('dark:bg-white/90')
    expect(pair.get('[data-testid="app-logo-dark"]').classes()).not.toContain('dark:bg-white/90')
  })

  it('always plates a lone upload on an always-dark surface', () => {
    stubTemplateGlobals({ logo: '/api/uploads/light.png' })
    const wrapper = mountWithKit(AppLogo, { props: { surface: LogoSurface.Dark } })

    expect(wrapper.get('[data-testid="app-logo-dark"]').classes()).toContain('bg-white/90')
  })

  it('renders only the dark-background wordmark on an always-dark surface', () => {
    const wrapper = mountWithKit(AppLogo, { props: { surface: LogoSurface.Dark } })

    expect(wrapper.find('[data-testid="app-logo-light"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="app-logo-dark"]').exists()).toBe(true)
  })

  it('passes the image classes to every image', () => {
    const wrapper = mountWithKit(AppLogo, { props: { imgClass: 'h-8' } })

    expect(wrapper.findAll('img').every((img) => img.classes().includes('h-8'))).toBe(true)
  })
})
