<template>
  <section class="relative min-h-dvh flex items-center justify-center bg-ui-page overflow-hidden" data-testid="sections-hero">
    <!-- Animated background -->
    <UiDecor class="inset-0 overflow-hidden">
      <UiDecor class="top-0 left-1/4 w-[500px] h-[500px] bg-primary-500/30 rounded-full blur-[120px] animate-blob" />
      <UiDecor class="top-1/3 right-1/4 w-[600px] h-[600px] bg-primary-500/30 dark:bg-secondary-500/30 rounded-full blur-[120px] animate-blob animation-delay-2000" />
      <UiDecor class="bottom-0 left-1/2 w-[400px] h-[400px] bg-cyan-500/20 rounded-full blur-[120px] animate-blob animation-delay-4000" />
      <UiDecor class="inset-0 opacity-[0.02] pattern-grid-50" />
      <!-- Vignette for depth -->
      <UiDecor class="inset-0 bg-gradient-radial from-transparent via-transparent to-ui-page" />
    </UiDecor>

    <UiContainer class="relative z-10 py-20">
      <div class="text-center max-w-5xl mx-auto">
        <!-- Eyebrow badge -->
        <span
          class="inline-flex items-center gap-2 px-4 py-2 mb-8 rounded-full glass border border-primary-500/20 backdrop-blur-sm animate-reveal-up motion-reduce:animate-none"
        >
          <span class="relative flex h-2 w-2">
            <span class="animate-ping absolute inline-flex h-full w-full rounded-full bg-primary-400 opacity-75" />
            <span class="relative inline-flex rounded-full h-2 w-2 bg-primary-500" />
          </span>
          <UiText as="span" size="sm" weight="medium" tone="muted">{{ $t('hero.badge') }}</UiText>
        </span>

        <!-- Main headline with an animated gradient -->
        <UiHeading :level="1" size="display" class="text-5xl sm:text-6xl md:text-7xl lg:text-8xl mb-6 leading-[1.1] tracking-tight">
          <span
            v-for="(line, index) in titleLines"
            :key="index"
            class="block animate-reveal-up motion-reduce:animate-none"
            :class="revealDelay(300 + index * 150)"
          >
            <span :class="[GRADIENT_DELAYS[index % GRADIENT_DELAYS.length], 'block bg-gradient-to-r from-cyan-400 via-primary-400 to-primary-400 dark:to-secondary-400 bg-clip-text text-transparent animate-gradient bg-300 drop-shadow-2xl']">{{ line }}</span>
          </span>
        </UiHeading>

        <!-- Subtitle -->
        <UiText
          tone="muted"
          size="lg"
          class="md:text-xl lg:text-2xl mb-12 max-w-3xl mx-auto leading-relaxed font-light mt-4 animate-reveal-up motion-reduce:animate-none"
          :class="revealDelay(600)"
        >
          {{ $t('hero.subtitle') }}
        </UiText>

        <!-- Calls to action -->
        <div
          class="flex flex-col sm:flex-row items-center justify-center gap-6 sm:gap-10 mb-16 sm:mb-20 animate-reveal-up motion-reduce:animate-none"
          :class="revealDelay(800)"
        >
          <UiMagnetic :strength="0.3" :range="100">
            <UiButton
              :to="localePath('/contact')"
              size="lg"
              class="group w-full sm:w-auto sm:px-12 sm:py-6 font-bold uppercase tracking-wide bg-gradient-to-br from-primary-600 to-cyan-700 hover:shadow-2xl hover:shadow-primary-500/50 active:scale-95"
              data-testid="hero-primary"
            >
              <span>{{ $t('hero.cta.primary') }}</span>
              <ArrowRight :size="24" :stroke-width="2.5" class="ml-3 group-hover:translate-x-2 transition-transform duration-300" />
            </UiButton>
          </UiMagnetic>

          <UiMagnetic :strength="0.25" :range="100">
            <UiButton
              :to="localePath('/hosting')"
              variant="outline"
              size="lg"
              class="group w-full sm:w-auto sm:px-12 sm:py-6 font-bold uppercase tracking-wide"
              data-testid="hero-secondary"
            >
              <PlayCircle :size="24" :stroke-width="2" class="mr-3 group-hover:rotate-12 transition-transform" />
              <span>{{ $t('hero.cta.secondary') }}</span>
            </UiButton>
          </UiMagnetic>
        </div>

        <!-- Stats: only the figures the operator configured -->
        <div v-if="stats.length > 0" class="grid grid-cols-2 md:grid-cols-3 gap-4 sm:gap-8 lg:gap-12 relative" data-testid="hero-stats">
          <div
            v-for="(stat, index) in stats"
            :key="stat.labelKey"
            class="group relative animate-reveal-up motion-reduce:animate-none"
            :data-testid="`hero-stat-${index}`"
            :class="revealDelay(400 + index * 100)"
            @mouseenter="startStatCounter(index)"
          >
            <div class="relative text-center py-4 px-2">
              <div
                :class="[stat.gradient, 'text-4xl sm:text-5xl lg:text-7xl font-black mb-3 bg-gradient-to-br bg-clip-text text-transparent transition-all duration-500 group-hover:scale-110 drop-shadow-sm tabular-nums']"
              >
                {{ displayedValue(index) }}
              </div>

              <div class="text-xs sm:text-sm text-gray-600 dark:text-gray-400 font-bold uppercase tracking-[0.2em] group-hover:text-gray-700 dark:group-hover:text-gray-200 transition-colors">
                {{ $t(stat.labelKey) }}
              </div>

              <UiDecor :class="[stat.bar, '-bottom-1 left-1/2 -translate-x-1/2 h-1 w-0 group-hover:w-12 transition-all duration-500 rounded-full blur-[2px]']" />
            </div>
          </div>
        </div>

        <!-- Scroll indicator -->
        <div class="absolute bottom-10 left-1/2 -translate-x-1/2 hidden lg:block">
          <UiButton variant="ghost" class="group flex-col gap-3 hover:bg-transparent" data-testid="hero-scroll" @click="scrollToNextSection">
            <span class="text-[10px] text-gray-500 uppercase tracking-[0.4em] font-bold group-hover:text-primary-600 dark:group-hover:text-primary-400 transition-colors">
              {{ $t('hero.scroll') }}
            </span>
            <span class="flex items-start justify-center w-7 h-12 rounded-full border-2 border-gray-200 dark:border-gray-800 p-1.5 transition-colors group-hover:border-primary-500/50">
              <span class="w-1.5 h-3 bg-primary-500 rounded-full animate-scroll-hint" />
            </span>
          </UiButton>
        </div>
      </div>
    </UiContainer>

    <!-- Decorative elements -->
    <UiDecor class="inset-0 select-none">
      <UiDecor
        v-for="(particle, index) in PARTICLES"
        :key="index"
        :class="[particle, 'w-1 h-1 bg-primary-500/30 rounded-full animate-float-particle']"
      />
      <UiDecor class="top-12 right-12 w-48 h-48 border-r border-t border-primary-500/10 dark:border-secondary-500/10 rounded-tr-[40px] hidden md:block" />
      <UiDecor class="bottom-12 left-12 w-48 h-48 border-l border-b border-primary-500/10 rounded-bl-[40px] hidden md:block" />
    </UiDecor>
  </section>
