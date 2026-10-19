<template>
  <!-- The slider sits on photographs under a dark overlay, so it always uses the dark palette. -->
  <section ref="sectionRef" class="dark products-slider relative h-[75dvh] sm:h-[85dvh] md:h-dvh w-full overflow-hidden" data-testid="products-slider">
    <ClientOnly>
      <!-- Before hydration: the first slide, rendered at once so the page is never blank -->
      <template #fallback>
        <div class="absolute inset-0">
          <NuxtImg
            v-if="firstSlide?.image"
            :src="firstSlide.image"
            :alt="firstSlide.name || SITE_NAME"
            width="1920"
            height="1080"
            format="webp"
            quality="80"
            sizes="100vw"
            :preload="true"
            class="h-full w-full object-cover"
          />
          <div class="absolute inset-0 bg-gradient-to-b from-black/80 via-black/40 to-black/90" />
        </div>
        <UiAccent v-if="firstSlide" :color="firstSlide.color">
          <UiContainer class="relative z-10 flex h-full items-center justify-center">
            <UiGlassPanel class="max-w-4xl text-center mb-10 sm:mb-0">
              <UiAccentTile :icon="`mdi:${firstSlide.icon}`" size="xl" class="mb-6 sm:mb-8 mx-auto" />
              <UiHeading :level="2" class="mb-4 sm:mb-6 text-3xl sm:text-5xl md:text-6xl lg:text-7xl font-black tracking-tight leading-none">{{ firstSlide.name }}</UiHeading>
              <UiText tone="primary" class="mb-4 sm:mb-6 text-lg sm:text-xl md:text-2xl font-semibold tracking-wide uppercase italic">{{ firstSlide.tagline }}</UiText>
              <UiText tone="muted" class="mb-6 sm:mb-8 text-sm sm:text-base md:text-lg leading-relaxed font-light line-clamp-3 px-4">{{ firstSlide.description }}</UiText>
              <div class="flex flex-col sm:flex-row justify-center gap-4 sm:gap-6 mt-8">
                <UiAccentButton :to="localePath(firstSlide.learnMoreUrl)" size="lg" class="w-full sm:w-auto sm:px-10 sm:py-4 font-bold" data-testid="slider-fallback-cta">
                  {{ $t('products.pricing.cta.learnMore') }}
                  <ArrowRight :size="18" :stroke-width="3" />
                </UiAccentButton>
              </div>
            </UiGlassPanel>
          </UiContainer>
        </UiAccent>
      </template>

      <Swiper
        :key="sliderItems.length"
        :modules="modules"
        :slides-per-view="1"
        :space-between="0"
        :autoplay="sliderItems.length > 1 ? { delay: 5000, disableOnInteraction: false } : false"
        :navigation="{ nextEl: '.swiper-button-next-custom', prevEl: '.swiper-button-prev-custom' }"
        :pagination="{ clickable: true }"
        :loop="sliderItems.length > 1"
        :speed="800"
        class="h-full w-full"
      >
        <SwiperSlide v-for="(item, index) in sliderItems" :key="item.id" class="relative h-full w-full">
          <UiAccent :color="item.color">
            <!-- Background with a Ken Burns effect -->
            <div :class="[item.image ? '' : 'bg-[radial-gradient(ellipse_at_60%_40%,color-mix(in_srgb,var(--accent)_19%,transparent)_0%,var(--ui-page)_70%)]', 'absolute inset-0 overflow-hidden']">
              <UiImage
                v-if="item.image"
                :src="item.image"
                :alt="item.name"
                :loading="index === 0 ? 'eager' : 'lazy'"
                :fetchpriority="index === 0 ? 'high' : 'auto'"
                class="h-full w-full animate-ken-burns"
              />
              <div class="absolute inset-0 bg-gradient-to-b from-black/80 via-black/40 to-black/90" />
              <UiDecor class="inset-0 opacity-[0.03] slider-grain" />
            </div>

            <!-- Content -->
            <UiContainer class="relative z-10 flex h-full items-center justify-center">
              <UiGlassPanel
                v-motion
                class="max-w-4xl text-center mb-10 sm:mb-0"
                :initial="{ opacity: 0, scale: 0.9, y: 30 }"
                :enter="{ opacity: 1, scale: 1, y: 0, transition: { duration: 800, type: 'spring' } }"
              >
                <UiAccentTile
                  v-motion
                  :icon="`mdi:${item.icon}`"
                  size="xl"
                  :initial="{ opacity: 0, scale: 0.5, rotate: -20 }"
                  :enter="{ opacity: 1, scale: 1, rotate: 0, transition: { delay: 200, duration: 600 } }"
                  :class="[FLOAT_DELAYS[index % FLOAT_DELAYS.length], 'mb-6 sm:mb-8 mx-auto animate-float']"
                />

                <UiHeading
                  v-motion
                  :level="2"
                  :initial="{ opacity: 0, y: 20 }"
                  :enter="{ opacity: 1, y: 0, transition: { delay: 400, duration: 600 } }"
                  class="mb-4 sm:mb-6 text-3xl sm:text-5xl md:text-6xl lg:text-7xl font-black tracking-tight leading-none drop-shadow-lg"
                >
                  {{ item.name }}
                </UiHeading>

                <UiText
                  v-motion
                  tone="primary"
                  :initial="{ opacity: 0, y: 15 }"
                  :enter="{ opacity: 1, y: 0, transition: { delay: 550, duration: 600 } }"
                  class="mb-4 sm:mb-6 text-lg sm:text-xl md:text-2xl font-semibold tracking-wide uppercase italic ml-2"
                >
                  {{ item.tagline }}
                </UiText>

                <UiText
                  v-motion
                  tone="muted"
                  :initial="{ opacity: 0, y: 15 }"
                  :enter="{ opacity: 1, y: 0, transition: { delay: 700, duration: 600 } }"
                  class="mb-6 sm:mb-8 text-sm sm:text-base md:text-lg lg:text-xl leading-relaxed font-light line-clamp-3 md:line-clamp-none px-4"
                >
                  {{ item.description }}
                </UiText>

                <!-- Features, one after another -->
                <div v-if="item.features.length" class="mb-8 mt-4 flex flex-wrap justify-center gap-3">
                  <span
                    v-for="(feature, featureIndex) in item.features"
                    :key="feature"
                    v-motion
                    :initial="{ opacity: 0, x: -10 }"
                    :enter="{ opacity: 1, x: 0, transition: { delay: 800 + featureIndex * 100 } }"
                    :class="[accent.tintStrong, accent.borderMedium, 'inline-flex items-center gap-2 rounded-full px-4 py-1.5 text-xs sm:text-sm font-bold text-white backdrop-blur-md border transition-all hover:scale-110 cursor-default']"
                  >
                    <CheckCircle :size="14" :stroke-width="3" :class="accent.text" />
                    {{ feature }}
                  </span>
                </div>

                <!-- Calls to action -->
                <div
                  v-motion
                  :initial="{ opacity: 0, y: 20 }"
                  :enter="{ opacity: 1, y: 0, transition: { delay: 1000, duration: 600 } }"
                  class="flex flex-col sm:flex-row justify-center gap-4 sm:gap-6 mt-8"
                >
                  <UiMagnetic :strength="0.2">
                    <UiAccentButton :to="localePath(item.learnMoreUrl)" size="lg" class="w-full sm:w-auto sm:px-10 sm:py-4 font-bold" :data-testid="`slider-cta-${item.id}`">
                      <span class="truncate">{{ $t('products.pricing.cta.learnMore') }}</span>
                      <ArrowRight :size="18" :stroke-width="3" class="flex-shrink-0" />
                    </UiAccentButton>
                  </UiMagnetic>

                  <UiMagnetic v-if="item.demoUrl" :strength="0.15">
                    <UiButton :href="item.demoUrl" target="_blank" rel="noopener noreferrer" variant="subtle" size="lg" class="w-full sm:w-auto sm:px-10 sm:py-4 font-bold" :data-testid="`slider-demo-${item.id}`">
                      <span class="truncate">{{ $t('products.pricing.cta.viewDemo') }}</span>
                      <PlayCircle :size="18" :stroke-width="2.5" class="ml-2 text-primary-400 flex-shrink-0" />
                    </UiButton>
                  </UiMagnetic>
                </div>
              </UiGlassPanel>
            </UiContainer>
          </UiAccent>
        </SwiperSlide>
      </Swiper>

      <!-- Navigation arrows, desktop only -->
      <UiIconButton
        v-show="sliderItems.length > 1"
        :icon="ChevronLeft"
        :label="$t('common.previous')"
        size="lg"
        class="swiper-button-prev-custom absolute left-4 md:left-8 top-1/2 z-20 -translate-y-1/2 rounded-full backdrop-blur-md hover:scale-110 hidden md:flex"
        testid="slider-prev"
      />
      <UiIconButton
        v-show="sliderItems.length > 1"
        :icon="ChevronRight"
        :label="$t('common.next')"
        size="lg"
        class="swiper-button-next-custom absolute right-4 md:right-8 top-1/2 z-20 -translate-y-1/2 rounded-full backdrop-blur-md hover:scale-110 hidden md:flex"
        testid="slider-next"
      />
    </ClientOnly>

    <!-- Scroll down; hidden on mobile to avoid overlapping the content -->
    <UiIconButton
      :icon="ChevronDown"
      :label="$t('common.scrollDown')"
      class="absolute bottom-10 left-1/2 z-30 -translate-x-1/2 animate-bounce rounded-full backdrop-blur-md hidden sm:flex"
      testid="slider-scroll"
      @click="scrollDown"
    />
  </section>
