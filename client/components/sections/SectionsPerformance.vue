<template>
  <section v-if="stats.length" class="py-12 md:py-20 bg-ui-page" data-testid="sections-performance">
    <UiContainer>
      <MarketingSectionHeader
        class="mb-12"
        :badge="$t('hostingSections.performance.badge')"
        :title="$t('hostingSections.performance.title')"
        :subtitle="$t('hostingSections.performance.subtitle')"
      />

      <UiBalancedGrid :count="stats.length" data-testid="sections-performance-grid">
        <UiCard
          v-for="stat in stats"
          :key="stat.key"
          hover
          class="text-center"
          :data-testid="`performance-${stat.key}`"
        >
          <UiStack align="center" gap="sm">
            <UiAccentTile :icon="STAT_ICONS[stat.key]" :class="ACCENT_BRAND_CLASS" />
            <UiText as="p" tone="inherit" weight="bold" class="text-3xl md:text-4xl text-gray-900 dark:text-white" :data-testid="`performance-value-${stat.key}`">{{ stat.value }}</UiText>
            <UiText tone="muted" size="sm">{{ $t(`hostingSections.performance.stats.${stat.key}`) }}</UiText>
          </UiStack>
        </UiCard>
      </UiBalancedGrid>
    </UiContainer>
  </section>
</template>

<script setup lang="ts">
/**
 * Performance section: the figures the operator publishes (uptime, response time, sites hosted,
 * support response) as large numbers. Only set figures show; with none the section renders nothing.
 */
import { PerformanceStatKey } from '~/constants/performanceStatKey'
import { ACCENT_BRAND_CLASS } from '~/utils/ui/accent'

/** The published figures, in display order. */
const stats = usePerformanceStats()

/** Icon of each figure. */
const STAT_ICONS: Record<PerformanceStatKey, string> = {
  [PerformanceStatKey.Uptime]: 'mdi:server-network',
  [PerformanceStatKey.ResponseTime]: 'mdi:speedometer',
  [PerformanceStatKey.SitesHosted]: 'mdi:web',
  [PerformanceStatKey.SupportResponse]: 'mdi:timer-outline',
}
</script>
