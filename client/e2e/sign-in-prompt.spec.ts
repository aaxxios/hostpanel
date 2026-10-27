/**
 * Flow: the sign-in prompt after a silent sign-in found no session.
 *
 * The sign-in callback returns such a visitor to the page they were on with `?signin=required`.
 * The page then says they are not signed in and offers the sign-in, instead of rendering as a
 * silent guest page; dismissing the prompt leaves them on the page without it.
 */
import { expect, test } from './support/test'
import { waitForHydration } from './support/helpers'

test('a flagged page shows the sign-in prompt, which returns to the page and carries the account hint', async ({ page }) => {
  await page.goto('/?authuser=person%40example.test&signin=required')
  await waitForHydration(page)

  await expect(page.getByTestId('signin-prompt')).toBeVisible()
  const href = await page.getByTestId('signin-prompt-signin').getAttribute('href')
  const target = new URL(href ?? '', 'http://127.0.0.1')
  expect(target.pathname).toBe('/api/portal/auth/sso/authorize')
  expect(target.searchParams.get('returnTo')).toBe('/?authuser=person%40example.test')
  expect(target.searchParams.get('authuser')).toBe('person@example.test')
})

test('dismissing the prompt removes it and the flag', async ({ page }) => {
  await page.goto('/?signin=required')
  await waitForHydration(page)

  await page.getByTestId('signin-prompt-dismiss').click()

  await expect(page.getByTestId('signin-prompt')).toHaveCount(0)
  await expect(page).not.toHaveURL(/signin=required/)
})

test('an unflagged page shows no prompt', async ({ page }) => {
  await page.goto('/')
  await waitForHydration(page)

  await expect(page.getByTestId('signin-prompt')).toHaveCount(0)
})
