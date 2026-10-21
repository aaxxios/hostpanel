/**
 * Tests for `useSupportChannels`: the ticket form is always offered, the other channels only
 * when the operator configured them, and each card carries its response time when set.
 *
 * @module tests/composables/useSupportChannels.test
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { CLIENT_TICKET_NEW_PATH } from '~/constants/clientRoutes'
import { PortalSettingKey } from '~/constants/portalSettingKey'
import { SupportChannel } from '~/constants/supportChannel'
import { useSupportChannels } from '~/composables/useSupportChannels'

/**
 * Stubs the settings reader with a
 * fixed set of values.
 *
 * @param values Setting key to value.
 */
const stubSettings = (values: Record<string, string>): void => {
  vi.stubGlobal('usePortalSettings', () => ({ get: (key: string) => values[key] ?? '' }))
}

describe('useSupportChannels', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('offers only the ticket form when nothing is configured', () => {
    stubSettings({})

    expect(useSupportChannels().value).toEqual([{ channel: SupportChannel.Ticket, href: CLIENT_TICKET_NEW_PATH }])
  })

  it('offers every channel with links and response times when configured', () => {
    stubSettings({
      [PortalSettingKey.ChatProvider]: 'widget',
      [PortalSettingKey.ContactEmail]: 'help@acme.test',
      [PortalSettingKey.ContactPhone]: '+1 (555) 010-200',
      [PortalSettingKey.SupportResponseChat]: '1 min',
      [PortalSettingKey.SupportResponseTicket]: '1 hour',
      [PortalSettingKey.SupportResponseEmail]: '4 hours',
      [PortalSettingKey.SupportResponsePhone]: '5 min',
    })

    expect(useSupportChannels().value).toEqual([
      { channel: SupportChannel.Chat, responseTime: '1 min' },
      { channel: SupportChannel.Ticket, href: CLIENT_TICKET_NEW_PATH, responseTime: '1 hour' },
      { channel: SupportChannel.Email, value: 'help@acme.test', href: 'mailto:help@acme.test', responseTime: '4 hours' },
      { channel: SupportChannel.Phone, value: '+1 (555) 010-200', href: 'tel:+1555010200', responseTime: '5 min' },
    ])
  })
})
