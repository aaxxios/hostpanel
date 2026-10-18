<script setup lang="ts">
/**
 * A swatch of a colour that is only known at run time: a filled block, a filled button sample,
 * or coloured text.
 *
 * A Tailwind class cannot carry a colour the operator typed, and an inline `style` is not allowed,
 * so the swatch is an SVG whose `fill` attributes take the values. Give it a `label` to draw a
 * line of text; give it a `color` to fill the background; give it both for a button sample.
 */
import { computed } from 'vue'

/** Props for UiColorSwatch. */
const props = withDefaults(defineProps<{
  /** Fill of the block; omit for text only. */
  color?: string
  /** Colour of the label; the surrounding text colour when omitted. */
  textColor?: string
  /** Text drawn on the swatch (already translated). */
  label?: string
  /** Native tooltip, e.g. the hex value. */
  title?: string
}>(), {
  color: undefined,
  textColor: undefined,
  label: undefined,
  title: undefined,
})

/** Width of the drawing: a text sample is wider than a plain block. */
const width = computed<number>(() => (props.label === undefined ? 24 : Math.max(48, props.label.length * 8 + 24)))
</script>

<template>
  <svg
    class="block h-8"
    :class="label === undefined ? 'w-full' : 'w-auto'"
    :viewBox="`0 0 ${width} 32`"
    :preserveAspectRatio="label === undefined ? 'none' : 'xMidYMid meet'"
    role="img"
    :aria-label="title ?? label"
  >
    <title v-if="title">{{ title }}</title>
    <rect v-if="color" x="0" y="0" :width="width" height="32" :rx="label === undefined ? 0 : 6" :fill="color" />
    <text
      v-if="label !== undefined"
      :x="width / 2"
      y="21"
      text-anchor="middle"
      font-size="12"
      font-weight="600"
      :fill="textColor ?? 'currentColor'"
    >{{ label }}</text>
  </svg>
</template>
