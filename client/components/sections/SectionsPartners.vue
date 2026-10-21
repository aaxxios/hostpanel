<template>
  <section v-if="partners.length" class="py-10 sm:py-16 bg-ui-page relative overflow-hidden" data-testid="sections-partners">
    <!-- Background -->
    <UiDecor class="inset-0">
      <UiDecor class="inset-0 bg-gradient-to-b from-transparent via-white/[0.02] to-transparent" />
      <UiDecor class="top-10 left-10 w-2 h-2 bg-primary-400/40 rounded-full animate-float" />
      <UiDecor class="top-20 right-20 w-3 h-3 bg-secondary-600/30 dark:bg-secondary-400/30 rounded-full animate-float [animation-delay:1s]" />
      <UiDecor class="bottom-10 left-1/3 w-2 h-2 bg-cyan-400/40 rounded-full animate-float [animation-delay:2s]" />
    </UiDecor>

    <UiContainer class="relative z-10">
      <!-- Header -->
      <div
        class="text-center mb-8 sm:mb-12 animate-reveal-up motion-reduce:animate-none"
      >
        <UiText size="sm" tone="subtle" class="uppercase tracking-wider mb-2">{{ $t('clients.subtitle') }}</UiText>
        <UiHeading :level="3" size="subsection" class="text-xl">{{ $t('clients.heading') }}</UiHeading>
      </div>

      <!-- A short list sits centred and wrapped; a marquee of two names reads as a half-empty strip. -->
      <UiStack v-if="!marquee" direction="row" wrap justify="center" gap="md" class="py-4" data-testid="sections-partners-static">
        <UiCard
          v-for="partner in partners"
          :key="partner.id"
          padding="none"
          class="px-4 py-2.5 sm:px-6 sm:py-3 group hover:border-primary-500/50 hover:shadow-lg transition-all duration-300"
          :data-testid="`partner-${partner.id}`"
        >
          <UiStack direction="row" align="center" gap="xs">
            <UiImage
              v-if="partner.logoUrl"
              :src="partner.logoUrl"
              :alt="partner.name"
              unoptimised
              class="h-6 sm:h-8 w-auto flex-shrink-0"
              :testid="`partner-logo-${partner.id}`"
            />
            <UiText as="span" tone="inherit" weight="semibold" class="text-sm sm:text-base text-gray-600 dark:text-gray-400 group-hover:text-gray-900 dark:group-hover:text-white transition-colors duration-300 whitespace-nowrap">{{ partner.name }}</UiText>
          </UiStack>
        </UiCard>
      </UiStack>

      <!-- Logos marquee -->
      <div v-else class="relative overflow-hidden" data-testid="sections-partners-marquee">
        <!-- Fade edges -->
        <UiDecor class="left-0 top-0 bottom-0 w-20 bg-gradient-to-r from-ui-page to-transparent z-10" />
        <UiDecor class="right-0 top-0 bottom-0 w-20 bg-gradient-to-l from-ui-page to-transparent z-10" />

        <!-- Scrolling logos (the list is doubled so the loop has no gap) -->
        <div class="flex animate-marquee hover:[animation-play-state:paused] py-4">
          <div
            v-for="(partner, index) in loopedPartners"
            :key="index"
            class="flex-shrink-0 mx-3 sm:mx-6 flex items-center justify-center"
          >
            <UiCard
              padding="none"
              class="px-4 py-2.5 sm:px-6 sm:py-3 group hover:border-primary-500/50 hover:scale-105 hover:shadow-xl hover:shadow-primary-500/30 transition-all duration-300"
              :data-testid="`partner-${partner.id}`"
            >
              <div class="flex items-center gap-2">
                <UiImage
                  v-if="partner.logoUrl"
                  :src="partner.logoUrl"
                  :alt="partner.name"
                  unoptimised
                  class="h-6 sm:h-8 w-auto flex-shrink-0"
                  :testid="`partner-logo-${partner.id}`"
                />
                <span class="text-sm sm:text-base font-semibold text-gray-500 group-hover:text-gray-900 dark:group-hover:text-white transition-colors duration-300 whitespace-nowrap">{{ partner.name }}</span>
              </div>
            </UiCard>
          </div>
        </div>
      </div>
    </UiContainer>

    <!-- Decorative corner accents -->
    <UiDecor class="top-0 right-0 w-32 h-32 border-r-2 border-t-2 border-secondary-500/25" />
    <UiDecor class="bottom-0 left-0 w-32 h-32 border-l-2 border-b-2 border-primary-500/25" />
  </section>
</template>

<script setup lang="ts">
/**
 * Partners section: a marquee of partner logos and names. The partners come from the storefront
 * store (`GET /api/portal/public/partners`, localised by the API); with none the section renders
 * nothing.
 */
import { useStorefrontStore } from '~/stores/storefront'

/** The active language; the partners are re-read when it changes. */
const { locale } = useI18n()

/** The storefront content: testimonials, partners, projects and FAQs. */
const storefront = useStorefrontStore()

// Not awaited: Nuxt still holds the server render for it, and the section stays synchronous.
useAsyncData('storefront:partners', () => storefront.loadPartners(), { watch: [locale] })

/** The partners to show, in display order. */
const partners = computed(() => storefront.partners)

/** Fewest partners that fill a scrolling strip; below it the logos sit centred instead. */
const MARQUEE_MIN_PARTNERS = 6

/** Whether there are enough partners to run the marquee. */
const marquee = computed(() => partners.value.length >= MARQUEE_MIN_PARTNERS)

/** The partners twice over, so the marquee loops without a gap. */
const loopedPartners = computed(() => [...partners.value, ...partners.value])
</script>
