<script setup lang="ts">
/**
 * Ticket tags report: how many tickets carry each tag in a date range, as a bar chart and a table.
 */
import { computed, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import ReportChartCard from '@/components/reports/ReportChartCard.vue'
import ReportField from '@/components/reports/ReportField.vue'
import ReportFilterBar from '@/components/reports/ReportFilterBar.vue'
import ReportPage from '@/components/reports/ReportPage.vue'
import ReportTable from '@/components/reports/ReportTable.vue'
import UiDateRangePicker from '@/components/ui/UiDateRangePicker.vue'
import { useReport } from '@/composables/useReport'
import { reportPath } from '@/lib/reportsQuery'
import { currentYearRange } from '@/lib/reportsPeriod'
import type { ReportColumn } from '@/types/reportcolumn'
import type { ReportTags } from '@/types/reporttags'
import { UiChartType } from '@/utils/ui/chart'
import type { UiChartSeries } from '@/utils/ui/chart'
import { UiAlign } from '@/utils/ui/table'

const { t } = useI18n()

/** The period to report; the current year at first. */
const dateRange = ref<[string, string] | null>(currentYearRange())

const { data, load } = useReport<ReportTags>(() =>
  reportPath('/reports/ticket-tags', { from: dateRange.value?.[0], to: dateRange.value?.[1] }),
)

/** Table columns. */
const columns = computed<ReportColumn[]>(() => [
  { key: 'tag', label: t('reports.ticketTags.tag') },
  { key: 'count', label: t('reports.ticketTags.count'), align: UiAlign.Right },
])

/** Chart labels: the tags. */
const labels = computed<string[]>(() => (data.value?.rows ?? []).map(row => row.tag))

/** Chart series: the ticket count per tag. */
const series = computed<UiChartSeries[]>(() => [
  { label: t('reports.ticketTags.count'), data: (data.value?.rows ?? []).map(row => row.count) },
])
</script>

<template>
  <ReportPage :title="t('reports.ticketTags.ticketTagsOverview')" :description="t('reports.ticketTags.thisReportProvidesAnOverview')">
    <template #filters>
      <ReportFilterBar :generate-label="t('shared.generateReport')" @generate="load">
        <template #fields>
          <ReportField :label="t('shared.dateRange')" width="grow">
            <UiDateRangePicker v-model="dateRange" :placeholder="t('shared.selectDateRange')" />
          </ReportField>
        </template>
      </ReportFilterBar>
    </template>

    <template v-if="data">
      <ReportChartCard v-if="data.rows.length > 0" :type="UiChartType.Bar" :labels="labels" :series="series" horizontal />
      <ReportTable :columns="columns" :rows="data.rows" row-key="tag" :empty-text="t('shared.noDataFoundForThis2')" />
    </template>
  </ReportPage>
</template>
