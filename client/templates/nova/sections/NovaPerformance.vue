<template>
  <UiSection v-if="stats.length || features.length" padding="lg" class="border-b border-nova-border bg-nova-surface" data-testid="nova-performance">
    <NovaSectionHeader
      :eyebrow="t('nova.performance.eyebrow')"
      :title="t('nova.performance.title')"
      :lead="t('nova.performance.lead')"
    />

    <UiBalancedGrid v-if="stats.length" as="ul" :count="stats.length" class="mt-10" data-testid="nova-performance-stats">
      <li v-for="stat in stats" :key="stat.key" :data-testid="`nova-performance-stat-${stat.key}`">
        <UiCard padding="lg" class="h-full text-center !border-nova-border !bg-nova-bg">
          <span class="mx-auto grid h-11 w-11 place-items-center rounded-xl bg-nova-surface-2 text-nova-brand">
            <Icon :name="PERFORMANCE_STAT_CARDS[stat.key].icon" class="h-[22px] w-[22px]" aria-hidden="true" />
          </span>
          <UiText as="p" size="inherit" tone="inherit" class="mt-4 text-[28px] font-extrabold tracking-tight text-nova-ink">
            {{ stat.value }}
          </UiText>
          <UiText as="p" size="inherit" tone="inherit" class="mt-1 text-[14px] text-nova-muted">
            {{ t(PERFORMANCE_STAT_CARDS[stat.key].titleKey) }}
          </UiText>
        </UiCard>
      </li>
    </UiBalancedGrid>

    <UiGrid v-if="features.length" as="ul" cols="3" gap="lg" class="mt-10">
      <NovaFeatureCard v-for="feature in features" :key="feature.key" :feature="feature" />
    </UiGrid>
  </UiSection>
</template>

<script setup lang="ts">
/**
 * nova performance section.
 *
 * Opens with the operator's four published figures (uptime, response time, sites hosted, support
 * response) from `usePerformanceStats()`, each set value as a tile. Beneath them sit the capability
 * cards: storage medium, caching and a CDN are properties of the operator's machines, not of this
 * application, so those three wait on a setting; what remains is true of every install. The
 * section hides itself only when there are neither figures nor cards.
 */
import NovaFeatureCard from '~/templates/nova/sections/FeatureCard.vue'
import NovaSectionHeader from '~/templates/nova/sections/NovaSectionHeader.vue'
import { PERFORMANCE_FEATURES, PERFORMANCE_STAT_CARDS } from '~/templates/nova/content'
import { useVisibleFeatures } from '~/templates/nova/features'

/** Translator for the copy. */
const { t } = useI18n()
/** The performance figures the operator has filled in. */
const stats = usePerformanceStats()
/** The performance claims this install may state. */
const features = useVisibleFeatures(PERFORMANCE_FEATURES)
</script>
