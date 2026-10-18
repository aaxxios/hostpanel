/**
 * Mount tests for ChangePasswordModal: validation, save and close.
 */
import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import ChangePasswordModal from '@/components/clients/ChangePasswordModal.vue'
import { createTestI18n } from '../../helpers/i18n'

/**
 * Mounts the modal with the dialog teleport inlined.
 *
 * @returns The wrapper.
 */
const mountModal = () =>
  mount(ChangePasswordModal, { global: { plugins: [createTestI18n()], stubs: { teleport: true } } })

describe('ChangePasswordModal', () => {
  it('refuses an empty password and shows the field error', async () => {
    const wrapper = mountModal()

    await wrapper.get('[data-testid="clients-change-password-save"]').trigger('click')

    expect(wrapper.emitted('save')).toBeUndefined()
    expect(wrapper.get('[role="alert"]').text()).not.toBe('')
  })

  it('emits the typed password on save', async () => {
    const wrapper = mountModal()

    await wrapper.get('[data-testid="clients-change-password-input"]').setValue('s3cret!')
    await wrapper.get('[data-testid="clients-change-password-save"]').trigger('click')

    expect(wrapper.emitted('save')).toEqual([['s3cret!']])
  })

  it('emits close from cancel', async () => {
    const wrapper = mountModal()

    await wrapper.get('[data-testid="clients-change-password-cancel"]').trigger('click')

    expect(wrapper.emitted('close')).toHaveLength(1)
  })
})
