<script setup lang="ts">
/**
 * Transactions report: a custom export of transactions by chosen columns, a date range and up to
 * five filters, with a CSV export.
 */
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import ReportQueryPage from '@/components/reports/ReportQueryPage.vue'
import { reportApiPath } from '@/lib/apiPaths'
import type { FieldOption } from '@/components/ui/UiAdvancedFilters.vue'
import { useQueryReport } from '@/composables/useQueryReport'
import { ReportCellFormat } from '@/types/reportcolumn'
import type { ReportColumn } from '@/types/reportcolumn'
import type { ReportQueryConfig, ReportQueryRange } from '@/types/reportquery'
import type { ReportTransactionRow } from '@/types/reporttransactionrow'
import { REPORT_PAGE_SIZE } from '@/lib/limits'

const { t } = useI18n()

/** Every column the report can show. */
const columns = computed<ReportColumn[]>(() => [
  { key: 'id', label: t('shared.id'), format: ReportCellFormat.Id },
  { key: 'clientId', label: t('reports.transactionsReport.userId') },
  { key: 'clientName', label: t('shared.clientName') },
  { key: 'currency', label: t('shared.currency') },
  { key: 'paymentMethod', label: t('shared.paymentMethod') },
  { key: 'date', label: t('shared.date') },
  { key: 'description', label: t('shared.description') },
  { key: 'invoiceId', label: t('shared.invoiceId') },
  { key: 'transactionId', label: t('shared.transactionId') },
  { key: 'amountIn', label: t('shared.amountIn'), format: ReportCellFormat.Money, currencyKey: 'currency' },
  { key: 'fees', label: t('shared.fees'), format: ReportCellFormat.Money, currencyKey: 'currency' },
  { key: 'amountOut', label: t('shared.amountOut'), format: ReportCellFormat.Money, currencyKey: 'currency' },
])

/** Date-range filters. */
const ranges = computed<ReportQueryRange[]>(() => [
  { key: 'date', label: t('shared.dateRange'), param: 'date' },
])

/** Fields the advanced filters can target: every column. */
const filterFields = computed<FieldOption[]>(() =>
  columns.value.map(column => ({ value: column.key, label: column.label })),
)

/** What the report is. */
const config: ReportQueryConfig = {
  path: reportApiPath('transactions'),
  serverFields: ['paymentMethod'],
  exportPath: reportApiPath('transactions', 'export'),
  pageSize: REPORT_PAGE_SIZE,
}

const report = useQueryReport<ReportTransactionRow>(config, { columns, ranges })
</script>

<template>
  <ReportQueryPage
    :title="t('shared.transactions')"
    :description="t('reports.transactionsReport.thisReportCanBeUsed')"
    :report="report"
    :columns="columns"
    :ranges="ranges"
    :filter-fields="filterFields"
    :found-text="t('reports.transactionsReport.totalcountTransactionSFound', { totalCount: report.totalCount })"
    :empty-text="t('reports.transactionsReport.noTransactionsFound')"
    exportable
  />
</template>
