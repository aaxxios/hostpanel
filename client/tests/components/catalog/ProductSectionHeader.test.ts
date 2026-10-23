/**
 * Unit tests for ProductSectionHeader: an h2 title with an optional lead line.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import ProductSectionHeader from '~/components/catalog/ProductSectionHeader.vue'

describe('ProductSectionHeader', () => {
  it('renders the title as an h2', () => {
    const title = mountWithKit(ProductSectionHeader, { props: { title: 'Choose Your Plan' } }).find('[data-testid="product-section-title"]')

    expect(title.element.tagName).toBe('H2')
    expect(title.text()).toBe('Choose Your Plan')
  })

  it('renders the lead line only when there is one', () => {
    const without = mountWithKit(ProductSectionHeader, { props: { title: 'Plans' } })
    expect(without.find('[data-testid="product-section-subtitle"]').exists()).toBe(false)

    const withLead = mountWithKit(ProductSectionHeader, { props: { title: 'Plans', subtitle: 'Pick one' } })
    expect(withLead.find('[data-testid="product-section-subtitle"]').text()).toBe('Pick one')
  })
})
