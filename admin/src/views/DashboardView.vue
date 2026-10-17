<script setup lang="ts">
/**
 * Admin dashboard — a grid of KPI tiles with the platform's headline numbers.
 *
 * Reads `stores/dashboard`; a refusal shows the store's sentence in an alert.
 */
import { onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import UiAlert from '@/components/ui/UiAlert.vue'
import UiBadge from '@/components/ui/UiBadge.vue'
import UiGrid from '@/components/ui/UiGrid.vue'
import UiIcon from '@/components/ui/UiIcon.vue'
import UiPageHeader from '@/components/ui/UiPageHeader.vue'
import UiSkeleton from '@/components/ui/UiSkeleton.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiStatTile from '@/components/ui/UiStatTile.vue'
import UiText from '@/components/ui/UiText.vue'
import { Icon } from '@/lib/icons'
import { formatMoneyWhole } from '@/lib/money'
import { useDashboardStore } from '@/stores/dashboard'
import type { DashboardStats } from '@/types/dashboardstats'
import { UiTone } from '@/utils/ui/tone'

/** One KPI tile of the dashboard. */
interface StatTileConfig {
  /** Field of `DashboardStats` the tile shows. */
  key: keyof DashboardStats
  /** Locale key of the tile caption. */
  labelKey: string
  /** Colour of the number. */
  tone: UiTone
  /** Inner SVG markup of the tile icon. */
  icon: string
  /** Turns the raw number into the text shown on the tile. */
  format: (value: number) => string
}

const store = useDashboardStore()
const { t } = useI18n()

/**
 * Formats a count as plain text.
 *
 * @param value - The count.
 * @returns The count as a string.
 */
const formatCount = (value: number): string => value.toString()

/** The tiles, in display order. */
const statConfig: StatTileConfig[] = [
  { key: 'totalRevenue', labelKey: 'dashboard.stats.totalRevenue', tone: UiTone.Neutral, icon: Icon.Dollar, format: formatMoneyWhole },
  { key: 'monthlyRevenue', labelKey: 'dashboard.stats.monthlyRevenue', tone: UiTone.Neutral, icon: Icon.Calendar, format: formatMoneyWhole },
  { key: 'activeServices', labelKey: 'dashboard.stats.activeServices', tone: UiTone.Success, icon: Icon.CheckCircle, format: formatCount },
  { key: 'overdueInvoices', labelKey: 'dashboard.stats.overdueInvoices', tone: UiTone.Danger, icon: Icon.Alert, format: formatCount },
  { key: 'openTickets', labelKey: 'dashboard.stats.openTickets', tone: UiTone.Warning, icon: Icon.Chat, format: formatCount },
  { key: 'totalClients', labelKey: 'dashboard.stats.totalClients', tone: UiTone.Neutral, icon: Icon.Users, format: formatCount },
]

onMounted(store.fetchStats)
</script>

<template>
  <UiStack>
    <UiPageHeader
      :title="t('dashboard.title')"
      :subtitle="t('dashboard.subtitle')"
      data-testid="dashboard-header"
    >
      <template #actions>
        <UiBadge tone="success" dot data-testid="dashboard-live">
          {{ t('dashboard.live') }}
        </UiBadge>
      </template>
    </UiPageHeader>

    <UiStack v-if="store.loading" aria-busy="true" data-testid="dashboard-loading">
      <UiText tone="secondary" size="md">
        {{ t('dashboard.loadingMetrics') }}
      </UiText>
      <UiSkeleton :lines="3" />
    </UiStack>

    <UiAlert v-else-if="store.error" variant="danger" data-testid="dashboard-error">
      {{ store.error }}
    </UiAlert>

    <UiGrid v-else-if="store.stats" :columns="3" data-testid="dashboard-stats">
      <UiStatTile
        v-for="stat in statConfig"
        :key="stat.key"
        :label="t(stat.labelKey)"
        :value="stat.format(store.stats[stat.key])"
        :tone="stat.tone"
        :data-testid="`dashboard-stat-${stat.key}`"
      >
        <template #icon>
          <UiIcon :markup="stat.icon" />
        </template>
      </UiStatTile>
    </UiGrid>
  </UiStack>
</template>
