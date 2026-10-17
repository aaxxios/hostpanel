/**
 * Flow: the contact form shows the field the backend refused.
 *
 * A 400 `VALIDATION_ERROR` carries the sentence per field; the form puts that sentence on the
 * field it names (the page writes none of its own) and stays open so the visitor can correct it.
 */
import { expect, test } from './support/test'
import { BAD_CONTACT_EMAIL, BAD_CONTACT_EMAIL_MESSAGE } from './support/fixtures.mjs'
import { submitContactForm } from './support/forms'
import { resetStub, waitForHydration } from './support/helpers'

test('a refused address is marked invalid with the backend sentence and the form stays', async ({ page, request }) => {
  await resetStub(request)
  await page.goto('/contact')
  await waitForHydration(page)

  await submitContactForm(page, BAD_CONTACT_EMAIL)

  const email = page.getByTestId('contact-email')
  await expect(email.locator('input')).toHaveAttribute('aria-invalid', 'true')
  await expect(email).toContainText(BAD_CONTACT_EMAIL_MESSAGE)
  await expect(page.getByTestId('contact-success')).toHaveCount(0)
  await expect(page.getByTestId('contact-form')).toBeVisible()
})
