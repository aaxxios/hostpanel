<script setup lang="ts">
/**
 * Table of units sold and value per product, grouped by product group with a sub total per group
 * and a grand total. Shared by the income by product and monthly orders reports.
 */
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import ReportGroupRow from '@/components/reports/ReportGroupRow.vue'
import ReportTable from '@/components/reports/ReportTable.vue'
import UiTableCell from '@/components/ui/UiTableCell.vue'
import UiTableRow from '@/components/ui/UiTableRow.vue'
import { formatReportAmount } from '@/lib/money'
import { ReportCellFormat } from '@/types/reportcolumn'
import type { ReportColumn } from '@/types/reportcolumn'
import type { ReportSalesGroup } from '@/types/reportproductsales'
import { UiAlign } from '@/utils/ui/table'

const { t } = useI18n()

/** Props for ReportProductSalesTable. */
const props = withDefaults(defineProps<{
  /** Product groups with their products. */
  groups: ReportSalesGroup[]
  /** Units sold overall. */
  totalUnitsSold: number
  /** Value overall (in USD, restated by `formatMoney`). */
  totalValue: number
  /** Formats a value cell; US dollars when omitted. */
  formatMoney?: (amount: number) => string
}>(), {
  /**
   * Default money formatter: US dollars.
   *
   * @param amount - Value to format.
   * @returns The formatted amount.
   */
  formatMoney: (amount: number) => formatReportAmount(amount),
})

/** Table columns. */
const columns = computed<ReportColumn[]>(() => [
  { key: 'productName', label: t('shared.productName') },
  { key: 'unitsSold', label: t('reports.monthlyOrders.unitsSold'), align: UiAlign.Right },
  { key: 'value', label: t('shared.value'), align: UiAlign.Right, format: ReportCellFormat.Money },
])

/** The grand total row, by column key. */
const totals = computed<Record<string, number>>(() => ({ unitsSold: props.totalUnitsSold, value: props.totalValue }))
</script>

<template>
  <ReportTable
    :columns="columns"
    :empty="groups.length === 0"
    :totals="totals"
    :format-money="formatMoney"
    testid="report-sales-table"
  >
    <template #body>
      <template v-for="group in groups" :key="group.groupName">
        <ReportGroupRow :label="group.groupName" :colspan="columns.length" />
        <UiTableRow v-for="product in group.products" :key="product.productId" hoverable>
          <UiTableCell>{{ product.productName }}</UiTableCell>
          <UiTableCell :align="UiAlign.Right">{{ product.unitsSold }}</UiTableCell>
          <UiTableCell :align="UiAlign.Right">{{ formatMoney(product.value) }}</UiTableCell>
        </UiTableRow>
        <UiTableRow>
          <UiTableCell muted>{{ t('shared.subTotal2') }}</UiTableCell>
          <UiTableCell :align="UiAlign.Right" muted>{{ group.groupUnitsSold }}</UiTableCell>
          <UiTableCell :align="UiAlign.Right" muted>{{ formatMoney(group.groupValue) }}</UiTableCell>
        </UiTableRow>
      </template>
    </template>
  </ReportTable>
</template>
