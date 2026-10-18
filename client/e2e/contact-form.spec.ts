/**
 * Flow: contact form submit.
 *
 * Fills the public contact form, submits it and checks both ends: the visitor sees the success
 * message, and the backend actually received the enquiry with what was typed (a success banner
 * alone proved nothing the day this route answered "sent" while sending nothing).
 */
import { expect, test } from './support/test'
import { CONTACT } from './support/fixtures.mjs'
import { resetStub, stubRequests, waitForHydration } from './support/helpers'

test('submitting the contact form reaches the backend and confirms to the visitor', async ({ page, request }) => {
  await resetStub(request)
  await page.goto('/contact')
  await waitForHydration(page)

  // The kit components carry the test id on their root; the control is the element inside it.
  await page.getByTestId('contact-name').locator('input').fill(CONTACT.name)
  await page.getByTestId('contact-email').locator('input').fill(CONTACT.email)

  await page.getByTestId('contact-service').locator('button').first().click()
  await page.getByTestId('ui-dropdown-option-hosting').click()

  await page.getByTestId('contact-message').locator('textarea').fill(CONTACT.message)
  await page.getByTestId('contact-consent').locator('button').click()

  await page.getByTestId('contact-submit').click()

  await expect(page.getByTestId('contact-success')).toBeVisible()
  await expect(page.getByTestId('contact-error')).toHaveCount(0)

  // The enquiry reached the backend, with the typed values.
  await expect.poll(async () => {
    const requests = await stubRequests(request)
    return requests.find(r => r.method === 'POST' && r.path === '/api/contact')?.body
  }).toMatchObject({ name: CONTACT.name, email: CONTACT.email, message: CONTACT.message })
})
