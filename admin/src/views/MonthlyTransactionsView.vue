<script setup lang="ts">
/**
 * Monthly transactions report: money in, fees and money out per day of a month, as a line chart and
 * a table with totals, in the base currency the API converted them to.
 */
import { computed, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import ReportChartCard from '@/components/reports/ReportChartCard.vue'
import ReportPage from '@/components/reports/ReportPage.vue'
import ReportPeriodFilter from '@/components/reports/ReportPeriodFilter.vue'
import ReportTable from '@/components/reports/ReportTable.vue'
import UiText from '@/components/ui/UiText.vue'
import { useReport } from '@/composables/useReport'
import { formatCurrencyCode } from '@/lib/money'
import { currentMonthNumber, currentYear, monthName } from '@/lib/reportsPeriod'
import { reportPath } from '@/lib/reportsQuery'
import { ReportCellFormat } from '@/types/reportcolumn'
import type { ReportColumn } from '@/types/reportcolumn'
import type { ReportDailyTransactions } from '@/types/reportdailytransactions'
import { UiChartType } from '@/utils/ui/chart'
import type { UiChartSeries } from '@/utils/ui/chart'
import { UiAlign } from '@/utils/ui/table'

const { t } = useI18n()

/** Year being reported; the current one at first. */
const year = ref(currentYear())

/** Month being reported, 1 to 12; the current one at first. */
const month = ref(currentMonthNumber())

const { data, load } = useReport<ReportDailyTransactions>(() =>
  reportPath('/reports/daily-transactions', { year: year.value, month: month.value }),
)

/** Title naming the month once a report is loaded. */
const title = computed<string>(() =>
  data.value
    ? t('reports.titles.monthlyTransactionsFor', { month: monthName(data.value.month), year: data.value.year })
    : t('reports.titles.reports.monthlyTransactions'),
)

/** Base currency the figures are converted to; empty until loaded. */
const currency = computed<string>(() => data.value?.currency ?? '')

/**
 * Formats an amount in the base currency the API converted the report to.
 *
 * @param amount - Amount in the report currency.
 * @returns The amount with its currency symbol.
 */
const formatMoney = (amount: number): string => formatCurrencyCode(amount, currency.value)

/** Table columns. */
const columns = computed<ReportColumn[]>(() => [
  { key: 'date', label: t('shared.date') },
  { key: 'amountIn', label: t('shared.amountIn'), align: UiAlign.Right, format: ReportCellFormat.Money },
  { key: 'fees', label: t('shared.fees'), align: UiAlign.Right, format: ReportCellFormat.Money },
  { key: 'amountOut', label: t('shared.amountOut'), align: UiAlign.Right, format: ReportCellFormat.Money },
  { key: 'balance', label: t('shared.balance'), align: UiAlign.Right, format: ReportCellFormat.Money },
])

/** Footer figures; the balance is what came in less fees and payouts. */
const totals = computed<Record<string, number>>(() => ({
  amountIn: data.value?.totalAmountIn ?? 0,
  fees: data.value?.totalFees ?? 0,
  amountOut: data.value?.totalAmountOut ?? 0,
  balance: (data.value?.totalAmountIn ?? 0) - (data.value?.totalFees ?? 0) - (data.value?.totalAmountOut ?? 0),
}))

/** Chart labels: month and day of each row. */
const labels = computed<string[]>(() => (data.value?.rows ?? []).map(row => row.date.slice(5)))

/** Chart series. */
const series = computed<UiChartSeries[]>(() => {
  const rows = data.value?.rows ?? []
  return [
    { label: t('shared.amountIn'), data: rows.map(row => row.amountIn) },
    { label: t('shared.fees'), data: rows.map(row => row.fees) },
    { label: t('shared.amountOut'), data: rows.map(row => row.amountOut) },
  ]
})
</script>

<template>
  <ReportPage :retry="load" :title="title" :description="t('reports.monthlyTransactions.thisReportProvidesASummary')">
    <template #filters>
      <ReportPeriodFilter v-model:year="year" v-model:month="month" show-month :years-back="2" :years-ahead="2" @generate="load" />
    </template>
    <template v-if="data">
      <ReportChartCard :type="UiChartType.Line" :labels="labels" :series="series" :value-formatter="formatMoney" height="lg" />
      <ReportTable
        :columns="columns"
        :rows="data.rows"
        row-key="date"
        :totals="totals"
        :format-money="formatMoney"
        :empty-text="t('reports.monthlyTransactions.noTransactionsFoundForThis')"
      />
      <UiText tone="muted" size="sm">
        {{ t('reports.common.convertedToBaseCurrency', { currency }) }}
      </UiText>
    </template>
  </ReportPage>
</template>
