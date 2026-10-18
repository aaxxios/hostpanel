/**
 * Unit tests for SupportCheckboxField: the box and its caption both toggle the model.
 */
import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import SupportCheckboxField from '@/components/support/SupportCheckboxField.vue'

/**
 * Mounts the field.
 *
 * @param modelValue - Whether the box starts checked.
 * @returns The wrapper.
 */
const mountField = (modelValue: boolean) =>
  mount(SupportCheckboxField, { props: { modelValue, label: 'Hidden', testid: 'hidden-box' } })

describe('SupportCheckboxField', () => {
  it('shows the caption and reflects the checked state', () => {
    const wrapper = mountField(true)

    expect(wrapper.get('[data-testid="hidden-box-label"]').text()).toBe('Hidden')
    expect(wrapper.get('[data-testid="hidden-box"]').attributes('aria-checked')).toBe('true')
  })

  it('toggles from the box', async () => {
    const wrapper = mountField(false)

    await wrapper.get('[data-testid="hidden-box"]').trigger('click')

    expect(wrapper.emitted('update:modelValue')?.[0]).toEqual([true])
  })

  it('toggles from the caption', async () => {
    const wrapper = mountField(true)

    await wrapper.get('[data-testid="hidden-box-label"]').trigger('click')

    expect(wrapper.emitted('update:modelValue')?.[0]).toEqual([false])
  })
})
