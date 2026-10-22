<template>
  <UiSection v-if="items.length" padding="md" class="border-b border-nova-border bg-nova-bg" data-testid="nova-trusted">
    <UiText
      as="p"
      size="inherit"
      tone="inherit"
      class="text-center text-[13px] font-semibold uppercase tracking-[0.1em] text-nova-muted"
    >
      {{ t('nova.trusted.title') }}
    </UiText>

    <UiLogoWall
      :items="items"
      testid-prefix="trusted"
      :layout="UiLogoWallLayout.Row"
      class="mt-6 text-nova-muted"
      data-testid="nova-trusted-strip"
    />
  </UiSection>
</template>

<script setup lang="ts">
/**
 * nova "trusted by" strip: a compact row of the partners' logos (their name when one has no logo),
 * linked to the partner's website when it has one. It reads the same storefront partners the
 * partners section shows; with none the strip renders nothing.
 */
import { useStorefrontStore } from '~/stores/storefront'
import { UiLogoWallLayout } from '~/utils/ui/logoWall'

/** Translator and the active language. */
const { t, locale } = useI18n()

/** The storefront content: partners, locations and the rest. */
const storefront = useStorefrontStore()

// Not awaited: Nuxt still holds the server render for it, and the section stays synchronous.
useAsyncData('storefront:partners', () => storefront.loadPartners(), { watch: [locale] })

/** The partners, in display order. */
const items = computed(() => storefront.partners)
</script>
