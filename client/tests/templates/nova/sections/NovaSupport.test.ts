/**
 * Unit tests for NovaSupport: a card per channel, the response time when set, and a link only for
 * channels that have an address.
 *
 * @module tests/templates/nova/sections/NovaSupport.test
 */
import { computed } from 'vue'
import { describe, expect, it, vi } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import NovaSupport from '~/templates/nova/sections/NovaSupport.vue'
import { SupportChannel } from '~/constants/supportChannel'
import type { SupportChannelItem } from '~/types/supportchannelitem'
import { stubTemplateGlobals } from '~/tests/helpers/templateGlobals'

/**
 * Stands in for `useSupportChannels`.
 *
 * @param items The available channels.
 */
const stubChannels = (items: SupportChannelItem[]): void => {
  vi.stubGlobal('useSupportChannels', () => computed(() => items))
}

describe('NovaSupport', () => {
  it('renders a card per channel', () => {
    stubTemplateGlobals()
    stubChannels([{ channel: SupportChannel.Ticket, href: '/client/tickets/new' }, { channel: SupportChannel.Chat }])

    expect(mountWithKit(NovaSupport).findAll('[data-testid="nova-support-grid"] > li')).toHaveLength(2)
  })

  it('links only the channels that have an address', () => {
    stubTemplateGlobals()
    stubChannels([
      { channel: SupportChannel.Email, value: 'help@example.test', href: 'mailto:help@example.test' },
      { channel: SupportChannel.Chat },
    ])
    const wrapper = mountWithKit(NovaSupport)

    expect(wrapper.find('[data-testid="nova-support-email"] [data-testid="nova-support-link"]').attributes('href')).toBe('mailto:help@example.test')
    expect(wrapper.find('[data-testid="nova-support-chat"] [data-testid="nova-support-link"]').exists()).toBe(false)
  })

  it('shows the response time only where one is set', () => {
    stubTemplateGlobals()
    stubChannels([
      { channel: SupportChannel.Phone, responseTime: '2 min' },
      { channel: SupportChannel.Ticket },
    ])
    const wrapper = mountWithKit(NovaSupport)

    expect(wrapper.find('[data-testid="nova-support-phone"] [data-testid="nova-support-response"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="nova-support-ticket"] [data-testid="nova-support-response"]').exists()).toBe(false)
  })
})
