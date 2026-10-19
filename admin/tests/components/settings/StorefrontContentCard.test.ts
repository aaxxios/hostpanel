/**
 * Tests for StorefrontContentCard: the figures, hero badge and contact fields, each saved on its
 * own, and the warning for a row the API never seeded.
 */
import { describe, expect, it } from 'vitest'
import StorefrontContentCard from '@/components/settings/StorefrontContentCard.vue'
import type { Setting } from '@/types/setting'
import { mountScreen } from '../../helpers/mount'

/** Every row the card edits, as the API seeds them. */
const rows: Setting[] = [
  { id: 1, key: 'portal.stats.clients', value: '5,000+' },
  { id: 2, key: 'portal.stats.uptime', value: '99.9%' },
  { id: 3, key: 'portal.stats.regions', value: '12' },
  { id: 4, key: 'portal.hero.badge', value: 'New' },
  { id: 5, key: 'portal.contact.email', value: 'hi@example.com' },
  { id: 6, key: 'portal.contact.phone', value: '+1 555' },
  { id: 7, key: 'portal.contact.whatsapp', value: '+1 556' },
  { id: 8, key: 'portal.contact.telegram', value: '@acme' },
  { id: 9, key: 'portal.contact.address', value: '1 Main St' },
  { id: 10, key: 'portal.contact.map_url', value: 'https://maps.example.com/x' },
]

/**
 * Reads the value of one field.
 *
 * @param wrapper - The mounted card.
 * @param testid - The field's test id.
 * @returns The input's value.
 */
const valueOf = (wrapper: Awaited<ReturnType<typeof mountScreen>>['wrapper'], testid: string): string =>
  (wrapper.get(`[data-testid="${testid}"]`).element as HTMLInputElement).value

describe('StorefrontContentCard', () => {
  it('shows the stored figures, badge and contact details', async () => {
    const { wrapper } = await mountScreen(StorefrontContentCard, { settings: rows })

    expect(valueOf(wrapper, 'storefront-stats-clients')).toBe('5,000+')
    expect(valueOf(wrapper, 'storefront-stats-uptime')).toBe('99.9%')
    expect(valueOf(wrapper, 'storefront-stats-regions')).toBe('12')
    expect(valueOf(wrapper, 'storefront-hero-badge')).toBe('New')
    expect(valueOf(wrapper, 'storefront-contact-email')).toBe('hi@example.com')
    expect(valueOf(wrapper, 'storefront-contact-phone')).toBe('+1 555')
    expect(valueOf(wrapper, 'storefront-contact-whatsapp')).toBe('+1 556')
    expect(valueOf(wrapper, 'storefront-contact-telegram')).toBe('@acme')
    expect(valueOf(wrapper, 'storefront-contact-address')).toBe('1 Main St')
    expect(valueOf(wrapper, 'storefront-contact-map-url')).toBe('https://maps.example.com/x')
  })

  it('keeps Save disabled until a field changes, then emits that row only', async () => {
    const { wrapper } = await mountScreen(StorefrontContentCard, { settings: rows })
    const save = wrapper.get('[data-testid="storefront-save-stats-uptime"]')
    expect(save.attributes('disabled')).toBeDefined()

    await wrapper.get('[data-testid="storefront-stats-uptime"]').setValue('99.99%')
    expect(save.attributes('disabled')).toBeUndefined()
    await save.trigger('click')

    expect(wrapper.emitted('save')).toEqual([[2, '99.99%']])
  })

  it('saves on Enter', async () => {
    const { wrapper } = await mountScreen(StorefrontContentCard, { settings: rows })
    const telegram = wrapper.get('[data-testid="storefront-contact-telegram"]')

    await telegram.setValue('@new')
    await telegram.trigger('keyup.enter')

    expect(wrapper.emitted('save')).toEqual([[8, '@new']])
  })

  it('says which rows are missing instead of drawing dead fields', async () => {
    const { wrapper } = await mountScreen(StorefrontContentCard, { settings: [rows[0] as Setting] })

    expect(wrapper.find('[data-testid="storefront-hero-badge"]').exists()).toBe(false)
    expect(wrapper.findAll('[data-testid="setting-not-seeded"]')).toHaveLength(9)
    expect(wrapper.text()).toContain('portal.hero.badge')
  })
})
