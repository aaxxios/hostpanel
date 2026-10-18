/**
 * Stand-in for the chart.js module, for tests that mount a screen with a chart: the library needs
 * a real canvas, which the test environment does not have. Use it from a test file with
 * `vi.mock('chart.js', async () => (await import('../helpers/chartJsMock')).chartJsMock)`.
 */
import { vi } from 'vitest'

/** Stand-in for a chart.js element, controller or scale class. */
class Registerable {}

/** Stand-in for the Chart class; it draws nothing and can be destroyed. */
class Chart {
  /** Registration is a no-op. */
  static register = vi.fn()

  /** Destroying a stand-in chart is a no-op. */
  destroy = vi.fn()
}

/** The exports of chart.js the app imports. */
export const chartJsMock = {
  Chart,
  ArcElement: Registerable,
  BarController: Registerable,
  BarElement: Registerable,
  CategoryScale: Registerable,
  DoughnutController: Registerable,
  Filler: Registerable,
  Legend: Registerable,
  LineController: Registerable,
  LineElement: Registerable,
  LinearScale: Registerable,
  PieController: Registerable,
  PointElement: Registerable,
  Tooltip: Registerable,
}
