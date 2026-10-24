/**
 * Tests for `useContactDetails`: the links are built from the operator's settings and every
 * item is empty (so its row is hidden) when the setting is not set.
 *
 * @module tests/composables/useContactDetails.test
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'
import { useContactDetails } from '~/composables/useContactDetails'

/**
 * Stubs the settings reader with a fixed set of values.
 *
 * @param values Setting key to value.
 * @param locale The visitor's language.
 */
const stubSettings = (values: Record<string, string>, locale = 'en'): void => {
  vi.stubGlobal('usePortalSettings', () => ({ get: (key: string) => values[key] ?? '' }))
  vi.stubGlobal('useI18n', () => ({ locale: ref(locale) }))
}

describe('useContactDetails', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('builds the phone, Telegram and WhatsApp links from the configured values', () => {
    stubSettings({
      'portal.contact.email': 'hello@acme.test',
      'portal.contact.phone': '+1 (555) 010-200',
      'portal.contact.telegram': '@acme',
      'portal.contact.whatsapp': '+1 555 010 200',
      'portal.contact.address': '1 Example Street',
      'portal.contact.map_url': 'https://maps.example.test/embed',
    })

    const details = useContactDetails()

    expect(details.email.value).toBe('hello@acme.test')
    expect(details.phone.value).toBe('+1 (555) 010-200')
    expect(details.phoneLink.value).toBe('+1555010200')
    expect(details.telegramUrl.value).toBe('https://t.me/acme')
    expect(details.whatsappUrl.value).toBe('https://wa.me/1555010200')
    expect(details.address.value).toBe('1 Example Street')
    expect(details.mapUrl.value).toBe('https://maps.example.test/embed')
  })

  it('answers empty strings when nothing is configured', () => {
    stubSettings({})

    const details = useContactDetails()

    expect(details.email.value).toBe('')
    expect(details.phoneLink.value).toBe('')
    expect(details.telegramUrl.value).toBe('')
    expect(details.whatsappUrl.value).toBe('')
    expect(details.address.value).toBe('')
    expect(details.mapUrl.value).toBe('')
  })

  it('refuses a map address that is not https', () => {
    stubSettings({ 'portal.contact.map_url': 'http://maps.example.test/embed' })

    expect(useContactDetails().mapUrl.value).toBe('')
  })

  it('reads the address in the visitor language and falls back to the default one', () => {
    stubSettings({ 'portal.contact.address': '1 Example Street', 'portal.contact.address.ru': 'ул. Примерная, 1' }, 'ru')
    expect(useContactDetails().address.value).toBe('ул. Примерная, 1')

    stubSettings({ 'portal.contact.address': '1 Example Street', 'portal.contact.address.ru': 'ул. Примерная, 1' }, 'hy')
    expect(useContactDetails().address.value).toBe('1 Example Street')
  })
})
