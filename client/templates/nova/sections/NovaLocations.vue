<template>
  <UiSection v-if="items.length" padding="lg" class="border-b border-nova-border bg-nova-surface" data-testid="nova-locations">
    <NovaSectionHeader
      :eyebrow="t('nova.locations.eyebrow')"
      :title="t('nova.locations.title')"
      :lead="t('nova.locations.lead')"
    />

    <UiBalancedGrid as="ul" :count="items.length" class="mt-10" data-testid="nova-locations-grid">
      <li v-for="location in items" :key="location.id" class="h-full" :data-testid="`nova-location-${location.id}`">
        <UiCard padding="lg" class="h-full !border-nova-border !bg-nova-bg">
          <UiStack direction="row" align="center" gap="md">
            <span
              class="grid h-11 w-11 flex-shrink-0 place-items-center rounded-xl bg-nova-surface-2 text-[13px] font-extrabold tracking-wider text-nova-brand"
              aria-hidden="true"
            >{{ location.countryCode }}</span>
            <div class="min-w-0">
              <UiHeading :level="3" size="bare" class="text-[17px] font-bold text-nova-ink">{{ location.name }}</UiHeading>
              <UiText v-if="location.city" as="p" size="inherit" tone="inherit" class="text-[14px] text-nova-muted">
                {{ location.city }}
              </UiText>
            </div>
          </UiStack>
          <UiText
            v-if="location.latencyNote"
            as="p"
            size="inherit"
            tone="inherit"
            class="mt-4 text-[14px] leading-relaxed text-nova-muted"
          >
            {{ location.latencyNote }}
          </UiText>
        </UiCard>
      </li>
    </UiBalancedGrid>
  </UiSection>
</template>

<script setup lang="ts">
/**
 * nova server locations: one card per active location with its country code, name, city and
 * latency note. No flag emoji and no map; the code is plain text. The locations come from the
 * storefront store (`GET /api/public/locations`); with none the section renders nothing.
 */
import NovaSectionHeader from '~/templates/nova/sections/NovaSectionHeader.vue'
import { useStorefrontStore } from '~/stores/storefront'

/** Translator and the active language. */
const { t, locale } = useI18n()

/** The storefront content: partners, locations and the rest. */
const storefront = useStorefrontStore()

// Not awaited: Nuxt still holds the server render for it, and the section stays synchronous.
useAsyncData('storefront:locations', () => storefront.loadLocations(), { watch: [locale] })

/** The locations, in display order. */
const items = computed(() => storefront.locations)
</script>
