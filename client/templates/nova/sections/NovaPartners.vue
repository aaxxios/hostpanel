<template>
  <UiSection v-if="items.length" padding="md" class="border-b border-nova-border bg-nova-bg" data-testid="nova-partners">
    <NovaSectionHeader :eyebrow="t('nova.partners.eyebrow')" :title="t('nova.partners.title')" />

    <UiLogoWall
      :items="items"
      testid-prefix="partner"
      :layout="items.length >= MARQUEE_MIN_PARTNERS ? UiLogoWallLayout.Marquee : UiLogoWallLayout.Grid"
      class="mt-9 text-nova-muted"
      data-testid="nova-partners-grid"
    />
  </UiSection>
</template>

<script setup lang="ts">
/**
 * nova partners strip: each partner's logo (a plain wordmark when it has none), linked to the partner's
 * website when it has one. The partners come from the storefront store
 * (`GET /api/public/partners`); with none the section renders nothing.
 */
import NovaSectionHeader from '~/templates/nova/sections/NovaSectionHeader.vue'
import { useStorefrontStore } from '~/stores/storefront'
import { UiLogoWallLayout } from '~/utils/ui/logoWall'

/** Translator and the active language. */
const { t, locale } = useI18n()

/** The storefront content: testimonials, partners, projects and FAQs. */
const storefront = useStorefrontStore()

// Not awaited: Nuxt still holds the server render for it, and the section stays synchronous.
useAsyncData('storefront:partners', () => storefront.loadPartners(), { watch: [locale] })

/** Fewest partners that fill a scrolling strip; below it the logos sit in a balanced grid. */
const MARQUEE_MIN_PARTNERS = 8

/** The partners, in display order. */
const items = computed(() => storefront.partners)
</script>
