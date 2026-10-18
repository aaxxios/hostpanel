/**
 * Tests for `pages/client/services/[id]/index.vue`: the panels, the notices, the control-panel
 * sign-in and the redirect of a service that has no account yet.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { mountPage, stubClientRuntime } from '~/tests/helpers/mountClient'
import ServicePage from '~/pages/client/services/[id]/index.vue'

/** The mocked client endpoints the stores reach. */
const clientApi = vi.hoisted(() => ({
  fetchService: vi.fn(), fetchHostingInfo: vi.fn(), fetchCancellationStatus: vi.fn(), fetchServiceInvoices: vi.fn(),
  fetchCpanelSsoUrl: vi.fn(), fetchMe: vi.fn()
}))

vi.mock('~/composables/apis/useClientApi', () => ({ useClientApi: () => clientApi }))

/** Stands in for Nuxt's `navigateTo`. */
const navigate = vi.fn()

/**
 * Builds a service.
 *
 * @param overrides - Fields to set.
 * @returns The service.
 */
const service = (overrides: Record<string, unknown> = {}) => ({
  id: 4, name: 'Starter', status: 'Active', username: 'webmstr', domain: 'example.com', groupname: 'Hosting', serverhostname: 'srv1.example.net',
  billingcycle: 'monthly', regdate: '2026-01-01', nextduedate: '2026-12-01', recurringamount: '10', firstpaymentamount: '12',
  diskusage: '1', disklimit: '10', bwusage: '0', bwlimit: '0', ...overrides
})

describe('service page', () => {
  beforeEach(() => {
    Object.values(clientApi).forEach((fn) => fn.mockReset())
    clientApi.fetchService.mockResolvedValue(service())
    clientApi.fetchHostingInfo.mockResolvedValue({ nameservers: ['ns1.example.net'], ssl: null })
    clientApi.fetchCancellationStatus.mockResolvedValue({ pending: false })
    clientApi.fetchServiceInvoices.mockResolvedValue({ invoices: [], unattributedInvoiceCount: 0 })
    clientApi.fetchMe.mockResolvedValue({ currency: 'USD' })
    navigate.mockReset()
    vi.stubGlobal('navigateTo', navigate)
    stubClientRuntime({ params: { id: '4' } })
  })

  it('reads the service, its hosting info and its cancellation status', async () => {
    await mountPage(ServicePage)

    expect(clientApi.fetchService).toHaveBeenCalledWith('4')
    expect(clientApi.fetchHostingInfo).toHaveBeenCalledWith('4')
    expect(clientApi.fetchCancellationStatus).toHaveBeenCalledWith('4')
  })

  it('opens on the overview with the details, the server and the invoices', async () => {
    const wrapper = await mountPage(ServicePage)

    expect(wrapper.find('[data-testid="service-details"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="service-server"]').text()).toContain('ns1.example.net')
    expect(wrapper.find('[data-testid="service-invoices"]').exists()).toBe(true)
  })

  it('switches to the password and the cancellation panels', async () => {
    const wrapper = await mountPage(ServicePage)

    await wrapper.find('[data-testid="service-tab-password"]').trigger('click')
    expect(wrapper.find('[data-testid="service-password"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="service-overview"]').exists()).toBe(false)

    await wrapper.find('[data-testid="service-tab-cancel"]').trigger('click')
    expect(wrapper.find('[data-testid="service-cancel"]').exists()).toBe(true)
  })

  it('opens the cancellation panel from the quick action', async () => {
    const wrapper = await mountPage(ServicePage)

    await wrapper.find('[data-testid="service-quick-cancel"]').trigger('click')

    expect(wrapper.find('[data-testid="service-cancel"]').exists()).toBe(true)
  })

  it('shows the suspension reason of a suspended service', async () => {
    clientApi.fetchService.mockResolvedValue(service({ status: 'Suspended', suspensionreason: 'Overdue invoice' }))

    const wrapper = await mountPage(ServicePage)

    expect(wrapper.text()).toContain('Overdue invoice')
  })

  it('sends a service without a control-panel account to its setup wizard', async () => {
    clientApi.fetchService.mockResolvedValue(service({ username: '', status: 'Pending' }))

    await mountPage(ServicePage)

    expect(navigate).toHaveBeenCalledWith('/client/services/4/setup')
  })

  it('does not send a terminated service to setup', async () => {
    clientApi.fetchService.mockResolvedValue(service({ username: '', status: 'Terminated' }))

    await mountPage(ServicePage)

    expect(navigate).not.toHaveBeenCalled()
  })

  it('says the service was not found when the read fails', async () => {
    clientApi.fetchService.mockRejectedValue({ data: { detail: 'No such service.' } })

    const wrapper = await mountPage(ServicePage)

    expect(wrapper.find('[data-testid="ui-empty-state"]').text()).toContain('client.services.notFound')
  })
})
