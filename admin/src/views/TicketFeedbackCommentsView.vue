<script setup lang="ts">
/**
 * Ticket feedback comments report: the comments customers left on support tickets, with the
 * rating, filtered by staff member and date range.
 */
import { computed, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import ReportField from '@/components/reports/ReportField.vue'
import ReportFilterBar from '@/components/reports/ReportFilterBar.vue'
import ReportPage from '@/components/reports/ReportPage.vue'
import ReportTable from '@/components/reports/ReportTable.vue'
import UiBadge from '@/components/ui/UiBadge.vue'
import UiDateRangePicker from '@/components/ui/UiDateRangePicker.vue'
import UiInput from '@/components/ui/UiInput.vue'
import { useReport } from '@/composables/useReport'
import { formatDate } from '@/lib/format'
import { ratingTone } from '@/lib/reportsRating'
import { reportPath } from '@/lib/reportsQuery'
import { currentYearRange } from '@/lib/reportsPeriod'
import type { ReportColumn } from '@/types/reportcolumn'
import type { ReportFeedbackComments } from '@/types/reportfeedback'
import { UiAlign } from '@/utils/ui/table'

const { t } = useI18n()

/** Staff member to look for; empty for any. */
const staffName = ref('')

/** The period to report; the current year at first. */
const dateRange = ref<[string, string] | null>(currentYearRange())

const { data, load } = useReport<ReportFeedbackComments>(() =>
  reportPath('/reports/ticket-feedback-comments', {
    staffName: staffName.value.trim(),
    from: dateRange.value?.[0],
    to: dateRange.value?.[1],
  }),
)

/** Table columns. */
const columns = computed<ReportColumn[]>(() => [
  { key: 'ticketId', label: t('reports.ticketFeedbackComments.ticketId') },
  { key: 'staffName', label: t('reports.ticketFeedbackComments.staffName') },
  { key: 'subject', label: t('shared.subject') },
  { key: 'feedbackLeftBy', label: t('reports.ticketFeedbackComments.feedbackLeft') },
  { key: 'rating', label: t('reports.ticketFeedbackComments.rating'), align: UiAlign.Center },
  { key: 'feedbackComment', label: t('reports.ticketFeedbackComments.comments') },
])
</script>

<template>
  <ReportPage
    :title="t('reports.ticketFeedbackComments.ticketFeedbackComments')"
    :description="t('reports.ticketFeedbackComments.thisReportAllowsYouTo')"
  >
    <template #filters>
      <ReportFilterBar :generate-label="t('shared.generateReport')" @generate="load">
        <template #fields>
          <ReportField :label="t('reports.ticketFeedbackComments.staffName')">
            <UiInput v-model="staffName" :placeholder="t('shared.any')" data-testid="report-staff-name" />
          </ReportField>
          <ReportField :label="t('shared.dateRange')" width="grow">
            <UiDateRangePicker v-model="dateRange" :placeholder="t('shared.selectDateRange')" />
          </ReportField>
        </template>
      </ReportFilterBar>
    </template>

    <ReportTable
      v-if="data"
      :columns="columns"
      :rows="data.rows"
      row-key="ticketId"
      :empty-text="t('shared.noDataFoundForThis2')"
    >
      <template #cell-feedbackLeftBy="{ row }">
        {{ row.feedbackLeftBy }} · {{ formatDate(row.feedbackAt) }}
      </template>
      <template #cell-rating="{ row }">
        <UiBadge :tone="ratingTone(row.rating)" small>
          {{ row.rating }}
        </UiBadge>
      </template>
    </ReportTable>
  </ReportPage>
</template>
