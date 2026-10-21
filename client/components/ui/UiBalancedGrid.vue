<template>
  <component :is="as" :class="classes" :data-columns="columns">
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

/** Classes that size a child to one column on a tablet-width screen, keyed by column count. */
const TABLET_CHILD_CLASSES: Record<number, string> = {
  1: 'sm:[&>*]:basis-full',
  2: 'sm:[&>*]:basis-[calc((100%-1.5rem)/2)]',
}

/** Classes that size a child to one column on a desktop screen, keyed by column count. */
const DESKTOP_CHILD_CLASSES: Record<number, string> = {
  1: 'lg:[&>*]:basis-full',
  2: 'lg:[&>*]:basis-[calc((100%-1.5rem)/2)]',
  3: 'lg:[&>*]:basis-[calc((100%-3rem)/3)]',
  4: 'lg:[&>*]:basis-[calc((100%-4.5rem)/4)]',
}

/** Classes of the root: a centred wrapping row whose children are sized to the column count. */
const classes = computed(() => [
  'ui-balanced-grid flex flex-wrap justify-center gap-6 [&>*]:min-w-0 [&>*]:flex-none [&>*]:basis-full',
  TABLET_CHILD_CLASSES[tabletColumns(props.count)],
  DESKTOP_CHILD_CLASSES[columns.value],
])
</script>
