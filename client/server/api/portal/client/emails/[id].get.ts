/**
 * GET /api/portal/client/emails/:id
 * Returns the full message of a single email from the C# backend, in the shape the email page
 * reads (`ClientEmail`): the backend names the send time `sentAt` and the text `body`, and the
 * page needs them as `date` and `message`, the same renaming the list route does for `date`.
 *
 * @module server/api/portal/client/emails/[id].get
 */

/** One email as the C# backend describes it. */
interface EmailLogEntry {
  /** Email primary key. */
  id: number
  /** Subject line. */
  subject: string
  /** The message as the system sent it. */
  body: string
  /** When the email was sent (ISO 8601). */
  sentAt: string
}

export default defineEventHandler(async (event) => {
  const id = requireRouteParam(event, 'id')

  const entry = await internalApiCall<EmailLogEntry>(event, `/me/emails/${id}`)

  return {
    id: String(entry.id),
    date: entry.sentAt,
    subject: entry.subject,
    message: entry.body,
  }
})
