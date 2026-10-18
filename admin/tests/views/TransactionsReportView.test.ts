/**
 * Tests of TransactionsReportView: it mounts, shows its title and data, and shows the refusal sentence when the API
 * refuses.
 */
import { vi } from 'vitest'
import TransactionsReportView from '@/views/TransactionsReportView.vue'
import { json } from '../helpers/fetch'
import { runReportCase } from '../helpers/reportViews'

vi.mock('chart.js', async () => (await import('../helpers/chartJsMock')).chartJsMock)

runReportCase({
  name: 'TransactionsReportView',
  view: TransactionsReportView,
  routes: { 'GET /reports/transactions': () => json({ items: [], totalCount: 0 }) },
  expected: ['Transactions', '0 transaction(s) found', 'No transactions found.'],
})
