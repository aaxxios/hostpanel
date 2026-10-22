<template>
  <UiSection v-if="items.length" padding="md" :contained="false" class="relative border-t border-line" data-testid="aurora-partners">
    <UiStack gap="none" align="center" class="text-center">
      <UiText tone="inherit" size="inherit" :class="[EYEBROW_CLASS, 'text-ac3']">{{ t('aurora.partners.eyebrow') }}</UiText>
      <UiHeading :level="2" size="bare" :class="[SECTION_TITLE_CLASS, 'mt-3']">
        {{ t('aurora.partners.title') }}
      </UiHeading>
    </UiStack>

    <UiLogoWall
      :items="items"
      testid-prefix="partner"
      :layout="items.length >= MARQUEE_MIN_PARTNERS ? UiLogoWallLayout.Marquee : UiLogoWallLayout.Grid"
      class="mt-9 text-mut2"
      data-testid="aurora-partners-grid"
    />
  </UiSection>
</template>

<script setup lang="ts">
/**
 * aurora partners strip: each partner's logo (a plain wordmark when it has none), linked to the partner's
 * website when it has one. The partners come from the storefront store
 * (`GET /api/public/partners`); with none the section renders nothing.
 */
import { EYEBROW_CLASS, SECTION_TITLE_CLASS } from '~/templates/aurora/typography'
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
