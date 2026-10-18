/**
 * Unit tests for UiChart: it builds the chart.js configuration from labels and series, shows the
 * empty state, formats values and tears the chart down.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises, mount } from '@vue/test-utils'
import UiChart from '@/components/ui/UiChart.vue'
import { createTestI18n } from '../../helpers/i18n'

/** The part of the chart.js configuration these tests read. */
interface CapturedConfig {
  type: string
  data: {
    labels: string[]
    datasets: { label: string; data: number[]; borderColor?: string; backgroundColor?: unknown }[]
  }
  options: {
    indexAxis: string
    plugins: {
      legend: { display: boolean }
      tooltip: { callbacks: { label: (context: { raw: number; label: string; dataset: { label: string } }) => string } }
    }
    scales: {
      x?: { stacked: boolean }
      y?: { ticks: { callback: (value: number) => string } }
    }
  }
}

/** Charts built by the mocked constructor, newest last. */
const created = vi.hoisted(() => [] as { config: CapturedConfig; destroy: ReturnType<typeof vi.fn> }[])

// chart.js needs a real canvas, which the test environment does not have; the library is the one
// thing replaced, so the test still exercises UiChart's own configuration logic.
vi.mock('chart.js', () => {
  /** Stand-in for a chart.js element or controller class. */
  class Registerable {}
  /** Stand-in for the Chart class that records its configuration. */
  class Chart {
    static register = vi.fn()
    config: CapturedConfig
    destroy = vi.fn()
    constructor(_canvas: unknown, config: CapturedConfig) {
      this.config = config
      created.push(this)
    }
  }
  return {
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
})

/**
 * Mounts the chart with the English catalogue.
 *
 * @param props - Props for the chart.
 * @returns The wrapper.
 */
const mountChart = (props: InstanceType<typeof UiChart>['$props']) =>
  mount(UiChart, { props, global: { plugins: [createTestI18n()] } })

beforeEach(() => {
  created.length = 0
})

describe('UiChart', () => {
  it('builds a bar chart from labels and series', async () => {
    mountChart({ type: 'bar', labels: ['Jan', 'Feb'], series: [{ label: 'Income', data: [1, 2] }] })
    await flushPromises()

    const config = created[0]?.config
    expect(config?.type).toBe('bar')
    expect(config?.data.labels).toEqual(['Jan', 'Feb'])
    expect(config?.data.datasets[0]?.label).toBe('Income')
    expect(config?.data.datasets[0]?.data).toEqual([1, 2])
  })

  it('colours series from the theme palette unless one is given', async () => {
    mountChart({
      type: 'line',
      labels: ['a'],
      series: [{ label: 'A', data: [1] }, { label: 'B', data: [2], color: '#123456' }],
    })
    await flushPromises()

    const datasets = created[0]?.config.data.datasets
    expect(datasets?.[0]?.borderColor).toBeTruthy()
    expect(datasets?.[1]?.borderColor).toBe('#123456')
  })

  it('colours each slice of a pie and drops the axes', async () => {
    mountChart({ type: 'pie', labels: ['x', 'y', 'z'], series: [{ label: 'Share', data: [1, 2, 3] }] })
    await flushPromises()

    const config = created[0]?.config
    expect(config?.data.datasets[0]?.backgroundColor).toHaveLength(3)
    expect(config?.options.scales).toEqual({})
    expect(config?.options.plugins.legend.display).toBe(true)
  })

  it('shows the legend only when it says something', async () => {
    mountChart({ type: 'bar', labels: ['a'], series: [{ label: 'A', data: [1] }] })
    mountChart({ type: 'bar', labels: ['a'], series: [{ label: 'A', data: [1] }], legend: true })
    await flushPromises()

    expect(created[0]?.config.options.plugins.legend.display).toBe(false)
    expect(created[1]?.config.options.plugins.legend.display).toBe(true)
  })

  it('formats tooltip and axis numbers with the formatter', async () => {
    mountChart({
      type: 'bar',
      labels: ['a'],
      series: [{ label: 'Income', data: [5] }],
      valueFormatter: (v: number) => `$${v}`,
    })
    await flushPromises()

    const options = created[0]?.config.options
    expect(options?.plugins.tooltip.callbacks.label({ raw: 5, label: 'a', dataset: { label: 'Income' } })).toBe('Income: $5')
    expect(options?.scales.y?.ticks.callback(10)).toBe('$10')
  })

  it('draws bars horizontally and stacks them on request', async () => {
    mountChart({ type: 'bar', labels: ['a'], series: [{ label: 'A', data: [1] }], horizontal: true, stacked: true })
    await flushPromises()

    const options = created[0]?.config.options
    expect(options?.indexAxis).toBe('y')
    expect(options?.scales.x?.stacked).toBe(true)
  })

  it('shows the empty state and builds no chart without data', async () => {
    const wrapper = mountChart({ type: 'bar', labels: [], series: [] })
    await flushPromises()

    expect(wrapper.get('[data-testid="chart-empty"]').text()).toBe('No data to chart')
    expect(wrapper.find('canvas').exists()).toBe(false)
    expect(created).toHaveLength(0)
  })

  it('rebuilds when the data changes and destroys on unmount', async () => {
    const wrapper = mountChart({ type: 'bar', labels: ['a'], series: [{ label: 'A', data: [1] }] })
    await flushPromises()
    const first = created[0]

    await wrapper.setProps({ labels: ['a', 'b'], series: [{ label: 'A', data: [1, 2] }] })
    await flushPromises()

    expect(first?.destroy).toHaveBeenCalled()
    expect(created).toHaveLength(2)

    wrapper.unmount()
    expect(created[1]?.destroy).toHaveBeenCalled()
  })
})
