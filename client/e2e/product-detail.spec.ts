/**
 * Flow: a product page.
 *
 * `/products/<family>` shows the family's plans with their prices, and a plan can be put into the
 * cart from there. An address that names no product shows the not-found state with a way back
 * instead of an empty page.
 */
import { expect, test } from './support/test'
import { PLAN, PLAN_PRO } from './support/fixtures.mjs'
import { resetStub, waitForHydration } from './support/helpers'

test.beforeEach(async ({ request }) => {
  await resetStub(request)
})

test('the product page lists the plans of the family with their prices', async ({ page }) => {
  await page.goto('/products/web-hosting')
  await waitForHydration(page)

  await expect(page.getByTestId('product-page')).toBeVisible()
  await expect(page.getByTestId('product-name')).toHaveText(PLAN.name)
  await expect(page.getByTestId(`product-plan-${PLAN.id}`)).toContainText(PLAN.name)
  await expect(page.getByTestId(`product-plan-${PLAN.id}`).getByTestId('product-plan-price')).toContainText('5.00')
  await expect(page.getByTestId(`product-plan-${PLAN_PRO.id}`).getByTestId('product-plan-price')).toContainText('15.00')
})

test('a plan added from the product page is in the cart', async ({ page }) => {
  await page.goto('/products/web-hosting')
  await waitForHydration(page)

  await page.getByTestId(`product-add-${PLAN.id}`).click()

  // Adding opens the cart drawer with the plan in it; it covers the page until it is closed.
  await expect(page.getByTestId('cart-drawer-item')).toHaveCount(1)
  await page.getByTestId('ui-drawer-close').click()
  await expect(page.getByTestId('ui-drawer')).toHaveCount(0)

  await page.getByTestId('nav-cart').click()
  await expect(page).toHaveURL(/\/cart$/)
  await expect(page.getByTestId('cart-item')).toHaveCount(1)
  await expect(page.getByTestId('cart-item-name')).toHaveText(PLAN.name)
})

test('an unknown product shows the not-found state', async ({ page }) => {
  await page.goto('/products/no-such-product')
  await waitForHydration(page)

  await expect(page.getByTestId('product-not-found')).toBeVisible()
  await expect(page.getByTestId('product-back')).toBeVisible()
  await expect(page.getByTestId('product-page')).toHaveCount(0)
})
