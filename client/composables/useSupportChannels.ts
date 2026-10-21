/**
 * The channels of the support section: which ways of reaching support exist on this install,
 * where each one leads and how fast the operator answers on it.
 *
 * @module composables/useSupportChannels
 */

import { CLIENT_TICKET_NEW_PATH } from '~/constants/clientRoutes'
import { MAILTO_PREFIX, TEL_PREFIX } from '~/constants/contactChannel'
import { PortalSettingKey } from '~/constants/portalSettingKey'
import { SupportChannel } from '~/constants/supportChannel'
import { useContactDetails } from '~/composables/useContactDetails'
import type { SupportChannelItem } from '~/types/supportchannelitem'

/** The setting holding how fast each channel is answered. */
const RESPONSE_SETTING: Record<SupportChannel, PortalSettingKey> = {
  [SupportChannel.Chat]: PortalSettingKey.SupportResponseChat,
  [SupportChannel.Ticket]: PortalSettingKey.SupportResponseTicket,
  [SupportChannel.Email]: PortalSettingKey.SupportResponseEmail,
  [SupportChannel.Phone]: PortalSettingKey.SupportResponsePhone,
}

/**
 * Reads the support channels.
 *
 * @returns The available channels in enum order: chat when a chat provider is set, the ticket
 * form always, e-mail and phone when the operator gave them.
 */
export const useSupportChannels = () => {
  const { get } = usePortalSettings()
  const { email, phone, phoneLink } = useContactDetails()

  return computed<SupportChannelItem[]>(() => {
    /**
     * Builds one channel with its response time, leaving the time out when it is not set.
     *
     * @param channel - The channel.
     * @param extra - Its value and link, when it has them.
     * @returns The channel item.
     */
    const item = (channel: SupportChannel, extra: Pick<SupportChannelItem, 'value' | 'href'> = {}): SupportChannelItem => {
      const responseTime = get(RESPONSE_SETTING[channel], '')
      return responseTime ? { channel, ...extra, responseTime } : { channel, ...extra }
    }

    const items: SupportChannelItem[] = []

    if (get(PortalSettingKey.ChatProvider, '')) {
      items.push(item(SupportChannel.Chat))
    }

    items.push(item(SupportChannel.Ticket, { href: CLIENT_TICKET_NEW_PATH }))

    if (email.value) {
      items.push(item(SupportChannel.Email, { value: email.value, href: `${MAILTO_PREFIX}${email.value}` }))
    }

    if (phone.value) {
      items.push(item(SupportChannel.Phone, { value: phone.value, href: `${TEL_PREFIX}${phoneLink.value}` }))
    }

    return items
  })
}
