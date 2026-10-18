/**
 * One invoice on the client's account.
 *
 * ## One vocabulary, and it is the backend's
 *
 * This file used to declare legacy names — `date`, `duedate`, `datepaid`, `subtotal`,
 * `currencycode`, `currencyprefix`, `currencysuffix` — none of which the API has ever sent.
 * `server/api/portal/client/invoices/index.get.ts` and `[id].get.ts` are bare passthroughs of
 * `GET /api/me/invoices` and `/api/me/invoices/{id}`, both of which answer with
 * `Innovayse.Application.Billing.Common.InvoiceDto` serialised camelCase. So every field named
 * here was `undefined` in production, and invoice dates rendered blank on
 * `pages/client/invoices/index.vue` and on the dashboard.
 *
 * The names below are `InvoiceDto`'s own. The backend's are the correct ones and the DTO was
 * deliberately not renamed to match the portal: this type is the seam, and it is the half that
 * was wrong.
 *
 * ## Both endpoints return this same shape
 *
 * `MyBillingController.GetMineAsync` returns `IReadOnlyList<InvoiceDto>`, not
 * `InvoiceListItemDto` — the list carries `items` and `transactions` too. One interface
 * therefore covers the list page, the detail page and the payment page, and there is no
 * lighter list variant to keep in step with it.
 *
 * ## The invoice carries its own currency
 *
 * `InvoiceDto.Currency` (added alongside the multi-currency pricing work — see
 * `docs/superpowers/specs/2026-09-13-multi-currency-pricing-design.md` §3) is an ISO 4217
 * code set once, at creation, from the payer's currency at the time, and never changed. An
 * invoice's own currency is the authority for formatting it — never the account's *current*
 * currency (`ClientUser.currency`), which can differ if the account's currency were ever
 * changed, and never the page's language. Amounts here are plain `number`s in {@link
 * ClientInvoice.currency}; format them via `stores/currency.ts` and `utils/formatMoney.ts`
 * when the currency is a configured one, falling back to `utils/formatCurrency.ts`'s bare ISO
 * code path otherwise.
 *
 * @module types/clientinvoice
 */

import type { ClientInvoiceStatus } from '~/types/clientinvoicestatus'
import type { ClientInvoiceItem } from '~/types/clientinvoiceitem'
import type { ClientInvoiceTransaction } from '~/types/clientinvoicetransaction'

/**
 * One invoice, as `GET /api/portal/client/invoices` and `.../invoices/:id` return it.
 *
 * Field names and types are `InvoiceDto`'s. Every date is an ISO 8601 string carrying an
 * offset (a serialised `DateTimeOffset`); every amount is a `number`, not the decimal string a
 * legacy-shaped endpoint would have sent.
 */
export interface ClientInvoice {
  /** Invoice primary key. */
  id: number
  /** FK to the owning client. */
  clientId: number
  /** Display name of the owning client. */
  clientName: string
  /** Current lifecycle status. */
  status: ClientInvoiceStatus
  /** ISO 4217 code this invoice was raised and is payable in — fixed at creation, never changed. */
  currency: string
  /** Issue date (UTC, ISO 8601). */
  invoiceDate: string
  /** Payment due date (UTC, ISO 8601). */
  dueDate: string
  /** Creation timestamp (UTC, ISO 8601). */
  createdAt: string
  /** Payment timestamp (UTC, ISO 8601); null while unpaid. */
  paidAt: string | null
  /** Final total after tax and credit. */
  total: number
  /** Sum of all line item amounts before tax and credit. */
  subTotal: number
  /** Computed tax amount. */
  tax: number
  /** Tax rate percentage. */
  taxRate: number
  /** Total credit applied. */
  credit: number
  /** Payment gateway reference; null when unpaid. */
  gatewayTransactionId: string | null
  /** Optional invoice notes. */
  notes: string | null
  /** Preferred payment method; null when not specified. */
  paymentMethod: string | null
  /** Line items on the invoice. */
  items: ClientInvoiceItem[]
  /** Financial transactions recorded against the invoice. */
  transactions: ClientInvoiceTransaction[]
}
