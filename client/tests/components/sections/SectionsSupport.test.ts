/**
 * Unit tests for SectionsSupport: one card per channel, the reply time and link only when the
 * channel carries them, and no section without channels.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import SectionsSupport from '~/components/sections/SectionsSupport.vue'
import { SupportChannel } from '~/constants/supportChannel'
import type { SupportChannelItem } from '~/types/supportchannelitem'
import { CLASSIC_COMPONENTS } from '~/tests/helpers/classicKit'

/** The channels the stubbed composable returns. */
const channels = ref<SupportChannelItem[]>([])

vi.stubGlobal('useSupportChannels', () => channels)

describe('SectionsSupport', () => {
  beforeEach(() => {
    channels.value = [
      { channel: SupportChannel.Ticket, href: '/client/tickets/new', responseTime: '15 min' },
      { channel: SupportChannel.Email, value: 'help@example.test', href: 'mailto:help@example.test' },
      { channel: SupportChannel.Chat },
    ]
  })

  it('shows a card for each channel', () => {
    const wrapper = mountWithKit(SectionsSupport, { components: CLASSIC_COMPONENTS })

    expect(wrapper.find('[data-testid="support-ticket"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="support-email"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="support-phone"]').exists()).toBe(false)
  })

  it('shows the reply time only when the channel has one', () => {
    const wrapper = mountWithKit(SectionsSupport, { components: CLASSIC_COMPONENTS })

    expect(wrapper.find('[data-testid="support-response-ticket"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="support-response-email"]').exists()).toBe(false)
  })

  it('links a channel that has a target and shows its value', () => {
    const wrapper = mountWithKit(SectionsSupport, { components: CLASSIC_COMPONENTS })

    const link = wrapper.find('[data-testid="support-link-email"]')
    expect(link.attributes('href')).toBe('mailto:help@example.test')
    expect(link.text()).toBe('help@example.test')
    expect(wrapper.find('[data-testid="support-link-chat"]').exists()).toBe(false)
  })

  it('renders nothing without channels', () => {
    channels.value = []

    const wrapper = mountWithKit(SectionsSupport, { components: CLASSIC_COMPONENTS })

    expect(wrapper.find('[data-testid="sections-support"]').exists()).toBe(false)
  })

  it('draws every icon on a flat brand tile with no accent glow', () => {
    const wrapper = mountWithKit(SectionsSupport, { components: CLASSIC_COMPONENTS })

    const tiles = wrapper.findAll('[data-testid="classic-icon-tile"]')
    expect(tiles.length).toBeGreaterThan(0)
    expect(wrapper.html()).not.toContain('--accent')
  })

  it('caps the grid width for one and two cards and leaves more uncapped', () => {
    const cases: Array<[number, string | null]> = [[1, 'max-w-xl'], [2, 'max-w-4xl'], [3, null]]
    for (const [count, cap] of cases) {
      channels.value = [SupportChannel.Ticket, SupportChannel.Email, SupportChannel.Chat].slice(0, count).map((channel) => ({ channel }))
      const wrapper = mountWithKit(SectionsSupport, { components: CLASSIC_COMPONENTS })
      const classes = wrapper.find('[data-testid="sections-support-grid"]').classes()

      if (cap) {
        expect(classes).toContain(cap)
      } else {
        expect(classes).not.toContain('max-w-xl')
        expect(classes).not.toContain('max-w-4xl')
      }
    }
  })
})