</template>

<script setup lang="ts">
/**
 * Full-screen product slider hero section.
 *
 * Fetches the slides from the backend through the catalogue store and shows them in a Swiper
 * carousel with a Ken Burns background, a floating icon and staggered feature badges. Every slide
 * is painted in its own brand colour (`UiAccent`). Before the browser takes over, the first slide
 * is rendered by the server so the page is never blank.
 */
import { ArrowRight, CheckCircle, ChevronDown, ChevronLeft, ChevronRight, PlayCircle } from 'lucide-vue-next'
import { Swiper, SwiperSlide } from 'swiper/vue'
import { Autoplay, Navigation, Pagination } from 'swiper/modules'
import 'swiper/css'
import 'swiper/css/navigation'
import 'swiper/css/pagination'
import { useCatalogStore } from '~/stores/catalog'
import { ACCENT_CLASSES } from '~/utils/ui/accent'

/** Alt text of that image when there is no slide at all: the operator's site name. */
const { name: SITE_NAME } = useSiteIdentity()

/** Float delay of the icon on each slide in turn, so the icons do not bob in step. */
const FLOAT_DELAYS = ['[animation-delay:0s]', '[animation-delay:0.2s]', '[animation-delay:0.4s]', '[animation-delay:0.6s]', '[animation-delay:0.8s]', '[animation-delay:1s]']

