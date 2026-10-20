<template>
  <section class="relative py-12 md:py-32 lg:py-40 bg-ui-page overflow-hidden" data-testid="products-hero">
    <!-- Animated background -->
    <UiDecor class="inset-0">
      <UiDecor class="top-0 right-1/4 w-[600px] h-[600px] bg-primary-500/25 dark:bg-secondary-500/25 rounded-full blur-[150px] animate-blob" />
      <UiDecor class="bottom-0 left-1/3 w-[500px] h-[500px] bg-cyan-500/20 rounded-full blur-[140px] animate-blob animation-delay-2000" />
      <UiDecor class="top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[400px] h-[400px] bg-primary-500/15 rounded-full blur-[130px] animate-blob animation-delay-4000" />
      <UiDecor class="inset-0 opacity-[0.025] pattern-grid-80" />
      <UiDecor class="top-1/4 left-1/4 w-3 h-3 bg-primary-400/60 dark:bg-secondary-400/60 rounded-full animate-float [animation-delay:0.5s]" />
      <UiDecor class="top-1/3 right-1/3 w-2 h-2 bg-cyan-400/70 rounded-full animate-float [animation-delay:1.2s]" />
      <UiDecor class="bottom-1/3 left-1/3 w-2 h-2 bg-primary-400/70 rounded-full animate-float [animation-delay:2s]" />
    </UiDecor>

    <UiContainer class="relative z-10">
      <div class="max-w-4xl mx-auto text-center">
        <!-- Badge -->
        <span class="inline-flex items-center gap-2 px-4 py-2 mb-6 rounded-full glass border border-primary-500/20 dark:border-secondary-500/20 backdrop-blur-sm">
          <span class="relative flex h-2 w-2">
            <span class="animate-ping absolute inline-flex h-full w-full rounded-full bg-primary-400 dark:bg-secondary-400 opacity-75" />
            <span class="relative inline-flex rounded-full h-2 w-2 bg-primary-500 dark:bg-secondary-500" />
          </span>
          <UiText as="span" size="sm" weight="medium" tone="muted">{{ $t('products.hero.badge') }}</UiText>
        </span>

        <!-- Headline -->
        <UiHeading :level="1" class="text-5xl md:text-6xl lg:text-7xl mb-8 leading-[1.1]">
          <span class="block text-primary-600 dark:text-primary-400">{{ $t('products.hero.headline1') }}</span>
          <span class="block text-gray-900 dark:text-white mt-4">{{ $t('products.hero.headline2') }}</span>
        </UiHeading>

        <!-- Subtitle -->
        <UiText tone="muted" size="lg" class="text-xl mb-12 leading-relaxed max-w-3xl mx-auto">
          <i18n-t keypath="products.hero.subtitle" tag="span">
            <template #whiteLabel>
              <span class="text-primary-700 dark:text-secondary-400 font-semibold">{{ $t('products.hero.whiteLabelHighlight') }}</span>
            </template>
          </i18n-t>
        </UiText>

        <!-- Calls to action -->
        <div class="flex flex-col sm:flex-row items-center justify-center gap-3 sm:gap-4 mb-12 sm:mb-14 md:mb-16">
          <UiButton
            size="lg"
            class="w-full sm:w-auto group md:px-8 md:py-4 font-semibold bg-gradient-to-r from-primary-600 dark:from-secondary-600 to-cyan-600 hover:scale-105 hover:shadow-2xl hover:shadow-primary-500/50 dark:hover:shadow-secondary-500/50"
            data-testid="products-hero-view"
            @click="scrollToProducts"
          >
            {{ $t('products.hero.cta.viewProducts') }}
            <ArrowDown :size="20" :stroke-width="2" class="ml-2 group-hover:translate-y-1 transition-transform" />
          </UiButton>

          <UiButton
            variant="outline"
            size="lg"
            :to="localePath('/contact')"
            class="w-full sm:w-auto md:px-8 md:py-4 font-semibold hover:scale-105"
            data-testid="products-hero-demo"
          >
            <Calendar :size="20" :stroke-width="2" class="mr-2" />
            {{ $t('products.hero.cta.bookDemo') }}
          </UiButton>
        </div>

        <!-- Trust badges -->
        <div
          class="flex flex-wrap items-center justify-center gap-8 text-sm text-gray-600 dark:text-gray-400 animate-reveal-up motion-reduce:animate-none"
          :style="revealStyle(400)"
        >
          <div class="flex items-center gap-2">
            <ShieldCheck :size="20" :stroke-width="2" class="text-green-600 dark:text-green-400" />
            <span>{{ $t('products.hero.trustBadges.soc2') }}</span>
          </div>
          <div class="flex items-center gap-2">
            <Lock :size="20" :stroke-width="2" class="text-cyan-700 dark:text-cyan-400" />
            <span>{{ $t('products.hero.trustBadges.encryption') }}</span>
          </div>
          <div class="flex items-center gap-2">
            <Trophy :size="20" :stroke-width="2" class="text-primary-700 dark:text-secondary-400" />
            <span>{{ $t('products.hero.trustBadges.uptime') }}</span>
          </div>
        </div>
      </div>
    </UiContainer>

    <!-- Corner decorations -->
    <UiDecor class="top-0 right-0 w-48 h-48 border-r-2 border-t-2 border-cyan-500/30" />
    <UiDecor class="bottom-0 left-0 w-48 h-48 border-l-2 border-b-2 border-primary-500/30 dark:border-secondary-500/30" />
  </section>
</template>

<script setup lang="ts">
/**
 * Hero of the products page: badge, headline, the two calls to action and the trust badges.
 *
 * "View Products" scrolls to the catalogue section of the same page (`products-section`); "Book a
 * Demo" goes to the contact page. Everything it shows is a locale key.
 */

import { revealStyle } from '~/utils/reveal'
import { ArrowDown, Calendar, Lock, ShieldCheck, Trophy } from 'lucide-vue-next'

/** Id of the catalogue section the first call to action scrolls to. */
const CATALOG_SECTION_ID = 'products-section'

/** Prefixes a path with the active language. */
const localePath = useLocalePath()

/** Scrolls the catalogue section into view. */
const scrollToProducts = (): void => {
  document.getElementById(CATALOG_SECTION_ID)?.scrollIntoView({ behavior: 'smooth' })
}
</script>
