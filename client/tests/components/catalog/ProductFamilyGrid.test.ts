/**
 * Unit tests for ProductFamilyGrid: one shortcut and one card per family, the links, the accent
 * colour of each card and the tagline and feature rules.
 */
import { beforeEach, describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import { stubPublicPageRuntime } from '~/tests/helpers/publicPage'
import ProductFamilyGrid from '~/components/catalog/ProductFamilyGrid.vue'
import type { ProductFamily } from '~/types/productfamily'

/**
 * Builds a family.
 *
 * @param overrides Fields that differ from the defaults.
 * @returns The family.
 */
const family = (overrides: Partial<ProductFamily> = {}): ProductFamily => ({
  id: 'smartlearn-system',
  gid: 3,
  icon: 'school',
  color: '#8b5cf6',
  learnMoreUrl: '/products/smartlearn-system',
  name: 'SmartLearn System',
  tagline: '',
  description: 'Learn online',
  features: ['One', 'Two', 'Three', 'Four'],
  pricing: [],
  ...overrides,
})

describe('ProductFamilyGrid', () => {
  beforeEach(() => {
    stubPublicPageRuntime()
  })

  it('renders a shortcut and a card for every family', () => {
    const wrapper = mountWithKit(ProductFamilyGrid, { props: { products: [family(), family({ id: 'taskero', name: 'Taskero' })] } })

    expect(wrapper.findAll('[data-testid^="product-nav-"]')).toHaveLength(2)
    expect(wrapper.findAll('[data-testid^="product-card-"]')).toHaveLength(2)
  })

  it('links a family to its learn-more page, or to its detail page when it has none', () => {
    const wrapper = mountWithKit(ProductFamilyGrid, {
      props: { products: [family(), family({ id: 'taskero', name: 'Taskero', learnMoreUrl: '' })] },
    })

    expect(wrapper.find('[data-testid="product-link-smartlearn-system"]').attributes('href')).toBe('/en/products/smartlearn-system')
    expect(wrapper.find('[data-testid="product-link-taskero"]').attributes('href')).toBe('/en/products/taskero')
  })

  it('paints each card in its family accent colour', () => {
    const wrapper = mountWithKit(ProductFamilyGrid, { props: { products: [family()] } })

    const accent = wrapper.find('[data-testid="ui-accent"]').element as HTMLElement
    expect(accent.style.getPropertyValue('--accent')).toBe('#8b5cf6')
  })

  it('shows at most three features and a tagline only when the family has one', () => {
    const wrapper = mountWithKit(ProductFamilyGrid, {
      props: { products: [family(), family({ id: 'domains', name: 'Domains', tagline: 'Find yours', features: [] })] },
    })

    const first = wrapper.find('[data-testid="product-card-smartlearn-system"]')
    expect(first.text()).toContain('Three')
    expect(first.text()).not.toContain('Four')
    expect(first.text()).not.toContain('Find yours')
    const domains = wrapper.find('[data-testid="product-card-domains"]')
    expect(domains.text()).toContain('Find yours')
    expect(domains.text()).toContain('products.products.badge')
  })
})
