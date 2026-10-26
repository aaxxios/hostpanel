/**
 * Unit tests for UiPhoneInput: country code trigger, the searchable country list (empty search,
 * filtering, keyboard choice, selected mark), typed number and the per-country hint. The
 * component carries no stylesheet of its own.
 */
import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import UiPhoneInput from '@/components/ui/UiPhoneInput.vue'
import { createTestI18n } from '../../helpers/i18n'

/**
 * Mounts the phone input attached to the document, with the English catalogue.
 *
 * @param props - Props for the field.
 * @returns The wrapper.
 */
const mountPhone = (props: Record<string, unknown> = {}) =>
  mount(UiPhoneInput, { props, attachTo: document.body, global: { plugins: [createTestI18n()] } })

describe('UiPhoneInput', () => {
  it('defaults to the first listed country', () => {
    const wrapper = mountPhone()

    expect(wrapper.get('[data-testid="phone-country-trigger"]').text()).toContain('+1')
    expect(wrapper.get('[data-testid="phone-number-input"]').attributes('placeholder')).toBe('201-555-0123')
  })

  it('shows the code and sample number of the chosen country', () => {
    const wrapper = mountPhone({ countryCode: 'GB' })

    expect(wrapper.get('[data-testid="phone-country-trigger"]').text()).toContain('+44')
    expect(wrapper.get('[data-testid="phone-number-input"]').attributes('placeholder')).toBe('7700 900000')
  })

  it('falls back to a generic hint for a country without a sample number', () => {
    const wrapper = mountPhone({ countryCode: 'AF' })

    expect(wrapper.get('[data-testid="phone-number-input"]').attributes('placeholder')).toBe('Phone number')
  })

  it('shows the number it was given and emits typing', async () => {
    const wrapper = mountPhone({ phoneNumber: '123' })
    const input = wrapper.get('[data-testid="phone-number-input"]')

    expect((input.element as HTMLInputElement).value).toBe('123')

    await input.setValue('456')

    expect(wrapper.emitted('update:phoneNumber')?.[0]).toEqual(['456'])
  })

  it('opens with an empty search field that has a placeholder', async () => {
    const wrapper = mountPhone()

    await wrapper.get('[data-testid="phone-country-trigger"]').trigger('click')

    const search = wrapper.get('[data-testid="phone-country-search"]')
    expect((search.element as HTMLInputElement).value).toBe('')
    expect(search.attributes('placeholder')).toBe('Search country or code')
  })

  it('offers every country, with name, native name and calling code, and marks the chosen one', async () => {
    const wrapper = mountPhone({ countryCode: 'AM' })

    await wrapper.get('[data-testid="phone-country-trigger"]').trigger('click')

    const row = wrapper.get('[data-testid="phone-country-AM"]')
    expect(row.attributes('aria-selected')).toBe('true')
    expect(row.text()).toContain('Armenia')
    expect(row.text()).toContain('Հայաստան')
    expect(row.text()).toContain('+374')
    expect(row.find('svg').exists()).toBe(true)
    expect(wrapper.find('[data-testid="phone-country-AF"]').exists()).toBe(true)
  })

  it('filters by name, native name, ISO code and calling code', async () => {
    const wrapper = mountPhone()
    await wrapper.get('[data-testid="phone-country-trigger"]').trigger('click')
    const search = wrapper.get('[data-testid="phone-country-search"]')

    await search.setValue('afgh')
    expect(wrapper.find('[data-testid="phone-country-AF"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="phone-country-AM"]').exists()).toBe(false)

    await search.setValue('Հայաստան')
    expect(wrapper.find('[data-testid="phone-country-AM"]').exists()).toBe(true)

    await search.setValue('de')
    expect(wrapper.find('[data-testid="phone-country-DE"]').exists()).toBe(true)

    await search.setValue('+44')
    expect(wrapper.find('[data-testid="phone-country-GB"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="phone-country-AM"]').exists()).toBe(false)
  })

  it('shows an empty state when nothing matches', async () => {
    const wrapper = mountPhone()
    await wrapper.get('[data-testid="phone-country-trigger"]').trigger('click')

    await wrapper.get('[data-testid="phone-country-search"]').setValue('zzzzqq')

    expect(wrapper.get('[data-testid="phone-country-empty"]').text()).toBe('No results')
  })

  it('opens the list, emits the picked country and closes', async () => {
    const wrapper = mountPhone()

    await wrapper.get('[data-testid="phone-country-trigger"]').trigger('click')
    expect(wrapper.find('[data-testid="phone-country-GB"]').exists()).toBe(true)

    await wrapper.get('[data-testid="phone-country-GB"]').trigger('click')

    expect(wrapper.emitted('update:countryCode')?.[0]).toEqual(['GB'])
    expect(wrapper.find('[data-testid="phone-country-GB"]').exists()).toBe(false)
  })

  it('moves the highlight with the arrow keys and chooses with Enter', async () => {
    const wrapper = mountPhone()
    await wrapper.get('[data-testid="phone-country-trigger"]').trigger('click')
    const search = wrapper.get('[data-testid="phone-country-search"]')
    await search.setValue('united')
    const ids = wrapper.findAll('[role="option"]').map((option) => option.attributes('data-testid')!.replace('phone-country-', '')) // the search matches several countries

    await search.trigger('keydown', { key: 'ArrowDown' })
    expect(search.attributes('aria-activedescendant')).toContain(ids[1])
    await search.trigger('keydown', { key: 'ArrowUp' })
    expect(search.attributes('aria-activedescendant')).toContain(ids[0])
    await search.trigger('keydown', { key: 'ArrowDown' })
    await search.trigger('keydown', { key: 'Enter' })

    expect(wrapper.emitted('update:countryCode')?.[0]).toEqual([ids[1]])
  })

  it('closes on Escape and returns focus to the trigger', async () => {
    const wrapper = mountPhone()
    await wrapper.get('[data-testid="phone-country-trigger"]').trigger('click')

    await wrapper.get('[data-testid="phone-country-search"]').trigger('keydown', { key: 'Escape' })

    expect(wrapper.find('[data-testid="phone-country-panel"]').exists()).toBe(false)
    expect(document.activeElement).toBe(wrapper.get('[data-testid="phone-country-trigger"]').element)
  })

  it('closes when the pointer goes down outside', async () => {
    const wrapper = mountPhone()
    await wrapper.get('[data-testid="phone-country-trigger"]').trigger('click')

    document.body.dispatchEvent(new MouseEvent('mousedown', { bubbles: true }))
    await wrapper.vm.$nextTick()

    expect(wrapper.find('[data-testid="phone-country-panel"]').exists()).toBe(false)
  })

  it('draws each flag as text from the country list, not as an image from a host', async () => {
    const wrapper = mountPhone()

    await wrapper.get('[data-testid="phone-country-trigger"]').trigger('click')

    expect(wrapper.html()).not.toContain('style=')
    expect(wrapper.html()).not.toContain('http')
    expect(wrapper.get('[data-testid="phone-country-AM"]').text()).toContain('+374')
  })
})
