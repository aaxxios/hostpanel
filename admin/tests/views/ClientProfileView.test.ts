/**
 * Tests for ClientProfileView: it renders against the real kit and settles on an API refusal, and
 * choosing Suspended or Closed spells out what the status does and asks the operator before saving.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises, mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import { createMemoryHistory, createRouter } from 'vue-router'
import ClientProfileView from '@/views/ClientProfileView.vue'
import UiDialog from '@/components/ui/UiDialog.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import { useClientsStore } from '@/stores/clients'
import { ClientStatus, type ClientDetail } from '@/types/client'
import { createTestI18n } from '../helpers/i18n'
import { mountClientView } from '../helpers/clientViews'
import { stubPathRoutes } from '../helpers/routes'

/** The test id of the status select on the form. */
const STATUS_SELECT = 'client-profile-status'

/** The active client the profile shows, as `GET /api/clients/5` returns it. */
const detail: ClientDetail = {
  id: 5,
  userId: 'user-5',
  email: 'ada@example.test',
  firstName: 'Ada',
  lastName: 'Lovelace',
  status: ClientStatus.Active,
  isUserDeleted: false,
  twoFactorEnabled: false,
  createdAt: '2026-01-01T00:00:00Z',
  contacts: [],
  address: { street: null, address2: null, city: null, state: null, postCode: null, country: null },
  preferences: { language: null, currency: null, paymentMethod: null, billingContact: null, adminNotes: null },
  notifications: { general: true, invoice: true, support: true, product: true, domain: true, affiliate: true },
  settings: {
    overdueNotices: true,
    taxExempt: false,
    separateInvoices: false,
    disableCcProcessing: false,
    marketingOptIn: false,
    statusUpdate: true,
    allowSso: true,
  },
}

/**
 * Mounts the profile view over a store that already holds an active client. The client route
 * answers the detail, so the re-read after a save fills the form again.
 *
 * @returns The wrapper and the fetch mock the save goes through.
 */
const mountProfile = async () => {
  const fetchMock = stubPathRoutes([{ path: '/api/clients/5', body: detail }])
  const pinia = createPinia()
  setActivePinia(pinia)
  useClientsStore().current = detail
  const router = createRouter({
    history: createMemoryHistory(),
    routes: [{ path: '/clients/:id/profile', component: { render: () => null } }],
  })
  await router.push('/clients/5/profile')
  await router.isReady()
  const wrapper = mount(ClientProfileView, {
    global: { plugins: [createTestI18n(), pinia, router], stubs: { teleport: true } },
  })
  await flushPromises()
  return { wrapper, fetchMock }
}

/**
 * Chooses a status in the form's status select.
 *
 * @param wrapper - The mounted view.
 * @param status - The status to choose.
 * @returns Promise that resolves once the view has re-rendered.
 */
const chooseStatus = async (wrapper: Awaited<ReturnType<typeof mountProfile>>['wrapper'], status: ClientStatus): Promise<void> => {
  const select = wrapper.findAllComponents(UiSelect).find(c => c.attributes('data-testid') === STATUS_SELECT)
  select?.vm.$emit('update:modelValue', status)
  await flushPromises()
}

/**
 * Counts the writes the save made.
 *
 * @param fetchMock - The stubbed fetch.
 * @returns How many calls were not reads.
 */
const writesOf = (fetchMock: ReturnType<typeof vi.fn>): number =>
  fetchMock.mock.calls.filter(([, init]) => (init as RequestInit | undefined)?.method === 'PUT').length

describe('ClientProfileView', () => {
  beforeEach(() => {
    vi.unstubAllGlobals()
  })

  it('mounts without throwing', async () => {
    const wrapper = await mountClientView(ClientProfileView)

    expect(wrapper.exists()).toBe(true)
  })

  it('spells out the effects of Suspended and Closed under the select and stays quiet for Active', async () => {
    const { wrapper } = await mountProfile()

    expect(wrapper.find('[data-testid="client-profile-status-effects"]').exists()).toBe(false)

    await chooseStatus(wrapper, ClientStatus.Suspended)
    expect(wrapper.get('[data-testid="client-profile-status-effects"]').text()).toContain('cannot place orders')

    await chooseStatus(wrapper, ClientStatus.Closed)
    expect(wrapper.get('[data-testid="client-profile-status-effects"]').text()).toContain('Closing is permanent')
  })

  it('asks before closing, and saves only after the operator confirms', async () => {
    const { wrapper, fetchMock } = await mountProfile()
    await chooseStatus(wrapper, ClientStatus.Closed)

    await wrapper.get('form').trigger('submit')
    await flushPromises()

    const dialog = wrapper.getComponent(UiDialog)
    expect(dialog.props('message')).toContain('unpaid renewal invoices are cancelled')
    expect(writesOf(fetchMock)).toBe(0)

    dialog.vm.$emit('confirm')
    await flushPromises()

    expect(writesOf(fetchMock)).toBe(1)
    expect(wrapper.findComponent(UiDialog).exists()).toBe(false)
  })

  it('sends the nested preferences group together with the confirmed status', async () => {
    const { wrapper, fetchMock } = await mountProfile()
    await chooseStatus(wrapper, ClientStatus.Suspended)
    await wrapper.get('form').trigger('submit')
    await flushPromises()

    wrapper.getComponent(UiDialog).vm.$emit('confirm')
    await flushPromises()

    const put = fetchMock.mock.calls.find(([, init]) => (init as RequestInit | undefined)?.method === 'PUT')
    expect(JSON.parse(String((put?.[1] as RequestInit | undefined)?.body))).toMatchObject({
      status: ClientStatus.Suspended,
      preferences: { language: null, currency: null, paymentMethod: null, billingContact: null, adminNotes: null },
    })
  })

  it('does not save when the operator backs out of the prompt', async () => {
    const { wrapper, fetchMock } = await mountProfile()
    await chooseStatus(wrapper, ClientStatus.Suspended)
    await wrapper.get('form').trigger('submit')
    await flushPromises()

    wrapper.getComponent(UiDialog).vm.$emit('close')
    await flushPromises()

    expect(writesOf(fetchMock)).toBe(0)
    expect(wrapper.findComponent(UiDialog).exists()).toBe(false)
  })

  it('saves a change that restricts nothing without asking', async () => {
    const { wrapper, fetchMock } = await mountProfile()
    await chooseStatus(wrapper, ClientStatus.Inactive)

    await wrapper.get('form').trigger('submit')
    await flushPromises()

    expect(wrapper.findComponent(UiDialog).exists()).toBe(false)
    expect(writesOf(fetchMock)).toBe(1)
  })
})
