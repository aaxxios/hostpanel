/**
 * Flow: the home page's product section in the classic template.
 *
 * The catalogue holds more families than the home shows (hosting, VPS, dedicated servers, email
 * and SSL, plus the static domains card). The section lists the first three in a three column
 * grid, never stretches a card across the container, and "View all plans" leads to the products
 * page. (A refused refresh of the catalogue is covered by the catalogue store's unit tests.)
 */
import { expect, test } from './support/test'
import { PortalSettingKey } from '../constants/portalSettingKey'
import { HOME_PRODUCT_FAMILY_LIMIT } from '../constants/homeProducts'
import { resetStub, setPortalSetting, waitForHydration } from './support/helpers'

test.describe('home product families (classic)', () => {
  test.beforeEach(async ({ request }) => {
    await resetStub(request)
    await setPortalSetting(request, PortalSettingKey.Template, 'classic')
  })

  test.afterEach(async ({ request }) => {
    await resetStub(request)
  })

  test('shows three family cards at one column width and leads to every plan', async ({ page }) => {
    await page.goto('/')
    await waitForHydration(page)

    const grid = page.getByTestId('home-products-grid')
    await expect(grid.locator('[data-testid^="product-card-"]')).toHaveCount(HOME_PRODUCT_FAMILY_LIMIT)

    const widths = await grid.locator('[data-testid^="product-card-"]').evaluateAll((cards) => cards.map((card) => Math.round(card.getBoundingClientRect().width)))
    const gridWidth = await grid.evaluate((element) => element.getBoundingClientRect().width)
    // Each card is about a third of the grid, never the whole container.
    for (const width of widths) {
      expect(width).toBeLessThan(gridWidth / 2)
    }

    await expect(page.getByTestId('home-products-all')).toHaveAttribute('href', /\/products$/)
  })
})
