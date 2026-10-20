<template>
  <UiSection v-if="items.length" padding="md" class="border-b border-nova-border bg-nova-bg" data-testid="nova-partners">
    <NovaSectionHeader :eyebrow="t('nova.partners.eyebrow')" :title="t('nova.partners.title')" />

    <UiStack direction="row" wrap align="center" gap="md" class="mt-8">
      <component
        :is="partner.url ? UiLink : UiStack"
        v-for="partner in items"
        :key="partner.id"
        v-bind="partner.url ? { href: partner.url, tone: 'inherit', underline: false } : {}"
        :data-testid="`partner-${partner.id}`"
      >
        <UiCard padding="none" class="!border-nova-border !bg-nova-surface px-5 py-3">
          <UiStack direction="row" align="center" gap="sm">
            <UiImage
              v-if="partner.logoUrl"
              :src="partner.logoUrl"
              :alt="partner.name"
              unoptimised
              class="h-7 w-auto flex-shrink-0"
              :testid="`partner-logo-${partner.id}`"
            />
            <UiText as="span" tone="inherit" weight="semibold" class="whitespace-nowrap text-nova-ink">{{ partner.name }}</UiText>
          </UiStack>
        </UiCard>
      </component>
    </UiStack>
  </UiSection>
</template>

<script setup lang="ts">
/**
 * nova partners strip: each partner's logo (when it has one) and name, linked to the partner's
 * website when it has one. The partners come from the storefront store
 * (`GET /api/public/partners`); with none the section renders nothing.
 */
import NovaSectionHeader from '~/templates/nova/sections/NovaSectionHeader.vue'
import { useStorefrontStore } from '~/stores/storefront'
import UiLink from '~/components/ui/UiLink.vue'
import UiStack from '~/components/ui/UiStack.vue'

/** Translator and the active language. */
const { t, locale } = useI18n()

/** The storefront content: testimonials, partners, projects and FAQs. */
const storefront = useStorefrontStore()

// Not awaited: Nuxt still holds the server render for it, and the section stays synchronous.
useAsyncData('storefront:partners', () => storefront.loadPartners(), { watch: [locale] })

/** The partners, in display order. */
const items = computed(() => storefront.partners)
</script>
