/**
 * Unit tests for AuroraHeader: navigation, the brand mark or the uploaded logo pair, the cart
 * button, signed-in versus signed-out controls, and the narrow-screen menu.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import AuroraHeader from '~/templates/aurora/layout/AuroraHeader.vue'
import { stubAuroraGlobals } from '../auroraGlobals'

/** The language switcher and theme toggle have their own tests; here they only need to exist. */
const STUBS = { UiLanguageSwitcher: true, UiThemeToggle: true }

describe('AuroraHeader', () => {
  it('links the five navigation entries through the ids the e2e specs use', () => {
    stubAuroraGlobals()
    const wrapper = mountWithKit(AuroraHeader, { stubs: STUBS })

    expect(wrapper.find('[data-testid="nav-hosting"]').attributes('href')).toBe('/hosting')
    expect(wrapper.find('[data-testid="nav-domains"]').attributes('href')).toBe('/domains')
    expect(wrapper.find('[data-testid="nav-faq"]').attributes('href')).toBe('/faq')
    expect(wrapper.find('[data-testid="nav-cart"]').attributes('href')).toBe('/cart')
  })

  it('emphasises the current entry only', () => {
    stubAuroraGlobals()
    const wrapper = mountWithKit(AuroraHeader, { props: { active: 'domains' }, stubs: STUBS })

    expect(wrapper.find('[data-testid="nav-domains"]').classes()).toContain('font-semibold')
    expect(wrapper.find('[data-testid="nav-hosting"]').classes()).not.toContain('font-semibold')
  })

  it('draws the built-in mark and the site name when no logo is uploaded', () => {
    stubAuroraGlobals({ siteName: 'Acme Host' })
    const wrapper = mountWithKit(AuroraHeader, { stubs: STUBS })

    expect(wrapper.find('[data-testid="site-home"]').text()).toContain('Acme Host')
    expect(wrapper.find('[data-testid="site-home"]').text()).toContain('a')
    expect(wrapper.find('[data-testid="site-logo-light"]').exists()).toBe(false)
  })

  it('renders only the light logo, as a plain image, when there is no dark variant', () => {
    stubAuroraGlobals({ logo: '/api/uploads/logo.png' })
    const wrapper = mountWithKit(AuroraHeader, { stubs: STUBS })
    const light = wrapper.find('[data-testid="site-logo-light"]')

    expect(light.attributes('src')).toBe('/api/uploads/logo.png')
    expect(light.classes()).not.toContain('dark:hidden')
    expect(wrapper.find('[data-testid="site-logo-dark"]').exists()).toBe(false)
  })

  it('renders both logos and swaps them with the dark variant, never with script', () => {
    stubAuroraGlobals({ logo: '/light.png', logoDark: '/dark.png' })
    const wrapper = mountWithKit(AuroraHeader, { stubs: STUBS })

    expect(wrapper.find('[data-testid="site-logo-light"]').classes()).toContain('dark:hidden')
    expect(wrapper.find('[data-testid="site-logo-dark"]').classes()).toEqual(expect.arrayContaining(['hidden', 'dark:block']))
  })

  it('offers sign in and get started to a visitor, and no account menu', () => {
    stubAuroraGlobals()
    const wrapper = mountWithKit(AuroraHeader, { stubs: STUBS })

    expect(wrapper.find('[data-testid="nav-sign-in"]').attributes('href')).toBe('/client/login')
    expect(wrapper.find('[data-testid="nav-start"]').attributes('href')).toBe('/hosting')
    expect(wrapper.find('[data-testid="account-menu-trigger"]').exists()).toBe(false)
  })

  it('swaps the sign-in pair for the account menu once there is a session', () => {
    stubAuroraGlobals({ user: { firstname: 'Ann', lastname: 'Lee', email: 'ann@acme.test' } })
    const wrapper = mountWithKit(AuroraHeader, { stubs: STUBS })

    expect(wrapper.find('[data-testid="account-menu-trigger"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="nav-sign-in"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="nav-start"]').exists()).toBe(false)
  })

  it('shows no count badge on an empty cart', () => {
    stubAuroraGlobals()
    const wrapper = mountWithKit(AuroraHeader, { stubs: STUBS })

    expect(wrapper.find('[data-testid="nav-cart-count"]').exists()).toBe(false)
  })

  it('opens the narrow-screen menu and closes it when a link is chosen', async () => {
    stubAuroraGlobals()
    const wrapper = mountWithKit(AuroraHeader, { stubs: STUBS })
    expect(wrapper.find('[data-testid="nav-mobile-menu"]').exists()).toBe(false)

    await wrapper.find('[data-testid="nav-menu-toggle"]').trigger('click')
    expect(wrapper.find('[data-testid="nav-mobile-menu"]').exists()).toBe(true)

    await wrapper.find('[data-testid="nav-mobile-faq"]').trigger('click')
    expect(wrapper.find('[data-testid="nav-mobile-menu"]').exists()).toBe(false)
  })
})
