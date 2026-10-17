<script setup lang="ts">
/**
 * Support ticket replies report: how many tickets each staff member replied to on every day of a
 * month, with row and day totals.
 */
import { computed, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import ReportField from '@/components/reports/ReportField.vue'
import ReportFilterBar from '@/components/reports/ReportFilterBar.vue'
import ReportPage from '@/components/reports/ReportPage.vue'
import ReportTable from '@/components/reports/ReportTable.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import { useReport } from '@/composables/useReport'
import { currentMonthNumber, currentYear, monthName, monthOptions, yearOptions } from '@/lib/reportsPeriod'
import { reportPath } from '@/lib/reportsQuery'
import type { ReportColumn } from '@/types/reportcolumn'
import type { ReportTicketReplies } from '@/types/reportticketreplies'
import { UiAlign } from '@/utils/ui/table'

const { t } = useI18n()

/** Year being reported; the current one at first. */
const year = ref(currentYear())

/** Month being reported, 1 to 12; the current one at first. */
const month = ref(currentMonthNumber())

const { data, load } = useReport<ReportTicketReplies>(() =>
  reportPath('/reports/support-ticket-replies', { year: year.value, month: month.value }),
)

/** Month dropdown choices. */
const months = monthOptions()

/** Year dropdown choices. */
const years = yearOptions(2, 2)

/** Title naming the month once a report is loaded. */
const title = computed<string>(() =>
  data.value
    ? t('reports.titles.supportTicketRepliesFor', { month: monthName(data.value.month), year: data.value.year })
    : t('reports.titles.reports.supportTicketReplies'),
)

/** Day numbers of the reported month. */
const days = computed<number[]>(() => Array.from({ length: data.value?.daysInMonth ?? 0 }, (_, index) => index + 1))

/** Table columns: the staff member, one per day, then the total. */
const columns = computed<ReportColumn[]>(() => [
  { key: 'adminName', label: t('reports.supportTicketsReport.admin') },
  ...days.value.map(day => ({ key: `d${day}`, label: String(day), align: UiAlign.Center })),
  { key: 'total', label: t('shared.total'), align: UiAlign.Right },
])

/** One row per staff member with a property per day. */
const rows = computed<Record<string, string | number>[]>(() =>
  (data.value?.rows ?? []).map(row => ({
    adminName: row.adminName,
    ...Object.fromEntries(days.value.map((day, index) => [`d${day}`, row.dailyCounts[index] ?? 0])),
    total: row.total,
  })),
)

/** Footer sums per day and overall; a day nobody replied on stays blank. */
const totals = computed<Record<string, string | number>>(() => ({
  ...Object.fromEntries(
    days.value.map((day, index) => {
      const sum = (data.value?.rows ?? []).reduce((acc, row) => acc + (row.dailyCounts[index] ?? 0), 0)
      return [`d${day}`, sum > 0 ? sum : '']
    }),
  ),
  total: (data.value?.rows ?? []).reduce((acc, row) => acc + row.total, 0),
}))
</script>

<template>
  <ReportPage :title="title" :description="t('reports.supportTicketsReport.thisReportShowsABreakdown')">
    <template #filters>
      <ReportFilterBar @generate="load">
        <template #fields>
          <ReportField :label="t('shared.month')">
            <UiSelect v-model="month" :options="months" :placeholder="t('shared.month')" data-testid="report-month" />
          </ReportField>
          <ReportField :label="t('shared.year')" width="sm">
            <UiSelect v-model="year" :options="years" :placeholder="t('shared.year')" data-testid="report-year" />
          </ReportField>
        </template>
      </ReportFilterBar>
    </template>

    <ReportTable
      v-if="data"
      :columns="columns"
      :rows="rows"
      row-key="adminName"
      :totals="totals"
      :empty-text="t('shared.noDataFoundForThis')"
    >
      <template v-for="day in days" #[`cell-d${day}`]="{ value }">
        {{ Number(value) > 0 ? value : '' }}
      </template>
    </ReportTable>
  </ReportPage>
</template>
