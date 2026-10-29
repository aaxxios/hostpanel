/**
 * Mount tests for ClientStatusConfirmDialog: the prompt names the status, spells out its effects,
 * colours the confirm button by severity and relays the operator's choice.
 */
import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import ClientStatusConfirmDialog from '@/components/clients/ClientStatusConfirmDialog.vue'
import UiDialog from '@/components/ui/UiDialog.vue'
import { ClientStatus } from '@/types/client'
import { createTestI18n } from '../../helpers/i18n'

/**
 * Mounts the dialog for a status.
 *
 * @param status - The restrictive status awaiting confirmation.
 * @returns The wrapper.
 */
const mountDialog = (status: ClientStatus.Suspended | ClientStatus.Closed) =>
  mount(ClientStatusConfirmDialog, {
    props: { status, testid: 'status-confirm' },
    global: { plugins: [createTestI18n()], stubs: { teleport: true } },
  })

describe('ClientStatusConfirmDialog', () => {
  it('prompts to suspend with a warning tone and the suspension effects', () => {
    const dialog = mountDialog(ClientStatus.Suspended).getComponent(UiDialog)

    expect(dialog.props('title')).toBe('Suspend this client?')
    expect(dialog.props('message')).toContain('cannot place orders')
    expect(dialog.props('confirmLabel')).toBe('Suspend client')
    expect(dialog.props('tone')).toBe('warning')
  })

  it('prompts to close with a danger tone and the permanence warning', () => {
    const dialog = mountDialog(ClientStatus.Closed).getComponent(UiDialog)

    expect(dialog.props('title')).toBe('Close this client?')
    expect(dialog.props('message')).toContain('Closing is permanent')
    expect(dialog.props('tone')).toBe('danger')
  })

  it('relays confirm and close', () => {
    const wrapper = mountDialog(ClientStatus.Closed)
    const dialog = wrapper.getComponent(UiDialog)

    dialog.vm.$emit('confirm')
    dialog.vm.$emit('close')

    expect(wrapper.emitted('confirm')).toHaveLength(1)
    expect(wrapper.emitted('close')).toHaveLength(1)
  })
})
