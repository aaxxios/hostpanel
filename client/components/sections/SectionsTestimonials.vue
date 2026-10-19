<template>
  <section v-if="testimonials.length" class="py-20 bg-ui-page relative overflow-hidden" data-testid="sections-testimonials">
    <!-- Background decorations -->
    <UiDecor class="inset-0">
      <UiDecor class="inset-0 bg-gradient-to-r from-primary-500/5 via-transparent to-primary-500/5 dark:to-secondary-500/5" />
      <UiDecor class="top-0 left-0 w-1/2 h-1/2 bg-primary-500/10 rounded-full blur-[150px]" />
      <UiDecor class="bottom-0 right-0 w-1/3 h-1/3 bg-primary-500/10 dark:bg-secondary-500/10 rounded-full blur-[120px]" />
      <UiDecor class="top-1/4 right-1/4 w-2 h-2 bg-primary-400/60 rounded-full animate-float [animation-delay:0.8s]" />
      <UiDecor class="bottom-1/3 left-1/4 w-3 h-3 bg-primary-400/50 dark:bg-secondary-400/50 rounded-full animate-float [animation-delay:1.5s]" />
      <UiDecor class="top-2/3 right-1/3 w-2 h-2 bg-cyan-400/60 rounded-full animate-float [animation-delay:2.3s]" />
      <UiDecor class="top-1/2 left-10 w-32 h-32 rounded-full border border-gray-200 dark:border-white/5" />
      <UiDecor class="bottom-1/4 right-20 w-24 h-24 rounded-full border border-gray-200 dark:border-white/5" />
    </UiDecor>

    <UiContainer class="relative z-10">
      <MarketingSectionHeader class="mb-16" :badge="$t('testimonials.badge')" :subtitle="$t('testimonials.subtitle')">
        <template #title>
          {{ $t('testimonials.heading') }} <span class="text-primary-700 dark:text-secondary-400">{{ $t('testimonials.headingHighlight') }}</span> {{ $t('testimonials.headingEnd') }}
        </template>
      </MarketingSectionHeader>

      <!-- Carousel -->
      <div class="relative max-w-4xl mx-auto">
        <ClientOnly>
          <Swiper
            :modules="modules"
            :slides-per-view="1"
            :space-between="30"
            :loop="testimonials.length > 1"
            :autoplay="{ delay: TESTIMONIAL_AUTOPLAY_MS, disableOnInteraction: false, pauseOnMouseEnter: true }"
            :pagination="{ clickable: true }"
            class="testimonials-swiper"
          >
            <SwiperSlide v-for="(testimonial, index) in testimonials" :key="testimonial.id">
              <UiCard
                padding="none"
                :class="[GRADIENTS[index % GRADIENTS.length], 'relative overflow-hidden']"
                :data-testid="`testimonial-${testimonial.id}`"
              >
                <!-- Pattern -->
                <UiDecor class="inset-0 opacity-10 pattern-dots-30" />

                <div class="relative p-8 md:p-10">
                  <!-- Rating -->
                  <div class="flex items-center gap-1 mb-6">
                    <Star
                      v-for="(delay, star) in STAR_DELAYS.slice(0, testimonial.rating)"
                      :key="star"
                      :size="20"
                      :stroke-width="2"
                      fill="currentColor"
                      :class="[delay, 'text-primary-600 dark:text-primary-400 animate-pulse']"
                    />
                  </div>

                  <!-- Quote mark -->
                  <div class="absolute top-6 right-8 text-gray-900/5 dark:text-white/5">
                    <Quote :size="64" :stroke-width="2" />
                  </div>

                  <UiText class="relative text-lg md:text-xl mb-8 leading-relaxed italic text-gray-800 dark:text-white/90">"{{ testimonial.quote }}"</UiText>

                  <!-- Author -->
                  <div class="flex items-center justify-between">
                    <div class="flex items-center gap-4">
                      <span class="relative flex w-14 h-14 rounded-full border-2 border-primary-400 p-0.5">
                        <UiDecor class="inset-0 rounded-full border-2 border-primary-500/50 animate-ping" />
                        <span class="relative flex w-full h-full items-center justify-center rounded-full bg-white dark:bg-gray-900 text-gray-900 dark:text-white font-bold text-sm">
                          {{ getInitials(testimonial.name) }}
                        </span>
                      </span>

                      <div>
                        <div class="font-semibold text-gray-900 dark:text-white">{{ testimonial.name }}</div>
                        <div v-if="testimonial.role" class="text-sm text-gray-700 dark:text-gray-300">{{ testimonial.role }}</div>
                        <div v-if="testimonial.company" class="text-sm text-primary-700 dark:text-secondary-400">{{ testimonial.company }}</div>
                      </div>
                    </div>

                    <!-- Company badge -->
                    <span v-if="testimonial.company" class="hidden md:flex items-center gap-2 px-4 py-2 bg-gray-100 dark:bg-white/10 backdrop-blur-sm rounded-lg border border-gray-300 dark:border-white/20 hover:border-primary-500/50 transition-colors duration-300">
                      <Building2 :size="18" :stroke-width="2" class="text-gray-700 dark:text-gray-300" />
                      <span class="text-sm text-gray-700 dark:text-gray-300">{{ testimonial.company }}</span>
                    </span>
                  </div>
                </div>

                <!-- Corner decorations -->
                <UiDecor class="top-0 left-0 w-14 h-14 border-l-2 border-t-2 border-primary-500/40 rounded-tl-xl" />
                <UiDecor class="bottom-0 right-0 w-14 h-14 border-r-2 border-b-2 border-primary-500/40 dark:border-secondary-500/40 rounded-br-xl" />
              </UiCard>
            </SwiperSlide>
          </Swiper>
        </ClientOnly>

        <UiText size="sm" tone="subtle" class="text-center mt-6">{{ $t('testimonials.hint') }}</UiText>
      </div>
    </UiContainer>

    <!-- Section corner decorations -->
    <UiDecor class="top-0 right-0 w-40 h-40 border-r-2 border-t-2 border-primary-500/20 dark:border-secondary-500/20" />
    <UiDecor class="bottom-0 left-0 w-40 h-40 border-l-2 border-b-2 border-primary-500/20" />
  </section>
