/**
 * Unit tests for UiDropdown: the trigger label, toggling the menu, choosing an option and the
 * disabled and error states.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiDropdown from '~/components/ui/UiDropdown.vue'

/** Options used by every test. */
const OPTIONS = [
  { value: 1, label: 'One' },
  { value: 2, label: 'Two' },
]

describe('UiDropdown', () => {
  it('shows the placeholder, then the selected label', () => {
    const empty = mountWithKit(UiDropdown, { props: { options: OPTIONS, placeholder: 'Choose' } })
    const chosen = mountWithKit(UiDropdown, { props: { options: OPTIONS, modelValue: 2, placeholder: 'Choose' } })

    expect(empty.find('button').text()).toBe('Choose')
    expect(chosen.find('button').text()).toBe('Two')
  })

  it('toggles aria-expanded when the trigger is clicked', async () => {
    const wrapper = mountWithKit(UiDropdown, { props: { options: OPTIONS, placeholder: 'Choose' } })
    expect(wrapper.find('button').attributes('aria-expanded')).toBe('false')

    await wrapper.find('button').trigger('click')

    expect(wrapper.find('button').attributes('aria-expanded')).toBe('true')
  })

  it('emits the chosen value and closes', async () => {
    const wrapper = mountWithKit(UiDropdown, { props: { options: OPTIONS, placeholder: 'Choose' } })
    await wrapper.find('button').trigger('click')

    await wrapper.find('[data-testid="ui-dropdown-option-2"]').trigger('click')

    expect(wrapper.emitted('update:modelValue')?.[0]).toEqual([2])
    expect(wrapper.find('button').attributes('aria-expanded')).toBe('false')
  })

  it('does not open when disabled', async () => {
    const wrapper = mountWithKit(UiDropdown, { props: { options: OPTIONS, disabled: true, placeholder: 'Choose' } })

    await wrapper.find('button').trigger('click')

    expect(wrapper.find('button').attributes('aria-expanded')).toBe('false')
  })

  it('shows the error line and the required asterisk', () => {
    const wrapper = mountWithKit(UiDropdown, {
      props: { options: OPTIONS, label: 'Plan', required: true, error: 'Pick a plan', placeholder: 'Choose' },
    })

    expect(wrapper.text()).toContain('Pick a plan')
    expect(wrapper.find('label span').text()).toBe('*')
  })

  it('passes the testid to the trigger', () => {
    const wrapper = mountWithKit(UiDropdown, { props: { options: OPTIONS, testid: 'plan-dropdown', placeholder: 'Choose' } })

    expect(wrapper.find('button').attributes('data-testid')).toBe('plan-dropdown')
  })
})
