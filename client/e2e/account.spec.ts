/**
 * Flow: the account page.
 *
 * The profile tab shows who the customer is; editing saves through the backend, the summary then
 * shows the saved value and the form stays open with its confirmation. When the profile cannot be
 * read at all, the page says so instead of rendering an empty form that would overwrite it.
 */
import { expect, test } from './support/test'
import { CLIENT, NEW_FIRST_NAME } from './support/fixtures.mjs'
import { resetStub, scriptStub, signIn, stubRequests, waitForHydration } from './support/helpers'
import { HttpMethod } from '../types/httpmethod'

test.beforeEach(async ({ context, request, baseURL }) => {
  await resetStub(request)
  await signIn(context, baseURL ?? 'http://127.0.0.1:3100')
})

test('editing the first name saves it and the summary shows the new name', async ({ page, request }) => {
  await page.goto('/client/account')
  await waitForHydration(page)
  await expect(page.getByTestId('account-full-name')).toContainText(CLIENT.firstName)

  await page.getByTestId('account-profile-edit').click()
  await page.getByTestId('account-firstname').fill(NEW_FIRST_NAME)
  await page.getByTestId('account-profile-save').click()

  await expect(page.getByTestId('account-full-name')).toContainText(NEW_FIRST_NAME)
  // A saved draft stays open with the form's confirmation, so the customer sees the save landed.
  await expect(page.getByTestId('account-profile-form').getByTestId('ui-form-success')).toBeVisible()

  const saved = (await stubRequests(request)).find(r => r.method === 'PATCH' && r.path === '/api/clients/me')
  expect(saved?.body).toMatchObject({ firstName: NEW_FIRST_NAME })
})

test('an unreadable profile shows the failure state, not an editable empty form', async ({ page, request }) => {
  await scriptStub(request, { method: HttpMethod.Get, path: '/api/clients/me', status: 503, code: 'SERVICE_UNAVAILABLE', detail: 'E2E: the service is unavailable.' })

  await page.goto('/client/account')
  await waitForHydration(page)

  await expect(page.getByTestId('account-profile-failed')).toBeVisible()
  await expect(page.getByTestId('account-profile-edit')).toHaveCount(0)
})
