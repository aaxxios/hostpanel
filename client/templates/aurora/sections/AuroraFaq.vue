<template>
  <UiSection v-if="entries.length" id="faq" padding="lg" :contained="false" class="relative border-t border-line">
    <UiSplit ratio="equal" gap="xl">
      <template #start>
        <UiHeading :level="2" size="bare" :class="BLOCK_TITLE_CLASS">
          {{ t('aurora.faq.title') }}
        </UiHeading>
      </template>
      <template #end>
        <!-- Answers stay in the DOM: this is indexable content and a crawler should read every one. -->
        <UiAccordion :items="entries" keep-content :default-open="[0]" data-testid="aurora-faq" />
      </template>
    </UiSplit>
  </UiSection>
</template>

<script setup lang="ts">
/**
 * aurora FAQ accordion. The questions come from the storefront store
 * (`GET /api/public/faqs`); with none the section renders nothing. Answers stay in the DOM so
 * they remain indexable.
 */
import { BLOCK_TITLE_CLASS } from '~/templates/aurora/typography'
import { useStorefrontStore } from '~/stores/storefront'

/** Translator and the active language. */
const { t, locale } = useI18n()

/** The storefront content: testimonials, partners, projects and FAQs. */
const storefront = useStorefrontStore()

// Not awaited: Nuxt still holds the server render for it, and the section stays synchronous.
useAsyncData('storefront:faqs', () => storefront.loadFaqs(), { watch: [locale] })

/** The questions in the shape the accordion takes. */
const entries = computed(() => storefront.faqs.map(faq => ({ title: faq.question, content: faq.answer })))
</script>
