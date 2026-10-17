<script setup lang="ts">
/**
 * Sales tax report: the invoices of a period with the tax liability they carry, shown in the chosen
 * currency.
 */
import { computed, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import ReportField from '@/components/reports/ReportField.vue'
import ReportFilterBar from '@/components/reports/ReportFilterBar.vue'
import ReportPage from '@/components/reports/ReportPage.vue'
import ReportTable from '@/components/reports/ReportTable.vue'
import UiDateRangePicker from '@/components/ui/UiDateRangePicker.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiText from '@/components/ui/UiText.vue'
import { useReport } from '@/composables/useReport'
import { formatReportAmount, REPORT_CURRENCY_OPTIONS, ReportCurrency } from '@/lib/money'
import { reportPath } from '@/lib/reportsQuery'
import { currentYearRange } from '@/lib/reportsPeriod'
import { ReportCellFormat } from '@/types/reportcolumn'
import type { ReportColumn } from '@/types/reportcolumn'
import type { ReportSalesTax } from '@/types/reportsalestax'
import { UiAlign } from '@/utils/ui/table'

const { t } = useI18n()

/** The period to report; the current year at first. */
const dateRange = ref<[string, string] | null>(currentYearRange())

/** Currency the figures are shown in. */
const currency = ref<ReportCurrency>(ReportCurrency.Usd)

const { data, load } = useReport<ReportSalesTax>(() =>
  reportPath('/reports/sales-tax', { from: dateRange.value?.[0], to: dateRange.value?.[1] }),
)

/**
 * Formats a figure in the chosen currency, with its code.
 *
 * @param amount - Figure in USD.
 * @returns The restated, formatted figure.
 */
const formatMoney = (amount: number): string => formatReportAmount(amount, currency.value, true)

/** Table columns. */
const columns = computed<ReportColumn[]>(() => [
  { key: 'id', label: t('shared.invoiceId') },
  { key: 'clientName', label: t('shared.clientName') },
  { key: 'invoiceDate', label: t('shared.invoiceDate') },
  { key: 'datePaid', label: t('shared.datePaid') },
  { key: 'subTotal', label: t('shared.subTotal2'), align: UiAlign.Right, format: ReportCellFormat.Money },
  { key: 'tax', label: t('reports.salesTax.tax'), align: UiAlign.Right, format: ReportCellFormat.Money },
  { key: 'credit', label: t('reports.salesTax.credit'), align: UiAlign.Right, format: ReportCellFormat.Money },
  { key: 'total', label: t('shared.total'), align: UiAlign.Right, format: ReportCellFormat.Money },
])

/** Footer figures: the sums of the columns, with the tax shown as the level 1 liability. */
const totals = computed<Record<string, number>>(() => ({
  subTotal: (data.value?.rows ?? []).reduce((sum, row) => sum + row.subTotal, 0),
  tax: data.value?.taxLevel1Liability ?? 0,
  credit: (data.value?.rows ?? []).reduce((sum, row) => sum + row.credit, 0),
  total: data.value?.totalInvoiced ?? 0,
}))
</script>

<template>
  <ReportPage :title="t('reports.salesTax.salesTaxLiability')" :description="t('reports.salesTax.thisReportShowsSalesTax')">
    <template #filters>
      <ReportFilterBar :generate-label="t('shared.generateReport')" @generate="load">
        <template #fields>
          <ReportField :label="t('shared.dateRange')" width="grow">
            <UiDateRangePicker v-model="dateRange" :placeholder="t('shared.selectDateRange')" />
          </ReportField>
          <ReportField :label="t('reports.salesTax.chooseCurrency')">
            <UiSelect v-model="currency" :options="REPORT_CURRENCY_OPTIONS" :placeholder="t('shared.currency')" data-testid="report-currency" />
          </ReportField>
        </template>
      </ReportFilterBar>
    </template>

    <template v-if="data">
      <UiStack gap="xs" data-testid="report-sales-tax-summary">
        <UiText tone="secondary" size="md">
          {{ data.totalInvoices }} {{ t('reports.salesTax.invoicesFound') }}
        </UiText>
        <UiText tone="secondary" size="md">
          {{ t('reports.salesTax.totalInvoiced') }} {{ formatMoney(data.totalInvoiced) }}
        </UiText>
        <UiText tone="secondary" size="md">
          {{ t('reports.salesTax.taxLevel1Liability') }} {{ formatMoney(data.taxLevel1Liability) }}
        </UiText>
        <UiText tone="secondary" size="md">
          {{ t('reports.salesTax.taxLevel2Liability') }} {{ formatMoney(data.taxLevel2Liability) }}
        </UiText>
      </UiStack>
      <ReportTable
        :columns="columns"
        :rows="data.rows"
        row-key="id"
        :totals="totals"
        :format-money="formatMoney"
        :empty-text="t('reports.salesTax.noInvoicesFoundForThis')"
      />
      <UiText tone="muted" size="sm">
        {{ t('reports.salesTax.thisReportExcludesInvoicesThat') }}
      </UiText>
    </template>
  </ReportPage>
</template>
