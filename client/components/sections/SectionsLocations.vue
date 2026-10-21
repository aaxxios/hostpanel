<template>
  <section v-if="locations.length" class="py-12 md:py-20 bg-ui-page" data-testid="sections-locations">
    <UiContainer>
      <MarketingSectionHeader
        class="mb-12"
        :badge="$t('hostingSections.locations.badge')"
        :title="$t('hostingSections.locations.title')"
        :subtitle="$t('hostingSections.locations.subtitle')"
      />

      <UiBalancedGrid :count="locations.length" :class="gridWidth" data-testid="sections-locations-grid">
        <UiCard
          v-for="location in locations"
          :key="location.id"
          hover
          :data-testid="`location-${location.id}`"
        >
          <UiStack direction="row" align="center" gap="md">
            <UiBadge variant="primary" size="lg" class="font-mono tracking-wider flex-shrink-0" :data-testid="`location-code-${location.id}`">{{ location.countryCode }}</UiBadge>
            <div class="min-w-0">
              <UiHeading :level="3" size="subsection" class="truncate">{{ location.name }}</UiHeading>
              <UiText v-if="location.city" tone="muted" size="sm" class="truncate" :data-testid="`location-city-${location.id}`">{{ location.city }}</UiText>
              <UiText v-if="location.latencyNote" tone="subtle" size="sm" :data-testid="`location-latency-${location.id}`">{{ location.latencyNote }}</UiText>
            </div>
          </UiStack>
        </UiCard>
      </UiBalancedGrid>
    </UiContainer>
  </section>
</template>

<script setup lang="ts">
/**
 * Server locations section: one card per data-centre location with its country as a code badge
 * (no flag emoji, no map), the name, the city and a latency note. The locations come from the
 * storefront store (`GET /api/portal/public/locations`, localised by the API); with none the
 * section renders nothing.
 */
import { useStorefrontStore } from '~/stores/storefront'

/** The active language; the locations are re-read when it changes. */
const { locale } = useI18n()

/** The storefront content, including the server locations. */
const storefront = useStorefrontStore()

// Not awaited: Nuxt still holds the server render for it, and the section stays synchronous.
useAsyncData('storefront:locations', () => storefront.loadLocations(), { watch: [locale] })

/** The locations to show, in display order. */
const locations = computed(() => storefront.locations)

/** Width caps by card count, so a lone card does not stretch across the whole page. */
const GRID_WIDTH_BY_COUNT: Record<number, string> = { 1: 'mx-auto max-w-xl', 2: 'mx-auto max-w-4xl' }

/** Classes that cap the grid width for the current number of locations. */
const gridWidth = computed(() => GRID_WIDTH_BY_COUNT[locations.value.length] ?? '')
</script>
