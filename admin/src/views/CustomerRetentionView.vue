<script setup lang="ts">
/**
 * Customer retention report: how long services of each product stay active, grouped by product
 * group, optionally counting active services as ending on their next due date.
 */
import { computed, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import ReportGroupRow from '@/components/reports/ReportGroupRow.vue'
import ReportPage from '@/components/reports/ReportPage.vue'
import ReportTable from '@/components/reports/ReportTable.vue'
import UiCheckbox from '@/components/ui/UiCheckbox.vue'
import UiLabel from '@/components/ui/UiLabel.vue'
import UiTableCell from '@/components/ui/UiTableCell.vue'
import UiTableRow from '@/components/ui/UiTableRow.vue'
import { useReport } from '@/composables/useReport'
import { reportPath } from '@/lib/reportsQuery'
import type { ReportColumn } from '@/types/reportcolumn'
import type { ReportRetention } from '@/types/reportretention'
import { UiAlign } from '@/utils/ui/table'

const { t } = useI18n()

/** Whether active services count, as ending on their next due date. */
const includeActive = ref(true)

const { data, load } = useReport<ReportRetention>(() =>
  reportPath('/reports/customer-retention', { includeActive: includeActive.value }),
)

// Ticking the box reloads at once.
watch(includeActive, load)

/** Table columns. */
const columns = computed<ReportColumn[]>(() => [
  { key: 'productName', label: t('shared.productName') },
  { key: 'billingCycle', label: t('shared.billingCycle') },
  { key: 'productCount', label: t('reports.customerRetention.productCount'), align: UiAlign.Center },
  { key: 'avgDaysActive', label: t('reports.customerRetention.averageDaysActive'), align: UiAlign.Center },
  { key: 'avgYearsMonthsActive', label: t('reports.customerRetention.averageYearsMonthsActive') },
])
</script>

<template>
  <ReportPage
    :title="t('reports.customerRetention.averageCustomerRetentionTime')"
    :description="t('reports.customerRetention.thisReportCalculatesAndProvides')"
  >
    <template v-if="data">
      <UiLabel variant="inline">
        <UiCheckbox v-model="includeActive" data-testid="report-include-active" />
        {{ t('reports.customerRetention.includeActiveProductsServicesAssuming') }}
      </UiLabel>
      <ReportTable :columns="columns" :empty="data.groups.length === 0" :empty-text="t('reports.customerRetention.noDataFound')">
        <template #body>
          <template v-for="group in data.groups" :key="group.groupName">
            <ReportGroupRow :label="group.groupName" :colspan="columns.length" />
            <UiTableRow v-for="row in group.rows" :key="row.productName + row.billingCycle" hoverable>
              <UiTableCell>{{ row.productName }}</UiTableCell>
              <UiTableCell>{{ row.billingCycle }}</UiTableCell>
              <UiTableCell :align="UiAlign.Center">{{ row.productCount }}</UiTableCell>
              <UiTableCell :align="UiAlign.Center">{{ row.avgDaysActive }}</UiTableCell>
              <UiTableCell>{{ row.avgYearsMonthsActive }}</UiTableCell>
            </UiTableRow>
          </template>
        </template>
      </ReportTable>
    </template>
  </ReportPage>
</template>
