<script setup lang="ts">
/**
 * Aging invoices report: the overdue amount per period and currency, with a pie of the periods.
 */
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import ReportChartCard from '@/components/reports/ReportChartCard.vue'
import ReportPage from '@/components/reports/ReportPage.vue'
import ReportTable from '@/components/reports/ReportTable.vue'
import { useReport } from '@/composables/useReport'
import { formatCurrencyCode, formatReportAmount } from '@/lib/money'
import type { ReportAging } from '@/types/reportaging'
import type { ReportColumn } from '@/types/reportcolumn'
import { UiChartType } from '@/utils/ui/chart'
import type { UiChartSeries } from '@/utils/ui/chart'
import { UiAlign } from '@/utils/ui/table'

const { t } = useI18n()

const { data } = useReport<ReportAging>(() => '/reports/aging-invoices-summary')

/** One column for the period and one per currency in the report. */
const columns = computed<ReportColumn[]>(() => [
  { key: 'period', label: t('reports.agingInvoices.period') },
  ...(data.value?.currencies ?? []).map(currency => ({
    key: currency,
    label: t('reports.agingInvoices.curAmount', { cur: currency }),
    align: UiAlign.Right,
  })),
])

/** One row per period with its amounts already formatted in their own currency. */
const rows = computed<Record<string, string>[]>(() =>
  (data.value?.periods ?? []).map(period => ({
    period: period.period,
    ...Object.fromEntries(
      (data.value?.currencies ?? []).map(currency => [currency, formatCurrencyCode(period.amountsByCurrency[currency] ?? 0, currency)]),
    ),
  })),
)

/** The footer figures, formatted per currency. */
const totals = computed<Record<string, string>>(() =>
  Object.fromEntries(
    (data.value?.currencies ?? []).map(currency => [currency, formatCurrencyCode(data.value?.totals[currency] ?? 0, currency)]),
  ),
)

/** Periods with something overdue; the pie compares periods, so each currency counts at face value. */
const overduePeriods = computed(() =>
  (data.value?.periods ?? [])
    .map(period => ({ label: period.period, total: Object.values(period.amountsByCurrency).reduce((sum, amount) => sum + amount, 0) }))
    .filter(period => period.total > 0),
)

/** Pie labels. */
const labels = computed<string[]>(() => overduePeriods.value.map(period => period.label))

/** Pie data. */
const series = computed<UiChartSeries[]>(() => [{ label: t('shared.total'), data: overduePeriods.value.map(period => period.total) }])
</script>

<template>
  <ReportPage :title="t('reports.agingInvoices.agingInvoices')" :description="t('reports.agingInvoices.aSummaryOfOutstandingInvoices')">
    <template v-if="data">
      <ReportTable :columns="columns" :rows="rows" row-key="period" :totals="totals" />
      <ReportChartCard
        :type="UiChartType.Pie"
        :labels="labels"
        :series="series"
        :value-formatter="formatReportAmount"
        height="lg"
      />
    </template>
  </ReportPage>
</template>
