/**
 * Tests for `pages/client/tickets/new.vue`: the form, what is sent and the confirmation.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { mountPage, stubClientRuntime } from '~/tests/helpers/mountClient'
import NewTicketPage from '~/pages/client/tickets/new.vue'
import { TicketPriority } from '~/types/ticketpriority'

/** The mocked support endpoints the store reaches. */
const supportApi = vi.hoisted(() => ({ fetchDepartments: vi.fn(), createTicket: vi.fn() }))

vi.mock('~/composables/apis/useSupportApi', () => ({ useSupportApi: () => supportApi }))

describe('new ticket page', () => {
  beforeEach(() => {
    supportApi.fetchDepartments.mockReset()
    supportApi.createTicket.mockReset()
    supportApi.fetchDepartments.mockResolvedValue([{ id: 3, name: 'Billing' }])
    stubClientRuntime()
  })

  it('shows the form with a placeholder on every field', async () => {
    const wrapper = await mountPage(NewTicketPage)

    expect(wrapper.find('[data-testid="ticket-subject"]').attributes('placeholder')).toBe('client.tickets.subjectPlaceholder')
    expect(wrapper.find('[data-testid="ticket-message"]').attributes('placeholder')).toBe('client.tickets.messagePlaceholder')
    expect(wrapper.find('[data-testid="ticket-success"]').exists()).toBe(false)
  })

  it('defaults the priority to medium', async () => {
    const wrapper = await mountPage(NewTicketPage)

    expect(wrapper.find('[data-testid="ticket-priority-Medium"]').classes()).toContain('bg-primary-600')
  })

  it('sends the typed ticket with the chosen priority and shows the confirmation', async () => {
    supportApi.createTicket.mockResolvedValue({})
    const wrapper = await mountPage(NewTicketPage)

    await wrapper.find('[data-testid="ticket-subject"]').setValue('Cannot log in')
    await wrapper.find('[data-testid="ticket-message"]').setValue('Please help')
    await wrapper.find('[data-testid="ticket-priority-High"]').trigger('click')
    await wrapper.find('form').trigger('submit')

    await vi.waitFor(() => expect(wrapper.find('[data-testid="ticket-success"]').exists()).toBe(true))
    expect(supportApi.createTicket).toHaveBeenCalledWith(expect.objectContaining({ subject: 'Cannot log in', message: 'Please help', priority: TicketPriority.High }))
    expect(wrapper.find('[data-testid="ticket-view-all"]').attributes('href')).toBe('/client/tickets')
  })

  it('sends no department as null and shows the API field error under the department field', async () => {
    supportApi.createTicket.mockRejectedValue({ data: { errors: { departmentId: ['Select a department.'] } } })
    const wrapper = await mountPage(NewTicketPage)

    await wrapper.find('form').trigger('submit')

    await vi.waitFor(() => expect(wrapper.find('[data-testid="ticket-department"]').attributes('aria-invalid')).toBe('true'))
    expect(supportApi.createTicket).toHaveBeenCalledWith(expect.objectContaining({ departmentId: null }))
    expect(wrapper.text()).toContain('Select a department.')
  })

  it('keeps the form and shows the refusal when the ticket is refused', async () => {
    supportApi.createTicket.mockRejectedValue({ data: { detail: 'Too many tickets.' } })
    const wrapper = await mountPage(NewTicketPage)

    await wrapper.find('form').trigger('submit')

    await vi.waitFor(() => expect(wrapper.find('[data-testid="ui-form-error"]').text()).toContain('Too many tickets.'))
    expect(wrapper.find('[data-testid="ticket-success"]').exists()).toBe(false)
  })
})
