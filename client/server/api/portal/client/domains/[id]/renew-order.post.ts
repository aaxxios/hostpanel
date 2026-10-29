/**
 * POST /api/portal/client/domains/:id/renew-order
 *
 * Places an order to renew one of the signed-in client's own domains.
 *
 * The backend route is `POST /me/domains/{id}/renew`, whose
 * `RenewMyDomainCommand` carries no client id and resolves the owning account from the
 * credential, not the admin `POST /api/domains/{id}/renew`, which is
 * `[Authorize(Roles = Admin,Reseller)]`, calls the registrar immediately and raises no invoice.
 *
 * A renewal is a purchase, exactly as a transfer-in is, so the API answers with the order and
 * the invoice raised for it, and the registrar is not called until that invoice is paid. The
 * page renders its success panel out of those two ids.
 *
 * The field names are translated here rather than in the page, the same way
 * `transfer-order.post.ts` does it: the form carries `paymentmethod`, and the request spells it
 * `paymentMethod`. Nothing is validated here:
 * `RenewMyDomainValidator` refuses a period outside 1 to 10 and an empty payment method, and its
 * sentences are the ones the page shows.
 */

import { HttpMethod } from '~/types/httpmethod'

/** The order the API raised, and the invoice the client has to pay for it. */
interface RenewOrderResult {
  /** Primary key of the new order. */
  orderId: number
  /** Primary key of the invoice raised for that order. */
  invoiceId: number
}

export default defineEventHandler(async (event): Promise<RenewOrderResult> => {
  const id = requireRouteParam(event, 'id')

  const body = await readBody<Record<string, unknown>>(event)

  return await internalApiCall<RenewOrderResult>(event, `/me/domains/${id}/renew`, {
    method: HttpMethod.Post,
    body: {
      years: body.years,
      paymentMethod: body.paymentmethod
    }
  })
})
