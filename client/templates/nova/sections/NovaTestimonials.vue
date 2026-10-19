<template>
  <UiSection v-if="testimonials.length" padding="lg" class="border-b border-nova-border bg-nova-surface">
    <NovaSectionHeader :eyebrow="t('nova.testimonials.eyebrow')" :title="t('nova.testimonials.title')" />

    <UiGrid as="ul" cols="3" gap="lg" class="mt-10">
      <li v-for="testimonial in testimonials" :key="testimonial.id" class="h-full">
        <UiCard padding="lg" class="flex h-full flex-col !border-nova-border !bg-nova-bg">
          <figure class="flex h-full flex-col">
            <Icon name="lucide:quote" class="h-6 w-6 text-nova-brand" aria-hidden="true" />
            <blockquote class="mt-4 flex-1 text-[15px] leading-[1.7] text-nova-ink">
              {{ testimonial.quote }}
            </blockquote>
            <figcaption class="mt-5 border-t border-nova-border pt-4 text-[15px]">
              <span class="font-semibold text-nova-ink">{{ testimonial.name }}</span>
              <span v-if="attribution(testimonial)" class="block text-sm text-nova-muted">{{ attribution(testimonial) }}</span>
            </figcaption>
          </figure>
        </UiCard>
      </li>
    </UiGrid>
  </UiSection>
</template>

<script setup lang="ts">
/**
 * nova testimonials. The quotes come from the storefront store
 * (`GET /api/public/testimonials`); with none the section renders nothing -- not a placeholder,
 * and certainly not an invented name.
 */
import NovaSectionHeader from '~/templates/nova/sections/NovaSectionHeader.vue'
import { useStorefrontStore } from '~/stores/storefront'
import type { Testimonial } from '~/types/testimonial'

/** Translator for the copy, and the active language. */
const { t, locale } = useI18n()

/** The storefront content: testimonials, partners, projects and FAQs. */
const storefront = useStorefrontStore()

// Not awaited: Nuxt still holds the server render for it, and the section stays synchronous.
useAsyncData('storefront:testimonials', () => storefront.loadTestimonials(), { watch: [locale] })

/** The quotes, in display order. */
const testimonials = computed(() => storefront.testimonials)

/**
 * Joins a quote's role and company for the line under the name.
 *
 * @param testimonial The quote.
 * @returns The role and company, comma-separated; empty when it has neither.
 */
const attribution = (testimonial: Testimonial): string =>
  [testimonial.role, testimonial.company].filter(Boolean).join(', ')
</script>
