/**
 * Flow: knowledge base.
 *
 * A visitor browses a category, opens an article and searches. The article body is operator HTML:
 * the page rebuilds it from an allow-list, so formatting survives while a script, an event
 * handler and a `javascript:` link do not.
 */
import { expect, test } from './support/test'
import { KB_ARTICLES } from './support/fixtures.mjs'
import { required, waitForHydration } from './support/helpers'

/** The article whose body carries hostile markup. */
const HOSTILE = required(KB_ARTICLES[0], 'hostile article')

/** The article whose body is plain. */
const BILLING = required(KB_ARTICLES[1], 'billing article')

test('an article opens from its category with formatting kept and active content removed', async ({ page }) => {
  await page.goto('/knowledgebase')
  await waitForHydration(page)

  await page.getByTestId(`kb-category-${HOSTILE.category}`).click()
  await expect(page.getByTestId('kb-category-page')).toBeVisible()

  await page.getByTestId(`kb-article-${HOSTILE.id}`).click()
  await expect(page.getByTestId('kb-article-heading')).toHaveText(HOSTILE.title)

  const body = page.getByTestId('ui-rich-text')
  await expect(body.locator('strong')).toHaveText('account settings')
  await expect(body.locator('script')).toHaveCount(0)
  await expect(body.locator('[onerror]')).toHaveCount(0)
  await expect(body.locator('a[href^="javascript"]')).toHaveCount(0)

  // Nothing the markup tried to run has run.
  expect(await page.evaluate(() => (window as unknown as { __kbXss?: number }).__kbXss)).toBeUndefined()
})

test('searching finds the matching article only', async ({ page }) => {
  await page.goto('/knowledgebase')
  await waitForHydration(page)

  await page.getByTestId('kb-search-input').fill('billing')
  await page.getByTestId('kb-search-submit').click()

  await expect(page.getByTestId('kb-search-results')).toBeVisible()
  await expect(page.getByTestId(`kb-article-${BILLING.id}`)).toBeVisible()
  await expect(page.getByTestId(`kb-article-${HOSTILE.id}`)).toHaveCount(0)
})
