/**
 * Tests of ClientStatementView: it mounts, shows its title and data, and shows the refusal sentence when the API
 * refuses.
 */
import { vi } from 'vitest'
import ClientStatementView from '@/views/ClientStatementView.vue'
import { runReportCase } from '../helpers/reportViews'

vi.mock('chart.js', async () => (await import('../helpers/chartJsMock')).chartJsMock)

runReportCase({
  name: 'ClientStatementView',
  view: ClientStatementView,
  routes: {},
  expected: ['Client Account Register Balance', 'Select Client'],
})
