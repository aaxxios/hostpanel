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

    <UiStack direction="row" wrap align="center" justify="center" gap="lg" class="mt-6" data-testid="nova-trusted-strip">
      <component
        :is="partner.url ? UiLink : UiStack"
        v-for="partner in items"
        :key="partner.id"
        v-bind="partner.url ? { href: partner.url, tone: 'inherit', underline: false } : {}"
        :data-testid="`trusted-${partner.id}`"
      >
        <UiImage
          v-if="partner.logoUrl"
          :src="partner.logoUrl"
          :alt="partner.name"
          unoptimised
          class="h-8 w-auto opacity-70 grayscale transition hover:opacity-100 hover:grayscale-0"
          :testid="`trusted-logo-${partner.id}`"
        />
        <UiText v-else as="span" tone="inherit" weight="semibold" class="whitespace-nowrap text-nova-muted">{{ partner.name }}</UiText>
      </component>
    </UiStack>
  </UiSection>
</template>

<script setup lang="ts">
/**
 * nova "trusted by" strip: a compact row of the partners' logos (their name when one has no logo),
 * linked to the partner's website when it has one. It reads the same storefront partners the
 * partners section shows; with none the strip renders nothing.
 */
import { useStorefrontStore } from '~/stores/storefront'
import UiLink from '~/components/ui/UiLink.vue'
import UiStack from '~/components/ui/UiStack.vue'

/** Translator and the active language. */
const { t, locale } = useI18n()

/** The storefront content: partners, locations and the rest. */
const storefront = useStorefrontStore()

// Not awaited: Nuxt still holds the server render for it, and the section stays synchronous.
useAsyncData('storefront:partners', () => storefront.loadPartners(), { watch: [locale] })

/** The partners, in display order. */
const items = computed(() => storefront.partners)
</script>
