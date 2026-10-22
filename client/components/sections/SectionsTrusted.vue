<template>
  <section v-if="partners.length" class="py-6 sm:py-8 bg-ui-page border-y border-gray-200 dark:border-white/10" data-testid="sections-trusted">
    <UiContainer>
      <UiStack align="center" gap="md">
        <UiText size="sm" tone="subtle" class="uppercase tracking-wider" data-testid="sections-trusted-label">{{ $t('hostingSections.trusted.label') }}</UiText>
        <UiLogoWall
          :items="partners"
          testid-prefix="trusted"
          :layout="UiLogoWallLayout.Row"
          class="text-gray-500 dark:text-gray-400"
        />
      </UiStack>
    </UiContainer>
  </section>
</template>

<script setup lang="ts">
/**
 * Trusted-by strip: a compact single row of partner logos (the name when a partner has no logo)
 * under a short label. It reads the same partners feed as the full partners section; with none
 * the strip renders nothing.
 */
import { useStorefrontStore } from '~/stores/storefront'
import { UiLogoWallLayout } from '~/utils/ui/logoWall'

/** The active language; the partners are re-read when it changes. */
const { locale } = useI18n()

/** The storefront content, including the partners. */
const storefront = useStorefrontStore()

// Not awaited: Nuxt still holds the server render for it, and the section stays synchronous.
useAsyncData('storefront:trusted', () => storefront.loadPartners(), { watch: [locale] })

/** The partners to show, in display order. */
const partners = computed(() => storefront.partners)
</script>
