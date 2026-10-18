/**
 * Tests of DomainRenewalEmailsView: it mounts, shows its title and data, and shows the refusal sentence when the API
 * refuses.
 */
import { vi } from 'vitest'
import DomainRenewalEmailsView from '@/views/DomainRenewalEmailsView.vue'
import { json } from '../helpers/fetch'
import { runReportCase } from '../helpers/reportViews'

vi.mock('chart.js', async () => (await import('../helpers/chartJsMock')).chartJsMock)

runReportCase({
  name: 'DomainRenewalEmailsView',
  view: DomainRenewalEmailsView,
  routes: {
    'GET /reports/domain-renewal-emails': () => json({
      rows: [{ clientName: 'Beta', domain: 'beta.test', sentAt: '2024-03-05T12:00:00Z', reminder: '30 days', recipients: 'a@b.test' }],
    }),
  },
  expected: ['Domain Renewal Reminder Emails', 'beta.test', '05/03/2024', 'a@b.test'],
})