/** Accent classes the slides read (the colour itself is set by each slide's UiAccent). */
const accent = ACCENT_CLASSES

/** Translation helpers and the active language. */
const { locale } = useI18n()
/** Prefixes a path with the active language. */
const localePath = useLocalePath()
/** The catalogue: products, plans, slides and TLD prices. */
const catalogStore = useCatalogStore()

/** Swiper modules the carousel uses. */
const modules = [Autoplay, Navigation, Pagination]

/** The section element, measured by {@link scrollDown}. */
const sectionRef = ref<HTMLElement | null>(null)

// The slides are re-read when the language changes.
await useAsyncData('home:slides', () => catalogStore.loadSlides(locale.value), { watch: [locale] })

/** The slides shaped for the template. */
const sliderItems = computed(() => (catalogStore.slides ?? []).map((slide) => ({
  id: String(slide.id),
  name: slide.title,
  tagline: slide.tagline ?? '',
  description: slide.description ?? '',
  features: slide.features ?? [],
  icon: slide.iconName,
  color: slide.brandColor,
  image: slide.imageUrl,
  demoUrl: slide.demoUrl ?? '',
  learnMoreUrl: slide.learnMoreUrl ?? slide.ctaUrl ?? '/',
})))

/** The first slide, which the server-rendered fallback shows. */
const firstSlide = computed(() => sliderItems.value[0])

/** Scrolls the viewport down to the bottom of this section. */
const scrollDown = (): void => {
  const section = sectionRef.value
  if (!section) {
    return
  }
  const bottom = section.getBoundingClientRect().bottom + window.scrollY
  window.scrollTo({ top: bottom, behavior: 'smooth' })
}
</script>
