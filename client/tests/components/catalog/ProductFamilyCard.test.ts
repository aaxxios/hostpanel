/**
 * Unit tests for ProductFamilyCard: the cover (generated per type, or the operator's image), the
 * badge on it, the bullet cap, and the layout that pins the button to the bottom.
 */
import { beforeEach, describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import { stubPublicPageRuntime } from '~/tests/helpers/publicPage'
import ProductFamilyCard from '~/components/catalog/ProductFamilyCard.vue'
import { PRODUCT_TYPE_COVER } from '~/utils/productTypeCover'
import { ProductType } from '~/types/producttype'
import type { ProductFamily } from '~/types/productfamily'

/**
 * Builds a family.
 *
 * @param overrides Fields that differ from the defaults.
 * @returns The family.
 */
const family = (overrides: Partial<ProductFamily> = {}): ProductFamily => ({
  id: 'cloud-vps',
  gid: 2,
  icon: 'mdi:server-network',
  color: '#06b6d4',
  learnMoreUrl: '/products/cloud-vps',
  name: 'Cloud VPS',
  tagline: '',
  description: 'Isolated virtual servers',
  features: ['2 vCPU', '4 GB RAM', '80 GB NVMe', '4 TB traffic'],
  pricing: [],
  type: ProductType.Vps,
  ...overrides,
})

describe('ProductFamilyCard', () => {
  beforeEach(() => {
    stubPublicPageRuntime()
  })

  it('draws the cover generated for the family type', () => {
    const wrapper = mountWithKit(ProductFamilyCard, { props: { product: family() } })

    const art = wrapper.find('[data-testid="product-cover-art"]')
    expect(art.exists()).toBe(true)
    expect(art.classes().join(' ')).toContain('/covers/vps.svg')
    expect(PRODUCT_TYPE_COVER[ProductType.Vps]).toContain('/covers/vps.svg')
    expect(wrapper.find('[data-testid="product-cover-image"]').exists()).toBe(false)
  })

  it('falls back to the generic cover when the family has no type', () => {
    const wrapper = mountWithKit(ProductFamilyCard, { props: { product: family({ type: null }) } })

    expect(wrapper.find('[data-testid="product-cover-art"]').classes().join(' ')).toContain('/covers/generic.svg')
  })

  it('shows the operator image instead of the generated cover when the group has one', () => {
    const wrapper = mountWithKit(ProductFamilyCard, { props: { product: family({ image: '/uploads/vps.webp' }) } })

    expect(wrapper.find('[data-testid="product-cover-image"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="product-cover-art"]').exists()).toBe(false)
  })

  it('puts the featured badge on the cover, and only for a featured family', () => {
    const featured = mountWithKit(ProductFamilyCard, { props: { product: family({ featured: true }) } })
    const plain = mountWithKit(ProductFamilyCard, { props: { product: family() } })

    expect(featured.find('[data-testid="product-cover"] [data-testid="product-featured"]').exists()).toBe(true)
    expect(plain.find('[data-testid="product-featured"]').exists()).toBe(false)
  })

  it('lists at most three bullets and no list when the family has none', () => {
    const wrapper = mountWithKit(ProductFamilyCard, { props: { product: family() } })
    const empty = mountWithKit(ProductFamilyCard, { props: { product: family({ features: [] }) } })

    expect(wrapper.find('[data-testid="product-bullets"]').text()).toContain('80 GB NVMe')
    expect(wrapper.find('[data-testid="product-bullets"]').text()).not.toContain('4 TB traffic')
    expect(empty.find('[data-testid="product-bullets"]').exists()).toBe(false)
  })

  it('is a column that fills its row, so the button is pinned to the bottom with no gap', () => {
    const wrapper = mountWithKit(ProductFamilyCard, { props: { product: family() } })
    const card = wrapper.find('[data-testid="product-card-cloud-vps"]')

    expect(card.classes()).toEqual(expect.arrayContaining(['flex', 'flex-col', 'self-stretch']))
    expect(wrapper.find('[data-testid="product-link-cloud-vps"]').classes()).toEqual(expect.arrayContaining(['flex-col', 'flex-1']))
    expect(wrapper.find('.line-clamp-3').exists()).toBe(true)
  })

  it('links to the learn-more page, or to the detail page when there is none', () => {
    const linked = mountWithKit(ProductFamilyCard, { props: { product: family() } })
    const bare = mountWithKit(ProductFamilyCard, { props: { product: family({ learnMoreUrl: '' }) } })

    expect(linked.find('a').attributes('href')).toBe('/en/products/cloud-vps')
    expect(bare.find('a').attributes('href')).toBe('/en/products/cloud-vps')
  })
})
