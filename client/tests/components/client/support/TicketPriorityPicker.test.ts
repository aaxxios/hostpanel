/**
 * Unit tests for TicketPriorityPicker: the three options, the chosen one and the emitted value.
 */
import { describe, expect, it } from 'vitest'
import { mountClient } from '~/tests/helpers/mountClient'
import TicketPriorityPicker from '~/components/client/support/TicketPriorityPicker.vue'
import { TicketPriority } from '~/types/ticketpriority'

describe('TicketPriorityPicker', () => {
  it('offers low, medium and high', () => {
    const wrapper = mountClient(TicketPriorityPicker, { props: { modelValue: TicketPriority.Medium } })

    expect(wrapper.findAll('button')).toHaveLength(3)
    expect(wrapper.find('[data-testid="ticket-priority-Low"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="ticket-priority-High"]').exists()).toBe(true)
  })

  it('fills only the chosen priority', () => {
    const wrapper = mountClient(TicketPriorityPicker, { props: { modelValue: TicketPriority.High } })

    expect(wrapper.find('[data-testid="ticket-priority-High"]').classes()).toContain('bg-primary-700')
    expect(wrapper.find('[data-testid="ticket-priority-Low"]').classes()).not.toContain('bg-primary-700')
  })

  it('emits the priority that was clicked', async () => {
    const wrapper = mountClient(TicketPriorityPicker, { props: { modelValue: TicketPriority.Medium } })

    await wrapper.find('[data-testid="ticket-priority-Low"]').trigger('click')

    expect(wrapper.emitted('update:modelValue')).toEqual([[TicketPriority.Low]])
  })
})
