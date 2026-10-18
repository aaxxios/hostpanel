<script setup lang="ts">
/**
 * Table body group (`<tbody>`).
 *
 * `divided` draws a hairline between rows from the border token.
 */

/** Props for UiTableBody. */
withDefaults(defineProps<{
  /** Draws a divider between rows. */
  divided?: boolean
}>(), {
  divided: false,
})
</script>

<template>
  <tbody :class="divided ? 'divide-y divide-border' : ''"><slot /></tbody>
</template>
