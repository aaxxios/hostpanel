<script setup lang="ts">
/**
 * Table row.
 *
 * `hoverable` tints the row under the pointer, `selected` marks the chosen row and `bordered`
 * draws the bottom hairline (none after the last row). All colours come from theme tokens.
 */
import { computed } from 'vue'

/** Props for UiTableRow. */
const props = withDefaults(defineProps<{
  /** Tints the row on hover. */
  hoverable?: boolean
  /** Marks the row as selected. */
  selected?: boolean
  /** Draws a bottom border, except on the last row. */
  bordered?: boolean
}>(), {
  hoverable: false,
  selected: false,
  bordered: false,
})

/** State classes. */
const classes = computed<string[]>(() => [
  props.hoverable ? 'hover:bg-surface-hover transition-colors' : '',
  props.selected ? 'bg-primary-500/10' : '',
  props.bordered ? 'border-b border-border last:border-0' : '',
])
</script>

<template>
  <tr :class="classes"><slot /></tr>
</template>
