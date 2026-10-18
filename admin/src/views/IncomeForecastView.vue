<script setup lang="ts">
/**
 * Income forecast report: projected income per month from the recurring billing cycles, as a line
 * chart of the monthly total and a table split by cycle.
 */
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import ReportChartCard from '@/components/reports/ReportChartCard.vue'
import ReportPage from '@/components/reports/ReportPage.vue'
import ReportTable from '@/components/reports/ReportTable.vue'
import { useReport } from '@/composables/useReport'
import { formatReportAmount } from '@/lib/money'
import { ReportCellFormat } from '@/types/reportcolumn'
import type { ReportColumn } from '@/types/reportcolumn'
import type { ReportForecastRow } from '@/types/reportforecast'
import { UiChartType } from '@/utils/ui/chart'
import type { UiChartSeries } from '@/utils/ui/chart'
import { UiAlign } from '@/utils/ui/table'

const { t } = useI18n()

const { data } = useReport<ReportForecastRow[]>(() => '/reports/income-forecast')

/** Months with their projected income; empty until loaded. */
const rows = computed<ReportForecastRow[]>(() => data.value ?? [])

/** Table columns. */
const columns = computed<ReportColumn[]>(() => [
  { key: 'month', label: t('shared.month') },
  { key: 'monthly', label: t('reports.incomeForecast.monthly'), align: UiAlign.Right, format: ReportCellFormat.Money },
  { key: 'quarterly', label: t('reports.incomeForecast.quarterly'), align: UiAlign.Right, format: ReportCellFormat.Money },
  { key: 'semiAnnual', label: t('reports.incomeForecast.semiAnnual'), align: UiAlign.Right, format: ReportCellFormat.Money },
  { key: 'annual', label: t('reports.incomeForecast.annual'), align: UiAlign.Right, format: ReportCellFormat.Money },
  { key: 'total', label: t('shared.total'), align: UiAlign.Right, format: ReportCellFormat.Money },
])

/** Chart labels. */
const labels = computed<string[]>(() => rows.value.map(row => row.month))

/** Chart series: the monthly total. */
const series = computed<UiChartSeries[]>(() => [
  { label: t('reports.incomeForecast.cumulativeForecast'), data: rows.value.map(row => row.total) },
])
</script>

<template>
  <ReportPage :title="t('reports.incomeForecast.incomeForecast')" :description="t('reports.incomeForecast.projectedIncomeBasedOnRecurring')">
    <ReportChartCard :type="UiChartType.Line" :labels="labels" :series="series" :value-formatter="formatReportAmount" />
    <ReportTable :columns="columns" :rows="rows" row-key="month" />
  </ReportPage>
</template>
