/**
 * Flow: announcements.
 *
 * The list links to each announcement; the detail renders the operator HTML through the same
 * allow-list as the knowledge base, so a script in the body never runs.
 */
import { expect, test } from './support/test'
import { ANNOUNCEMENT } from './support/fixtures.mjs'
import { waitForHydration } from './support/helpers'

test('an announcement opens from the list with its script stripped', async ({ page }) => {
  await page.goto('/announcements')
  await waitForHydration(page)

  await page.getByTestId(`announcement-${ANNOUNCEMENT.id}`).click()
  await expect(page.getByTestId('announcement-heading')).toHaveText(ANNOUNCEMENT.title)

  const body = page.getByTestId('ui-rich-text')
  await expect(body.locator('em')).toHaveText('upgrade')
  await expect(body.locator('script')).toHaveCount(0)
  expect(await page.evaluate(() => (window as unknown as { __newsXss?: number }).__newsXss)).toBeUndefined()
})
