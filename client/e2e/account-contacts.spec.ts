/**
 * Flow: the account's contacts.
 *
 * The tab lists the contacts; one is added, edited and removed (the removal only after a
 * confirmation), each change reaches the backend and the list is read again. A contact without a
 * name is refused on the page before anything is sent.
 */
import { expect, test } from './support/test'
import { EDITED_CONTACT_FIRST_NAME, NEW_CONTACT, SEEDED_CONTACT } from './support/fixtures.mjs'
import { resetStub, signIn, stubRequests, waitForHydration } from './support/helpers'

test.beforeEach(async ({ page, context, request, baseURL }) => {
  await resetStub(request)
  await signIn(context, baseURL ?? 'http://127.0.0.1:3100')

  await page.goto('/client/account')
  await waitForHydration(page)
  await page.getByTestId('ui-tab-contacts').click()
  await expect(page.getByTestId('account-contacts-tab')).toBeVisible()
})

test('the contact is listed with its name and address', async ({ page }) => {
  await expect(page.getByTestId(`account-contact-${SEEDED_CONTACT.id}`)).toContainText(`${SEEDED_CONTACT.firstName} ${SEEDED_CONTACT.lastName}`)
  await expect(page.getByTestId(`account-contact-${SEEDED_CONTACT.id}`)).toContainText(SEEDED_CONTACT.email)
})

test('a new contact is sent to the backend and joins the list', async ({ page, request }) => {
  await page.getByTestId('account-contact-add').click()
  await page.getByTestId('account-contact-firstname').fill(NEW_CONTACT.firstName)
  await page.getByTestId('account-contact-lastname').fill(NEW_CONTACT.lastName)
  await page.getByTestId('account-contact-email').fill(NEW_CONTACT.email)
  await page.getByTestId('account-contact-save').click()

  await expect(page.getByTestId('account-contact-form')).toHaveCount(0)
  await expect(page.getByTestId('account-contacts-tab')).toContainText(`${NEW_CONTACT.firstName} ${NEW_CONTACT.lastName}`)

  const created = (await stubRequests(request)).find(r => r.method === 'POST' && r.path === '/api/clients/me/contacts')
  expect(created?.body).toMatchObject({
    firstName: NEW_CONTACT.firstName, lastName: NEW_CONTACT.lastName, email: NEW_CONTACT.email, type: 'General', phone: null
  })
})

test('a contact without a name is refused on the page and nothing is sent', async ({ page, request }) => {
  await page.getByTestId('account-contact-add').click()
  await page.getByTestId('account-contact-email').fill(NEW_CONTACT.email)
  await page.getByTestId('account-contact-save').click()

  await expect(page.getByTestId('account-contact-form').getByTestId('ui-form-error')).toBeVisible()
  expect((await stubRequests(request)).some(r => r.method === 'POST' && r.path === '/api/clients/me/contacts')).toBe(false)
})

test('editing a contact saves the new name', async ({ page, request }) => {
  await page.getByTestId(`account-contact-edit-${SEEDED_CONTACT.id}`).click()
  await expect(page.getByTestId('account-contact-firstname')).toHaveValue(SEEDED_CONTACT.firstName)

  await page.getByTestId('account-contact-firstname').fill(EDITED_CONTACT_FIRST_NAME)
  await page.getByTestId('account-contact-save').click()

  await expect(page.getByTestId(`account-contact-${SEEDED_CONTACT.id}`)).toContainText(EDITED_CONTACT_FIRST_NAME)
  const saved = (await stubRequests(request)).find(r => r.method === 'PUT' && r.path === `/api/clients/me/contacts/${SEEDED_CONTACT.id}`)
  expect(saved?.body).toMatchObject({ firstName: EDITED_CONTACT_FIRST_NAME, lastName: SEEDED_CONTACT.lastName, type: SEEDED_CONTACT.type })
})

test('removing a contact asks first, then removes it and the empty state appears', async ({ page, request }) => {
  await page.getByTestId(`account-contact-delete-${SEEDED_CONTACT.id}`).click()

  // Nothing is deleted until the dialog is confirmed.
  await expect(page.getByTestId('ui-confirm-dialog')).toBeVisible()
  expect((await stubRequests(request)).some(r => r.method === 'DELETE')).toBe(false)

  await page.getByTestId('ui-confirm-confirm').click()

  await expect(page.getByTestId(`account-contact-${SEEDED_CONTACT.id}`)).toHaveCount(0)
  await expect(page.getByTestId('contacts-add')).toBeVisible()
  expect((await stubRequests(request)).some(r => r.method === 'DELETE' && r.path === `/api/clients/me/contacts/${SEEDED_CONTACT.id}`)).toBe(true)
})
