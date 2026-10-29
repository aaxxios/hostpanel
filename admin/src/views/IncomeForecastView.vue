<script setup lang="ts">
/**
 * Income forecast report: income booked per month of the year with its trailing quarter, half year
 * and year to date, as a line chart of the running total and a table.
 */
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import ReportChartCard from '@/components/reports/ReportChartCard.vue'
import ReportPage from '@/components/reports/ReportPage.vue'
import ReportTable from '@/components/reports/ReportTable.vue'
import UiText from '@/components/ui/UiText.vue'
import { useReport } from '@/composables/useReport'
import { formatCurrencyCode } from '@/lib/money'
import { ReportCellFormat } from '@/types/reportcolumn'
import type { ReportColumn } from '@/types/reportcolumn'
import type { ReportForecastRow } from '@/types/reportforecast'
import { UiChartType } from '@/utils/ui/chart'
import type { UiChartSeries } from '@/utils/ui/chart'
import { UiAlign } from '@/utils/ui/table'

const { t } = useI18n()

const { data, load } = useReport<ReportForecastRow[]>(() => '/reports/income-forecast')

/** Months with their income; empty until loaded. */
const rows = computed<ReportForecastRow[]>(() => data.value ?? [])

/** Base currency the amounts are converted to; empty until loaded. */
const currency = computed<string>(() => rows.value[0]?.currency ?? '')

/**
 * Formats an amount in the base currency the API converted the report to.
 *
 * @param amount - Amount in the report currency.
 * @returns The amount with its currency symbol.
 */
const formatMoney = (amount: number): string => formatCurrencyCode(amount, currency.value)

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

/** Chart series: the running total from January. */
const series = computed<UiChartSeries[]>(() => [
  { label: t('reports.incomeForecast.cumulativeForecast'), data: rows.value.map(row => row.total) },
])
</script>

<template>
  <ReportPage :retry="load" :title="t('reports.incomeForecast.incomeForecast')" :description="t('reports.incomeForecast.incomePerMonthWithTrailingWindows')">
    <ReportChartCard :type="UiChartType.Line" :labels="labels" :series="series" :value-formatter="formatMoney" />
    <ReportTable :columns="columns" :rows="rows" row-key="month" :format-money="formatMoney" />
    <UiText tone="muted" size="sm">
      {{ t('reports.common.convertedToBaseCurrency', { currency }) }}
    </UiText>
  </ReportPage>
</template>
