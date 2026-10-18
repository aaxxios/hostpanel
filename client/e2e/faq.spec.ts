/**
 * Flow: FAQ.
 *
 * The landing page lists the topics; a topic page shows its questions in an accordion whose first
 * answer is open and can be closed, and leads back to the full list.
 */
import { expect, test } from './support/test'
import { waitForHydration } from './support/helpers'

test('a topic opens its accordion and leads back to all questions', async ({ page }) => {
  await page.goto('/faq')
  await waitForHydration(page)
  await expect(page.getByTestId('faq-category-nav')).toBeVisible()

  await page.getByTestId('faq-category-general').click()
  await expect(page).toHaveURL(/\/faq\/general$/)
  await expect(page.getByTestId('faq-accordion')).toBeVisible()

  // The first answer starts open and closes on a click.
  const first = page.getByTestId('ui-accordion-toggle-0')
  await expect(first).toHaveAttribute('aria-expanded', 'true')
  await first.click()
  await expect(first).toHaveAttribute('aria-expanded', 'false')

  await page.getByTestId('faq-back-to-all').click()
  await expect(page).toHaveURL(/\/faq$/)
  await expect(page.getByTestId('faq-page')).toBeVisible()
})
