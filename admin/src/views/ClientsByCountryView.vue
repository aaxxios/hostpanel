<script setup lang="ts">
/**
 * Clients by country report: a world map of client cities, the top countries as a bar chart, and
 * the full country table. Clicking a city marks its country in the chart.
 */
import { computed, onMounted, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import ReportChartCard from '@/components/reports/ReportChartCard.vue'
import ReportPage from '@/components/reports/ReportPage.vue'
import ReportTable from '@/components/reports/ReportTable.vue'
import WorldMap from '@/components/reports/WorldMap.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiText from '@/components/ui/UiText.vue'
import { UNKNOWN_COUNTRY, reportCountryName } from '@/lib/reportsCountry'
import { useReportsStore } from '@/stores/reports'
import type { ReportColumn } from '@/types/reportcolumn'
import type { ReportCityRow, ReportCountryRow } from '@/types/reportcountry'
import { UiChartType } from '@/utils/ui/chart'
import type { UiChartSeries } from '@/utils/ui/chart'
import { UiAlign } from '@/utils/ui/table'

const { t, locale } = useI18n()

const store = useReportsStore()

/**
 * Names a country for display; an unresolved one (null) reads "Unknown".
 *
 * @param code - ISO-2 code, or null when the API could not resolve it.
 * @returns The localised country name.
 */
const countryName = (code: string | null): string => {
  return code === null || code === UNKNOWN_COUNTRY ? t('shared.unknown') : reportCountryName(code, locale.value)
}

/** Active clients per country. */
const countries = ref<ReportCountryRow[]>([])

/** Active clients per city, for the map. */
const cities = ref<ReportCityRow[]>([])

/** Country chosen on the map, if any. */
const selectedCountry = ref<string | null>(null)

/** Number of countries the bar chart shows. */
const TOP_COUNT = 10

/** Total clients across all countries. */
const totalClients = computed<number>(() => countries.value.reduce((sum, row) => sum + row.clientCount, 0))

/** The busiest countries, with the chosen one moved to the front. */
const top = computed<ReportCountryRow[]>(() => {
  const sorted = [...countries.value].sort((a, b) => b.clientCount - a.clientCount).slice(0, TOP_COUNT)
  const chosen = countries.value.find(row => row.country === selectedCountry.value)
  if (!chosen) {
    return sorted
  }
  return [chosen, ...sorted.filter(row => row.country !== chosen.country)]
})

/** Country rows with their display name. */
const rows = computed(() =>
  countries.value.map(row => ({ country: countryName(row.country), code: row.country, clientCount: row.clientCount })),
)

/** Table columns. */
const columns = computed<ReportColumn[]>(() => [
  { key: 'country', label: t('shared.country') },
  { key: 'clientCount', label: t('reports.clientsByCountry.activeClients'), align: UiAlign.Right },
])

/** Bar chart labels. */
const labels = computed<string[]>(() => top.value.map(row => countryName(row.country)))

/** Bar chart series. */
const series = computed<UiChartSeries[]>(() => [
  { label: t('reports.clientsByCountry.activeClients'), data: top.value.map(row => row.clientCount) },
])

/**
 * Marks a country, or clears the mark when it is already chosen.
 *
 * @param country - ISO-2 code of the clicked country.
 */
const toggleCountry = (country: string): void => {
  selectedCountry.value = selectedCountry.value === country ? null : country
}

/** Drops the marked country. */
const clearSelection = (): void => {
  selectedCountry.value = null
}

onMounted(async () => {
  const result = await store.fetchReports<[ReportCountryRow[], ReportCityRow[]]>([
    '/reports/clients-by-country',
    '/reports/clients-by-city',
  ])
  if (result !== null) {
    countries.value = result[0]
    cities.value = result[1]
  }
})
</script>

<template>
  <ReportPage :title="t('reports.clientsByCountry.clientsByCountry')" :description="t('reports.clientsByCountry.thisReportShowsTheTotal')">
    <WorldMap v-if="cities.length > 0" :city-data="cities" @select-country="toggleCountry" />

    <UiStack gap="sm">
      <UiStack direction="row" justify="between" align="center">
        <UiText tone="secondary" size="sm">
          {{ t('reports.clientsByCountry.top10Countries') }}
        </UiText>
        <UiButton v-if="selectedCountry" variant="link" data-testid="report-clear-country" @click="clearSelection">
          {{ t('reports.clientsByCountry.clearSelection') }}
        </UiButton>
      </UiStack>
      <ReportChartCard :type="UiChartType.Bar" :labels="labels" :series="series" horizontal height="lg" />
    </UiStack>

    <ReportTable :columns="columns" :rows="rows" row-key="code" />

    <UiText tone="secondary" size="md" data-testid="report-total-clients">
      {{ t('reports.clientsByCountry.totalClients') }} {{ totalClients }}
    </UiText>
  </ReportPage>
</template>
