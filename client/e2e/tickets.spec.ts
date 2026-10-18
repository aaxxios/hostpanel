/**
 * Flow: support tickets.
 *
 * A signed-in customer reads the ticket list, opens a ticket and sees the whole conversation,
 * replies to it, and opens a new one. The stub keeps tickets in memory, so a created ticket and a
 * reply appear on the next read, and every write is checked at the backend as well as on screen.
 */
import { expect, test } from './support/test'
import { NEW_TICKET, SEEDED_TICKET, TICKET_REPLY } from './support/fixtures.mjs'
import { resetStub, signIn, stubRequests, waitForHydration } from './support/helpers'

test.beforeEach(async ({ context, request, baseURL }) => {
  await resetStub(request)
  await signIn(context, baseURL ?? 'http://127.0.0.1:3100')
})

test('the list opens a ticket with its whole conversation', async ({ page }) => {
  await page.goto('/client/tickets')
  await waitForHydration(page)
  await expect(page.getByTestId('tickets-list')).toBeVisible()

  await page.getByTestId(`ticket-row-${SEEDED_TICKET.id}`).click()
  await expect(page).toHaveURL(new RegExp(`/client/tickets/${SEEDED_TICKET.id}$`))
  await expect(page.getByTestId('ticket-header')).toContainText(SEEDED_TICKET.subject)

  // The opening message and the staff answer.
  await expect(page.getByTestId('ticket-message')).toHaveCount(2)
})

test('a reply is sent to the backend and joins the conversation', async ({ page, request }) => {
  await page.goto(`/client/tickets/${SEEDED_TICKET.id}`)
  await waitForHydration(page)

  await page.getByTestId('ticket-reply-message').fill(TICKET_REPLY)
  await page.getByTestId('ticket-reply-submit').click()

  await expect(page.getByTestId('ui-form-success')).toBeVisible()
  await expect(page.getByTestId('ticket-message')).toHaveCount(3)

  const sent = (await stubRequests(request)).find(r => r.method === 'POST' && r.path === `/api/me/tickets/${SEEDED_TICKET.id}/reply`)
  // The author is the signed-in caller, resolved by the backend: the body carries the message only.
  expect(sent?.body).toEqual({ message: TICKET_REPLY })
})

test('a new ticket is created and shows in the list', async ({ page, request }) => {
  await page.goto('/client/tickets/new')
  await waitForHydration(page)

  await page.getByTestId('ticket-department').click()
  await page.getByTestId('ui-select-option-1').click()
  await page.getByTestId('ticket-subject').fill(NEW_TICKET.subject)
  await page.getByTestId('ticket-priority-High').click()
  await page.getByTestId('ticket-message').fill(NEW_TICKET.message)
  await page.getByTestId('ticket-submit').click()

  await expect(page.getByTestId('ticket-success')).toBeVisible()

  const created = (await stubRequests(request)).find(r => r.method === 'POST' && r.path === '/api/me/tickets')
  expect(created?.body).toMatchObject({ departmentId: 1, subject: NEW_TICKET.subject, message: NEW_TICKET.message, priority: 'High' })

  await page.getByTestId('ticket-view-all').click()
  await expect(page.getByTestId('tickets-list')).toContainText(NEW_TICKET.subject)
})
