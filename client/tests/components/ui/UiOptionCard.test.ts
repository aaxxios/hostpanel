/**
 * Unit tests for UiOptionCard: the selected state, choosing by the whole row, the single-line
 * layout pieces, the disabled state and the palette props a template passes.
 */
import { describe, expect, it } from 'vitest'
import { CreditCard } from 'lucide-vue-next'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiOptionCard from '~/components/ui/UiOptionCard.vue'

/** Props every test starts from. */
const base = { name: 'method', value: 'card', label: 'Card', testid: 'opt' }

describe('UiOptionCard', () => {
  it('marks only the matching row as checked', () => {
    const on = mountWithKit(UiOptionCard, { props: { ...base, modelValue: 'card' } })
    const off = mountWithKit(UiOptionCard, { props: { ...base, modelValue: 'bank' } })

    expect(on.find('[data-testid="opt"]').attributes('data-state')).toBe('checked')
    expect((on.find('[data-testid="opt-input"]').element as HTMLInputElement).checked).toBe(true)
    expect(off.find('[data-testid="opt"]').attributes('data-state')).toBe('unchecked')
    expect((off.find('[data-testid="opt-input"]').element as HTMLInputElement).checked).toBe(false)
  })

  it('is a label around a native radio, so the whole row is the click target', () => {
    const wrapper = mountWithKit(UiOptionCard, { props: base })

    const row = wrapper.find('[data-testid="opt"]')
    expect(row.element.tagName).toBe('LABEL')
    expect(row.attributes('for')).toBe(wrapper.find('[data-testid="opt-input"]').attributes('id'))
    expect(wrapper.find('input[type="radio"]').attributes('name')).toBe('method')
  })

  it('emits its value when the native radio changes', async () => {
    const wrapper = mountWithKit(UiOptionCard, { props: base })

    await wrapper.find('[data-testid="opt-input"]').setValue(true)

    expect(wrapper.emitted('update:modelValue')?.[0]).toEqual(['card'])
  })

  it('shows the icon tile, the label and the description together', () => {
    const wrapper = mountWithKit(UiOptionCard, { props: { ...base, icon: CreditCard, description: 'Pay by card' } })

    expect(wrapper.find('[data-testid="ui-tile"]').exists()).toBe(true)
    expect(wrapper.text()).toContain('Card')
    expect(wrapper.text()).toContain('Pay by card')
    expect(wrapper.find('[data-testid="opt"]').classes()).toContain('items-center')
  })

  it('leaves out the tile and the description when none is given', () => {
    const wrapper = mountWithKit(UiOptionCard, { props: base })

    expect(wrapper.find('[data-testid="ui-tile"]').exists()).toBe(false)
    expect(wrapper.text()).toBe('Card')
  })

  it('draws a focus ring when the radio has keyboard focus', () => {
    const wrapper = mountWithKit(UiOptionCard, { props: base })

    expect(wrapper.find('[data-testid="opt"]').classes()).toContain('has-[:focus-visible]:ring-2')
  })

  it('uses the active or idle palette classes it is given', () => {
    const palette = { activeClass: 'border-x-on', idleClass: 'border-x-off', accentClass: 'text-x-on', mutedClass: 'text-x-off' }
    const on = mountWithKit(UiOptionCard, { props: { ...base, ...palette, modelValue: 'card' } })
    const off = mountWithKit(UiOptionCard, { props: { ...base, ...palette, modelValue: 'other' } })

    expect(on.find('[data-testid="opt"]').classes()).toContain('border-x-on')
    expect(on.find('.ui-option-card-indicator').classes()).toContain('text-x-on')
    expect(off.find('[data-testid="opt"]').classes()).toContain('border-x-off')
    expect(off.find('.ui-option-card-indicator').classes()).toContain('text-x-off')
  })

  it('disables the radio and dims the row', () => {
    const wrapper = mountWithKit(UiOptionCard, { props: { ...base, disabled: true } })

    expect(wrapper.find('[data-testid="opt-input"]').attributes('disabled')).toBeDefined()
    expect(wrapper.find('[data-testid="opt"]').classes()).toContain('cursor-not-allowed')
  })
})
