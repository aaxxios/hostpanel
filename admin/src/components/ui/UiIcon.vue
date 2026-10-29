<script setup lang="ts">
/**
 * Line icon drawn from stored SVG markup, without `v-html`.
 *
 * `markup` is the inner SVG (shape tags only, such as `<path>`); it is parsed into nodes
 * whose tags and attributes are whitelisted, so a malformed or hostile string cannot inject
 * script or styles. The icon takes the current text colour.
 */
import { computed } from 'vue'
import { iconSizeClasses } from '@/utils/ui/size'
import type { UiSize } from '@/utils/ui/size'
import { svgNodes } from '@/utils/ui/svgIcon'

/** Props for UiIcon. */
const props = withDefaults(defineProps<{
  /** Inner SVG markup made of shape tags. */
  markup: string
  /** Edge length. */
  size?: UiSize
}>(), {
  size: 'md',
})

/** Size classes. */
const sizeClasses = computed<string>(() => iconSizeClasses[props.size])

/**
 * Functional component that renders the parsed shapes inside the `<svg>`.
 *
 * @returns One virtual node per recognised shape.
 */
const Shapes = () => svgNodes(props.markup)
</script>

<template>
  <svg
    class="shrink-0"
    :class="sizeClasses"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    stroke-width="1.5"
    stroke-linecap="round"
    stroke-linejoin="round"
    aria-hidden="true"
  >
    <Shapes />
  </svg>
</template>
