/**
 * Mount tests for QuotesListView: the table renders what the API sent, the subject filter
 * narrows the page, and a refused load shows the API sentence.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import QuotesListView from '@/views/QuotesListView.vue'
import { quoteRowFixture } from '../helpers/billingFixtures'
import { json, problem } from '../helpers/fetch'
import { mountRouted } from '../helpers/mount'
import { stubRoutes } from '../helpers/routes'

/** The quotes endpoint answer: two quotes on one page. */
const quotesPage = () =>
  json({
    items: [quoteRowFixture(), quoteRowFixture({ id: 4, subject: 'Other work', stage: 'Draft' })],
    totalCount: 2,
    page: 1,
    pageSize: 25,
  })

/**
 * Mounts the list and waits for its first load.
 *
 * @returns The wrapper.
 */
const mountList = async () => {
  const { wrapper } = await mountRouted(QuotesListView, '/billing/quotes', '/billing/quotes')
  await flushPromises()
  return wrapper
}

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('QuotesListView', () => {
  it('renders a row per quote with its stage and a link to the quote', async () => {
    stubRoutes({ 'GET /api/billing/quotes': quotesPage, 'GET /api/clients': () => json({ items: [], totalCount: 0 }) })
    const wrapper = await mountList()

    expect(wrapper.findAll('[data-testid="billing-quotes-table"] tbody tr')).toHaveLength(2)
    expect(wrapper.get('[data-testid="billing-quotes-table"]').text()).toContain('Hosting renewal')
    expect(wrapper.get('[data-testid="billing-quote-open-3"]').attributes('href')).toBe('/billing/quotes/3')
    expect(wrapper.get('[data-testid="billing-quotes-create"]').attributes('href')).toBe('/billing/quotes/add')
  })

  it('narrows the page by subject while typing and restores it on clear', async () => {
    stubRoutes({ 'GET /api/billing/quotes': quotesPage, 'GET /api/clients': () => json({ items: [], totalCount: 0 }) })
    const wrapper = await mountList()

    await wrapper.get('[data-testid="billing-quotes-filter-toggle"]').trigger('click')
    await wrapper.get('[data-testid="billing-quotes-filter-subject"]').setValue('renewal')
    expect(wrapper.findAll('[data-testid="billing-quotes-table"] tbody tr')).toHaveLength(1)

    await wrapper.get('[data-testid="billing-quotes-clear"]').trigger('click')
    expect(wrapper.findAll('[data-testid="billing-quotes-table"] tbody tr')).toHaveLength(2)
  })

  it('shows the API sentence when the load is refused', async () => {
    stubRoutes({
      'GET /api/billing/quotes': () => problem('Quotes are unavailable.', 403),
      'GET /api/clients': () => json({ items: [], totalCount: 0 }),
    })
    const wrapper = await mountList()

    expect(wrapper.get('[data-testid="billing-quotes-error"]').text()).toContain('Quotes are unavailable.')
  })

  it('prints a quote stage as a name, not the value the API sends', async () => {
    stubRoutes({
      'GET /api/billing/quotes': () =>
        json({ items: [quoteRowFixture({ stage: 'OnHold' })], totalCount: 1, page: 1, pageSize: 25 }),
      'GET /api/clients': () => json({ items: [], totalCount: 0 }),
    })
    const wrapper = await mountList()

    const table = wrapper.get('[data-testid="billing-quotes-table"]').text()
    expect(table).toContain('On Hold')
    expect(table).not.toContain('OnHold')
  })
})
