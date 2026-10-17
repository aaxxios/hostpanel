/**
 * Mount tests for ClientExportModal: field selection and the download request.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises, mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import ClientExportModal from '@/components/clients/ClientExportModal.vue'
import { createTestI18n } from '../../helpers/i18n'
import { json, stubFetch } from '../../helpers/fetch'

/**
 * Mounts the modal for client 7.
 *
 * @returns The wrapper.
 */
const mountModal = () =>
  mount(ClientExportModal, {
    props: { clientId: 7, clientName: 'Ada Lovelace' },
    global: { plugins: [createTestI18n(), createPinia()], stubs: { teleport: true } },
  })

describe('ClientExportModal', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.stubGlobal('URL', { createObjectURL: vi.fn(() => 'blob:x'), revokeObjectURL: vi.fn() })
    // Clicking the download anchor would make happy-dom navigate to the blob URL; the click is all that is asserted.
    vi.spyOn(HTMLAnchorElement.prototype, 'click').mockImplementation(() => undefined)
  })

  it('shows the client name with every field selected', () => {
    const wrapper = mountModal()

    expect(wrapper.text()).toContain('Ada Lovelace')
    expect(wrapper.get('[data-testid="clients-export-field-invoices"]').attributes('aria-checked')).toBe('true')
  })

  it('disables download when nothing is selected', async () => {
    const wrapper = mountModal()

    await wrapper.get('[data-testid="clients-export-select-all"]').trigger('click')

    expect(wrapper.get('[data-testid="clients-export-download"]').attributes('disabled')).toBeDefined()
  })

  it('requests the export and closes after downloading', async () => {
    const fetchMock = stubFetch(json({ profileData: {} }))
    const wrapper = mountModal()

    await wrapper.get('[data-testid="clients-export-download"]').trigger('click')
    await flushPromises()

    expect(fetchMock).toHaveBeenCalled()
    expect(wrapper.emitted('close')).toHaveLength(1)
  })
})
