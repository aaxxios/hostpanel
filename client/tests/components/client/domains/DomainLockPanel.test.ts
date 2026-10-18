/**
 * Unit tests for DomainLockPanel: both switches move only when the API accepts the change.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { mountClient } from '~/tests/helpers/mountClient'
import DomainLockPanel from '~/components/client/domains/DomainLockPanel.vue'

/** The mocked domains endpoints the store reaches. */
const domainsApi = vi.hoisted(() => ({ setLock: vi.fn(), setIdProtect: vi.fn() }))

vi.mock('~/composables/apis/useDomainsApi', () => ({ useDomainsApi: () => domainsApi }))
vi.mock('~/composables/apis/useClientApi', () => ({ useClientApi: () => ({}) }))
vi.mock('~/composables/apis/useBillingApi', () => ({ useBillingApi: () => ({}) }))
vi.mock('~/composables/apis/useCatalogApi', () => ({ useCatalogApi: () => ({}) }))
vi.mock('~/composables/apis/useCurrencyApi', () => ({ useCurrencyApi: () => ({}) }))

/**
 * Mounts the panel with both switches off.
 *
 * @returns The wrapper.
 */
const mountPanel = () => mountClient(DomainLockPanel, { props: { domainId: '7', lockEnabled: false, idProtectEnabled: false } })

describe('DomainLockPanel', () => {
  beforeEach(() => {
    domainsApi.setLock.mockReset()
    domainsApi.setIdProtect.mockReset()
  })

  it('shows the lock, the ID protection and the EPP card', () => {
    const wrapper = mountPanel()

    expect(wrapper.find('[data-testid="domain-lock-row"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="domain-idprotect-row"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="domain-epp"]').exists()).toBe(true)
  })

  it('turns the lock on when the API accepts', async () => {
    domainsApi.setLock.mockResolvedValue({})
    const wrapper = mountPanel()

    await wrapper.find('[data-testid="domain-lock-row-toggle"]').trigger('click')

    await vi.waitFor(() => expect(wrapper.emitted('update:lockEnabled')).toEqual([[true]]))
    expect(domainsApi.setLock).toHaveBeenCalledWith('7', true)
  })

  it('turns ID protection on when the API accepts', async () => {
    domainsApi.setIdProtect.mockResolvedValue({})
    const wrapper = mountPanel()

    await wrapper.find('[data-testid="domain-idprotect-row-toggle"]').trigger('click')

    await vi.waitFor(() => expect(wrapper.emitted('update:idProtectEnabled')).toEqual([[true]]))
    expect(domainsApi.setIdProtect).toHaveBeenCalledWith('7', true)
  })

  it('leaves the lock off and shows the refusal when the API refuses', async () => {
    domainsApi.setLock.mockRejectedValue({ data: { detail: 'Registrar refused.' } })
    const wrapper = mountPanel()

    await wrapper.find('[data-testid="domain-lock-row-toggle"]').trigger('click')

    await vi.waitFor(() => expect(wrapper.find('[data-testid="domain-toggle-error"]').exists()).toBe(true))
    expect(wrapper.emitted('update:lockEnabled')).toBeUndefined()
  })
})
