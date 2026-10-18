<script setup lang="ts">
/**
 * Flex layout primitive: children in a column or a row with a chosen gap.
 *
 * Replaces hand-written `flex flex-col gap-4` wrappers so spacing comes from one scale.
 */
import { computed } from 'vue'
import { alignItemsClasses, gapClasses, justifyClasses } from '@/utils/ui/layout'
import type { UiAlignItems, UiDirection, UiGap, UiJustify } from '@/utils/ui/layout'

/** Props for UiStack. */
const props = withDefaults(defineProps<{
  /** Main axis: `column` stacks vertically, `row` lays out horizontally. */
  direction?: UiDirection
  /** Space between children. */
  gap?: UiGap
  /** Cross-axis alignment. */
  align?: UiAlignItems
  /** Main-axis distribution. */
  justify?: UiJustify
  /** Lets a row wrap onto several lines. */
  wrap?: boolean
}>(), {
  direction: 'column',
  gap: 'md',
  align: 'stretch',
  justify: 'start',
  wrap: false,
})

/** Layout classes. */
const classes = computed<string[]>(() => [
  props.direction === 'row' ? 'flex-row' : 'flex-col',
  gapClasses[props.gap],
  alignItemsClasses[props.align],
  justifyClasses[props.justify],
  props.wrap ? 'flex-wrap' : '',
])
</script>

<template>
  <div class="flex" :class="classes">
    <slot />
  </div>
</template>
