<script setup lang="ts">
/**
 * Domains report: a custom export of domains by chosen columns, three date ranges and up to five
 * filters.
 */
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import ReportQueryPage from '@/components/reports/ReportQueryPage.vue'
import { reportApiPath } from '@/lib/apiPaths'
import type { FieldOption } from '@/components/ui/UiAdvancedFilters.vue'
import { useQueryReport } from '@/composables/useQueryReport'
import { ReportCellFormat } from '@/types/reportcolumn'
import type { ReportColumn } from '@/types/reportcolumn'
import type { ReportDomainRow } from '@/types/reportdomainrow'
import type { ReportQueryConfig, ReportQueryRange } from '@/types/reportquery'
import { REPORT_PAGE_SIZE } from '@/lib/limits'

const { t } = useI18n()

/** Every column the report can show. */
const columns = computed<ReportColumn[]>(() => [
  { key: 'id', label: t('shared.id'), format: ReportCellFormat.Id },
  { key: 'clientId', label: t('reports.domainsReport.userId') },
  { key: 'clientName', label: t('shared.clientName') },
  { key: 'orderId', label: t('reports.domainsReport.orderId') },
  { key: 'orderType', label: t('reports.domainsReport.regType') },
  { key: 'domainName', label: t('shared.domainName') },
  { key: 'firstPaymentAmount', label: t('clients.clientDomainDetail.firstPaymentAmount'), format: ReportCellFormat.Money },
  { key: 'recurringAmount', label: t('clients.clientDomainDetail.recurringAmount'), format: ReportCellFormat.Money },
  { key: 'registrationPeriod', label: t('clients.clientDomainDetail.registrationPeriod'), format: ReportCellFormat.Years },
  { key: 'registrationDate', label: t('shared.registrationDate') },
  { key: 'expiryDate', label: t('shared.expiryDate') },
  { key: 'nextDueDate', label: t('shared.nextDueDate') },
  { key: 'registrar', label: t('shared.registrar') },
  { key: 'paymentMethod', label: t('shared.paymentMethod') },
  { key: 'status', label: t('shared.status'), format: ReportCellFormat.Status },
  { key: 'notes', label: t('shared.notes') },
])

/** Date-range filters. */
const ranges = computed<ReportQueryRange[]>(() => [
  { key: 'registered', label: t('reports.domainsReport.registrationDateRange'), param: 'registered' },
  { key: 'nextDue', label: t('reports.domainsReport.nextDueDateRange'), param: 'nextDue' },
  { key: 'expiry', label: t('reports.domainsReport.expiryDateRange'), param: 'expires' },
])

/** Fields the advanced filters can target. */
const filterFields = computed<FieldOption[]>(() => [
  { value: 'domainName', label: t('shared.domainName') },
  { value: 'clientName', label: t('shared.clientName') },
  { value: 'registrar', label: t('shared.registrar') },
  { value: 'paymentMethod', label: t('shared.paymentMethod') },
  { value: 'status', label: t('shared.status') },
  { value: 'orderType', label: t('reports.domainsReport.regType') },
])

/** What the report is. */
const config: ReportQueryConfig = {
  path: reportApiPath('domains'),
  serverFields: ['status', 'registrar'],
  pageSize: REPORT_PAGE_SIZE,
}

const report = useQueryReport<ReportDomainRow>(config, { columns, ranges })
</script>

<template>
  <ReportQueryPage
    :title="t('shared.domains')"
    :description="t('reports.domainsReport.thisReportCanBeUsed')"
    :report="report"
    :columns="columns"
    :ranges="ranges"
    :filter-fields="filterFields"
    :found-text="t('reports.domainsReport.totalcountDomainSFound', { totalCount: report.totalCount })"
    :empty-text="t('reports.domainsReport.noDomainsFound')"
  />
</template>
