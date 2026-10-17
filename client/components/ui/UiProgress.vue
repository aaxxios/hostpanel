<template>
  <div
    ref="trackRef"
    role="progressbar"
    :aria-label="label"
    aria-valuemin="0"
    aria-valuemax="100"
    :aria-valuenow="percent"
    data-testid="ui-progress"
    class="h-1.5 w-full rounded-full bg-gray-200 dark:bg-white/10 overflow-hidden"
  >
    <div
      data-testid="ui-progress-bar"
      :class="['h-full rounded-full transition-all duration-500 w-[var(--ui-progress,0%)]', TONE_CLASSES[tone as UiTone]]"
    />
  </div>
</template>

<script setup lang="ts">
/**
 * UiProgress - a thin horizontal bar showing how much of a whole is used or done.
 *
 * The width is the CSS custom property `--ui-progress`, written on the track from script, and a
 * Tailwind class reads it. That keeps the component free of an inline `style` attribute while the
 * value stays dynamic. The value is clamped to 0-100 and a non-number reads as empty.
 */
import { computed, onMounted, ref, watch } from 'vue'
import { UiTone } from '~/utils/ui/enums'

/** Props of UiProgress. */
interface Props {
  /** How full the bar is, as a percentage from 0 to 100. */
  value: number
  /** Accessible name of the bar (what is being measured). */
  label: string
  /** Colour of the filled part. */
  tone?: `${UiTone}`
}

/** Props of the component with their defaults applied. */
const props = withDefaults(defineProps<Props>(), {
  tone: UiTone.Primary,
})

/** Fill colour per tone. */
const TONE_CLASSES: Record<UiTone, string> = {
  [UiTone.Default]: 'bg-gray-500',
  [UiTone.Primary]: 'bg-primary-500',
  [UiTone.Secondary]: 'bg-secondary-500',
  [UiTone.Success]: 'bg-green-500',
  [UiTone.Warning]: 'bg-yellow-500',
  [UiTone.Danger]: 'bg-red-500',
  [UiTone.Info]: 'bg-blue-500',
}

/** The track element the custom property is written on. */
const trackRef = ref<HTMLElement | null>(null)

/** The value as a whole percentage between 0 and 100. */
const percent = computed(() => {
  if (!Number.isFinite(props.value)) {
    return 0
  }
  return Math.round(Math.min(100, Math.max(0, props.value)))
})

/** Writes the custom property the bar's width class reads. */
const applyWidth = (): void => {
  trackRef.value?.style.setProperty('--ui-progress', `${percent.value}%`)
}

onMounted(applyWidth)
watch(percent, applyWidth)
</script>
