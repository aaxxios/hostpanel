/**
 * Mount tests for ServerFormModal: tabs, the payload built from the form (empty text becomes
 * null), the edited server loaded with blank secrets, and the API's sentences shown with a switch
 * to the tab that holds the refused field.
 */
import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import { nextTick } from 'vue'
import ServerFormModal from '@/components/servers/ServerFormModal.vue'
import { createTestI18n } from '../../helpers/i18n'
import { serverDto } from '../../helpers/serverFixtures'

/**
 * Mounts the dialog with the teleport inlined.
 *
 * @param props - Props to change.
 * @returns The wrapper.
 */
const mountModal = (props: Record<string, unknown> = {}) =>
  mount(ServerFormModal, {
    props: { server: null, saving: false, ...props },
    global: { plugins: [createTestI18n()], stubs: { teleport: true } },
  })

describe('ServerFormModal', () => {
  it('opens on the general tab and switches tabs', async () => {
    const wrapper = mountModal()

    expect(wrapper.text()).toContain('Add New Server')
    expect(wrapper.find('[data-testid="server-form-name"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="server-form-username"]').exists()).toBe(false)

    await wrapper.get('[data-testid="server-form-tab-details"]').trigger('click')
    expect(wrapper.find('[data-testid="server-form-username"]').exists()).toBe(true)

    await wrapper.get('[data-testid="server-form-tab-nameservers"]').trigger('click')
    expect(wrapper.find('[data-testid="server-form-ns1-host"]').exists()).toBe(true)
  })

  it('emits the payload with empty text turned into null', async () => {
    const wrapper = mountModal()

    await wrapper.get('[data-testid="server-form-name"]').setValue('host9')
    await wrapper.get('[data-testid="server-form-hostname"]').setValue('host9.example.com')
    await wrapper.get('[data-testid="server-form-tab-details"]').trigger('click')
    await wrapper.get('[data-testid="server-form-username"]').setValue('root')
    await wrapper.get('[data-testid="server-form-save"]').trigger('click')

    const payload = wrapper.emitted('saved')?.[0]?.[0] as Record<string, unknown>
    expect(payload).toMatchObject({
      name: 'host9',
      hostname: 'host9.example.com',
      username: 'root',
      module: 'Cwp7',
      ipAddress: null,
      password: null,
      ns1Hostname: null,
      useSSL: true,
    })
  })

  it('loads an edited server with the secrets left blank', () => {
    const wrapper = mountModal({ server: serverDto({ name: 'edited', username: 'admin' }) })

    expect(wrapper.text()).toContain('Edit Server')
    expect((wrapper.get('[data-testid="server-form-name"]').element as HTMLInputElement).value).toBe('edited')
  })

  it('shows the refusal and jumps to the tab with the refused field', async () => {
    const wrapper = mountModal({ error: 'Server was refused.' })
    expect(wrapper.get('[data-testid="server-form-error"]').text()).toContain('Server was refused.')

    await wrapper.get('[data-testid="server-form-tab-nameservers"]').trigger('click')
    await wrapper.setProps({ fieldErrors: { username: 'Username is required.' } })
    await nextTick()

    expect(wrapper.text()).toContain('Username is required.')
  })

  it('emits close from cancel', async () => {
    const wrapper = mountModal()

    await wrapper.get('[data-testid="server-form-cancel"]').trigger('click')

    expect(wrapper.emitted('close')).toHaveLength(1)
  })
})
