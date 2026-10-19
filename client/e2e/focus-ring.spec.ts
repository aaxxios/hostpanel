/**
 * Flow: focus ring.
 *
 * A mouse click on a link must leave no outline behind; the ring belongs to keyboard focus only.
 */
import { expect, test } from './support/test'
import { waitForHydration } from './support/helpers'

test('clicking a link leaves no focus outline or ring', async ({ page }) => {
  await page.goto('/faq')
  await waitForHydration(page)

  const link = page.getByTestId('faq-category-general')
  await link.click()
  await expect(page).toHaveURL(/\/faq\/general$/)

  const back = page.getByTestId('faq-back-to-all')
  await back.click()
  const style = await back.evaluate(el => ({ outline: getComputedStyle(el).outlineStyle, shadow: getComputedStyle(el).boxShadow }))
  expect(style.outline).toBe('none')
  expect(style.shadow).toBe('none')
})
