<template>
  <section v-if="partners.length" class="py-6 sm:py-8 bg-ui-page border-y border-gray-200 dark:border-white/10" data-testid="sections-trusted">
    <UiContainer>
      <UiStack direction="row" align="center" justify="center" gap="md" wrap>
        <UiText size="sm" tone="subtle" class="uppercase tracking-wider" data-testid="sections-trusted-label">{{ $t('hostingSections.trusted.label') }}</UiText>
        <UiStack
          v-for="partner in partners"
          :key="partner.id"
          direction="row"
          align="center"
          gap="xs"
          :data-testid="`trusted-${partner.id}`"
        >
          <UiImage
            v-if="partner.logoUrl"
            :src="partner.logoUrl"
            :alt="partner.name"
            unoptimised
            class="h-6 w-auto opacity-70"
            :testid="`trusted-logo-${partner.id}`"
          />
          <UiText v-else as="span" tone="muted" weight="semibold" size="sm" class="whitespace-nowrap">{{ partner.name }}</UiText>
        </UiStack>
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

/** The active language; the partners are re-read when it changes. */
const { locale } = useI18n()

/** The storefront content, including the partners. */
const storefront = useStorefrontStore()

// Not awaited: Nuxt still holds the server render for it, and the section stays synchronous.
useAsyncData('storefront:trusted', () => storefront.loadPartners(), { watch: [locale] })

/** The partners to show, in display order. */
const partners = computed(() => storefront.partners)
</script>
