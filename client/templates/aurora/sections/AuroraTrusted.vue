<template>
  <UiSection v-if="items.length" padding="sm" :contained="false" class="relative border-t border-line" data-testid="aurora-trusted">
    <UiStack gap="none" align="center" class="text-center">
      <UiText tone="inherit" size="inherit" :class="[EYEBROW_CLASS, 'text-mut']">{{ t('aurora.trusted.title') }}</UiText>
    </UiStack>

    <UiStack direction="row" align="center" justify="center" gap="lg" class="mt-6 flex-wrap" data-testid="aurora-trusted-strip">
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
          class="h-8 w-auto opacity-70 transition-opacity hover:opacity-100"
          :testid="`trusted-logo-${partner.id}`"
        />
        <UiText v-else as="span" tone="inherit" weight="semibold" class="whitespace-nowrap text-mut2">{{ partner.name }}</UiText>
      </component>
    </UiStack>
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
import UiLink from '~/components/ui/UiLink.vue'
import UiStack from '~/components/ui/UiStack.vue'

/** Translator and the active language. */
const { t, locale } = useI18n()

/** The storefront content, including the partners. */
const storefront = useStorefrontStore()

// Not awaited: Nuxt still holds the server render for it, and the section stays synchronous.
useAsyncData('storefront:partners', () => storefront.loadPartners(), { watch: [locale] })

/** The partners, in display order. */
const items = computed(() => storefront.partners)
</script>