</template>

<script setup lang="ts">
/**
 * Testimonials carousel (Swiper).
 *
 * The quotes come from the storefront store (`GET /api/portal/public/testimonials`, localised by the API);
 * with none the section renders nothing. Each card sits on a dark gradient in dark mode and on
 * the plain surface in light mode, so the quote text is always readable.
 */
import { Building2, Quote, Star } from 'lucide-vue-next'
import { Swiper, SwiperSlide } from 'swiper/vue'
import { Autoplay, Pagination } from 'swiper/modules'
import { TESTIMONIAL_AUTOPLAY_MS } from '~/constants/uiDefaults'
import 'swiper/css'
import 'swiper/css/pagination'
import { useStorefrontStore } from '~/stores/storefront'
import { getInitials } from '~/utils/design'

/** Swiper modules the carousel uses. */
const modules = [Autoplay, Pagination]

/** Dark-mode gradient of each card in turn (literal classes, so Tailwind emits them). */
const GRADIENTS = [
  'dark:bg-gradient-to-br dark:from-blue-950 dark:to-violet-950',
  'dark:bg-gradient-to-br dark:from-blue-900 dark:to-purple-900',
  'dark:bg-gradient-to-br dark:from-cyan-950 dark:to-indigo-950',
  'dark:bg-gradient-to-br dark:from-indigo-950 dark:to-blue-950',
  'dark:bg-gradient-to-br dark:from-emerald-950 dark:to-teal-950',
  'dark:bg-gradient-to-br dark:from-fuchsia-950 dark:to-violet-950',
]

/** Pulse delay of each of the five rating stars. */
const STAR_DELAYS = ['[animation-delay:0.1s]', '[animation-delay:0.2s]', '[animation-delay:0.3s]', '[animation-delay:0.4s]', '[animation-delay:0.5s]']

/** The active language; the quotes are re-read when it changes. */
const { locale } = useI18n()

/** The storefront content: testimonials, partners, projects and FAQs. */
const storefront = useStorefrontStore()

// Not awaited: Nuxt still holds the server render for it, and the section stays synchronous.
useAsyncData('storefront:testimonials', () => storefront.loadTestimonials(), { watch: [locale] })

/** The testimonials to show, in display order. */
const testimonials = computed(() => storefront.testimonials)
</script>
