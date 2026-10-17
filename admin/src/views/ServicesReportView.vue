<script setup lang="ts">
/**
 * Services report: a custom export of client services by chosen columns, three date ranges and up
 * to five filters.
 */
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import ReportQueryPage from '@/components/reports/ReportQueryPage.vue'
import type { FieldOption } from '@/components/ui/UiAdvancedFilters.vue'
import { useQueryReport } from '@/composables/useQueryReport'
import { ReportCellFormat } from '@/types/reportcolumn'
import type { ReportColumn } from '@/types/reportcolumn'
import type { ReportQueryConfig, ReportQueryRange } from '@/types/reportquery'
import type { ReportServiceRow } from '@/types/reportservicerow'

const { t } = useI18n()

/** Every column the report can show. */
const columns = computed<ReportColumn[]>(() => [
  { key: 'id', label: t('shared.id'), format: ReportCellFormat.Id },
  { key: 'clientId', label: t('reports.servicesReport.userId') },
  { key: 'clientName', label: t('shared.clientName') },
  { key: 'productId', label: t('reports.servicesReport.productId') },
  { key: 'productName', label: t('orders.orderDetail.product') },
  { key: 'domain', label: t('shared.domain') },
  { key: 'billingCycle', label: t('shared.billingCycle') },
  { key: 'firstPaymentAmount', label: t('clients.clientDomainDetail.firstPaymentAmount'), format: ReportCellFormat.Money },
  { key: 'recurringAmount', label: t('clients.clientDomainDetail.recurringAmount'), format: ReportCellFormat.Money },
  { key: 'paymentMethod', label: t('shared.paymentMethod') },
  { key: 'registrationDate', label: t('shared.registrationDate') },
  { key: 'nextDueDate', label: t('shared.nextDueDate') },
  { key: 'terminatedAt', label: t('reports.servicesReport.terminatedAt') },
  { key: 'status', label: t('shared.status'), format: ReportCellFormat.Status },
  { key: 'notes', label: t('shared.notes') },
])

/** Date-range filters. There is no completed range: a service stores no completion date. */
const ranges = computed<ReportQueryRange[]>(() => [
  { key: 'created', label: t('reports.invoicesReport.creationDate'), param: 'created' },
  { key: 'nextDue', label: t('shared.nextDueDate'), param: 'nextDue' },
  { key: 'terminated', label: t('reports.servicesReport.terminationDate'), param: 'terminated' },
])

/** Fields the advanced filters can target. */
const filterFields = computed<FieldOption[]>(() => [
  { value: 'clientName', label: t('shared.clientName') },
  { value: 'productName', label: t('orders.orderDetail.product') },
  { value: 'domain', label: t('shared.domain') },
  { value: 'billingCycle', label: t('shared.billingCycle') },
  { value: 'paymentMethod', label: t('shared.paymentMethod') },
  { value: 'status', label: t('shared.status') },
])

/** What the report is. */
const config: ReportQueryConfig = {
  path: '/reports/services',
  serverFields: ['status', 'billingCycle'],
  pageSize: 50,
}

const report = useQueryReport<ReportServiceRow>(config, { columns, ranges })
</script>

<template>
  <ReportQueryPage
    :title="t('reports.servicesReport.services')"
    :description="t('reports.servicesReport.thisReportCanBeUsed')"
    :report="report"
    :columns="columns"
    :ranges="ranges"
    :filter-fields="filterFields"
    :found-text="t('reports.servicesReport.totalcountServiceSFound', { totalCount: report.totalCount })"
    :empty-text="t('reports.servicesReport.noServicesFound')"
  />
</template>
