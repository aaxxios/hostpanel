/**
 * Form-filling helpers shared by the e2e specs that drive the same screen more than once.
 *
 * Selectors are `data-testid` only; the kit components carry the test id on their root, so a
 * control is found as the element inside it.
 *
 * @module e2e/support/forms
 */
import type { Page } from '@playwright/test'
import { CONTACT } from './fixtures.mjs'

/**
 * Fills the public contact form completely and submits it.
 *
 * @param page The page that is already on a loaded, hydrated contact screen.
 * @param email The address to type; defaults to the fixture address.
 * @returns Resolves once the submit button has been clicked.
 */
export const submitContactForm = async (page: Page, email: string = CONTACT.email): Promise<void> => {
  await page.getByTestId('contact-name').locator('input').fill(CONTACT.name)
  await page.getByTestId('contact-email').locator('input').fill(email)

  await page.getByTestId('contact-service').locator('button').first().click()
  await page.getByTestId('ui-dropdown-option-hosting').click()

  await page.getByTestId('contact-message').locator('textarea').fill(CONTACT.message)
  await page.getByTestId('contact-consent').locator('button').click()

  await page.getByTestId('contact-submit').click()
}

/**
 * Types a one-time code into the cell row of the two-factor dialog, one digit per cell.
 *
 * Each cell is filled on its own rather than typed through the keyboard, so the result does not
 * depend on where the focus happens to be when the dialog opens.
 *
 * @param page The page showing the code cells.
 * @param code The digits to enter, one per cell.
 * @returns Resolves once every cell is filled.
 */
export const enterOtp = async (page: Page, code: string): Promise<void> => {
  for (const [index, digit] of [...code].entries()) {
    await page.getByTestId(`otp-input-${index}`).fill(digit)
  }
}
