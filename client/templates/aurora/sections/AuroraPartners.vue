<template>
  <UiSection v-if="items.length" padding="md" :contained="false" class="relative border-t border-line" data-testid="aurora-partners">
    <UiStack gap="none" align="center" class="text-center">
      <UiText tone="inherit" size="inherit" :class="[EYEBROW_CLASS, 'text-ac3']">{{ t('aurora.partners.eyebrow') }}</UiText>
      <UiHeading :level="2" size="bare" :class="[SECTION_TITLE_CLASS, 'mt-3']">
        {{ t('aurora.partners.title') }}
      </UiHeading>
    </UiStack>

    <UiBalancedGrid :count="items.length" class="mt-9" data-testid="aurora-partners-grid">
      <component
        :is="partner.url ? UiLink : UiStack"
        v-for="partner in items"
        :key="partner.id"
        v-bind="partner.url ? { href: partner.url, tone: 'inherit', underline: false } : {}"
        :data-testid="`partner-${partner.id}`"
      >
        <UiCard
          tone="bare"
          padding="none"
          class="border-line2 bg-card-hi px-6 py-3 shadow-card transition-colors hover:border-ac1"
        >
          <UiStack direction="row" align="center" justify="center" gap="sm">
            <UiImage
              v-if="partner.logoUrl"
              :src="partner.logoUrl"
              :alt="partner.name"
              unoptimised
              class="h-7 w-auto flex-shrink-0"
              :testid="`partner-logo-${partner.id}`"
            />
            <UiText as="span" tone="inherit" weight="semibold" class="whitespace-nowrap text-mut2">{{ partner.name }}</UiText>
          </UiStack>
        </UiCard>
      </component>
    </UiBalancedGrid>
  </UiSection>
</template>

<script setup lang="ts">
/**
 * aurora partners strip: each partner's logo (when it has one) and name, linked to the partner's
 * website when it has one. The partners come from the storefront store
 * (`GET /api/public/partners`); with none the section renders nothing.
 */
import { EYEBROW_CLASS, SECTION_TITLE_CLASS } from '~/templates/aurora/typography'
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
