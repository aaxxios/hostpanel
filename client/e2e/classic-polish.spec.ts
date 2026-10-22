/**
 * Flow: classic template polish.
 *
 * Three things that looked wrong on the classic home and products pages, each checked in the light
 * and the dark palette: the hero slider follows the colour mode (a slide with no image is the plain
 * page colour, not a black panel), the cover of every product-family card is fitted whole instead
 * of cropped, and the page ends at the footer (the cookie card, which is still on screen here, does
 * not leave an empty band under it).
 */
import { expect, test } from './support/test'
import { PortalSettingKey } from '../constants/portalSettingKey'
import { enableSlides, resetStub, setPortalSetting, waitForHydration } from './support/helpers'

/** The luminance (0 to 255) above which a `rgb(r, g, b)` colour counts as light. */
const LIGHT_LUMINANCE = 128

/**
 * Average of the red, green and blue channels of a computed `rgb(...)` / `rgba(...)` colour.
 *
 * @param colour The computed colour string.
 * @returns The channel average, 0 for black and 255 for white.
 */
const luminance = (colour: string): number => {
  const channels = (colour.match(/\d+(\.\d+)?/g) ?? []).slice(0, 3).map(Number)
  return channels.reduce((sum, channel) => sum + channel, 0) / 3
}

// This flow is about the consent card that every new visitor sees, so no choice is pre-made.
test.use({ storageState: { cookies: [], origins: [] } })

for (const mode of ['light', 'dark'] as const) {
  test.describe(`classic in ${mode}`, () => {
    test.beforeEach(async ({ request, context, baseURL }) => {
      await resetStub(request)
      await setPortalSetting(request, PortalSettingKey.Template, 'classic')
      await enableSlides(request)
      await context.addCookies([{ name: 'color-mode', value: mode, url: baseURL as string }])
    })

    test.afterEach(async ({ request }) => {
      await resetStub(request)
    })

    test('the slider panel follows the mode, and a slide with no image has no dark scrim', async ({ page }) => {
      await page.goto('/')
      await waitForHydration(page)

      const panel = page.getByTestId('products-slider')
      await expect(panel).toBeVisible()
      const panelColour = await panel.evaluate((element) => getComputedStyle(element).backgroundColor)
      // Light page colour in light mode, dark page colour in dark mode: never the old black panel on a light page.
      if (mode === 'light') {
        expect(luminance(panelColour)).toBeGreaterThan(LIGHT_LUMINANCE)
      } else {
        expect(luminance(panelColour)).toBeLessThan(LIGHT_LUMINANCE)
      }

      // The slide with no image: no scrim, and its backdrop is the page colour of the mode.
      const plain = page.getByTestId('slider-background-2')
      await expect(plain.locator('.bg-black\\/60')).toHaveCount(0)
      const plainColour = await plain.evaluate((element) => getComputedStyle(element).backgroundColor)
      expect(luminance(plainColour) > LIGHT_LUMINANCE).toBe(mode === 'light')
      // The slide with a photograph keeps its scrim for the copy's contrast.
      await expect(page.getByTestId('slider-background-1').locator('.bg-black\\/60')).toHaveCount(1)
    })

    test('the domain card cover is fitted whole, and the page ends at the footer', async ({ page }) => {
      await page.goto('/products')
      await waitForHydration(page)

      // The cover art is fitted whole (contain), never cropped to fill the wide box (cover).
      const art = page.getByTestId('product-card-domains').getByTestId('product-cover-art')
      await expect(art).toBeVisible()
      const maskSize = await art.evaluate((element) => getComputedStyle(element).maskSize)
      expect(maskSize).toBe('contain')

      // The consent card is still showing, and the footer is the last thing on the page: no band below it.
      await expect(page.getByTestId('cookie-banner')).toBeVisible()
      const gap = await page.evaluate(() => {
        const footer = document.querySelector('[data-testid="layout-footer"]') as HTMLElement
        return document.documentElement.scrollHeight - (footer.getBoundingClientRect().bottom + window.scrollY)
      })
      expect(gap).toBeLessThanOrEqual(1)
    })
  })
}
