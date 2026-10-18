<script setup lang="ts">
/**
 * Product suspensions report: every suspended service with the reason recorded for it.
 */
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import ReportPage from '@/components/reports/ReportPage.vue'
import ReportTable from '@/components/reports/ReportTable.vue'
import { useReport } from '@/composables/useReport'
import type { ReportColumn } from '@/types/reportcolumn'
import type { ReportSuspensionRow } from '@/types/reportsuspension'

const { t } = useI18n()

const { data } = useReport<ReportSuspensionRow[]>(() => '/reports/product-suspensions')

/** Table columns. */
const columns = computed<ReportColumn[]>(() => [
  { key: 'serviceId', label: t('reports.productSuspensions.serviceId') },
  { key: 'clientName', label: t('shared.clientName') },
  { key: 'productName', label: t('shared.productName') },
  { key: 'domain', label: t('shared.domain') },
  { key: 'nextDueDate', label: t('shared.nextDueDate') },
  { key: 'suspendReason', label: t('reports.productSuspensions.suspendReason') },
])
</script>

<template>
  <ReportPage :title="t('reports.productSuspensions.productSuspensions')" :description="t('reports.productSuspensions.thisReportAllowsYouTo')">
    <ReportTable
      v-if="data"
      :columns="columns"
      :rows="data"
      row-key="serviceId"
      :empty-text="t('shared.noDataFoundForThis')"
    />
  </ReportPage>
</template>
