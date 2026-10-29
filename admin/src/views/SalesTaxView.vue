<script setup lang="ts">
/**
 * Sales tax report: the invoices paid in a period with the tax they carried. The totals come one
 * per invoice currency, never added across currencies; the invoice rows are paged and each amount
 * is shown in its invoice's currency.
 */
import { computed, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import ReportField from '@/components/reports/ReportField.vue'
import ReportFilterBar from '@/components/reports/ReportFilterBar.vue'
import ReportPage from '@/components/reports/ReportPage.vue'
import ReportTable from '@/components/reports/ReportTable.vue'
import UiDateRangePicker from '@/components/ui/UiDateRangePicker.vue'
import UiPagination from '@/components/ui/UiPagination.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiText from '@/components/ui/UiText.vue'
import { useReport } from '@/composables/useReport'
import { REPORT_PAGE_SIZE } from '@/lib/limits'
import { formatCurrencyCode } from '@/lib/money'
import { reportPath } from '@/lib/reportsQuery'
import { currentYearRange } from '@/lib/reportsPeriod'
import { ReportCellFormat } from '@/types/reportcolumn'
import type { ReportColumn } from '@/types/reportcolumn'
import type { ReportSalesTax } from '@/types/reportsalestax'
import { UiAlign } from '@/utils/ui/table'

const { t } = useI18n()

/** The payment days to report; the current year at first. */
const dateRange = ref<[string, string] | null>(currentYearRange())

/** Page of invoice rows shown, starting at 1. */
const page = ref(1)

const { data, load } = useReport<ReportSalesTax>(() =>
  reportPath('/reports/sales-tax', {
    from: dateRange.value?.[0],
    to: dateRange.value?.[1],
    page: page.value,
    pageSize: REPORT_PAGE_SIZE,
  }),
)

/** Runs the report for the chosen period from its first page. */
const generate = async (): Promise<void> => {
  page.value = 1
  await load()
}

/**
 * Shows another page of invoice rows.
 *
 * @param next - The page to show.
 */
const goToPage = async (next: number): Promise<void> => {
  page.value = next
  await load()
}

/** Table columns; every amount is written in its invoice's currency. */
const columns = computed<ReportColumn[]>(() => [
  { key: 'id', label: t('shared.invoiceId') },
  { key: 'clientName', label: t('shared.clientName') },
  { key: 'invoiceDate', label: t('shared.invoiceDate'), format: ReportCellFormat.Date },
  { key: 'datePaid', label: t('shared.datePaid'), format: ReportCellFormat.Date },
  { key: 'currency', label: t('shared.currency') },
  { key: 'subTotal', label: t('shared.subTotal2'), align: UiAlign.Right, format: ReportCellFormat.Money, currencyKey: 'currency' },
  { key: 'tax', label: t('reports.salesTax.tax'), align: UiAlign.Right, format: ReportCellFormat.Money, currencyKey: 'currency' },
  { key: 'credit', label: t('reports.salesTax.credit'), align: UiAlign.Right, format: ReportCellFormat.Money, currencyKey: 'currency' },
  { key: 'total', label: t('shared.total'), align: UiAlign.Right, format: ReportCellFormat.Money, currencyKey: 'currency' },
])
</script>

<template>
  <ReportPage :retry="load" :title="t('reports.salesTax.salesTaxLiability')" :description="t('reports.salesTax.thisReportShowsSalesTax')">
    <template #filters>
      <ReportFilterBar :generate-label="t('shared.generateReport')" @generate="generate">
        <template #fields>
          <ReportField :label="t('shared.dateRange')" width="grow">
            <UiDateRangePicker v-model="dateRange" :placeholder="t('shared.selectDateRange')" />
          </ReportField>
        </template>
      </ReportFilterBar>
    </template>

    <template v-if="data">
      <UiStack gap="xs" data-testid="report-sales-tax-summary">
        <UiText v-for="total in data.totals" :key="total.currency" tone="secondary" size="md">
          {{
            t('reports.salesTax.currencySummary', {
              currency: total.currency,
              count: total.totalInvoices,
              invoiced: formatCurrencyCode(total.totalInvoiced, total.currency),
              tax: formatCurrencyCode(total.taxLiability, total.currency),
            })
          }}
        </UiText>
      </UiStack>
      <ReportTable
        :columns="columns"
        :rows="data.rows.items"
        row-key="id"
        :empty-text="t('reports.salesTax.noInvoicesFoundForThis')"
      />
      <UiPagination
        :page="page"
        :total="data.rows.totalCount"
        :page-size="REPORT_PAGE_SIZE"
        testid="report-sales-tax-pagination"
        @update:page="goToPage"
      />
      <UiText tone="muted" size="sm">
        {{ t('reports.salesTax.thisReportExcludesInvoicesThat') }}
      </UiText>
    </template>
  </ReportPage>
</template>
