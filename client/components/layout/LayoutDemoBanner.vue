<template>
  <UiAlert v-if="demo.enabled" variant="info" class="rounded-none border-x-0 border-t-0" data-testid="demo-banner">
    <UiStack direction="row" gap="sm" wrap>
      <UiBadge variant="primary" data-testid="demo-banner-label">{{ $t('demo.label') }}</UiBadge>
      <UiText v-if="resetText" as="span" size="sm" data-testid="demo-banner-reset">{{ resetText }}</UiText>
      <UiText as="span" size="sm" tone="muted" data-testid="demo-banner-note">{{ $t('demo.note') }}</UiText>
    </UiStack>
  </UiAlert>
</template>

<script setup lang="ts">
/**
 * Banner shown on every page while the deployment is a public demo: a "Demo mode" label, the time
 * left until the next data reset, and the note that data resets. Renders nothing otherwise.
 */
import { useDemoStore } from '~/stores/demo'

/** How often the countdown is recomputed, in milliseconds. */
const TICK_MS = 30_000

/** Milliseconds in one minute. */
const MINUTE_MS = 60_000

/** Minutes in one hour. */
const HOUR_MINUTES = 60

/** Translator for the strings this file builds itself. */
const { t } = useI18n()

/** Demo store; owns the status the banner renders. */
const demo = useDemoStore()

/** The current time; shared state so server render and hydration agree, advanced on a timer so the countdown moves. */
const now = useState<number>('demo-now', () => Date.now())

/** Handle of the countdown timer, cleared on unmount. */
let timer: ReturnType<typeof setInterval> | undefined

/** Whole minutes left until the next reset, or null when none is scheduled. */
const minutesLeft = computed<number | null>(() => {
  if (!demo.nextResetAt) {
    return null
  }
  const target = Date.parse(demo.nextResetAt)
  if (Number.isNaN(target)) {
    return null
  }
  return Math.max(0, Math.ceil((target - now.value) / MINUTE_MS))
})

/** The sentence about the next reset; empty when no reset is scheduled. */
const resetText = computed<string>(() => {
  const minutes = minutesLeft.value
  if (minutes === null) {
    return ''
  }
  if (minutes < 1) {
    return t('demo.resetsSoon')
  }
  const time = minutes >= HOUR_MINUTES
    ? t('demo.hoursMinutes', { hours: Math.floor(minutes / HOUR_MINUTES), minutes: minutes % HOUR_MINUTES })
    : t('demo.minutes', { minutes })
  return t('demo.resetsIn', { time })
})

onMounted(() => {
  now.value = Date.now()
  timer = setInterval(() => {
    now.value = Date.now()
  }, TICK_MS)
})

onBeforeUnmount(() => {
  if (timer !== undefined) {
    clearInterval(timer)
  }
})
</script>
