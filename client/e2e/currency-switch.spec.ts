/**
 * Flow: the header currency switcher.
 *
 * A guest picks a billing currency from the switcher next to the language picker; the choice
 * persists across a reload, the catalogue is requested again from the portal in that currency
 * (the browser's own request is observed: the portal caches a guest's catalogue per currency for
 * an hour, so the stub only sees the first template's read) and the plan cards show the AMD prices. A signed-in client's
 * currency is fixed on their record, so they get a read-only chip with a hint and no menu. The
 * flow runs in every storefront template, since each has its own header.
 */
import { expect, test } from './support/test'
import { PortalSettingKey } from '../constants/portalSettingKey'
import { PLAN } from './support/fixtures.mjs'
import { resetStub, setPortalSetting, signIn, waitForHydration } from './support/helpers'

/**
 * Every template whose header carries the switcher, with the test id of the element on its
 * hosting page that prints the plan prices (each template builds that page its own way).
 */
const TEMPLATES: Record<string, string> = {
  aurora: `plan-card-${PLAN.id}`,
  nova: `nova-plan-${PLAN.id}`,
  classic: 'classic-hosting-plans',
}

for (const [template, pricesTestId] of Object.entries(TEMPLATES)) {
  test.describe(`${template} template`, () => {
    test.beforeEach(async ({ request }) => {
      await resetStub(request)
      await setPortalSetting(request, PortalSettingKey.Template, template)
    })

    test.afterEach(async ({ request }) => {
      await resetStub(request)
    })

    test('a guest switches to AMD, the prices are read in AMD and the choice survives a reload', async ({ page }) => {
      await page.goto('/hosting')
      await waitForHydration(page)

      // Priced in the base currency until the guest chooses otherwise.
      await expect(page.getByTestId('currency-switcher-current').first()).toHaveText('USD')
      await expect(page.getByTestId(pricesTestId)).toContainText('$')

      // Registered before the click so the request cannot be missed.
      const amdCatalogue = page.waitForRequest(req =>
        req.url().includes('/api/portal/public/products') && req.url().includes('currency=AMD'))
      await page.getByTestId('currency-switcher-trigger').first().click()
      await page.getByTestId('currency-option-AMD').first().click()

      // The catalogue is requested again in the chosen currency, and the card shows the AMD price.
      await amdCatalogue
      await expect(page.getByTestId('currency-switcher-current').first()).toHaveText('AMD')
      await expect(page.getByTestId(pricesTestId)).toContainText('AMD')

      // The choice is remembered after a reload.
      await page.reload()
      await waitForHydration(page)
      await expect(page.getByTestId('currency-switcher-current').first()).toHaveText('AMD')
      await expect(page.getByTestId(pricesTestId)).toContainText('AMD')
    })

    test('a signed-in client sees their fixed currency read-only with a hint', async ({ page, context, baseURL }) => {
      await signIn(context, baseURL ?? '')
      await page.goto('/hosting')
      await waitForHydration(page)

      await expect(page.getByTestId('currency-switcher-fixed').first()).toBeVisible()
      await expect(page.getByTestId('currency-switcher-current').first()).toHaveText('USD')
      await expect(page.getByTestId('currency-switcher-trigger')).toHaveCount(0)

      await page.getByTestId('currency-switcher-fixed').first().click()
      await expect(page.getByTestId('currency-switcher-hint').first()).toBeVisible()
    })
  })
}
