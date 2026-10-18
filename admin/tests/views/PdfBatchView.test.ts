/**
 * Tests of PdfBatchView: it mounts, shows its title and data, and shows the refusal sentence when the API
 * refuses.
 */
import { vi } from 'vitest'
import PdfBatchView from '@/views/PdfBatchView.vue'
import { runReportCase } from '../helpers/reportViews'

vi.mock('chart.js', async () => (await import('../helpers/chartJsMock')).chartJsMock)

runReportCase({ name: 'PdfBatchView', view: PdfBatchView, routes: {}, expected: ['Pdf Batch', 'Coming Soon'] })
