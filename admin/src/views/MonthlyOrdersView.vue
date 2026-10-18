<script setup lang="ts">
/**
 * Monthly orders report: units sold and their value per product for a month, grouped by product
 * group, shown in the chosen currency.
 */
import { computed, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import ReportPage from '@/components/reports/ReportPage.vue'
import ReportPeriodFilter from '@/components/reports/ReportPeriodFilter.vue'
import ReportProductSalesTable from '@/components/reports/ReportProductSalesTable.vue'
import { useReport } from '@/composables/useReport'
import { formatReportAmount, ReportCurrency } from '@/lib/money'
import { currentMonthNumber, currentYear, monthName } from '@/lib/reportsPeriod'
import { reportPath } from '@/lib/reportsQuery'
import type { ReportMonthlyOrders } from '@/types/reportproductsales'

const { t } = useI18n()

/** Year being reported; the current one at first. */
const year = ref(currentYear())

/** Month being reported, 1 to 12; the current one at first. */
const month = ref(currentMonthNumber())

/** Currency the figures are shown in. */
const currency = ref<ReportCurrency>(ReportCurrency.Usd)

const { data, load } = useReport<ReportMonthlyOrders>(() =>
  reportPath('/reports/monthly-orders', { year: year.value, month: month.value }),
)

/** Title naming the month once a report is loaded. */
const title = computed<string>(() =>
  data.value
    ? t('reports.titles.salesByProductFor', { month: monthName(data.value.month), year: data.value.year })
    : t('reports.titles.reports.monthlyOrders'),
)

/**
 * Formats a figure in the chosen currency, with its code.
 *
 * @param amount - Figure in USD.
 * @returns The restated, formatted figure.
 */
const formatMoney = (amount: number): string => formatReportAmount(amount, currency.value, true)
</script>

<template>
  <ReportPage :title="title" :description="t('reports.monthlyOrders.thisReportGivesABreakdown')">
    <template #filters>
      <ReportPeriodFilter v-model:year="year" v-model:month="month" v-model:currency="currency" show-month show-currency :years-back="2" :years-ahead="2" @generate="load" />
    </template>
    <ReportProductSalesTable
      v-if="data"
      :groups="data.groups"
      :total-units-sold="data.totalUnitsSold"
      :total-value="data.totalValue"
      :format-money="formatMoney"
    />
  </ReportPage>
</template>
