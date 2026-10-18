/**
 * Tests of SmartyCompatibilityView: it mounts, shows its title and data, and shows the refusal sentence when the API
 * refuses.
 */
import { vi } from 'vitest'
import SmartyCompatibilityView from '@/views/SmartyCompatibilityView.vue'
import { runReportCase } from '../helpers/reportViews'

vi.mock('chart.js', async () => (await import('../helpers/chartJsMock')).chartJsMock)

runReportCase({ name: 'SmartyCompatibilityView', view: SmartyCompatibilityView, routes: {}, expected: ['Smarty Compatibility', 'Coming Soon'] })
