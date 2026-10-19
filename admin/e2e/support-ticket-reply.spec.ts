/**
 * Support flow: a staff member answers a ticket; the reply is posted as JSON, the form clears
 * and the thread shows the reply once the ticket has been re-read.
 */
import { HttpMethod } from '../src/types/http'
import { expect, test } from '@playwright/test'
import { departments, openTicket } from './fixtures/support'
import type { Ticket } from '../src/types/ticket'
import { fieldOf } from './support/locators'
import { json, stubApi } from './support/stubApi'

test('replying to a ticket posts the message and shows it in the thread', async ({ page }) => {
  // The ticket as the "database" holds it; the reply POST appends to it so the re-read after
  // sending returns the thread with the new reply.
  const stored: Ticket = { ...openTicket, replies: [] }
  // The author is the signed-in staff member; the stubbed backend resolves it from the token.
  const author = 'Support Agent'
  const message = 'We restarted the web service; the site should be back.'

  const api = await stubApi(page, {
    signedIn: true,
    routes: [
      json(HttpMethod.Get, '/api/tickets/departments', departments),
      { method: HttpMethod.Get, path: `/api/tickets/${openTicket.id}`, respond: () => ({ body: stored }) },
      {
        method: HttpMethod.Post,
        path: `/api/tickets/${openTicket.id}/reply`,
        respond: (call) => {
          const sent = call.body as { message: string }
          stored.replies.push({
            id: stored.replies.length + 1,
            message: sent.message,
            authorName: author,
            isStaffReply: true,
            createdAt: '2026-10-14T09:30:00Z',
          })
          return { status: 204 }
        },
      },
    ],
  })

  await page.goto(`/support/tickets/${openTicket.id}`)
  await expect(page.getByTestId('ticket-title')).toContainText(openTicket.subject)
  await expect(page.getByTestId('ticket-reply')).toHaveCount(0)

  await expect(page.getByTestId('ticket-reply-author')).toHaveCount(0)
  await fieldOf(page, 'ticket-reply-message').fill(message)
  await page.getByTestId('ticket-reply-submit').click()

  const reply = page.getByTestId('ticket-reply')
  await expect(reply).toHaveCount(1)
  await expect(reply).toContainText(message)
  await expect(reply).toContainText(author)
  await expect(fieldOf(page, 'ticket-reply-message')).toHaveValue('')

  const [sent] = api.callsTo(HttpMethod.Post, `/api/tickets/${openTicket.id}/reply`)
  expect(sent?.body).toEqual({ message })
})
