<script setup lang="ts">
/**
 * Chart frame over chart.js: the only place a screen's data meets the chart library.
 *
 * A screen passes `type`, `labels` and `series`; this component owns the theme colours (read from
 * the light/dark tokens and re-read when the theme class changes), grid, tooltip format, legend and
 * the empty state, so no screen configures chart.js. `valueFormatter` formats tooltip and axis
 * numbers (currency, percent).
 */
import { computed, nextTick, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import {
  ArcElement,
  BarController,
  BarElement,
  CategoryScale,
  Chart,
  DoughnutController,
  Filler,
  Legend,
  LineController,
  LineElement,
  LinearScale,
  PieController,
  PointElement,
  Tooltip,
} from 'chart.js'
import type { ChartConfiguration, ChartType, TooltipItem } from 'chart.js'
import { paletteColor, readChartTheme, UiChartType } from '@/utils/ui/chart'
import type { UiChartHeight, UiChartSeries } from '@/utils/ui/chart'

Chart.register(
  ArcElement,
  BarController,
  BarElement,
  CategoryScale,
  DoughnutController,
  Filler,
  Legend,
  LineController,
  LineElement,
  LinearScale,
  PieController,
  PointElement,
  Tooltip,
)

const { t } = useI18n()

/** Props for UiChart. */
const props = withDefaults(defineProps<{
  /** Kind of chart. */
  type: UiChartType
  /** Category labels (x axis, or slice names for pie/doughnut). */
  labels: string[]
  /** Data series; pie and doughnut use the first one. */
  series: UiChartSeries[]
  /** Formats numbers in tooltips and on the value axis. */
  valueFormatter?: (value: number) => string
  /** Shows the legend; by default only when it says something (several series, or a pie). */
  legend?: boolean
  /** Stacks the bars of several series. */
  stacked?: boolean
  /** Draws bars horizontally. */
  horizontal?: boolean
  /** Height of the plot area. */
  height?: UiChartHeight
}>(), {
  valueFormatter: undefined,
  legend: undefined,
  stacked: false,
  horizontal: false,
  height: 'md',
})

/** Plot heights. */
const heightClasses: Record<UiChartHeight, string> = {
  sm: 'h-40',
  md: 'h-64',
  lg: 'h-80',
}

/** The canvas chart.js draws on; absent while the empty state shows. */
const canvas = ref<HTMLCanvasElement | null>(null)

/** The live chart instance, destroyed before every rebuild and on unmount. */
let chart: Chart | null = null

/** Watches the root element's class so a light/dark switch recolours the chart. */
let themeObserver: MutationObserver | null = null

/** Whether there is nothing to plot. */
const isEmpty = computed<boolean>(() =>
  props.labels.length === 0 || props.series.every(s => s.data.length === 0),
)

/** Whether the chart is a circular kind. */
const isCircular = computed<boolean>(() => props.type === UiChartType.Pie || props.type === UiChartType.Doughnut)

/**
 * Formats a number with the caller's formatter, or as-is.
 *
 * @param value - Number to format.
 * @returns The display text.
 */
const format = (value: number): string => (props.valueFormatter ? props.valueFormatter(value) : String(value))

/**
 * Builds the chart.js configuration from props and the current theme.
 *
 * @returns A configuration object for `new Chart`.
 */
const buildConfig = (): ChartConfiguration => {
  const theme = readChartTheme()
  const showLegend = props.legend ?? (isCircular.value || props.series.length > 1)
  const circular = isCircular.value
  const first = props.series[0]
  const datasets = circular
    ? [{
        label: first?.label ?? '',
        data: first?.data ?? [],
        backgroundColor: props.labels.map((_, index) => paletteColor(theme, index)),
        borderWidth: 0,
      }]
    : props.series.map((s, index) => {
        const color = s.color ?? paletteColor(theme, index)
        return {
          label: s.label,
          data: s.data,
          backgroundColor: color,
          borderColor: color,
          borderRadius: props.type === UiChartType.Bar ? 6 : 0,
          tension: 0.35,
          fill: false,
        }
      })
  /**
   * Formats a value-axis tick with the chart formatter.
   *
   * @param value - Tick value as chart.js passes it.
   * @returns The formatted tick label.
   */
  const formatTick = (value: string | number): string => format(Number(value))
  const valueAxis = {
    stacked: props.stacked,
    grid: { color: theme.grid },
    ticks: { color: theme.tick, callback: formatTick },
  }
  /**
   * Tooltip line: the slice label for circular charts, the series label otherwise.
   *
   * @param context - Hovered item.
   * @returns The text of the tooltip line.
   */
  const tooltipLine = (context: TooltipItem<ChartType>): string => {
    const raw = Number(context.raw)
    return circular ? `${context.label}: ${format(raw)}` : `${context.dataset.label ?? ''}: ${format(raw)}`
  }
  const categoryAxis = { stacked: props.stacked, grid: { color: theme.grid }, ticks: { color: theme.tick } }
  return {
    type: props.type as ChartType,
    data: { labels: props.labels, datasets },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      indexAxis: props.horizontal ? 'y' : 'x',
      plugins: {
        legend: { display: showLegend, labels: { color: theme.tick } },
        tooltip: {
          callbacks: {
            label: tooltipLine,
          },
        },
      },
      scales: circular
        ? {}
        : { x: props.horizontal ? valueAxis : categoryAxis, y: props.horizontal ? categoryAxis : valueAxis },
    },
  }
}

/** Destroys the current chart, if any. */
const destroy = (): void => {
  chart?.destroy()
  chart = null
}

/** Rebuilds the chart from the current props and theme. */
const render = async (): Promise<void> => {
  destroy()
  // The canvas exists only while the empty state is hidden, so wait for the DOM to settle.
  await nextTick()
  if (canvas.value !== null && !isEmpty.value) {
    chart = new Chart(canvas.value, buildConfig())
  }
}

onMounted(() => {
  void render()
  themeObserver = new MutationObserver(() => {
    void render()
  })
  themeObserver.observe(document.documentElement, { attributes: true, attributeFilter: ['class'] })
})

watch(
  () => [props.type, props.labels, props.series, props.stacked, props.horizontal, props.legend],
  () => {
    void render()
  },
  { deep: true },
)

onBeforeUnmount(() => {
  themeObserver?.disconnect()
  destroy()
})
</script>

<template>
  <div class="relative" :class="heightClasses[height]">
    <p
      v-if="isEmpty"
      class="absolute inset-0 flex items-center justify-center text-[0.82rem] text-text-secondary"
      data-testid="chart-empty"
    >
      {{ t('ui.chart.empty') }}
    </p>
    <canvas v-else ref="canvas" />
  </div>
</template>
