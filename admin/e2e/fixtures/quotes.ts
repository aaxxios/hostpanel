/**
 * Fixtures for the quotes list: a draft and an accepted quote on one page.
 */
import type { PagedResult } from '../../src/types/pagedresult'
import { QuoteStage } from '../../src/types/quote'
import type { QuoteListItem } from '../../src/types/quote'

/** A quote still being written. */
export const draftQuote: QuoteListItem = {
  id: 801,
  clientId: 7,
  clientName: 'Ada Lovelace',
  subject: 'Hosting renewal',
  dateCreated: '2026-10-01T00:00:00Z',
  validUntil: '2026-11-01T00:00:00Z',
  total: 240,
  stage: QuoteStage.Draft,
}

/** A quote the client accepted. */
export const acceptedQuote: QuoteListItem = {
  ...draftQuote,
  id: 802,
  clientId: 8,
  clientName: 'Grace Hopper',
  subject: 'Migration project',
  total: 1800,
  stage: QuoteStage.Accepted,
}

/** `GET /api/billing/quotes` — one page holding both quotes. */
export const quotesPage: PagedResult<QuoteListItem> = {
  items: [draftQuote, acceptedQuote],
  totalCount: 2,
  page: 1,
  pageSize: 20,
}
