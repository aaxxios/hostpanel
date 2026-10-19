<template>
  <UiGrid cols="4" gap="md" data-testid="dashboard-stats">
    <NuxtLink
      v-for="stat in stats"
      :key="stat.key"
      :to="stat.to"
      class="block"
      :data-testid="`dashboard-stat-${stat.key}`"
    >
      <UiStatTile
        :label="stat.label"
        :value="loading ? '—' : stat.value"
        :icon="stat.icon"
        :tone="stat.tone"
        :hint="stat.hint"
        class="h-full"
      />
    </NuxtLink>
  </UiGrid>
</template>

<script setup lang="ts">
/**
 * The four counters at the top of the dashboard — active services, domains, unpaid invoices
 * and open tickets — each a tile that opens the matching section.
 */
import { FileText, Globe, MessageSquare, Server } from 'lucide-vue-next'
import type { Component } from 'vue'
import { CLIENT_DOMAINS_PATH, CLIENT_INVOICES_PATH, CLIENT_SERVICES_PATH, CLIENT_TICKETS_PATH } from '~/constants/clientRoutes'
import { useClientStore } from '~/stores/client'
import { DashboardStat } from '~/types/dashboardstat'
import { UiTone } from '~/utils/ui/enums'

/** What one tile shows. */
interface StatTile {
  /** Which counter this is. */
  key: DashboardStat
  /** Translated caption. */
  label: string
  /** The count. */
  value: number
  /** Lucide icon component. */
  icon: Component
  /** Tone of the icon. */
  tone: UiTone
  /** Line under the count, when the counter needs attention. */
  hint?: string
  /** Section the tile opens. */
  to: string
}

/** Props of ClientDashboardStatGrid. */
defineProps<{
  /** True while any section the counters read is still loading. */
  loading: boolean
}>()

/** Translator for the strings this file builds itself. */
const { t } = useI18n()
/** Client store: the signed-in person and their services, invoices, domains and tickets. */
const store = useClientStore()

/** The four tiles, in display order. */
const stats = computed<StatTile[]>(() => [
  {
    key: DashboardStat.Services,
    label: t('client.dashboard.statActiveServices'),
    value: store.activeServiceCount,
    icon: Server,
    tone: UiTone.Info,
    to: CLIENT_SERVICES_PATH
  },
  {
    key: DashboardStat.Domains,
    label: t('client.dashboard.statDomains'),
    value: store.domains.length,
    icon: Globe,
    tone: UiTone.Primary,
    to: CLIENT_DOMAINS_PATH
  },
  {
    key: DashboardStat.Invoices,
    label: t('client.dashboard.statUnpaidInvoices'),
    value: store.unpaidCount,
    icon: FileText,
    tone: store.unpaidCount > 0 ? UiTone.Warning : UiTone.Default,
    hint: store.unpaidCount > 0 ? t('client.dashboard.statActionRequired') : undefined,
    to: CLIENT_INVOICES_PATH
  },
  {
    key: DashboardStat.Tickets,
    label: t('client.dashboard.statOpenTickets'),
    value: store.openTicketCount,
    icon: MessageSquare,
    tone: UiTone.Secondary,
    to: CLIENT_TICKETS_PATH
  }
])
</script>
