<template>
  <div :class="cardClasses">
    <slot name="header" />
    <h3
      v-if="title"
      class="text-sm font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider mb-5"
    >
      {{ title }}
    </h3>
    <slot />
    <div v-if="$slots.footer" class="border-t border-gray-200 dark:border-white/10 pt-4 mt-4">
      <slot name="footer" />
    </div>
  </div>
</template>

<script setup lang="ts">
/**
 * UiCard - the portal's surface: a rounded, framed panel on the theme surface colour.
 *
 * Slots: `header` (above the title), default (body), `footer` (below a hairline).
 */
import { computed } from 'vue'
import { UiCardPadding, UiCardTone } from '~/utils/ui/enums'

/** Props of UiCard. */
interface Props {
  /** Small uppercase title drawn above the body. */
  title?: string
  /** Inner padding. */
  padding?: `${UiCardPadding}`
  /** Adds a border highlight on hover. */
  hover?: boolean
  /** `bare` keeps radius, border width and padding and leaves the colours to the screen's class. */
  tone?: `${UiCardTone}`
}

/** Props of the component with their defaults applied. */
const props = withDefaults(defineProps<Props>(), {
  title: undefined,
  padding: UiCardPadding.Lg,
  hover: false,
  tone: UiCardTone.Default,
})

/** Padding utility per step. */
const PADDING_CLASSES: Record<UiCardPadding, string> = {
  [UiCardPadding.None]: '',
  [UiCardPadding.Sm]: 'p-4',
  [UiCardPadding.Md]: 'p-5',
  [UiCardPadding.Lg]: 'p-6',
  [UiCardPadding.Xl]: 'p-8',
}

/** Class list of the root element. */
const cardClasses = computed(() => {
  // A bare card paints nothing, so a screen with its own palette sets surface and border colour.
  const base = props.tone === UiCardTone.Bare
    ? 'rounded-2xl border'
    : 'rounded-2xl bg-white dark:bg-white/5 border border-gray-200 dark:border-white/10'
  const hoverClass = props.hover && props.tone !== UiCardTone.Bare
    ? 'transition-colors duration-200 hover:border-gray-300 dark:hover:border-white/20'
    : ''
  return [base, PADDING_CLASSES[props.padding as UiCardPadding], hoverClass].filter(Boolean).join(' ')
})
</script>
