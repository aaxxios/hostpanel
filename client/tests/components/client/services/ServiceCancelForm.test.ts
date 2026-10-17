/**
 * Unit tests for ServiceCancelForm: the default type, the confirmation before sending, the wire
 * value sent and the success state.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { mountClient } from '~/tests/helpers/mountClient'
import ServiceCancelForm from '~/components/client/services/ServiceCancelForm.vue'
import { CancellationType } from '~/types/cancellationtype'

/** The mocked client endpoints the store reaches. */
const api = vi.hoisted(() => ({ cancelService: vi.fn() }))

vi.mock('~/composables/apis/useClientApi', () => ({ useClientApi: () => api }))

describe('ServiceCancelForm', () => {
  beforeEach(() => {
    api.cancelService.mockReset()
  })

  it('preselects the end of the billing period', () => {
    const wrapper = mountClient(ServiceCancelForm, { props: { serviceId: '4' } })

    expect(wrapper.find('[data-testid="service-cancel-type-end"]').attributes('aria-checked')).toBe('true')
    expect(wrapper.find('[data-testid="service-cancel-type-immediate"]').attributes('aria-checked')).toBe('false')
  })

  it('does not send anything until the dialog is confirmed', async () => {
    const wrapper = mountClient(ServiceCancelForm, { props: { serviceId: '4' } })

    await wrapper.find('form').trigger('submit')

    expect(wrapper.find('[data-testid="ui-confirm-dialog"]').exists()).toBe(true)
    expect(api.cancelService).not.toHaveBeenCalled()
  })

  it('sends the chosen wire value and the reason once confirmed', async () => {
    api.cancelService.mockResolvedValue({})
    const wrapper = mountClient(ServiceCancelForm, { props: { serviceId: '4' } })

    await wrapper.find('[data-testid="service-cancel-type-immediate"]').trigger('click')
    await wrapper.find('[data-testid="service-cancel-reason"]').setValue('Moving away')
    await wrapper.find('form').trigger('submit')
    await wrapper.find('[data-testid="ui-confirm-confirm"]').trigger('click')

    await vi.waitFor(() => expect(api.cancelService).toHaveBeenCalledWith('4', { type: CancellationType.Immediate, reason: 'Moving away' }))
  })

  it('shows the success state after the request is accepted', async () => {
    api.cancelService.mockResolvedValue({})
    const wrapper = mountClient(ServiceCancelForm, { props: { serviceId: '4' } })

    await wrapper.find('form').trigger('submit')
    await wrapper.find('[data-testid="ui-confirm-confirm"]').trigger('click')

    await vi.waitFor(() => expect(wrapper.find('[data-testid="service-cancel-success"]').exists()).toBe(true))
  })

  it('cancels the confirmation without sending', async () => {
    const wrapper = mountClient(ServiceCancelForm, { props: { serviceId: '4' } })

    await wrapper.find('form').trigger('submit')
    await wrapper.find('[data-testid="ui-confirm-cancel"]').trigger('click')

    expect(wrapper.find('[data-testid="ui-confirm-dialog"]').exists()).toBe(false)
    expect(api.cancelService).not.toHaveBeenCalled()
  })
})
