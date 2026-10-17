/**
 * Unit tests for LayoutFooter: the four columns, the localised links, the contact details and
 * the back-to-top button.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { defineComponent, h } from 'vue'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import { stubPublicPageRuntime } from '~/tests/helpers/publicPage'
import LayoutFooter from '~/components/layout/LayoutFooter.vue'

/** Stand-in for the logo component, which needs the brand plugin. */
const AppLogoStub = defineComponent({
  name: 'AppLogo',
  setup: () => () => h('img', { 'data-testid': 'app-logo-stub' }),
})

/** Operator settings the stood-in `usePortalSettings` answers with, by setting key. */
let settings: Record<string, string> = {}

/**
 * Mounts the footer with the logo stood in for.
 *
 * @returns The wrapper.
 */
const mountFooter = () => mountWithKit(LayoutFooter, { components: { AppLogo: AppLogoStub } })

describe('LayoutFooter', () => {
  beforeEach(() => {
    stubPublicPageRuntime()
    settings = {
      'portal.contact.email': 'hello@portal.test',
      'portal.social.twitter': 'https://twitter.test/p',
      'portal.social.linkedin': 'https://linkedin.test/p',
      'portal.social.facebook': 'https://facebook.test/p',
    }
    vi.stubGlobal('usePortalSettings', () => ({ get: (key: string) => settings[key] ?? '' }))
  })

  it('renders the logo and the description', () => {
    const wrapper = mountFooter()

    expect(wrapper.find('[data-testid="app-logo-stub"]').exists()).toBe(true)
    expect(wrapper.text()).toContain('footer.description')
  })

  it('links the navigation and service columns through the locale path', () => {
    const wrapper = mountFooter()

    const nav = wrapper.findAll('[data-testid="footer-nav-link"]').map((link) => link.attributes('href'))
    expect(nav).toEqual(['/en/', '/en/announcements', '/en/knowledgebase', '/en/faq', '/en/contact'])
    expect(wrapper.find('[data-testid="footer-service-link"]').attributes('href')).toBe('/en/products')
  })

  it('links the four legal pages', () => {
    const wrapper = mountFooter()

    const legal = wrapper.findAll('[data-testid="footer-legal-link"]').map((link) => link.attributes('href'))
    expect(legal).toEqual(['/en/privacy', '/en/cookie-policy', '/en/terms', '/en/refund-policy'])
  })

  it('offers the contact address and phone as mailto and tel links', () => {
    const wrapper = mountFooter()

    expect(wrapper.find('[data-testid="footer-email"]').attributes('href')).toBe('mailto:hello@portal.test')
    expect(wrapper.find('[data-testid="footer-phone"]').attributes('href')).toBe('tel:+37433731673')
  })

  it('opens the social profiles in a new tab', () => {
    const wrapper = mountFooter()

    const social = wrapper.findAll('[data-testid="footer-social-link"]')
    expect(social).toHaveLength(3)
    for (const link of social) {
      expect(link.attributes('target')).toBe('_blank')
      expect(link.attributes('rel')).toBe('noopener noreferrer')
    }
  })

  it('hides the contact address and every social link that is not configured', () => {
    settings = { 'portal.social.linkedin': 'https://linkedin.test/p' }
    const wrapper = mountFooter()

    expect(wrapper.find('[data-testid="footer-email"]').exists()).toBe(false)
    const social = wrapper.findAll('[data-testid="footer-social-link"]')
    expect(social).toHaveLength(1)
    expect(social[0]?.attributes('href')).toBe('https://linkedin.test/p')
  })

  it('renders no social row at all when none is configured', () => {
    settings = {}
    const wrapper = mountFooter()

    expect(wrapper.findAll('[data-testid="footer-social-link"]')).toHaveLength(0)
  })

  it('scrolls to the top from the back-to-top button', async () => {
    const scrollTo = vi.fn()
    vi.stubGlobal('scrollTo', scrollTo)
    window.scrollTo = scrollTo
    const wrapper = mountFooter()

    await wrapper.find('[data-testid="footer-back-to-top"]').trigger('click')

    expect(scrollTo).toHaveBeenCalledWith({ top: 0, behavior: 'smooth' })
  })

  it('prints the current year in the copyright line', () => {
    const wrapper = mountFooter()

    expect(wrapper.text()).toContain(String(new Date().getFullYear()))
  })
})
