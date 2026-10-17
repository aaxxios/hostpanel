<template>
  <UiSection v-if="features.length" padding="lg" class="border-b border-nova-border bg-nova-surface">
    <NovaSectionHeader
      :eyebrow="t('nova.performance.eyebrow')"
      :title="t('nova.performance.title')"
      :lead="t('nova.performance.lead')"
    />

    <UiGrid as="ul" cols="3" gap="lg" class="mt-10">
      <NovaFeatureCard v-for="feature in features" :key="feature.key" :feature="feature" />
    </UiGrid>
  </UiSection>
</template>

<script setup lang="ts">
/**
 * nova performance section.
 *
 * Storage medium, caching and a CDN are properties of the operator's machines,
 * not of this application — it provisions through cPanel/WHM or CWP and never
 * learns what is underneath — so those three wait on a setting. What remains is
 * true of every install, and the section hides itself if even that is empty.
 */
import NovaFeatureCard from '~/templates/nova/sections/FeatureCard.vue'
import NovaSectionHeader from '~/templates/nova/sections/NovaSectionHeader.vue'
import { PERFORMANCE_FEATURES } from '~/templates/nova/content'
import { useVisibleFeatures } from '~/templates/nova/features'

/** Translator for the copy. */
const { t } = useI18n()
/** The performance claims this install may state. */
const features = useVisibleFeatures(PERFORMANCE_FEATURES)
</script>
