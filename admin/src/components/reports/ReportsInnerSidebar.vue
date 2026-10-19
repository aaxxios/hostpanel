<script setup lang="ts">
/**
 * Inner sidebar of the reports area: one icon entry per report, the open one marked.
 */
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import UiSidebar from '@/components/ui/UiSidebar.vue'
import { REPORTS } from '@/lib/reports'
import { reportIcon } from '@/lib/icons'
import type { UiSidebarItem } from '@/utils/ui/sidebar'
import { AppRoute, reportPagePath } from '@/lib/routes'

const route = useRoute()

/** Entries of the sidebar; labels are read each time, so a language switch applies. */
const items = computed<UiSidebarItem[]>(() =>
  REPORTS.map(report => ({
    key: report.slug,
    label: report.label,
    to: reportPagePath(report.slug),
    icon: reportIcon(report.icon),
  })),
)

/** Slug of the report that is open, or undefined outside any report. */
const activeKey = computed<string | undefined>(() => {
  const prefix = `${AppRoute.Reports}/`
  return route.path.startsWith(prefix) ? route.path.slice(prefix.length) : undefined
})
</script>

<template>
  <UiSidebar :items="items" :active-key="activeKey" testid="reports-nav" />
</template>
