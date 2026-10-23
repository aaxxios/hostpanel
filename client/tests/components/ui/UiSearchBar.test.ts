/**
 * Unit tests for SearchBar: v-model on the field, the search event from Enter and the button,
 * and the loading state.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import SearchBar from '~/components/ui/UiSearchBar.vue'

describe('SearchBar', () => {
  it('shows the query and the placeholder in the field', () => {
    const wrapper = mountWithKit(SearchBar, { props: { modelValue: 'example.com', placeholder: 'Find a domain' } })

    const input = wrapper.find('[data-testid="search-bar-input"]')
    expect((input.element as HTMLInputElement).value).toBe('example.com')
    expect(input.attributes('placeholder')).toBe('Find a domain')
  })

  it('emits the typed query', async () => {
    const wrapper = mountWithKit(SearchBar, { props: { placeholder: 'x' } })

    await wrapper.find('[data-testid="search-bar-input"]').setValue('abc')

    expect(wrapper.emitted('update:modelValue')?.[0]).toEqual(['abc'])
  })

  it('emits search from the button and from Enter', async () => {
    const wrapper = mountWithKit(SearchBar, { props: { placeholder: 'x' } })

    await wrapper.find('[data-testid="search-bar-submit"]').trigger('click')
    await wrapper.find('[data-testid="search-bar-input"]').trigger('keydown', { key: 'Enter' })

    expect(wrapper.emitted('search')).toHaveLength(2)
  })

  it('uses the translated default button label and accepts a custom one', () => {
    const defaults = mountWithKit(SearchBar, { props: { placeholder: 'x' } })
    const custom = mountWithKit(SearchBar, { props: { placeholder: 'x', buttonLabel: 'Check' } })

    expect(defaults.find('[data-testid="search-bar-submit"]').text()).toBe('ui.searchButton')
    expect(custom.find('[data-testid="search-bar-submit"]').text()).toBe('Check')
  })

  it('gives the field and the button the same size step, large by default', () => {
    const large = mountWithKit(SearchBar, { props: { placeholder: 'x' } })
    const medium = mountWithKit(SearchBar, { props: { placeholder: 'x', size: 'md' } })
    /** Reads the minimum-height class of the element with the given test id. */
    const height = (wrapper: typeof large, id: string): string | undefined =>
      wrapper.find(`[data-testid="${id}"]`).classes().find(c => c.startsWith('min-h-'))

    expect(height(large, 'search-bar-input')).toBe('min-h-14')
    expect(height(large, 'search-bar-submit')).toBe('min-h-14')
    expect(height(medium, 'search-bar-input')).toBe('min-h-11')
    expect(height(medium, 'search-bar-submit')).toBe('min-h-11')
  })

  it('disables the field and the button while loading', () => {
    const wrapper = mountWithKit(SearchBar, { props: { loading: true, placeholder: 'x' } })

    expect(wrapper.find('[data-testid="search-bar-input"]').attributes('disabled')).toBeDefined()
    expect(wrapper.find('[data-testid="search-bar-submit"]').attributes('disabled')).toBeDefined()
  })
})
