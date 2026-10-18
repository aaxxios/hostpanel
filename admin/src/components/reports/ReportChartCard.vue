<script setup lang="ts">
/**
 * Card around a report chart, with an optional title.
 *
 * The chart itself is `UiChart`: this component only frames it, so a report never configures the
 * chart library.
 */
import UiCard from '@/components/ui/UiCard.vue'
import UiChart from '@/components/ui/UiChart.vue'
import UiHeading from '@/components/ui/UiHeading.vue'
import type { UiChartHeight, UiChartSeries, UiChartType } from '@/utils/ui/chart'

/** Props for ReportChartCard. */
withDefaults(defineProps<{
  /** Caption above the chart (already translated). */
  title?: string
  /** Kind of chart. */
  type: UiChartType
  /** Category labels (or slice names for a pie). */
  labels: string[]
  /** Data series. */
  series: UiChartSeries[]
  /** Formats numbers in tooltips and on the value axis. */
  valueFormatter?: (value: number) => string
  /** Shows the legend; by default only when it says something. */
  legend?: boolean
  /** Draws bars horizontally. */
  horizontal?: boolean
  /** Height of the plot area. */
  height?: UiChartHeight
}>(), {
  title: undefined,
  valueFormatter: undefined,
  legend: undefined,
  horizontal: false,
  height: 'md',
})
</script>

<template>
  <UiCard padding="md" data-testid="report-chart">
    <UiHeading v-if="title" :level="2" variant="card" class="mb-4">
      {{ title }}
    </UiHeading>
    <UiChart
      :type="type"
      :labels="labels"
      :series="series"
      :value-formatter="valueFormatter"
      :legend="legend"
      :horizontal="horizontal"
      :height="height"
    />
  </UiCard>
</template>
