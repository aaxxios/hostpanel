<template>
  <section v-if="testimonials.length" class="py-14 md:py-20 bg-classic-surface border-y border-classic-border" data-testid="sections-testimonials">
    <UiContainer>
      <ClassicSectionHeader
        class="mb-10"
        :eyebrow="$t('testimonials.badge')"
        :title="$t('testimonials.title')"
        :subtitle="$t('testimonials.subtitle')"
      />

      <!-- Carousel -->
      <div class="relative">
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
            <SwiperSlide v-for="testimonial in testimonials" :key="testimonial.id">
              <ClassicCard :data-testid="`testimonial-${testimonial.id}`">
                <UiStack gap="md">
                  <!-- Rating -->
                  <UiStack direction="row" gap="none" class="gap-1">
                    <Star
                      v-for="star in testimonial.rating"
                      :key="star"
                      :size="16"
                      :stroke-width="2"
                      fill="currentColor"
                      class="text-classic-brand"
                    />
                  </UiStack>

                  <UiText tone="inherit" class="text-base leading-relaxed text-classic-ink">"{{ testimonial.quote }}"</UiText>

                  <!-- Author -->
                  <UiStack direction="row" align="center" gap="sm" class="border-t border-classic-border pt-4">
                    <UiStack align="center" justify="center" gap="none" class="h-10 w-10 shrink-0 rounded-full bg-classic-brand-soft text-xs font-bold text-classic-brand">
                      {{ getInitials(testimonial.name) }}
                    </UiStack>
                    <div>
                      <div class="font-semibold text-classic-ink">{{ testimonial.name }}</div>
                      <div v-if="testimonial.role" class="text-xs text-classic-muted">{{ testimonial.role }}</div>
                      <div v-if="testimonial.company" class="text-xs text-classic-muted">{{ testimonial.company }}</div>
                    </div>
                  </UiStack>
                </UiStack>
              </ClassicCard>
            </SwiperSlide>
          </Swiper>
          <!-- The carousel only exists in the browser; the server render carries every quote as plain text. -->
          <template #fallback>
            <UiStack as="ul" gap="none" class="sr-only" data-testid="testimonials-fallback">
              <li v-for="testimonial in testimonials" :key="testimonial.id">
                <UiText as="span">"{{ testimonial.quote }}" - {{ testimonial.name }}</UiText>
              </li>
            </UiStack>
          </template>
        </ClientOnly>

        <UiText size="sm" tone="inherit" class="text-center mt-6 text-classic-muted">{{ $t('testimonials.hint') }}</UiText>
      </div>
    </UiContainer>
  </section>
</template>

<script setup lang="ts">
/**
 * Testimonials carousel (Swiper).
 *
 * The quotes come from the storefront store (`GET /api/portal/public/testimonials`, localised by the API);
 * with none the section renders nothing. Each quote sits on a flat classic card with brand-coloured
 * stars and an initials badge, in light and dark mode alike.
 */
import { Star } from 'lucide-vue-next'
import { Swiper, SwiperSlide } from 'swiper/vue'
import { Autoplay, Pagination } from 'swiper/modules'
import { TESTIMONIAL_AUTOPLAY_MS } from '~/constants/uiDefaults'
import 'swiper/css'
import 'swiper/css/pagination'
import { useStorefrontStore } from '~/stores/storefront'
import { getInitials } from '~/utils/design'

/** Swiper modules the carousel uses. */
const modules = [Autoplay, Pagination]

/** The active language; the quotes are re-read when it changes. */
const { locale } = useI18n()

/** The storefront content: testimonials, partners, projects and FAQs. */
const storefront = useStorefrontStore()

// Not awaited: Nuxt still holds the server render for it, and the section stays synchronous.
useAsyncData('storefront:testimonials', () => storefront.loadTestimonials(), { watch: [locale] })

/** The testimonials to show, in display order. */
const testimonials = computed(() => storefront.testimonials)
</script>
