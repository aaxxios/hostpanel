/**
 * Tests of ServerRevenueForecastsView: it mounts, shows its title and data, and shows the refusal sentence when the API
 * refuses.
 */
import { vi } from 'vitest'
import ServerRevenueForecastsView from '@/views/ServerRevenueForecastsView.vue'
import { runReportCase } from '../helpers/reportViews'

vi.mock('chart.js', async () => (await import('../helpers/chartJsMock')).chartJsMock)

runReportCase({ name: 'ServerRevenueForecastsView', view: ServerRevenueForecastsView, routes: {}, expected: ['Server Revenue Forecasts', 'Coming Soon'] })
