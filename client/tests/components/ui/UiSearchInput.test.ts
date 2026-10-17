/**
 * Unit tests for SearchInput: the field, the clear button and Escape, the sizes and the placeholder.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import SearchInput from '~/components/ui/UiSearchInput.vue'
import { UiSize } from '~/utils/ui/enums'

describe('SearchInput', () => {
  it('uses the translated default placeholder and accepts a custom one', () => {
    expect(mountWithKit(SearchInput).find('input').attributes('placeholder')).toBe('ui.search')
    expect(mountWithKit(SearchInput, { props: { placeholder: 'Filter' } }).find('input').attributes('placeholder')).toBe('Filter')
  })

  it('emits the typed query', async () => {
    const wrapper = mountWithKit(SearchInput)

    await wrapper.find('input').setValue('abc')

    expect(wrapper.emitted('update:modelValue')?.[0]).toEqual(['abc'])
  })

  it('shows the clear button only while there is a query', () => {
    expect(mountWithKit(SearchInput).find('[data-testid="search-input-clear"]').exists()).toBe(false)
    expect(mountWithKit(SearchInput, { props: { modelValue: 'abc' } }).find('[data-testid="search-input-clear"]').exists()).toBe(true)
  })

  it('clears from the button and from Escape', async () => {
    const wrapper = mountWithKit(SearchInput, { props: { modelValue: 'abc' } })

    await wrapper.find('[data-testid="search-input-clear"]').trigger('click')
    await wrapper.find('input').trigger('keydown', { key: 'Escape' })

    expect(wrapper.emitted('update:modelValue')).toEqual([[''], ['']])
  })

  it('maps the sizes to vertical padding', () => {
    expect(mountWithKit(SearchInput).find('input').classes()).toContain('py-1.5')
    expect(mountWithKit(SearchInput, { props: { size: UiSize.Md } }).find('input').classes()).toContain('py-2')
  })

  it('passes the testid to the input', () => {
    const wrapper = mountWithKit(SearchInput, { props: { testid: 'table-filter' } })

    expect(wrapper.find('input').attributes('data-testid')).toBe('table-filter')
  })
})
