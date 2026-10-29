/**
 * `POST /api/contact`: the public contact form.
 *
 * Nothing is delivered here. This route hands the submission to the C# backend's
 * `POST /api/contact`, which sends the mail through the one `IEmailSender` the backend
 * configures and posts the enquiry to the operator's chat through `IContactNotifier`. Neither
 * the mail relay's password nor the chat bot token has any reason to be in a browser-facing
 * app, so this route holds no credential at all.
 *
 * A failure from the backend is rethrown with its status and its code intact, so the browser
 * never sees a success this route did not earn: answering success regardless would tell every
 * visitor of a misconfigured deployment that their message arrived while nothing was sent.
 *
 * The chat relay is best effort, and the backend handler enforces that where it can see
 * whether the mail went: the mail decides the visitor's answer, and a chat outage is logged
 * instead of turning a delivered enquiry into a failure.
 *
 * @module server/api/contact.post
 */

import type { H3Event } from 'h3'
import type { ContactMessage } from '~/types/contactmessage'
import { HttpMethod } from '~/types/httpmethod'

/**
 * Relays one contact-form submission to the backend.
 *
 * @param event - The incoming request.
 * @returns 204 No Content, and only once the backend has accepted the message.
 * @throws The backend's own error, status and `code` intact, when it refuses or cannot deliver.
 */
export default defineEventHandler(async (event: H3Event) => {
  const body = await readBody<ContactMessage>(event)

  // No field checks here. The backend validates every one of them and is the only side that
  // writes the wording; a copy in this file would be a second answer to the same question, in a
  // language this route cannot translate. See `api-driven-frontend.md`.
  // `unknown`, not `void`: the backend answers 204 with an empty body, and nothing here reads
  // the result: what matters is that this line throws when the backend refuses.
  await internalApiCall<unknown>(event, '/contact', {
    method: HttpMethod.Post,
    body: {
      name: body?.name,
      email: body?.email,
      phone: body?.phone,
      service: body?.service,
      message: body?.message,
      submittedAt: body?.timestamp,
    },
  })

  // 204, matching the backend. The page treats a resolved promise as sent, and a success flag
  // in the body is exactly the shape that lets a failed send be reported as a delivery.
  setResponseStatus(event, 204)
  return null
})
