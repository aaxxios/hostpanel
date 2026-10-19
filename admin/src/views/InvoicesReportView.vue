<script setup lang="ts">
/**
 * Invoices report: a custom export of invoices by chosen columns, three date ranges and up to five
 * filters, with a CSV export.
 */
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import ReportQueryPage from '@/components/reports/ReportQueryPage.vue'
import { reportApiPath } from '@/lib/apiPaths'
import type { FieldOption } from '@/components/ui/UiAdvancedFilters.vue'
import { useQueryReport } from '@/composables/useQueryReport'
import { ReportCellFormat } from '@/types/reportcolumn'
import type { ReportColumn } from '@/types/reportcolumn'
import type { ReportInvoiceRow } from '@/types/reportinvoicerow'
import type { ReportQueryConfig, ReportQueryRange } from '@/types/reportquery'
import { REPORT_PAGE_SIZE } from '@/lib/limits'

const { t } = useI18n()

/** Every column the report can show. */
const columns = computed<ReportColumn[]>(() => [
  { key: 'id', label: t('shared.id'), format: ReportCellFormat.Id },
  { key: 'clientId', label: t('reports.invoicesReport.userId') },
  { key: 'clientName', label: t('shared.clientName') },
  { key: 'invoiceNumber', label: t('reports.invoicesReport.invoiceNumber') },
  { key: 'createdDate', label: t('reports.invoicesReport.creationDate') },
  { key: 'dueDate', label: t('shared.dueDate') },
  { key: 'datePaid', label: t('shared.datePaid') },
  { key: 'dateRefunded', label: t('billing.invoicesList.dateRefunded') },
  { key: 'dateCancelled', label: t('billing.invoicesList.dateCancelled') },
  { key: 'subTotal', label: t('reports.directDebit.subtotal'), format: ReportCellFormat.Money },
  { key: 'credit', label: t('reports.directDebit.credit'), format: ReportCellFormat.Money },
  { key: 'tax', label: t('reports.directDebit.tax'), format: ReportCellFormat.Money },
  { key: 'tax2', label: t('reports.invoicesReport.tax2'), format: ReportCellFormat.Money },
  { key: 'total', label: t('shared.total'), format: ReportCellFormat.Money },
  { key: 'taxRate', label: t('billing.invoiceOptionsTab.taxRate'), format: ReportCellFormat.Percent },
  { key: 'taxRate2', label: t('reports.invoicesReport.taxRate2'), format: ReportCellFormat.Percent },
  { key: 'status', label: t('shared.status'), format: ReportCellFormat.Status },
  { key: 'paymentMethod', label: t('shared.paymentMethod') },
  { key: 'notes', label: t('shared.notes') },
])

/**
 * Date-range filters. There is no refunded or cancelled range: an invoice stores no such date, so
 * the endpoint could not filter by it.
 */
const ranges = computed<ReportQueryRange[]>(() => [
  { key: 'created', label: t('reports.invoicesReport.creationDate'), param: 'created' },
  { key: 'due', label: t('shared.dueDate'), param: 'due' },
  { key: 'paid', label: t('shared.datePaid'), param: 'paid' },
])

/** Fields the advanced filters can target: every column. */
const filterFields = computed<FieldOption[]>(() =>
  columns.value.map(column => ({ value: column.key, label: column.label })),
)

/** What the report is. */
const config: ReportQueryConfig = {
  path: reportApiPath('invoices'),
  serverFields: ['status'],
  exportPath: reportApiPath('invoices', 'export'),
  pageSize: REPORT_PAGE_SIZE,
}

const report = useQueryReport<ReportInvoiceRow>(config, { columns, ranges })
</script>

<template>
  <ReportQueryPage
    :title="t('shared.invoices')"
    :description="t('reports.invoicesReport.thisReportCanBeUsed')"
    :report="report"
    :columns="columns"
    :ranges="ranges"
    :filter-fields="filterFields"
    :found-text="t('reports.invoicesReport.totalcountInvoiceSFound', { totalCount: report.totalCount })"
    :empty-text="t('shared.noInvoicesFound')"
    exportable
  />
</template>
