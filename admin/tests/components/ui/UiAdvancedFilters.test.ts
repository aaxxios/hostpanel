/**
 * Unit tests for UiAdvancedFilters: row management and the emitted complete rows.
 */
import { describe, expect, it } from 'vitest'
import { nextTick } from 'vue'
import { mount } from '@vue/test-utils'
import UiAdvancedFilters from '@/components/ui/UiAdvancedFilters.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import { createTestI18n } from '../../helpers/i18n'

/** Fields offered to the filters. */
const fields = [
  { value: 'name', label: 'Name' },
  { value: 'email', label: 'Email' },
]

/**
 * Mounts the filters with the English catalogue.
 *
 * @param max - Row limit.
 * @returns The wrapper.
 */
const mountFilters = (max?: number) =>
  mount(UiAdvancedFilters, { props: { fields, max }, global: { plugins: [createTestI18n()] } })

describe('UiAdvancedFilters', () => {
  it('starts with one row and no remove button', () => {
    const wrapper = mountFilters()

    expect(wrapper.findAll('[data-testid^="advanced-filters-value-"]')).toHaveLength(1)
    expect(wrapper.find('[data-testid="advanced-filters-remove-0"]').exists()).toBe(false)
    expect(wrapper.text()).toContain('Filter 1')
  })

  it('adds rows up to the limit', async () => {
    const wrapper = mountFilters(2)

    await wrapper.get('[data-testid="advanced-filters-add"]').trigger('click')

    expect(wrapper.findAll('[data-testid^="advanced-filters-value-"]')).toHaveLength(2)
    expect(wrapper.find('[data-testid="advanced-filters-add"]').exists()).toBe(false)
  })

  it('removes a row but always keeps one', async () => {
    const wrapper = mountFilters()
    await wrapper.get('[data-testid="advanced-filters-add"]').trigger('click')

    await wrapper.get('[data-testid="advanced-filters-remove-0"]').trigger('click')

    expect(wrapper.findAll('[data-testid^="advanced-filters-value-"]')).toHaveLength(1)
    expect(wrapper.find('[data-testid="advanced-filters-remove-0"]').exists()).toBe(false)
  })

  it('emits only complete rows', async () => {
    const wrapper = mountFilters()

    await wrapper.get('[data-testid="advanced-filters-value-0"]').setValue('acme')
    await nextTick()
    const afterValueOnly = wrapper.emitted('update:filters')

    expect(afterValueOnly?.[afterValueOnly.length - 1]).toEqual([[]])

    wrapper.findAllComponents(UiSelect)[0]?.vm.$emit('update:modelValue', 'name')
    await nextTick()
    await nextTick()
    const emitted = wrapper.emitted('update:filters')

    expect(emitted?.[emitted.length - 1]).toEqual([[{ field: 'name', condition: 'exact', value: 'acme' }]])
  })
})
