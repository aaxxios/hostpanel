<script setup lang="ts">
/**
 * Direct debit report: every unpaid invoice assigned to the direct debit payment method, with the
 * client bank details.
 */
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import ReportPage from '@/components/reports/ReportPage.vue'
import ReportTable from '@/components/reports/ReportTable.vue'
import { useReport } from '@/composables/useReport'
import { ReportCellFormat } from '@/types/reportcolumn'
import type { ReportColumn } from '@/types/reportcolumn'
import type { ReportDirectDebit } from '@/types/reportdirectdebit'
import { UiAlign } from '@/utils/ui/table'

const { t } = useI18n()

const { data, load } = useReport<ReportDirectDebit>(() => '/reports/direct-debit')

/** Table columns. */
const columns = computed<ReportColumn[]>(() => [
  { key: 'invoiceId', label: t('shared.invoiceId') },
  { key: 'clientName', label: t('shared.clientName') },
  { key: 'invoiceDate', label: t('shared.invoiceDate'), align: UiAlign.Center },
  { key: 'dueDate', label: t('shared.dueDate'), align: UiAlign.Center },
  { key: 'subtotal', label: t('reports.directDebit.subtotal'), align: UiAlign.Right, format: ReportCellFormat.Money },
  { key: 'tax', label: t('reports.directDebit.tax'), align: UiAlign.Right, format: ReportCellFormat.Money },
  { key: 'credit', label: t('reports.directDebit.credit'), align: UiAlign.Right, format: ReportCellFormat.Money },
  { key: 'total', label: t('shared.total'), align: UiAlign.Right, format: ReportCellFormat.Money },
  { key: 'bankName', label: t('reports.directDebit.bankName') },
  { key: 'bankAccountType', label: t('reports.directDebit.bankAccountType') },
  { key: 'bankCode', label: t('reports.directDebit.bankCode') },
  { key: 'bankAccountNumber', label: t('reports.directDebit.bankAccountNumber') },
])
</script>

<template>
  <ReportPage :retry="load" :title="t('reports.directDebit.directDebitProcessing')" :description="t('reports.directDebit.thisReportDisplaysAllUnpaid')">
    <ReportTable
      v-if="data"
      :columns="columns"
      :rows="data.rows"
      row-key="invoiceId"
      :empty-text="t('shared.noDataFoundForThis')"
    />
  </ReportPage>
</template>