</template>

<script setup lang="ts">
/**
 * Hero of the home page: badge, animated headline, the two calls to action, up to three headline
 * figures and a scroll hint. The figures are operator settings (`portal.stats.*`), each hidden while
 * empty; a figure that is a whole number counts up when hovered (and once on load).
 */

import { revealDelay } from '~/utils/reveal'
import { ArrowRight, PlayCircle } from 'lucide-vue-next'
import { PortalSettingKey } from '~/constants/portalSettingKey'

/** Presentation of one headline figure; its value is an operator setting. */
interface StatDefinition {
  /** Setting that holds the figure. */
  settingKey: PortalSettingKey
  /** Runtime-config fallback of the setting. */
  envKey: string
  /** Locale key of the caption. */
  labelKey: string
  /** Gradient classes of the number. */
  gradient: string
  /** Fill of the bar that grows under the stat on hover. */
  bar: string
}

/** One figure the operator configured, ready to render. */
interface Stat extends StatDefinition {
  /** The figure as the operator wrote it. */
  value: string
  /** Where the counter stops; null when the figure is not a plain number and is shown as written. */
  target: number | null
  /** Text after the counted number. */
  suffix: string
}

/** The headline figures, each with its own gradient (literal classes, so Tailwind emits them). */
const STAT_DEFINITIONS: StatDefinition[] = [
  { settingKey: PortalSettingKey.StatsClients, envKey: 'portalStatsClients', labelKey: 'hero.stats.clients', gradient: 'from-sky-500 to-violet-500', bar: 'bg-sky-500' },
  { settingKey: PortalSettingKey.StatsUptime, envKey: 'portalStatsUptime', labelKey: 'hero.stats.uptime', gradient: 'from-violet-500 to-cyan-500', bar: 'bg-violet-500' },
  { settingKey: PortalSettingKey.StatsRegions, envKey: 'portalStatsRegions', labelKey: 'hero.stats.regions', gradient: 'from-cyan-500 to-sky-500', bar: 'bg-cyan-500' },
]

/** A figure that is a whole number optionally followed by a sign or unit (`150+`); anything else is shown as written. */
const COUNTABLE_FIGURE = /^(\d+)([^\d.,\s].*)?$/

/** Animation delay of each headline line, so the gradient sweeps line after line. */
const GRADIENT_DELAYS = ['[animation-delay:0s]', '[animation-delay:0.5s]', '[animation-delay:1s]', '[animation-delay:1.5s]']

