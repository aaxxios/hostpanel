/**
 * POST /api/portal/client/services/:id/ssh/keys
 * Adds an ED25519 public key. Only `label` and `publicKey` are forwarded; every other field the
 * browser sends (a server, an account, a path) is dropped here, and the backend accepts none.
 *
 * Body: { label: string, publicKey: string }
 *
 * @module server/api/portal/client/services/[id]/ssh/keys.post
 */

import { HttpMethod } from '~/types/httpmethod'

/** Body this route accepts. */
interface AddSshKeyBody {
  /** Friendly label. */
  label?: unknown
  /** The public key line. */
  publicKey?: unknown
}

export default defineEventHandler(async (event) => {
  const id = requireRouteParam(event, 'id')
  const body = await readBody<AddSshKeyBody>(event)

  // No checks here: the backend validates both fields and answers in the reader's language.
  setResponseStatus(event, 201)
  return await internalApiCall<Record<string, unknown>>(event, `/me/services/${id}/ssh/keys`, {
    method: HttpMethod.Post,
    body: {
      label: typeof body?.label === 'string' ? body.label : '',
      publicKey: typeof body?.publicKey === 'string' ? body.publicKey : '',
    },
  })
})
