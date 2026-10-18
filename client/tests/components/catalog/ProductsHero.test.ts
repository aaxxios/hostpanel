/**
 * Unit tests for ProductsHero: the headline, the trust badges, the scroll to the catalogue and the
 * link to the contact page.
 */
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import { stubPublicPageRuntime } from '~/tests/helpers/publicPage'
import ProductsHero from '~/components/catalog/ProductsHero.vue'

/** Mounts the hero with the translation component stood in for. */
const mountHero = () => mountWithKit(ProductsHero, { stubs: { 'i18n-t': true } })

describe('ProductsHero', () => {
  beforeEach(() => {
    stubPublicPageRuntime()
  })

  afterEach(() => {
    document.body.innerHTML = ''
  })

  it('renders the badge, the two headline lines and the trust badges', () => {
    const wrapper = mountHero()

    expect(wrapper.find('h1').exists()).toBe(true)
    expect(wrapper.text()).toContain('products.hero.badge')
    expect(wrapper.text()).toContain('products.hero.headline1')
    expect(wrapper.text()).toContain('products.hero.headline2')
    expect(wrapper.text()).toContain('products.hero.trustBadges.soc2')
    expect(wrapper.text()).toContain('products.hero.trustBadges.uptime')
  })

  it('links Book a Demo to the contact page', () => {
    const wrapper = mountHero()

    expect(wrapper.find('[data-testid="products-hero-demo"]').attributes('href')).toBe('/en/contact')
  })

  it('scrolls the catalogue section into view from View Products', async () => {
    const target = document.createElement('section')
    target.id = 'products-section'
    target.scrollIntoView = vi.fn()
    document.body.appendChild(target)
    const wrapper = mountHero()

    await wrapper.find('[data-testid="products-hero-view"]').trigger('click')

    expect(target.scrollIntoView).toHaveBeenCalledWith({ behavior: 'smooth' })
  })
})
