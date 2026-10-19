/**
 * Unit tests for FloatingActions: the fan opens and closes, each action appears only when the
 * operator configured it, and Escape or an outside click closes the fan.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { nextTick } from 'vue'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import FloatingActions from '~/components/ui/UiFloatingActions.vue'
import { ChatProvider } from '~/types/chatprovider'

/**
 * Stubs the portal settings composable with the given values.
 *
 * @param values Setting values by key.
 */
const stubSettings = (values: Record<string, string>): void => {
  vi.stubGlobal('usePortalSettings', () => ({ get: (key: string) => values[key] ?? '' }))
}

/**
 * Mounts the component attached to the page so document listeners see its elements.
 *
 * @returns The wrapper.
 */
const mountAttached = () => mountWithKit(FloatingActions, { attachToBody: true })

describe('FloatingActions', () => {
  beforeEach(() => {
    stubSettings({})
    vi.stubGlobal('openLiveChat', vi.fn(() => true))
  })

  it('starts closed with only the toggle button', () => {
    stubSettings({ 'portal.contact.whatsapp': '37410123456' })

    const wrapper = mountAttached()

    expect(wrapper.find('[data-testid="floating-toggle"]').attributes('aria-expanded')).toBe('false')
    expect(wrapper.find('[data-testid="floating-whatsapp"]').exists()).toBe(false)
    wrapper.unmount()
  })

  it('opens the fan with the configured channels only', async () => {
    stubSettings({ 'portal.contact.whatsapp': '37410123456', 'portal.contact.telegram': 'support' })
    const wrapper = mountAttached()

    await wrapper.find('[data-testid="floating-toggle"]').trigger('click')

    expect(wrapper.find('[data-testid="floating-toggle"]').attributes('aria-expanded')).toBe('true')
    expect(wrapper.find('[data-testid="floating-whatsapp"]').attributes('href')).toContain('https://wa.me/37410123456')
    expect(wrapper.find('[data-testid="floating-telegram"]').attributes('href')).toBe('https://t.me/support')
    expect(wrapper.find('[data-testid="floating-live-chat"]').exists()).toBe(false)
    wrapper.unmount()
  })

  it.each([[ChatProvider.LegacyChat], [ChatProvider.Innochat], ['InnoChat']])('offers live chat for the %s provider', async (provider) => {
    stubSettings({ 'portal.chat.provider': provider })
    const wrapper = mountAttached()

    await wrapper.find('[data-testid="floating-toggle"]').trigger('click')

    expect(wrapper.find('[data-testid="floating-live-chat"]').exists()).toBe(true)
    wrapper.unmount()
  })

  it('does not offer live chat for an unknown provider', async () => {
    stubSettings({ 'portal.chat.provider': 'other' })
    const wrapper = mountAttached()

    await wrapper.find('[data-testid="floating-toggle"]').trigger('click')

    expect(wrapper.find('[data-testid="floating-live-chat"]').exists()).toBe(false)
    wrapper.unmount()
  })

  it('closes the fan after choosing live chat', async () => {
    stubSettings({ 'portal.chat.provider': 'innochat' })
    const wrapper = mountAttached()
    await wrapper.find('[data-testid="floating-toggle"]').trigger('click')

    await wrapper.find('[data-testid="floating-live-chat"]').trigger('click')

    expect(wrapper.find('[data-testid="floating-toggle"]').attributes('aria-expanded')).toBe('false')
    wrapper.unmount()
  })

  it('closes on Escape', async () => {
    stubSettings({ 'portal.contact.telegram': 'support' })
    const wrapper = mountAttached()
    await wrapper.find('[data-testid="floating-toggle"]').trigger('click')

    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape' }))
    await nextTick()

    expect(wrapper.find('[data-testid="floating-toggle"]').attributes('aria-expanded')).toBe('false')
    wrapper.unmount()
  })

  it('sits above the cookie banner through the shared height variable', () => {
    const wrapper = mountAttached()

    expect(wrapper.find('[data-testid="floating-actions"]').classes())
      .toContain('bottom-[calc(1.5rem+var(--cookie-banner-height,0px))]')
    wrapper.unmount()
  })

  it('closes on a click outside', async () => {
    stubSettings({ 'portal.contact.telegram': 'support' })
    const wrapper = mountAttached()
    await wrapper.find('[data-testid="floating-toggle"]').trigger('click')

    document.body.click()
    await nextTick()

    expect(wrapper.find('[data-testid="floating-toggle"]').attributes('aria-expanded')).toBe('false')
    wrapper.unmount()
  })
})
