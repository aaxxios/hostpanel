/**
 * Unit tests for UiRadio: the selected state, choosing from the circle and the label, the
 * disabled state and the group name.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiRadio from '~/components/ui/UiRadio.vue'

describe('UiRadio', () => {
  it('marks only the matching option as checked', () => {
    const selected = mountWithKit(UiRadio, { props: { name: 'plan', value: 'pro', modelValue: 'pro' } })
    const other = mountWithKit(UiRadio, { props: { name: 'plan', value: 'free', modelValue: 'pro' } })

    expect(selected.find('[role="radio"]').attributes('aria-checked')).toBe('true')
    expect(other.find('[role="radio"]').attributes('aria-checked')).toBe('false')
  })

  it('emits its value when the circle is clicked', async () => {
    const wrapper = mountWithKit(UiRadio, { props: { name: 'plan', value: 'pro' } })

    await wrapper.find('[role="radio"]').trigger('click')

    expect(wrapper.emitted('update:modelValue')?.[0]).toEqual(['pro'])
  })

  it('emits its value when the label is clicked', async () => {
    const wrapper = mountWithKit(UiRadio, { props: { name: 'plan', value: 'pro', label: 'Pro' } })

    await wrapper.find('label').trigger('click')

    expect(wrapper.emitted('update:modelValue')?.[0]).toEqual(['pro'])
  })

  it('does not emit when disabled', async () => {
    const wrapper = mountWithKit(UiRadio, { props: { name: 'plan', value: 'pro', label: 'Pro', disabled: true } })

    await wrapper.find('[role="radio"]').trigger('click')
    await wrapper.find('label').trigger('click')

    expect(wrapper.emitted('update:modelValue')).toBeUndefined()
  })

  it('keeps the group name on the native input and shows the description', () => {
    const wrapper = mountWithKit(UiRadio, { props: { name: 'plan', value: 'pro', label: 'Pro', description: 'Best value', testid: 'plan-pro' } })

    expect(wrapper.find('input[type="radio"]').attributes('name')).toBe('plan')
    expect(wrapper.text()).toContain('Best value')
    expect(wrapper.find('[role="radio"]').attributes('data-testid')).toBe('plan-pro')
  })

  it('tags the native input derived from the testid, and leaves it untagged without one', () => {
    const tagged = mountWithKit(UiRadio, { props: { name: 'plan', value: 'pro', testid: 'plan-pro' } })
    const plain = mountWithKit(UiRadio, { props: { name: 'plan', value: 'pro' } })

    expect(tagged.find('input').attributes('data-testid')).toBe('plan-pro-input')
    expect(plain.find('input').attributes('data-testid')).toBeUndefined()
  })
})
