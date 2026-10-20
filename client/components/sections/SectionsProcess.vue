<template>
  <section class="py-12 md:py-20 bg-ui-page relative overflow-hidden" data-testid="sections-process">
    <!-- Background -->
    <UiDecor class="inset-0">
      <UiDecor class="inset-0 bg-gradient-to-tr from-primary-500/5 dark:from-secondary-500/5 via-transparent to-primary-500/5" />
      <UiDecor class="top-1/4 right-1/4 w-64 h-64 bg-primary-500/15 dark:bg-secondary-500/15 rounded-full blur-[120px]" />
      <UiDecor class="bottom-1/3 left-1/3 w-80 h-80 bg-primary-500/10 rounded-full blur-[140px]" />
      <UiDecor class="top-1/3 left-1/4 w-2 h-2 bg-primary-400/50 dark:bg-secondary-400/50 rounded-full animate-float [animation-delay:0.7s]" />
      <UiDecor class="bottom-1/4 right-1/3 w-3 h-3 bg-primary-400/40 rounded-full animate-float [animation-delay:1.4s]" />
      <UiDecor class="top-0 left-1/4 w-px h-full bg-gradient-to-b from-transparent via-gray-900/5 dark:via-white/5 to-transparent" />
      <UiDecor class="top-0 right-1/3 w-px h-full bg-gradient-to-b from-transparent via-gray-900/5 dark:via-white/5 to-transparent" />
    </UiDecor>

    <UiContainer class="relative z-10">
      <MarketingSectionHeader class="mb-16" :badge="$t('process.badge')" :subtitle="$t('process.description')">
        <template #title>
          {{ $t('process.heading') }} <span class="text-primary-700 dark:text-secondary-400">{{ $t('process.headingHighlight') }}</span>
        </template>
      </MarketingSectionHeader>

      <!-- Process steps -->
      <UiGrid cols="2" gap="lg" class="lg:grid-cols-4">
        <div
          v-for="(step, index) in processSteps"
          :key="step.id"
          class="relative group animate-reveal-up motion-reduce:animate-none"
          :data-testid="`process-step-${step.id}`"
          :style="revealStyle(index * 150)"
        >
          <UiCard class="relative h-full group-hover:-translate-y-2 hover:border-primary-500/50 dark:hover:border-secondary-500/50 transition-all duration-300">
            <!-- Hover glow -->
            <UiDecor class="inset-0 rounded-2xl opacity-0 group-hover:opacity-100 transition-opacity duration-500 bg-gradient-to-br from-primary-500/10 dark:from-secondary-500/10 to-primary-500/5" />

            <!-- Step number -->
            <span :class="[SHADOWS[index % SHADOWS.length], 'relative w-14 h-14 rounded-full bg-gradient-to-br from-primary-500 dark:from-secondary-500 to-primary-500 flex items-center justify-center text-white font-bold text-lg mb-6 group-hover:scale-110 group-hover:rotate-12 transition-all duration-300 shadow-lg']">
              {{ String(step.order).padStart(2, '0') }}
              <UiDecor class="inset-0 rounded-full border-2 border-white/30 opacity-0 group-hover:opacity-100 group-hover:scale-150 transition-all duration-500" />
            </span>

            <Icon :name="step.icon" class="relative text-3xl text-primary-700 dark:text-secondary-400 mb-4 group-hover:scale-110 group-hover:rotate-6 transition-transform duration-300" />

            <UiHeading :level="3" size="subsection" class="relative text-xl mb-3 group-hover:text-primary-700 dark:group-hover:text-secondary-300 transition-colors duration-300">{{ step.title }}</UiHeading>
            <UiText tone="muted" size="sm" class="relative leading-relaxed">{{ step.description }}</UiText>

            <UiDecor class="bottom-0 left-1/2 -translate-x-1/2 h-1 w-0 group-hover:w-20 bg-gradient-to-r from-primary-500 dark:from-secondary-500 to-primary-500 transition-all duration-500 rounded-full" />
            <UiDecor class="top-0 right-0 w-12 h-12 border-r-2 border-t-2 border-primary-500/50 dark:border-secondary-500/50 rounded-tr-xl opacity-0 group-hover:opacity-100 transition-opacity duration-300" />
          </UiCard>

          <!-- Connection arrow (hidden on mobile) -->
          <div v-if="index < processSteps.length - 1" class="hidden lg:block absolute top-10 -right-3 z-10">
            <ArrowRight :size="24" :stroke-width="2" class="text-primary-700/50 dark:text-secondary-500/50" />
          </div>
        </div>
      </UiGrid>
    </UiContainer>

    <!-- Corner decorative accents -->
    <UiDecor class="top-0 right-0 w-40 h-40 border-r-2 border-t-2 border-primary-500/25 dark:border-secondary-500/25" />
    <UiDecor class="bottom-0 left-0 w-40 h-40 border-l-2 border-b-2 border-primary-500/25" />
  </section>
</template>

<script setup lang="ts">
/**
 * Process section showing the work methodology.
 * The steps (id, icon, order) come from constants/processStep.ts; their text from the locale files.
 */

import { revealStyle } from '~/utils/reveal'
import { ArrowRight } from 'lucide-vue-next'
import { PROCESS_STEPS } from '~/constants/processStep'

/** Glow under the step number, alternating between two hues (literal classes, so Tailwind emits them). */
const SHADOWS = ['shadow-purple-500/25', 'shadow-sky-500/25']

/** Translation helpers and the active language. */
const { t } = useI18n()

/** The steps with their translated title and description. */
const processSteps = computed(() => PROCESS_STEPS.map((step) => ({
  ...step,
  title: t(`process.steps.${step.id}.title`),
  description: t(`process.steps.${step.id}.description`),
})))
</script>
