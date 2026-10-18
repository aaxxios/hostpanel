/**
 * Unit tests for ServicePasswordForm: the local checks, the request through the store and the
 * success and refusal states.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { mountClient } from '~/tests/helpers/mountClient'
import ServicePasswordForm from '~/components/client/services/ServicePasswordForm.vue'
import { useServiceManagementStore } from '~/stores/servicemanagement'

/** The mocked client endpoints the store reaches. */
const api = vi.hoisted(() => ({ changeServicePassword: vi.fn() }))

vi.mock('~/composables/apis/useClientApi', () => ({ useClientApi: () => api }))

/**
 * Types a password pair and submits the form.
 *
 * @param wrapper - The mounted form.
 * @param first - The new password.
 * @param second - The confirmation.
 */
const submitWith = async (wrapper: ReturnType<typeof mountClient>, first: string, second: string): Promise<void> => {
  await wrapper.find('[data-testid="service-new-password"]').setValue(first)
  await wrapper.find('[data-testid="service-confirm-password"]').setValue(second)
  await wrapper.find('form').trigger('submit')
}

describe('ServicePasswordForm', () => {
  beforeEach(() => {
    api.changeServicePassword.mockReset()
  })

  it('refuses a short password without calling the API', async () => {
    const wrapper = mountClient(ServicePasswordForm, { props: { serviceId: '4' } })

    await submitWith(wrapper, 'short', 'short')

    expect(wrapper.find('[data-testid="ui-form-error"]').text()).toContain('client.services.passwordTooShort')
    expect(api.changeServicePassword).not.toHaveBeenCalled()
  })

  it('refuses a mismatching confirmation without calling the API', async () => {
    const wrapper = mountClient(ServicePasswordForm, { props: { serviceId: '4' } })

    await submitWith(wrapper, 'longenough1', 'longenough2')

    expect(wrapper.find('[data-testid="ui-form-error"]').text()).toContain('client.services.passwordMismatch')
    expect(api.changeServicePassword).not.toHaveBeenCalled()
  })

  it('sends a good password for this service and shows the success state', async () => {
    api.changeServicePassword.mockResolvedValue({})
    const wrapper = mountClient(ServicePasswordForm, { props: { serviceId: '4' } })

    await submitWith(wrapper, 'longenough1', 'longenough1')
    await vi.waitFor(() => expect(wrapper.find('[data-testid="service-password-success"]').exists()).toBe(true))

    expect(api.changeServicePassword).toHaveBeenCalledWith('4', 'longenough1')
  })

  it('shows the API refusal above the form', async () => {
    api.changeServicePassword.mockRejectedValue({ data: { detail: 'Password too common.' } })
    const wrapper = mountClient(ServicePasswordForm, { props: { serviceId: '4' } })

    await submitWith(wrapper, 'longenough1', 'longenough1')
    await vi.waitFor(() => expect(useServiceManagementStore().passwordError).toBe('Password too common.'))
    await wrapper.vm.$nextTick()

    expect(wrapper.find('[data-testid="ui-form-error"]').text()).toContain('Password too common.')
  })
})
