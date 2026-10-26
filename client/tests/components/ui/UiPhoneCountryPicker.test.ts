/**
 * Unit tests for the phone country picker: the trigger, the empty search, filtering, keyboard
 * choice, the selected mark, closing, and the flip above the trigger.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import type { VueWrapper } from '@vue/test-utils'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import PhoneCountryPicker from '~/components/ui/UiPhoneCountryPicker.vue'

/** Wrappers mounted into the document, unmounted after each test. */
const mounted: VueWrapper[] = []

/**
 * Mounts the picker attached to the document.
 *
 * @param props Props of the picker.
 * @returns The wrapper.
 */
const mountPicker = (props: Record<string, unknown> = {}): VueWrapper => {
  const wrapper = mountWithKit(PhoneCountryPicker, { props: { modelValue: 'AM', ...props }, attachToBody: true })
  mounted.push(wrapper)
  return wrapper
}

afterEach(() => {
  mounted.splice(0).forEach((wrapper) => wrapper.unmount())
  vi.restoreAllMocks()
  vi.unstubAllGlobals()
})

describe('PhoneCountryPicker', () => {
  it('shows the flag and calling code of the chosen country on the trigger', () => {
    const wrapper = mountPicker({ testid: 'trigger' })

    const trigger = wrapper.get('[data-testid="trigger"]')
    expect(trigger.text()).toContain('+374')
    expect(trigger.find('.vti__flag.am').exists()).toBe(true)
    expect(trigger.attributes('aria-expanded')).toBe('false')
  })

  it('opens with an empty search field that has a placeholder', async () => {
    const wrapper = mountPicker()

    await wrapper.get('button').trigger('click')

    const search = wrapper.get('[data-testid="phone-country-search"]')
    expect((search.element as HTMLInputElement).value).toBe('')
    expect(search.attributes('placeholder')).toBe('ui.searchCountry')
    expect(wrapper.get('button').attributes('aria-expanded')).toBe('true')
  })

  it('lists countries with name, muted native name, calling code, and marks the chosen one', async () => {
    const wrapper = mountPicker()

    await wrapper.get('button').trigger('click')

    const row = wrapper.get('[data-testid="phone-country-AM"]')
    expect(row.attributes('aria-selected')).toBe('true')
    expect(row.text()).toContain('Armenia')
    expect(row.text()).toContain('Հայաստան')
    expect(row.text()).toContain('+374')
    expect(row.find('svg').exists()).toBe(true)
    expect(wrapper.get('[data-testid="phone-country-US"]').attributes('aria-selected')).toBe('false')
  })

  it('filters by name, native name, ISO code and calling code', async () => {
    const wrapper = mountPicker()
    await wrapper.get('button').trigger('click')
    const search = wrapper.get('[data-testid="phone-country-search"]')

    await search.setValue('afgh')
    expect(wrapper.find('[data-testid="phone-country-AF"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="phone-country-AM"]').exists()).toBe(false)

    await search.setValue('Հայաստան')
    expect(wrapper.findAll('[role="option"]').map((option) => option.attributes('data-testid'))).toEqual(['phone-country-AM'])

    await search.setValue('de')
    expect(wrapper.find('[data-testid="phone-country-DE"]').exists()).toBe(true)

    await search.setValue('+44')
    expect(wrapper.find('[data-testid="phone-country-GB"]').exists()).toBe(true)
  })

  it('shows an empty state when nothing matches', async () => {
    const wrapper = mountPicker()
    await wrapper.get('button').trigger('click')

    await wrapper.get('[data-testid="phone-country-search"]').setValue('zzzzqq')

    expect(wrapper.get('[data-testid="phone-country-empty"]').text()).toBe('ui.noCountries')
  })

  it('chooses a country with a click and closes', async () => {
    const wrapper = mountPicker()
    await wrapper.get('button').trigger('click')

    await wrapper.get('[data-testid="phone-country-GB"]').trigger('click')

    expect(wrapper.emitted('update:modelValue')?.[0]).toEqual(['GB'])
    expect(wrapper.find('[data-testid="phone-country-panel"]').exists()).toBe(false)
  })

  it('moves the highlight with the arrow keys and chooses with Enter', async () => {
    const wrapper = mountPicker()
    await wrapper.get('button').trigger('click')
    const search = wrapper.get('[data-testid="phone-country-search"]')
    await search.setValue('united')
    const options = wrapper.findAll('[role="option"]')
    const first = options[0]!.attributes('data-testid')!.replace('phone-country-', '') // the search matched at least two countries
    const second = options[1]!.attributes('data-testid')!.replace('phone-country-', '') // same

    await search.trigger('keydown', { key: 'ArrowDown' })
    expect(search.attributes('aria-activedescendant')).toContain(second)
    await search.trigger('keydown', { key: 'ArrowUp' })
    expect(search.attributes('aria-activedescendant')).toContain(first)
    await search.trigger('keydown', { key: 'ArrowDown' })
    await search.trigger('keydown', { key: 'Enter' })

    expect(wrapper.emitted('update:modelValue')?.[0]).toEqual([second])
  })

  it('closes on Escape and returns focus to the trigger', async () => {
    const wrapper = mountPicker()
    await wrapper.get('button').trigger('click')

    await wrapper.get('[data-testid="phone-country-search"]').trigger('keydown', { key: 'Escape' })

    expect(wrapper.find('[data-testid="phone-country-panel"]').exists()).toBe(false)
    expect(document.activeElement).toBe(wrapper.get('button').element)
  })

  it('opens from the trigger with the arrow key', async () => {
    const wrapper = mountPicker()

    await wrapper.get('button').trigger('keydown', { key: 'ArrowDown' })

    expect(wrapper.find('[data-testid="phone-country-panel"]').exists()).toBe(true)
  })

  it('does not open while disabled', async () => {
    const wrapper = mountPicker({ disabled: true })

    await wrapper.get('button').trigger('click')

    expect(wrapper.find('[data-testid="phone-country-panel"]').exists()).toBe(false)
  })

  it('opens below the trigger when there is room, and above it when there is not', async () => {
    const wrapper = mountPicker()
    const trigger = wrapper.get('button').element as HTMLButtonElement
    const rect = vi.spyOn(trigger, 'getBoundingClientRect')
    vi.stubGlobal('innerHeight', 900)
    rect.mockReturnValue({ top: 100, bottom: 144 } as DOMRect)
    await wrapper.get('button').trigger('click')
    expect(wrapper.get('[data-testid="phone-country-panel"]').classes()).toContain('top-full')
    await wrapper.get('button').trigger('click')

    vi.stubGlobal('innerHeight', 300)
    rect.mockReturnValue({ top: 200, bottom: 244 } as DOMRect)
    await wrapper.get('button').trigger('click')

    expect(wrapper.get('[data-testid="phone-country-panel"]').classes()).toContain('bottom-full')
  })
})
