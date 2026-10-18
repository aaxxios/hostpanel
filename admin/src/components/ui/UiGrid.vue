<script setup lang="ts">
/**
 * Grid layout primitive: equal columns that collapse to one on narrow screens.
 */
import { computed } from 'vue'
import { gapClasses, gridColumnClasses } from '@/utils/ui/layout'
import type { UiGap, UiGridColumns } from '@/utils/ui/layout'

/** Props for UiGrid. */
const props = withDefaults(defineProps<{
  /** Column count on wide screens. */
  columns?: UiGridColumns
  /** Space between cells. */
  gap?: UiGap
}>(), {
  columns: 2,
  gap: 'md',
})

/** Layout classes. */
const classes = computed<string[]>(() => [gridColumnClasses[props.columns], gapClasses[props.gap]])
</script>

<template>
  <div class="grid" :class="classes">
    <slot />
  </div>
</template>
