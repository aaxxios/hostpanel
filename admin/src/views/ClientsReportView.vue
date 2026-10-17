<script setup lang="ts">
/**
 * Clients report: a custom export of clients by chosen columns, a creation-date range and up to
 * five filters.
 */
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import ReportQueryPage from '@/components/reports/ReportQueryPage.vue'
import type { FieldOption } from '@/components/ui/UiAdvancedFilters.vue'
import { useQueryReport } from '@/composables/useQueryReport'
import type { ReportClientRow } from '@/types/reportclientrow'
import { ReportCellFormat } from '@/types/reportcolumn'
import type { ReportColumn } from '@/types/reportcolumn'
import type { ReportQueryConfig, ReportQueryRange } from '@/types/reportquery'

const { t } = useI18n()

/** Every column the report can show. */
const columns = computed<ReportColumn[]>(() => [
  { key: 'id', label: t('shared.id'), format: ReportCellFormat.Id },
  { key: 'firstName', label: t('shared.firstName') },
  { key: 'lastName', label: t('shared.lastName') },
  { key: 'companyName', label: t('shared.companyName') },
  { key: 'email', label: t('shared.email') },
  { key: 'address1', label: t('shared.address1') },
  { key: 'address2', label: t('shared.address2') },
  { key: 'city', label: t('shared.city') },
  { key: 'state', label: t('shared.state') },
  { key: 'postCode', label: t('shared.postcode') },
  { key: 'country', label: t('shared.country') },
  { key: 'phone', label: t('shared.phoneNumber') },
  { key: 'currency', label: t('shared.currency') },
  { key: 'creditBalance', label: t('reports.directDebit.credit'), format: ReportCellFormat.Money },
  { key: 'status', label: t('shared.status'), format: ReportCellFormat.Status },
  { key: 'createdAt', label: t('reports.invoicesReport.creationDate') },
  { key: 'notes', label: t('shared.notes') },
])

/** Date-range filters. */
const ranges = computed<ReportQueryRange[]>(() => [
  { key: 'created', label: t('reports.clientsReport.creationDateRange'), param: 'created' },
])

/** Fields the advanced filters can target. */
const filterFields = computed<FieldOption[]>(() => [
  { value: 'firstName', label: t('shared.firstName') },
  { value: 'lastName', label: t('shared.lastName') },
  { value: 'companyName', label: t('shared.companyName') },
  { value: 'email', label: t('shared.email') },
  { value: 'country', label: t('shared.country') },
  { value: 'city', label: t('shared.city') },
  { value: 'phone', label: t('shared.phoneNumber') },
  { value: 'currency', label: t('shared.currency') },
  { value: 'status', label: t('shared.status') },
])

/** What the report is. */
const config: ReportQueryConfig = {
  path: '/reports/clients',
  serverFields: ['status', 'country'],
  pageSize: 50,
}

const report = useQueryReport<ReportClientRow>(config, { columns, ranges })
</script>

<template>
  <ReportQueryPage
    :title="t('shared.clients')"
    :description="t('reports.clientsReport.thisReportCanBeUsed')"
    :report="report"
    :columns="columns"
    :ranges="ranges"
    :filter-fields="filterFields"
    :found-text="t('reports.clientsReport.totalcountClientSFound', { totalCount: report.totalCount })"
    :empty-text="t('reports.clientsReport.noClientsFoundClickFilter')"
  />
</template>
