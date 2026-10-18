/**
 * GET /api/portal/client/emails
 * Returns email history for the authenticated client from the C# backend.
 *
 * The API answers with a page object, the same shape the admin route uses. This unwraps it to
 * the list the account screen renders, and asks for a page big enough to cover what that screen
 * paginates client-side. The mapping lives here rather than in the API because it exists for
 * one screen — the API keeps the shape every other caller already reads.
 *
 * @module server/api/portal/client/emails.get
 */

/** One email as the C# backend describes it. */
interface EmailLogEntry {
  /** Email primary key. */
  id: number
  /** Recipient address. */
  to: string
  /** Subject line. */
  subject: string
  /** When the email was sent (ISO 8601). */
  sentAt: string
  /** Whether delivery succeeded. */
  success: boolean
  /** Delivery error, when it failed. */
  error: string | null
}

/** A page of them. */
interface EmailLogPage {
  /** The emails on this page. */
  items: EmailLogEntry[]
  /** How many emails exist in total. */
  totalCount: number
  /** One-based page number. */
  page: number
  /** Page size the backend applied. */
  pageSize: number
}

export default defineEventHandler(async (event) => {
  // 100 is the API's per-page ceiling; asking for more silently returns 100, so the number is
  // spelled out here rather than left to look like an unlimited request.
  const result = await internalApiCall<EmailLogPage>(event, '/me/emails?page=1&pageSize=100')

  return (result?.items ?? []).map((entry) => ({
    id: String(entry.id),
    date: entry.sentAt,
    subject: entry.subject,
  }))
})
