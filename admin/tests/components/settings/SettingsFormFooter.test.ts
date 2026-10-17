/**
 * Tests for SettingsFormFooter: Cancel and Save events, the save label and the saving state.
 */
import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import SettingsFormFooter from '@/components/settings/SettingsFormFooter.vue'
import { createTestI18n } from '../../helpers/i18n'

/**
 * Mounts the footer with the English catalogue.
 *
 * @param props - Props for the footer.
 * @returns The wrapper.
 */
const mountFooter = (props: { saveLabel: string; saving: boolean }) =>
  mount(SettingsFormFooter, { props, global: { plugins: [createTestI18n()] } })

describe('SettingsFormFooter', () => {
  it('shows Cancel and the given save label', () => {
    const wrapper = mountFooter({ saveLabel: 'Create slide', saving: false })

    expect(wrapper.get('[data-testid="settings-form-cancel"]').text()).toBe('Cancel')
    expect(wrapper.get('[data-testid="settings-form-save"]').text()).toBe('Create slide')
  })

  it('emits cancel and save from their buttons', async () => {
    const wrapper = mountFooter({ saveLabel: 'Save', saving: false })

    await wrapper.get('[data-testid="settings-form-cancel"]').trigger('click')
    await wrapper.get('[data-testid="settings-form-save"]').trigger('click')

    expect(wrapper.emitted('cancel')).toHaveLength(1)
    expect(wrapper.emitted('save')).toHaveLength(1)
  })

  it('shows the saving label and disables Save while saving', () => {
    const wrapper = mountFooter({ saveLabel: 'Save', saving: true })
    const save = wrapper.get('[data-testid="settings-form-save"]')

    expect(save.text()).toBe('Saving...')
    expect(save.attributes('disabled')).toBeDefined()
  })
})
