<script setup lang="ts">
/**
 * Placeholder shown for a report whose screen is not built yet.
 */
import { useI18n } from 'vue-i18n'
import UiCard from '@/components/ui/UiCard.vue'
import UiEmptyState from '@/components/ui/UiEmptyState.vue'
import UiIcon from '@/components/ui/UiIcon.vue'
import { FALLBACK_REPORT_ICON } from '@/lib/icons'

const { t } = useI18n()
</script>

<template>
  <UiCard data-testid="report-coming-soon">
    <UiEmptyState
      :title="t('reports.reportComingSoon.comingSoon')"
      :description="t('reports.reportComingSoon.thisReportIsUnderDevelopment')"
    >
      <template #icon>
        <UiIcon :markup="FALLBACK_REPORT_ICON" size="lg" />
      </template>
    </UiEmptyState>
  </UiCard>
</template>
