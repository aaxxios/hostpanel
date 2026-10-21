/**
 * Unit tests for AuroraSupport: a card per channel, links only where the channel has one, the response time only when set.
 */
import { describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import AuroraSupport from '~/templates/aurora/sections/AuroraSupport.vue'
import { SupportChannel } from '~/constants/supportChannel'
import type { SupportChannelItem } from '~/types/supportchannelitem'
import { stubAuroraGlobals } from '../auroraGlobals'
import { balancedColumns } from '~/utils/balancedGrid'

/** The channels the mocked composable reports. */
const channels = vi.hoisted(() => ({ value: [] as SupportChannelItem[] }))

vi.mock('~/composables/useSupportChannels', () => ({
  /**
   * Stands in for the channels composable.
   *
   * @returns The mocked channels as a ref.
   */
  useSupportChannels: () => ref(channels.value),
}))

describe('AuroraSupport', () => {
  it('links a channel that has an address and shows its value and response time', () => {
    channels.value = [
      { channel: SupportChannel.Chat, responseTime: '2 min' },
      { channel: SupportChannel.Email, value: 'help@example.test', href: 'mailto:help@example.test' },
    ]
    stubAuroraGlobals()

    const wrapper = mountWithKit(AuroraSupport)

    expect(wrapper.find('[data-testid="support-email"]').attributes('href')).toBe('mailto:help@example.test')
    expect(wrapper.find('[data-testid="support-value-email"]').text()).toBe('help@example.test')
    expect(wrapper.find('[data-testid="support-chat"]').attributes('href')).toBeUndefined()
    expect(wrapper.find('[data-testid="support-response-chat"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="support-response-email"]').exists()).toBe(false)
  })

  it('renders nothing without channels', () => {
    channels.value = []
    stubAuroraGlobals()

    expect(mountWithKit(AuroraSupport).find('[data-testid="aurora-support"]').exists()).toBe(false)
  })

  it('lays the cards out in the balanced grid', () => {
    channels.value = [
      { channel: SupportChannel.Ticket, href: '/client/tickets/new' },
      { channel: SupportChannel.Phone, value: '+1 555 0100', href: 'tel:+15550100' },
    ]
    stubAuroraGlobals()

    const grid = mountWithKit(AuroraSupport).find('[data-testid="aurora-support-grid"]')

    expect(grid.attributes('data-columns')).toBe(String(balancedColumns(2)))
  })
})
