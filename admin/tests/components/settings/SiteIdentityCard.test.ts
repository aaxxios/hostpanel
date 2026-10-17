/**
 * Tests for SiteIdentityCard: the name and tagline fields, each saved on its own, and the warning
 * for a row the API never seeded.
 */
import { describe, expect, it } from 'vitest'
import SiteIdentityCard from '@/components/settings/SiteIdentityCard.vue'
import type { Setting } from '@/types/setting'
import { mountScreen } from '../../helpers/mount'

/** Both identity rows, as the API seeds them. */
const rows: Setting[] = [
  { id: 1, key: 'portal.site.name', value: 'Acme Hosting' },
  { id: 2, key: 'portal.site.tagline', value: 'Fast and calm' },
]

describe('SiteIdentityCard', () => {
  it('shows the stored name and tagline', async () => {
    const { wrapper } = await mountScreen(SiteIdentityCard, { settings: rows })

    expect((wrapper.get('[data-testid="site-identity-name"]').element as HTMLInputElement).value).toBe('Acme Hosting')
    expect((wrapper.get('[data-testid="site-identity-tagline"]').element as HTMLInputElement).value).toBe('Fast and calm')
  })

  it('keeps Save disabled until a field changes, then emits that row only', async () => {
    const { wrapper } = await mountScreen(SiteIdentityCard, { settings: rows })
    const save = wrapper.get('[data-testid="site-identity-save-name"]')
    expect(save.attributes('disabled')).toBeDefined()

    await wrapper.get('[data-testid="site-identity-name"]').setValue('Acme Cloud')
    expect(save.attributes('disabled')).toBeUndefined()
    await save.trigger('click')

    expect(wrapper.emitted('save')).toEqual([[1, 'Acme Cloud']])
  })

  it('saves on Enter', async () => {
    const { wrapper } = await mountScreen(SiteIdentityCard, { settings: rows })
    const tagline = wrapper.get('[data-testid="site-identity-tagline"]')

    await tagline.setValue('Faster')
    await tagline.trigger('keyup.enter')

    expect(wrapper.emitted('save')).toEqual([[2, 'Faster']])
  })

  it('says which row is missing instead of drawing a dead field', async () => {
    const { wrapper } = await mountScreen(SiteIdentityCard, { settings: [rows[0] as Setting] })

    expect(wrapper.find('[data-testid="site-identity-tagline"]').exists()).toBe(false)
    expect(wrapper.get('[data-testid="setting-not-seeded"]').text()).toContain('portal.site.tagline')
  })

  it('disables Save while a save is in flight', async () => {
    const { wrapper } = await mountScreen(SiteIdentityCard, { settings: rows, saving: true })
    await wrapper.get('[data-testid="site-identity-name"]').setValue('X')

    expect(wrapper.get('[data-testid="site-identity-save-name"]').attributes('disabled')).toBeDefined()
  })
})
