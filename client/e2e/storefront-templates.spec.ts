/**
 * Flow: the storefront in each template.
 *
 * The operator picks the template in the admin settings; the stub serves that setting, so the
 * same home address renders a different design. Each template's home carries its own test id, and
 * the checkout of a seeded cart renders in all three (nova reuses aurora's checkout on purpose,
 * classic has its own) with the summary and the total.
 */
import { expect, test } from './support/test'
import { PortalSettingKey } from '../constants/portalSettingKey'
import { PLAN } from './support/fixtures.mjs'
import { resetStub, setPortalSetting, waitForHydration } from './support/helpers'

/** Every template with the test id of its home page. */
const TEMPLATES = [
  { name: 'aurora', home: 'aurora-home' },
  { name: 'nova', home: 'nova-home' },
  { name: 'classic', home: 'classic-home' },
]

/** localStorage key the cart is persisted under (`stores/cart.ts`). */
const CART_KEY = 'innovayse_cart'

for (const { name, home } of TEMPLATES) {
  test.describe(`${name} template`, () => {
    test.afterEach(async ({ request }) => {
      await resetStub(request)
    })

    test('the home page renders in this design', async ({ page, request }) => {
      await resetStub(request)
      await setPortalSetting(request, PortalSettingKey.Template, name)

      await page.goto('/')
      await waitForHydration(page)

      await expect(page.getByTestId(home)).toBeVisible()
    })

    test('a cart reaches the checkout summary and total', async ({ page, request }) => {
      await resetStub(request)
      await setPortalSetting(request, PortalSettingKey.Template, name)
      await page.addInitScript(([key, item]) => {
        window.localStorage.setItem(key as string, JSON.stringify([item]))
      }, [CART_KEY, {
        pid: PLAN.id,
        name: PLAN.name,
        billingcycle: 'monthly',
        cycleLabel: 'Monthly',
        amount: 5,
        currency: 'USD',
      }])

      await page.goto('/cart')
      await waitForHydration(page)
      await expect(page.getByTestId('cart-item')).toHaveCount(1)

      await page.getByTestId('cart-checkout').click()
      await expect(page).toHaveURL(/\/checkout$/)
      await expect(page.getByTestId('checkout-summary')).toBeVisible()
      await expect(page.getByTestId('checkout-total')).toBeVisible()
    })
  })
}
