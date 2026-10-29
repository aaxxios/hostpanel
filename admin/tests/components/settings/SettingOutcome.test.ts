/**
 * Tests for SettingOutcome: nothing before a save finishes, the success sentence after a stored
 * value, and the API's sentence (or the generic one) after a refusal.
 */
import { describe, expect, it } from 'vitest'
import SettingOutcome from '@/components/settings/SettingOutcome.vue'
import { mountScreen } from '../../helpers/mount'

describe('SettingOutcome', () => {
  it('shows nothing before a save has finished', async () => {
    const { wrapper } = await mountScreen(SettingOutcome, { outcome: null })

    expect(wrapper.text()).toBe('')
  })

  it('shows the success sentence for a stored value', async () => {
    const { wrapper } = await mountScreen(SettingOutcome, { outcome: { saved: true, error: null } })

    expect(wrapper.text()).toBe('Changes saved successfully.')
    expect(wrapper.get('[role="status"]').attributes('data-tone')).toBe('success')
  })

  it('shows the API sentence for a refusal', async () => {
    const { wrapper } = await mountScreen(SettingOutcome, { outcome: { saved: false, error: 'Not a valid e-mail address.' } })

    expect(wrapper.text()).toBe('Not a valid e-mail address.')
    expect(wrapper.get('[role="status"]').attributes('data-tone')).toBe('danger')
  })

  it('falls back to the generic sentence when the API gave none', async () => {
    const { wrapper } = await mountScreen(SettingOutcome, { outcome: { saved: false, error: null } })

    expect(wrapper.text()).toContain('Could not reach the server')
  })
})
