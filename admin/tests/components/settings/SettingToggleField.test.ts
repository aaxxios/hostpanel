/**
 * Tests for SettingToggleField: the switch shows the stored value, an empty row reads as the
 * default, a flip saves the literal string, and an unseeded row is replaced by the warning.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import SettingToggleField from '@/components/settings/SettingToggleField.vue'
import { SettingKey } from '@/types/setting'
import type { Setting, SettingSave } from '@/types/setting'
import { mountScreen } from '../../helpers/mount'

/** The save callback the card is given; it answers that the value was stored. */
const save = vi.fn<SettingSave>(async () => ({ saved: true, error: null }))

beforeEach(() => {
  save.mockClear()
})

/**
 * Builds the props of the field for one stored value.
 *
 * @param value - The stored value of the row.
 * @param defaultOn - Whether an empty row reads as on.
 * @returns The props.
 */
const propsFor = (value: string, defaultOn = true) => ({
  settings: [{ id: 4, key: SettingKey.SectionFaq, value }] as Setting[],
  settingKey: SettingKey.SectionFaq,
  label: 'FAQ',
  hint: 'Frequently asked questions.',
  defaultOn,
  testid: 'field',
  save,
})

describe('SettingToggleField', () => {
  it('shows the stored value with its label and hint', async () => {
    const { wrapper } = await mountScreen(SettingToggleField, propsFor('false'))

    expect(wrapper.get('[data-testid="field"]').attributes('aria-checked')).toBe('false')
    expect(wrapper.text()).toContain('FAQ')
    expect(wrapper.text()).toContain('Frequently asked questions.')
  })

  it('reads an empty row as its default', async () => {
    const on = await mountScreen(SettingToggleField, propsFor('', true))
    const off = await mountScreen(SettingToggleField, propsFor('', false))

    expect(on.wrapper.get('[data-testid="field"]').attributes('aria-checked')).toBe('true')
    expect(off.wrapper.get('[data-testid="field"]').attributes('aria-checked')).toBe('false')
  })

  it('saves the flipped value as the literal string', async () => {
    const { wrapper } = await mountScreen(SettingToggleField, propsFor('true'))

    await wrapper.get('[data-testid="field"]').trigger('click')
    await wrapper.get('[data-testid="field-save"]').trigger('click')

    expect(save.mock.calls).toEqual([[4, 'false']])
  })

  it('does not save before anything changed', async () => {
    const { wrapper } = await mountScreen(SettingToggleField, propsFor('true'))

    await wrapper.get('[data-testid="field-save"]').trigger('click')

    expect(save).not.toHaveBeenCalled()
  })

  it('names the key and hides the switch when the row was never seeded', async () => {
    const { wrapper } = await mountScreen(SettingToggleField, { ...propsFor('true'), settings: [] })

    expect(wrapper.get('[data-testid="setting-not-seeded"]').text()).toContain('portal.sections.faq')
    expect(wrapper.find('[data-testid="field"]').exists()).toBe(false)
  })
})
