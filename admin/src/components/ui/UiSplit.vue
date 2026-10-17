<script setup lang="ts">
/**
 * Two-column layout (main area and aside) that stacks on narrow screens.
 *
 * Put exactly two children in the default slot; `ratio` decides how the width is shared.
 */
import { computed } from 'vue'
import { gapClasses, splitRatioClasses } from '@/utils/ui/layout'
import type { UiGap, UiSplitRatio } from '@/utils/ui/layout'

/** Props for UiSplit. */
const props = withDefaults(defineProps<{
  /** Width share of main : aside. */
  ratio?: UiSplitRatio
  /** Space between the columns. */
  gap?: UiGap
}>(), {
  ratio: 'two-thirds',
  gap: 'lg',
})

/** Layout classes. */
const classes = computed<string[]>(() => [splitRatioClasses[props.ratio], gapClasses[props.gap]])
</script>

<template>
  <div class="grid" :class="classes">
    <slot />
  </div>
</template>
