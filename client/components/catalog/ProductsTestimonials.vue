<template>
  <section v-if="items.length" class="py-8 md:py-20 bg-ui-page relative" data-testid="products-testimonials">
    <UiContainer>
      <MarketingSectionHeader class="mb-12" :subtitle="$t('products.testimonials.subtitle')">
        <template #title>
          <i18n-t keypath="products.testimonials.title" tag="span">
            <template #thousands>
              <span class="text-primary-700 dark:text-secondary-400">{{ $t('products.testimonials.thousandsHighlight') }}</span>
            </template>
          </i18n-t>
        </template>
      </MarketingSectionHeader>

      <UiGrid cols="1" gap="lg" class="max-w-5xl mx-auto md:grid-cols-3">
        <UiCard
          v-for="(testimonial, index) in items"
          :key="testimonial.id"
          v-motion
          :initial="{ opacity: 0, y: 30 }"
          :visible-once="{ opacity: 1, y: 0, transition: { delay: index * 100, duration: 400 } }"
          class="group hover:border-primary-500/30 dark:hover:border-secondary-500/30 hover:-translate-y-2 transition-all duration-300"
          :data-testid="`product-testimonial-${testimonial.id}`"
        >
          <div class="flex gap-1 mb-4">
            <Star v-for="star in testimonial.rating" :key="star" :size="14" :stroke-width="2" fill="currentColor" class="text-primary-700 dark:text-secondary-400" />
          </div>

          <UiText size="sm" class="mb-4 italic leading-relaxed text-gray-700 dark:text-gray-300">"{{ testimonial.text }}"</UiText>

          <div class="flex items-center gap-3">
            <UiAvatar :first-name="testimonial.name" />
            <div>
              <UiText size="sm" weight="semibold">{{ testimonial.name }}</UiText>
              <UiText v-if="testimonial.company" size="xs" tone="subtle">{{ testimonial.company }}</UiText>
            </div>
          </div>
        </UiCard>
      </UiGrid>
    </UiContainer>
  </section>
</template>

<script setup lang="ts">
/**
 * Three client testimonials for the products page, each quote cut to 120 characters. They come
 * from the storefront store; with none the section renders nothing.
 */
import { Star } from 'lucide-vue-next'
import { useStorefrontStore } from '~/stores/storefront'

/** Longest quote shown before it is cut with an ellipsis. */
const QUOTE_LIMIT = 120

/** The active language; the quotes are re-read when it changes. */
const { locale } = useI18n()

/** The storefront content: testimonials, partners, projects and FAQs. */
const storefront = useStorefrontStore()

// Not awaited: Nuxt still holds the server render for it, and the section stays synchronous.
useAsyncData('storefront:testimonials', () => storefront.loadTestimonials(), { watch: [locale] })

/** The first three testimonials with their shortened text. */
const items = computed(() => storefront.testimonials.slice(0, 3).map((testimonial) => ({
  id: testimonial.id,
  name: testimonial.name,
  company: testimonial.company,
  rating: testimonial.rating,
  text: testimonial.quote.length > QUOTE_LIMIT ? `${testimonial.quote.slice(0, QUOTE_LIMIT)}...` : testimonial.quote,
})))
</script>