/** Position, delay and pace of the 15 drifting dots; fixed, so the server and the browser draw the same ones. */
const PARTICLES = [
  'left-[6%] top-[18%] [animation-delay:0s] [animation-duration:9s]',
  'left-[14%] top-[62%] [animation-delay:1.2s] [animation-duration:12s]',
  'left-[22%] top-[34%] [animation-delay:2.4s] [animation-duration:7s]',
  'left-[29%] top-[82%] [animation-delay:0.6s] [animation-duration:14s]',
  'left-[36%] top-[12%] [animation-delay:3.1s] [animation-duration:10s]',
  'left-[43%] top-[48%] [animation-delay:1.8s] [animation-duration:8s]',
  'left-[51%] top-[74%] [animation-delay:4.2s] [animation-duration:13s]',
  'left-[58%] top-[26%] [animation-delay:0.3s] [animation-duration:11s]',
  'left-[64%] top-[58%] [animation-delay:2.9s] [animation-duration:9s]',
  'left-[71%] top-[8%] [animation-delay:3.7s] [animation-duration:12s]',
  'left-[78%] top-[44%] [animation-delay:1.5s] [animation-duration:7s]',
  'left-[84%] top-[70%] [animation-delay:4.8s] [animation-duration:14s]',
  'left-[89%] top-[22%] [animation-delay:2.1s] [animation-duration:10s]',
  'left-[94%] top-[54%] [animation-delay:0.9s] [animation-duration:8s]',
  'left-[47%] top-[90%] [animation-delay:3.4s] [animation-duration:13s]',
]

/** How long a counter takes to reach its target, in ms. */
const COUNTER_DURATION = 1500

/** Delay before the counters start on load, in ms. */
const COUNTER_START_DELAY = 800

/** Translation helpers and the active language. */
const { t } = useI18n()
/** Operator settings, with their runtime-config fallbacks. */
const { get } = usePortalSettings()
/** Prefixes a path with the active language. */
const localePath = useLocalePath()

/** The headline split into its lines. */
const titleLines = computed(() => t('hero.title').split('\n'))

/** The figures the operator configured; one that is empty is left out. */
const stats = computed<Stat[]>(() => STAT_DEFINITIONS.flatMap((definition) => {
  const value = get(definition.settingKey, definition.envKey)
  if (value === '') {
    return []
  }
  const match = COUNTABLE_FIGURE.exec(value)
  return [{ ...definition, value, target: match ? Number(match[1]) : null, suffix: match?.[2] ?? '' }]
}))

/** The number each counter currently shows; re-initialised whenever {@link stats} changes. */
const values = ref<number[]>(stats.value.map(() => 0))

/** Running counter timers by stat index: a running counter is not restarted, and all stop when the section goes away. */
const timers = new Map<number, ReturnType<typeof setInterval>>()

/**
 * The text a stat shows right now: the running counter for a countable figure, the figure as written otherwise.
 *
 * @param index Index of the stat in {@link stats}.
 * @returns The text.
 */
const displayedValue = (index: number): string => {
  const stat = stats.value[index]
  if (!stat) {
    return ''
  }
  return stat.target === null ? stat.value : `${values.value[index]}${stat.suffix}`
}

/**
 * Runs one counter up to its target; a figure that is not a plain number has nothing to count.
 *
 * @param index Index of the stat in {@link stats}.
 */
const startStatCounter = (index: number): void => {
  const stat = stats.value[index]
  if (!stat || stat.target === null || timers.has(index) || values.value[index] === stat.target) {
    return
  }
  const target = stat.target

  let current = 0
  const stepTime = Math.max(10, Math.floor(COUNTER_DURATION / Math.max(target, 1)))
  const timer = setInterval(() => {
    current += 1
    values.value[index] = current
    if (current >= target) {
      values.value[index] = target
      clearInterval(timer)
      timers.delete(index)
    }
  }, stepTime)
  timers.set(index, timer)
}

/** Whether the start delay has passed, so counters whose figures change later start at once. */
const countersStarted = ref(false)

/** Stops every running counter. */
const stopCounters = (): void => {
  timers.forEach(timer => clearInterval(timer))
  timers.clear()
}

// The settings can arrive or change after the first render; `values` must follow the figures or
// a stat would read an index that was never initialised ("undefined+").
watch(stats, (next) => {
  stopCounters()
  values.value = next.map(() => 0)
  if (countersStarted.value) {
    next.forEach((_, index) => startStatCounter(index))
  }
})

onMounted(() => {
  setTimeout(() => {
    countersStarted.value = true
    stats.value.forEach((_, index) => startStatCounter(index))
  }, COUNTER_START_DELAY)
})

onBeforeUnmount(stopCounters)

/** Scrolls the page down by one screen. */
const scrollToNextSection = (): void => {
  window.scrollTo({ top: window.innerHeight, behavior: 'smooth' })
}
</script>
