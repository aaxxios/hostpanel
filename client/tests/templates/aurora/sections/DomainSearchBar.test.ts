/**
 * Unit tests for DomainSearchBar: the e2e test ids, the search event, and the category chips.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import DomainSearchBar from '~/templates/aurora/sections/DomainSearchBar.vue'
import { ALL_CATEGORY } from '~/templates/aurora/types/allcategory'
import { stubAuroraGlobals } from '../auroraGlobals'

describe('DomainSearchBar', () => {
  it('emits the typed term when submitted, through the ids the e2e specs use', async () => {
    stubAuroraGlobals()
    const wrapper = mountWithKit(DomainSearchBar)

    await wrapper.find('[data-testid="domain-search-input"]').setValue('shop')
    await wrapper.find('form').trigger('submit')

    expect(wrapper.emitted('search')).toEqual([['shop']])
    expect(wrapper.find('[data-testid="domain-search-submit"]').attributes('type')).toBe('submit')
  })

  it('gives the field a placeholder and disables it without priced extensions', () => {
    stubAuroraGlobals()
    const input = mountWithKit(DomainSearchBar, { props: { hasZones: false } }).find('[data-testid="domain-search-input"]')

    expect(input.attributes('placeholder')).toBe('aurora.domainSearch.placeholder')
    expect(input.attributes('disabled')).toBeDefined()
  })

  it('hides the filter when the registrar supplied one category or none', () => {
    stubAuroraGlobals()

    expect(mountWithKit(DomainSearchBar, { props: { categories: [ALL_CATEGORY] } }).find('[data-testid^="domain-category-"]').exists()).toBe(false)
  })

  it('shows a chip per category, labels the sentinel "all" and marks the active one', () => {
    stubAuroraGlobals()
    const wrapper = mountWithKit(DomainSearchBar, { props: { categories: [ALL_CATEGORY, 'Popular'], active: 'Popular' } })

    expect(wrapper.find(`[data-testid="domain-category-${ALL_CATEGORY}"]`).text()).toBe('aurora.tldTable.all')
    expect(wrapper.find('[data-testid="domain-category-Popular"]').attributes('aria-selected')).toBe('true')
    expect(wrapper.find(`[data-testid="domain-category-${ALL_CATEGORY}"]`).attributes('aria-selected')).toBe('false')
    expect(wrapper.find('[data-testid="domain-category-Popular"]').text()).toBe('domains.pricing.categories.popular')
  })

  it('capitalises a category that has no translation', () => {
    stubAuroraGlobals()
    const wrapper = mountWithKit(DomainSearchBar, { props: { categories: [ALL_CATEGORY, 'gaming'] } })

    expect(wrapper.find('[data-testid="domain-category-gaming"]').text()).toBe('Gaming')
  })

  it('emits the chosen category', async () => {
    stubAuroraGlobals()
    const wrapper = mountWithKit(DomainSearchBar, { props: { categories: [ALL_CATEGORY, 'Popular'] } })

    await wrapper.find('[data-testid="domain-category-Popular"]').trigger('click')

    expect(wrapper.emitted('update:category')).toEqual([['Popular']])
  })
})
