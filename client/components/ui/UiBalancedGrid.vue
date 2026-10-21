<template>
  <component :is="as" class="ui-balanced-grid" :style="style" :data-columns="columns">
    <slot />
  </component>
</template>

<script setup lang="ts">
/**
 * UiBalancedGrid - a grid of cards that stays balanced for any item count.
 *
 * Columns come from the item count ({@link balancedColumns}) and every row, the incomplete last
 * one included, is centred, so one, two, five or seven cards never leave an orphan against the
 * left edge. Direct children are sized to a column; they are one column on a phone and at most
 * two on a tablet. Children stretch to the tallest in their row.
 */
import { computed } from 'vue'
import { balancedColumns, tabletColumns } from '~/utils/balancedGrid'

/** Props of UiBalancedGrid. */
interface Props {
  /** Number of items the slot renders; decides the column count. */
  count: number
  /** Tag to render (use `ul` for a list of cards). */
  as?: string
}

/** Props of the component with their defaults applied. */
const props = withDefaults(defineProps<Props>(), { as: 'div' })

/** Columns on a desktop screen. */
const columns = computed(() => balancedColumns(props.count))

/** CSS variables the stylesheet reads. */
const style = computed(() => ({
  '--ui-bg-cols': String(columns.value),
  '--ui-bg-cols-md': String(tabletColumns(props.count)),
}))
</script>

<style>
.ui-balanced-grid {
  --ui-bg-gap: 1.5rem;
  --ui-bg-active: 1;
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: var(--ui-bg-gap);
}

.ui-balanced-grid > * {
  flex: 0 0 calc((100% - (var(--ui-bg-active) - 1) * var(--ui-bg-gap)) / var(--ui-bg-active));
  min-width: 0;
}

@media (min-width: 640px) {
  .ui-balanced-grid {
    --ui-bg-active: var(--ui-bg-cols-md);
  }
}

@media (min-width: 1024px) {
  .ui-balanced-grid {
    --ui-bg-active: var(--ui-bg-cols);
  }
}
</style>
