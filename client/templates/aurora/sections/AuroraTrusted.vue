<template>
  <UiSection v-if="items.length" padding="sm" :contained="false" class="relative" data-testid="aurora-trusted">
    <UiStack gap="none" align="center" class="text-center">
      <UiText tone="inherit" size="inherit" :class="[EYEBROW_CLASS, 'text-mut']">{{ t('aurora.trusted.title') }}</UiText>
    </UiStack>

    <UiLogoWall
      :items="items"
      testid-prefix="trusted"
      :layout="UiLogoWallLayout.Row"
      class="mt-6 text-mut2"
      data-testid="aurora-trusted-strip"
    />
  </UiSection>
</template>

<script setup lang="ts">
/**
 * aurora "trusted by" strip: a compact row of the partners' logos (the name when a partner has
 * no logo), linked to the partner's site when it has one. It reads the same partners as the
 * partners section; with none it renders nothing.
 */
import { EYEBROW_CLASS } from '~/templates/aurora/typography'
import { useStorefrontStore } from '~/stores/storefront'
import { UiLogoWallLayout } from '~/utils/ui/logoWall'

/** Translator and the active language. */
const { t, locale } = useI18n()

/** The storefront content, including the partners. */
const storefront = useStorefrontStore()

// Not awaited: Nuxt still holds the server render for it, and the section stays synchronous.
useAsyncData('storefront:partners', () => storefront.loadPartners(), { watch: [locale] })

/** The partners, in display order. */
const items = computed(() => storefront.partners)
</script>
