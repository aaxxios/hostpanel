<script setup lang="ts">
/**
 * Monthly orders report: units sold and their value per product for a month, grouped by product
 * group, in the base currency the API converted them to.
 */
import { computed, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import ReportPage from '@/components/reports/ReportPage.vue'
import ReportPeriodFilter from '@/components/reports/ReportPeriodFilter.vue'
import ReportProductSalesTable from '@/components/reports/ReportProductSalesTable.vue'
import UiText from '@/components/ui/UiText.vue'
import { useReport } from '@/composables/useReport'
import { formatCurrencyCode } from '@/lib/money'
import { currentMonthNumber, currentYear, monthName } from '@/lib/reportsPeriod'
import { reportPath } from '@/lib/reportsQuery'
import type { ReportMonthlyOrders } from '@/types/reportproductsales'

const { t } = useI18n()

/** Year being reported; the current one at first. */
const year = ref(currentYear())

/** Month being reported, 1 to 12; the current one at first. */
const month = ref(currentMonthNumber())

const { data, load } = useReport<ReportMonthlyOrders>(() =>
  reportPath('/reports/monthly-orders', { year: year.value, month: month.value }),
)

/** Title naming the month once a report is loaded. */
const title = computed<string>(() =>
  data.value
    ? t('reports.titles.salesByProductFor', { month: monthName(data.value.month), year: data.value.year })
    : t('reports.titles.reports.monthlyOrders'),
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
</script>

<template>
  <ReportPage :retry="load" :title="title" :description="t('reports.monthlyOrders.thisReportGivesABreakdown')">
    <template #filters>
      <ReportPeriodFilter v-model:year="year" v-model:month="month" show-month :years-back="2" :years-ahead="2" @generate="load" />
    </template>
    <ReportProductSalesTable
      v-if="data"
      :groups="data.groups"
      :total-units-sold="data.totalUnitsSold"
      :total-value="data.totalValue"
      :format-money="formatMoney"
    />
    <UiText v-if="data" tone="muted" size="sm">
      {{ t('reports.common.convertedToBaseCurrency', { currency }) }}
    </UiText>
  </ReportPage>
</template>
