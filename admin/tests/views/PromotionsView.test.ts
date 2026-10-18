/**
 * Tests of PromotionsView: it mounts, shows its title and data, and shows the refusal sentence when the API
 * refuses.
 */
import { vi } from 'vitest'
import PromotionsView from '@/views/PromotionsView.vue'
import { runReportCase } from '../helpers/reportViews'

vi.mock('chart.js', async () => (await import('../helpers/chartJsMock')).chartJsMock)

runReportCase({ name: 'PromotionsView', view: PromotionsView, routes: {}, expected: ['Promotions', 'Coming Soon'] })
