<script setup lang="ts">
/**
 * VAT MOSS report: the invoiced value and VAT collected per country for a quarter, shown in the
 * chosen currency, with totals.
 */
import { computed, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import ReportField from '@/components/reports/ReportField.vue'
import ReportFilterBar from '@/components/reports/ReportFilterBar.vue'
import ReportPage from '@/components/reports/ReportPage.vue'
import ReportTable from '@/components/reports/ReportTable.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import UiText from '@/components/ui/UiText.vue'
import { useReport } from '@/composables/useReport'
import { formatReportAmount, REPORT_CURRENCY_OPTIONS, ReportCurrency } from '@/lib/money'
import { reportPath } from '@/lib/reportsQuery'
import { ReportCellFormat } from '@/types/reportcolumn'
import type { ReportColumn } from '@/types/reportcolumn'
import type { ReportVat } from '@/types/reportvat'
import { UiAlign } from '@/utils/ui/table'

const { t } = useI18n()

/** Number of calendar years, ending with the current one, whose quarters are on offer. */
const YEARS_ON_OFFER = 4

/** The moment the screen opened; the offered quarters count back from it. */
const now = new Date()

/** The quarter being reported as `<year>-<quarter>`; the current one at first. */
const quarter = ref(`${now.getFullYear()}-${Math.floor(now.getMonth() / 3) + 1}`)

/** Currency the figures are shown in. */
const currency = ref<ReportCurrency>(ReportCurrency.Usd)

/** Quarters of the last few years, newest first. */
const quarterOptions = computed(() => {
  const options: { value: string; label: string }[] = []
  for (let year = now.getFullYear(); year > now.getFullYear() - YEARS_ON_OFFER; year--) {
    for (let number = 4; number >= 1; number--) {
      options.push({
        value: `${year}-${number}`,
        label: t('reports.vatMoss.quarterOption', { year, quarter: number, months: t(`reports.vatMoss.quarter${number}`) }),
      })
    }
  }
  return options
})

const { data, load } = useReport<ReportVat>(() => {
  const [year = '', number = ''] = quarter.value.split('-')
  return reportPath('/reports/vat-moss', { year, quarter: number })
})

/**
 * Formats a figure in the chosen currency.
 *
 * @param amount - Figure in USD.
 * @returns The restated, formatted figure.
 */
const formatMoney = (amount: number): string => formatReportAmount(amount, currency.value)

/** Table columns. */
const columns = computed<ReportColumn[]>(() => [
  { key: 'countryName', label: t('reports.vatMoss.countryName') },
  { key: 'countryCode', label: t('reports.vatMoss.countryCode') },
  { key: 'vatRate', label: t('reports.vatMoss.vatRate'), align: UiAlign.Right, format: ReportCellFormat.Percent },
  { key: 'numberOfInvoices', label: t('reports.vatMoss.numberOfInvoices'), align: UiAlign.Right },
  { key: 'totalExclVat', label: t('reports.vatMoss.totalValueInvoicedExclVat'), align: UiAlign.Right, format: ReportCellFormat.Money },
  { key: 'totalVatCollected', label: t('reports.vatMoss.totalVatCollected'), align: UiAlign.Right, format: ReportCellFormat.Money },
  { key: 'currency', label: t('shared.currency') },
])

/** Footer sums. */
const totals = computed<Record<string, number>>(() => {
  const rows = data.value?.rows ?? []
  return {
    numberOfInvoices: rows.reduce((sum, row) => sum + row.numberOfInvoices, 0),
    totalExclVat: rows.reduce((sum, row) => sum + row.totalExclVat, 0),
    totalVatCollected: rows.reduce((sum, row) => sum + row.totalVatCollected, 0),
  }
})
</script>

<template>
  <ReportPage :title="t('reports.vatMoss.vatMossSettlementData')" :description="t('reports.vatMoss.thisReportProvidesTheInformation')">
    <template #filters>
      <ReportFilterBar :generate-label="t('shared.generateReport')" @generate="load">
        <template #fields>
          <ReportField :label="t('reports.vatMoss.selectQuarter')" width="lg">
            <UiSelect v-model="quarter" :options="quarterOptions" :placeholder="t('reports.vatMoss.selectQuarter')" data-testid="report-quarter" />
          </ReportField>
          <ReportField :label="t('shared.currency')">
            <UiSelect v-model="currency" :options="REPORT_CURRENCY_OPTIONS" :placeholder="t('shared.currency')" data-testid="report-currency" />
          </ReportField>
        </template>
      </ReportFilterBar>
    </template>

    <template v-if="data">
      <UiText v-if="data.periodLabel" tone="secondary" size="md" data-testid="report-period-label">
        {{ data.periodLabel }}
      </UiText>
      <ReportTable
        :columns="columns"
        :rows="data.rows"
        :totals="totals"
        :totals-label="t('reports.vatMoss.totals')"
        :format-money="formatMoney"
        :empty-text="t('shared.noDataFoundForThis2')"
      />
      <UiText tone="muted" size="sm">
        {{ t('reports.vatMoss.ifACountryDoesNot') }}
      </UiText>
      <UiText tone="muted" size="sm">
        {{ t('reports.vatMoss.isleOfManGbAnd') }}
      </UiText>
    </template>
  </ReportPage>
</template>
