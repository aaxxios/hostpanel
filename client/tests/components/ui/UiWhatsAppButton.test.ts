/**
 * Unit tests for WhatsAppButton: each channel renders only when the operator configured it, with
 * the right link.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import WhatsAppButton from '~/components/ui/UiWhatsAppButton.vue'

/**
 * Stubs the portal settings composable with the given values.
 *
 * @param values Setting values by key.
 */
const stubSettings = (values: Record<string, string>): void => {
  vi.stubGlobal('usePortalSettings', () => ({ get: (key: string) => values[key] ?? '' }))
}

describe('WhatsAppButton', () => {
  beforeEach(() => {
    stubSettings({})
  })

  it('renders nothing when no channel is configured', () => {
    const wrapper = mountWithKit(WhatsAppButton)

    expect(wrapper.find('[data-testid="contact-whatsapp"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="contact-telegram"]').exists()).toBe(false)
  })

  it('links WhatsApp with the digits of the number and the encoded greeting', () => {
    stubSettings({ 'portal.contact.whatsapp': '+374 (10) 123-456' })

    const wrapper = mountWithKit(WhatsAppButton)

    const link = wrapper.find('[data-testid="contact-whatsapp"]')
    expect(link.attributes('href')).toBe('https://wa.me/37410123456?text=whatsapp.defaultMessage')
    expect(link.attributes('target')).toBe('_blank')
    expect(link.attributes('rel')).toBe('noopener noreferrer')
    expect(link.attributes('aria-label')).toBe('whatsapp.contactUs')
  })

  it('links Telegram with the handle', () => {
    stubSettings({ 'portal.contact.telegram': 'hostpanel_support' })

    const wrapper = mountWithKit(WhatsAppButton)

    expect(wrapper.find('[data-testid="contact-telegram"]').attributes('href')).toBe('https://t.me/hostpanel_support')
    expect(wrapper.find('[data-testid="contact-whatsapp"]').exists()).toBe(false)
  })

  it('shows both channels when both are configured', () => {
    stubSettings({ 'portal.contact.whatsapp': '37410123456', 'portal.contact.telegram': 'support' })

    const wrapper = mountWithKit(WhatsAppButton)

    expect(wrapper.findAll('a')).toHaveLength(2)
  })
})
