/**
 * Unit tests for TicketMessage: the author line, the badge, the staff tint and the plain-text body.
 */
import { describe, expect, it } from 'vitest'
import { mountClient } from '~/tests/helpers/mountClient'
import TicketMessage from '~/components/client/support/TicketMessage.vue'

describe('TicketMessage', () => {
  it('shows the author, the date and the message', () => {
    const wrapper = mountClient(TicketMessage, { props: { author: 'Ada Lovelace', date: '1 Jan', message: 'Hello' } })

    expect(wrapper.text()).toContain('Ada Lovelace')
    expect(wrapper.text()).toContain('1 Jan')
    expect(wrapper.text()).toContain('Hello')
  })

  it('shows the badge only when one is given', () => {
    const without = mountClient(TicketMessage, { props: { author: 'A', date: 'd', message: 'm' } })
    const withBadge = mountClient(TicketMessage, { props: { author: 'A', date: 'd', message: 'm', badge: 'You' } })

    expect(without.text()).not.toContain('You')
    expect(withBadge.text()).toContain('You')
  })

  it('tints a staff message', () => {
    const staff = mountClient(TicketMessage, { props: { author: 'A', date: 'd', message: 'm', staff: true } })
    const client = mountClient(TicketMessage, { props: { author: 'A', date: 'd', message: 'm' } })

    expect(staff.classes()).toContain('bg-primary-500/5')
    expect(client.classes()).not.toContain('bg-primary-500/5')
  })

  it('renders markup in the message as text, never as elements', () => {
    const wrapper = mountClient(TicketMessage, { props: { author: 'A', date: 'd', message: '<img src=x onerror=alert(1)>' } })

    expect(wrapper.find('img').exists()).toBe(false)
    expect(wrapper.text()).toContain('<img src=x onerror=alert(1)>')
  })
})
