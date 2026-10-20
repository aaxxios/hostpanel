<script setup lang="ts">
/**
 * Surface card that groups a block of a screen.
 *
 * Draws the rounded, bordered surface from the theme tokens. `padding` is optional: the default
 * `none` leaves spacing to the caller's classes (`p-5`, `mb-5`, `overflow-hidden`), which fall
 * through to the root, while `sm`/`md`/`lg` give the card its own inner padding.
 */
import { computed } from 'vue'
import type { UiCardPadding } from '@/utils/ui/card'

/** Props for UiCard. */
const props = withDefaults(defineProps<{
  /** Inner padding of the card. */
  padding?: UiCardPadding
  /** Raises the card onto the elevated surface (menus, popovers). */
  elevated?: boolean
}>(), {
  padding: 'none',
  elevated: false,
})

/** Padding classes. */
const paddingClasses: Record<UiCardPadding, string> = {
  none: '',
  sm: 'p-3',
  md: 'p-5',
  lg: 'p-8',
}

/** Surface and padding classes. */
const classes = computed<string[]>(() => [
  props.elevated ? 'bg-surface-elevated shadow-lg' : 'bg-surface-card light:shadow-sm',
  paddingClasses[props.padding],
])
</script>

<template>
  <div class="border border-border rounded-2xl" :class="classes">
    <slot />
  </div>
</template>
