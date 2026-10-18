<script setup lang="ts">
/**
 * Credits reviewer: every credit issued to clients between two dates, optionally for one client
 * and within an amount range.
 */
import { computed, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import ReportField from '@/components/reports/ReportField.vue'
import ReportFilterBar from '@/components/reports/ReportFilterBar.vue'
import ReportPage from '@/components/reports/ReportPage.vue'
import ReportTable from '@/components/reports/ReportTable.vue'
import UiClientSelect from '@/components/ui/UiClientSelect.vue'
import UiDateRangePicker from '@/components/ui/UiDateRangePicker.vue'
import UiSpinner from '@/components/ui/UiSpinner.vue'
import UiText from '@/components/ui/UiText.vue'
import { useReport } from '@/composables/useReport'
import { useReportClients } from '@/composables/useReportClients'
import { reportPath } from '@/lib/reportsQuery'
import { formatUsd } from '@/lib/money'
import { currentYearRange } from '@/lib/reportsPeriod'
import { ReportCellFormat } from '@/types/reportcolumn'
import type { ReportColumn } from '@/types/reportcolumn'
import type { ReportCredits } from '@/types/reportcredit'
import { UiAlign } from '@/utils/ui/table'

const { t } = useI18n()

const { clients } = useReportClients()

/** The client chosen in the picker; empty for all clients. */
const selectedClient = ref<string | number | null>('')

/** The period to review; the current year at first. */
const dateRange = ref<[string, string] | null>(currentYearRange())

/** Smallest credit to list; 0 means no lower limit. */
const minAmount = ref(0)

/** Largest credit to list; 0 means no upper limit. */
const maxAmount = ref(0)

const { data, load } = useReport<ReportCredits>(() =>
  reportPath('/reports/credits-reviewer', {
    clientId: selectedClient.value,
    from: dateRange.value?.[0],
    to: dateRange.value?.[1],
    minAmount: minAmount.value > 0 ? minAmount.value : undefined,
    maxAmount: maxAmount.value > 0 ? maxAmount.value : undefined,
  }),
)

/** Table columns. */
const columns = computed<ReportColumn[]>(() => [
  { key: 'id', label: t('reports.creditsReviewer.creditId') },
  { key: 'clientId', label: t('reports.creditsReviewer.clientId') },
  { key: 'clientName', label: t('shared.clientName') },
  { key: 'date', label: t('shared.date'), align: UiAlign.Center },
  { key: 'description', label: t('shared.description') },
  { key: 'amount', label: t('shared.amount'), align: UiAlign.Right, format: ReportCellFormat.Money },
  { key: 'adminUser', label: t('reports.creditsReviewer.adminUser') },
])
</script>

<template>
  <ReportPage :title="t('reports.creditsReviewer.creditsReviewer')" :description="t('reports.creditsReviewer.thisReportAllowsYouTo')">
    <template #filters>
      <ReportFilterBar @generate="load">
        <template #fields>
          <ReportField :label="t('shared.client')" width="grow">
            <UiClientSelect v-model="selectedClient" :clients="clients" :placeholder="t('reports.creditsReviewer.allClients')" />
          </ReportField>
          <ReportField :label="t('shared.dateRange')" width="grow">
            <UiDateRangePicker v-model="dateRange" :placeholder="t('shared.selectDateRange')" />
          </ReportField>
          <ReportField :label="t('reports.creditsReviewer.minAmount')">
            <UiSpinner v-model="minAmount" :step="1" :min="0" :placeholder="t('shared.any')" />
          </ReportField>
          <ReportField :label="t('reports.creditsReviewer.maxAmount')">
            <UiSpinner v-model="maxAmount" :step="1" :min="0" :placeholder="t('shared.any')" />
          </ReportField>
        </template>
      </ReportFilterBar>
    </template>

    <template v-if="data">
      <UiText v-if="data.totalCount > 0" tone="secondary" size="md" data-testid="report-credits-summary">
        {{ data.totalCount }} {{ t('reports.creditsReviewer.creditsFoundTotal') }} {{ formatUsd(data.totalAmount) }}
      </UiText>
      <ReportTable
        :columns="columns"
        :rows="data.rows"
        row-key="id"
        :empty-text="t('reports.creditsReviewer.noCreditsFoundForThis')"
        :totals="{ amount: data.totalAmount }"
      />
    </template>
  </ReportPage>
</template>
