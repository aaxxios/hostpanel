<script setup lang="ts">
/**
 * New customers report: sign-ups, orders placed and orders completed per month of a year, as a
 * line chart and a table with totals.
 */
import { computed, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import ReportChartCard from '@/components/reports/ReportChartCard.vue'
import ReportPage from '@/components/reports/ReportPage.vue'
import ReportPeriodFilter from '@/components/reports/ReportPeriodFilter.vue'
import ReportTable from '@/components/reports/ReportTable.vue'
import { useReport } from '@/composables/useReport'
import { reportPath } from '@/lib/reportsQuery'
import { currentYear } from '@/lib/reportsPeriod'
import type { ReportColumn } from '@/types/reportcolumn'
import type { ReportNewCustomersRow } from '@/types/reportnewcustomers'
import { UiChartType } from '@/utils/ui/chart'
import type { UiChartSeries } from '@/utils/ui/chart'
import { UiAlign } from '@/utils/ui/table'

const { t } = useI18n()

/** The year being reported. */
const year = ref(currentYear())

const { data, load } = useReport<ReportNewCustomersRow[]>(() => reportPath('/reports/new-customers', { year: year.value }))

// Picking another year reloads at once, as the dropdown has always done.
watch(year, load)

/** Months with their counters; empty until loaded. */
const rows = computed<ReportNewCustomersRow[]>(() => data.value ?? [])

/** Table columns. */
const columns = computed<ReportColumn[]>(() => [
  { key: 'month', label: t('shared.month') },
  { key: 'newSignups', label: t('reports.newCustomers.newSignups'), align: UiAlign.Right },
  { key: 'ordersPlaced', label: t('reports.newCustomers.ordersPlaced'), align: UiAlign.Right },
  { key: 'ordersCompleted', label: t('reports.newCustomers.ordersCompleted'), align: UiAlign.Right },
])

/** Footer sums of the three counters. */
const totals = computed<Record<string, number>>(() => ({
  newSignups: rows.value.reduce((sum, row) => sum + row.newSignups, 0),
  ordersPlaced: rows.value.reduce((sum, row) => sum + row.ordersPlaced, 0),
  ordersCompleted: rows.value.reduce((sum, row) => sum + row.ordersCompleted, 0),
}))

/** Chart labels. */
const labels = computed<string[]>(() => rows.value.map(row => row.month))

/** One chart series per counter. */
const series = computed<UiChartSeries[]>(() => [
  { label: t('reports.newCustomers.newSignups'), data: rows.value.map(row => row.newSignups) },
  { label: t('reports.newCustomers.ordersPlaced'), data: rows.value.map(row => row.ordersPlaced) },
  { label: t('reports.newCustomers.ordersCompleted'), data: rows.value.map(row => row.ordersCompleted) },
])
</script>

<template>
  <ReportPage :title="t('reports.newCustomers.newCustomers')" :description="t('reports.newCustomers.monthlyNewSignupsAndOrder')">
    <template #filters>
      <ReportPeriodFilter v-model:year="year" :years-back="9" @generate="load" />
    </template>
    <ReportChartCard :type="UiChartType.Line" :labels="labels" :series="series" />
    <ReportTable :columns="columns" :rows="rows" row-key="month" :totals="totals" />
  </ReportPage>
</template>
