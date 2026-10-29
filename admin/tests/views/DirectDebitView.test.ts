/**
 * Tests of DirectDebitView: it mounts, shows its title and data, and shows the refusal sentence with Retry when the API
 * refuses.
 */
import { vi } from 'vitest'
import DirectDebitView from '@/views/DirectDebitView.vue'
import { json } from '../helpers/fetch'
import { runReportCase } from '../helpers/reportViews'

vi.mock('chart.js', async () => (await import('../helpers/chartJsMock')).chartJsMock)

runReportCase({
  name: 'DirectDebitView',
  view: DirectDebitView,
  routes: {
    'GET /reports/direct-debit': () => json({
      rows: [{
        invoiceId: 7, clientName: 'Beta', invoiceDate: '2024-01-01', dueDate: '2024-01-10', subtotal: 10, tax: 1,
        credit: 0, total: 11, bankName: 'First Bank', bankAccountType: null, bankCode: null, bankAccountNumber: null,
      }],
    }),
  },
  expected: ['Direct Debit Processing', 'Beta', 'First Bank', '$11.00'],
  retryable: true,
})
