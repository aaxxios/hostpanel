<template>
  <section class="py-10 sm:py-16 bg-ui-page relative overflow-hidden" data-testid="sections-partners">
    <!-- Background -->
    <UiDecor class="inset-0">
      <UiDecor class="inset-0 bg-gradient-to-b from-transparent via-white/[0.02] to-transparent" />
      <UiDecor class="top-10 left-10 w-2 h-2 bg-primary-400/40 rounded-full animate-float" />
      <UiDecor class="top-20 right-20 w-3 h-3 bg-primary-400/30 dark:bg-secondary-400/30 rounded-full animate-float [animation-delay:1s]" />
      <UiDecor class="bottom-10 left-1/3 w-2 h-2 bg-cyan-400/40 rounded-full animate-float [animation-delay:2s]" />
    </UiDecor>

    <UiContainer class="relative z-10">
      <!-- Header -->
      <div
        v-motion
        :initial="{ opacity: 0, y: 20 }"
        :visible-once="{ opacity: 1, y: 0, transition: { duration: 500 } }"
        class="text-center mb-8 sm:mb-12"
      >
        <UiText size="sm" tone="subtle" class="uppercase tracking-wider mb-2">{{ $t('clients.subtitle') }}</UiText>
        <UiHeading :level="3" size="subsection" class="text-xl">{{ $t('clients.heading') }}</UiHeading>
      </div>

      <!-- Logos marquee -->
      <div class="relative overflow-hidden">
        <!-- Fade edges -->
        <UiDecor class="left-0 top-0 bottom-0 w-20 bg-gradient-to-r from-ui-page to-transparent z-10" />
        <UiDecor class="right-0 top-0 bottom-0 w-20 bg-gradient-to-l from-ui-page to-transparent z-10" />

        <!-- Scrolling logos (the list is doubled so the loop has no gap) -->
        <div class="flex animate-marquee hover:[animation-play-state:paused] py-4">
          <div
            v-for="(partner, index) in loopedClients"
            :key="index"
            class="flex-shrink-0 mx-3 sm:mx-6 flex items-center justify-center"
          >
            <UiCard
              padding="none"
              class="px-4 py-2.5 sm:px-6 sm:py-3 group hover:border-primary-500/50 hover:scale-105 hover:shadow-xl hover:shadow-primary-500/30 transition-all duration-300"
              :data-testid="`partner-${partner.id}`"
            >
              <div class="flex items-center gap-2">
                <Icon :name="partner.icon" class="text-xl sm:text-2xl text-gray-500 group-hover:text-primary-600 dark:group-hover:text-primary-400 transition-colors duration-300 flex-shrink-0" />
                <span class="text-sm sm:text-base font-semibold text-gray-500 group-hover:text-gray-900 dark:group-hover:text-white transition-colors duration-300 whitespace-nowrap">{{ partner.name }}</span>
              </div>
            </UiCard>
          </div>
        </div>
      </div>

      <!-- Stats bar -->
      <UiGrid cols="4" gap="sm" class="mt-8 sm:mt-12 md:grid-cols-4 sm:gap-6">
        <UiCard
          v-for="(stat, index) in partnerStats"
          :key="stat.label"
          v-motion
          :initial="{ opacity: 0, y: 30 }"
          :visible-once="{ opacity: 1, y: 0, transition: { delay: index * 100, duration: 400 } }"
          padding="sm"
          class="relative text-center group hover:border-primary-500/30 hover:-translate-y-1 transition-all duration-300"
          :data-testid="`partner-stat-${index}`"
        >
          <UiDecor class="inset-0 rounded-xl opacity-0 group-hover:opacity-100 transition-opacity duration-500 bg-gradient-to-br from-primary-500/10 to-primary-500/10 dark:to-secondary-500/10" />

          <div class="relative">
            <div class="text-2xl md:text-3xl font-bold text-gray-900 dark:text-white mb-1 group-hover:scale-110 transition-transform duration-300">{{ stat.value }}</div>
            <div class="text-xs text-gray-500 uppercase tracking-wider">{{ stat.label }}</div>
          </div>

          <UiDecor class="top-0 right-0 w-8 h-8 border-r-2 border-t-2 border-primary-500/40 rounded-tr-xl opacity-0 group-hover:opacity-100 transition-opacity duration-300" />
        </UiCard>
      </UiGrid>
    </UiContainer>

    <!-- Decorative corner accents -->
    <UiDecor class="top-0 right-0 w-32 h-32 border-r-2 border-t-2 border-primary-500/25 dark:border-secondary-500/25" />
    <UiDecor class="bottom-0 left-0 w-32 h-32 border-l-2 border-b-2 border-primary-500/25" />
  </section>
</template>

<script setup lang="ts">
/**
 * Partners section: a marquee of client names and a bar of company figures.
 * Uses the client list from lib/data.ts.
 */
import { clients } from '~/lib/data'

/** Translation helpers and the active language. */
const { t } = useI18n()

/** The clients twice over, so the marquee loops without a gap. */
const loopedClients = [...clients, ...clients]

/** The four company figures under the marquee. */
const partnerStats = computed(() => [
  { value: '50+', label: t('clients.stats.clients') },
  { value: '15+', label: t('clients.stats.countries') },
  { value: '$10M+', label: t('clients.stats.revenue') },
  { value: '99%', label: t('clients.stats.retention') },
])
</script>
