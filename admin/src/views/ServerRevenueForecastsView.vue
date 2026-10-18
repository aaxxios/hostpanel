<script setup lang="ts">
/**
 * Server revenue forecasts report: not built yet, so it shows the "coming soon" placeholder under its title.
 */
import { useI18n } from 'vue-i18n'
import ReportComingSoon from '@/components/reports/ReportComingSoon.vue'
import ReportPage from '@/components/reports/ReportPage.vue'

const { t } = useI18n()
</script>

<template>
  <ReportPage :title="t('reports.serverRevenueForecasts.serverRevenueForecasts')">
    <ReportComingSoon />
  </ReportPage>
</template>
