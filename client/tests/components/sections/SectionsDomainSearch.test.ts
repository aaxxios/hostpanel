/**
 * Unit tests for SectionsDomainSearch: the field has a placeholder, and a search opens the
 * domains page with the typed name (or without one when the field is empty).
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import { stubPublicPageRuntime } from '~/tests/helpers/publicPage'
import { DOMAIN_SEARCH_QUERY_PARAM } from '~/constants/domainSearchQuery'
import SectionsDomainSearch from '~/components/sections/SectionsDomainSearch.vue'

describe('SectionsDomainSearch', () => {
  beforeEach(() => {
    stubPublicPageRuntime()
    vi.mocked(navigateTo).mockClear()
  })

  it('gives the field a placeholder', () => {
    const input = mountWithKit(SectionsDomainSearch).find('[data-testid="search-bar-input"]')

    expect(input.attributes('placeholder')).toBe('hero.search.placeholder')
  })

  it('opens the domains page with the typed name', async () => {
    const wrapper = mountWithKit(SectionsDomainSearch)
    await wrapper.find('[data-testid="search-bar-input"]').setValue(' my site ')

    await wrapper.find('[data-testid="search-bar-submit"]').trigger('click')

    expect(navigateTo).toHaveBeenCalledWith(`/en/domains?${DOMAIN_SEARCH_QUERY_PARAM}=my%20site`)
  })

  it('opens the domains page without a search when the field is empty', async () => {
    const wrapper = mountWithKit(SectionsDomainSearch)

    await wrapper.find('[data-testid="search-bar-submit"]').trigger('click')

    expect(navigateTo).toHaveBeenCalledWith('/en/domains')
  })
})
