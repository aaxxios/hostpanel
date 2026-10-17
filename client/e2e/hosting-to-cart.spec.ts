/**
 * Flow: home -> hosting pricing -> choose a plan -> cart.
 *
 * The core purchase path of the storefront. It proves the catalogue is rendered on the server
 * from the backend's products, that "choose plan" leads to the configure page of that very
 * plan, and that confirming it puts the plan, at the payer's price, into the cart.
 */
import { expect, test } from './support/test'
import { PLAN } from './support/fixtures.mjs'
import { waitForHydration } from './support/helpers'

test('a visitor picks a hosting plan from the home page and finds it in the cart', async ({ page }) => {
  await page.goto('/')
  await waitForHydration(page)

  // Home -> hosting pricing, through the header navigation.
  await page.getByTestId('nav-hosting').click()
  await expect(page).toHaveURL(/\/hosting$/)
  await expect(page.getByTestId(`plan-card-${PLAN.id}`)).toContainText(PLAN.name)

  // Choose the plan -> its configure page.
  await page.getByTestId(`plan-choose-${PLAN.id}`).click()
  await expect(page).toHaveURL(new RegExp(`/configure/${PLAN.id}$`))

  // Continue -> the order moves on to checkout with the plan in the cart.
  await page.getByTestId('configure-continue').click()
  await expect(page).toHaveURL(/\/checkout$/)

  // The cart holds exactly that plan, priced as the backend quoted it ($5.00 monthly).
  await page.getByTestId('nav-cart').click()
  await expect(page).toHaveURL(/\/cart$/)
  await expect(page.getByTestId('cart-item')).toHaveCount(1)
  await expect(page.getByTestId('cart-item-name')).toHaveText(PLAN.name)
  await expect(page.getByTestId('cart-total')).toContainText('5.00')
  await expect(page.getByTestId('cart-checkout')).toBeVisible()
})
