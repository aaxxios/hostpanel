<script setup lang="ts">
/**
 * Ticket ratings reviewer: the rated ticket replies of a period, optionally only those rated at or
 * above a chosen score.
 */
import { computed, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import ReportField from '@/components/reports/ReportField.vue'
import ReportFilterBar from '@/components/reports/ReportFilterBar.vue'
import ReportPage from '@/components/reports/ReportPage.vue'
import ReportTable from '@/components/reports/ReportTable.vue'
import UiBadge from '@/components/ui/UiBadge.vue'
import UiDateRangePicker from '@/components/ui/UiDateRangePicker.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import { useReport } from '@/composables/useReport'
import { ratingTone } from '@/lib/reportsRating'
import { reportPath } from '@/lib/reportsQuery'
import { currentYearRange } from '@/lib/reportsPeriod'
import { ReportCellFormat } from '@/types/reportcolumn'
import type { ReportColumn } from '@/types/reportcolumn'
import type { ReportRatings } from '@/types/reportrating'
import { UiAlign } from '@/utils/ui/table'

const { t } = useI18n()

/** The period to report; the current year at first. */
const dateRange = ref<[string, string] | null>(currentYearRange())

/** Lowest rating to list, as text; empty for any. */
const minRating = ref('')

/** Rating choices: any, then every second score from 1. */
const ratingOptions = computed(() => [
  { value: '', label: t('shared.any') },
  ...[1, 3, 5, 7, 9].map(rating => ({ value: String(rating), label: t('reports.ticketRatingsReviewer.ratingAtLeast', { rating }) })),
])

const { data, load } = useReport<ReportRatings>(() =>
  reportPath('/reports/ticket-ratings-reviewer', {
    minRating: minRating.value,
    from: dateRange.value?.[0],
    to: dateRange.value?.[1],
  }),
)

/** Table columns. */
const columns = computed<ReportColumn[]>(() => [
  { key: 'ticketId', label: t('reports.ticketRatingsReviewer.ticket'), format: ReportCellFormat.Id },
  { key: 'date', label: t('shared.date'), format: ReportCellFormat.Date },
  { key: 'message', label: t('shared.message') },
  { key: 'adminName', label: t('reports.ticketRatingsReviewer.admin') },
  { key: 'rating', label: t('reports.ticketRatingsReviewer.rating'), align: UiAlign.Center },
])
</script>

<template>
  <ReportPage
    :title="t('reports.ticketRatingsReviewer.supportTicketRatingsReviewer')"
    :description="t('reports.ticketRatingsReviewer.thisReportIsShowingAll')"
  >
    <template #filters>
      <ReportFilterBar :generate-label="t('shared.generateReport')" @generate="load">
        <template #fields>
          <ReportField :label="t('reports.ticketRatingsReviewer.rating')" width="sm">
            <UiSelect v-model="minRating" :options="ratingOptions" :placeholder="t('shared.any')" data-testid="report-min-rating" />
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
      <template #cell-rating="{ row }">
        <UiBadge :tone="ratingTone(row.rating)" small>
          {{ row.rating }}
        </UiBadge>
      </template>
    </ReportTable>
  </ReportPage>
</template>
