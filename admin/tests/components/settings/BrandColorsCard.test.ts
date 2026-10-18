/**
 * Tests for BrandColorsCard: the hex field and colour well share one draft, an invalid hex cannot
 * be saved, the contrast line follows the colour, and the swatches are SVG (no inline style).
 */
import { describe, expect, it } from 'vitest'
import BrandColorsCard from '@/components/settings/BrandColorsCard.vue'
import type { Setting } from '@/types/setting'
import { mountScreen } from '../../helpers/mount'

/** Both colour rows, as the API seeds them. */
const rows: Setting[] = [
  { id: 1, key: 'portal.brand.primary', value: '#0ea5e9' },
  { id: 2, key: 'portal.brand.accent', value: '' },
]

describe('BrandColorsCard', () => {
  it('shows the stored colour in the hex field and the colour well', async () => {
    const { wrapper } = await mountScreen(BrandColorsCard, { settings: rows })

    expect((wrapper.get('[data-testid="brand-colors-hex-primary"]').element as HTMLInputElement).value).toBe('#0ea5e9')
    expect((wrapper.get('[data-testid="brand-colors-well-primary"]').element as HTMLInputElement).value).toBe('#0ea5e9')
  })

  it('previews the built-in default when the colour is empty', async () => {
    const { wrapper } = await mountScreen(BrandColorsCard, { settings: rows })

    expect((wrapper.get('[data-testid="brand-colors-well-accent"]').element as HTMLInputElement).value).toBe('#a855f7')
  })

  it('saves a valid hex typed in the field', async () => {
    const { wrapper } = await mountScreen(BrandColorsCard, { settings: rows })

    await wrapper.get('[data-testid="brand-colors-hex-primary"]').setValue('#112233')
    await wrapper.get('[data-testid="brand-colors-save-primary"]').trigger('click')

    expect(wrapper.emitted('save')).toEqual([[1, '#112233']])
  })

  it('refuses to enable Save for text that is not #rrggbb', async () => {
    const { wrapper } = await mountScreen(BrandColorsCard, { settings: rows })

    await wrapper.get('[data-testid="brand-colors-hex-primary"]').setValue('#12')

    expect(wrapper.get('[data-testid="brand-colors-save-primary"]').attributes('disabled')).toBeDefined()
    expect(wrapper.get('[data-testid="brand-colors-hex-primary"]').attributes('aria-invalid')).toBe('true')
  })

  it('resets to the template colour by saving an empty value', async () => {
    const { wrapper } = await mountScreen(BrandColorsCard, { settings: rows })

    await wrapper.get('[data-testid="brand-colors-reset-primary"]').trigger('click')

    expect(wrapper.emitted('save')).toEqual([[1, '']])
  })

  it('says whether white text passes on the colour', async () => {
    const { wrapper } = await mountScreen(BrandColorsCard, { settings: rows })

    await wrapper.get('[data-testid="brand-colors-hex-primary"]').setValue('#ffffff')

    expect(wrapper.get('[data-testid="brand-colors-contrast-primary"]').text()).toContain('1.0')
  })

  it('draws the shades as SVG without any inline style', async () => {
    const { wrapper } = await mountScreen(BrandColorsCard, { settings: rows })

    expect(wrapper.findAll('svg rect').length).toBeGreaterThan(10)
    expect(wrapper.html()).not.toContain('style=')
  })
})
