<script setup lang="ts">
/**
 * Pulsing placeholder shown while content loads.
 *
 * `shape` picks a text line, a block or a circle; `lines` repeats the text shape (the last line is
 * shorter, like a paragraph). It is hidden from assistive technology; mark the loading region with
 * `aria-busy` on the container.
 */
import { computed } from 'vue'
import { UiSkeletonShape } from '@/utils/ui/skeleton'

/** Props for UiSkeleton. */
const props = withDefaults(defineProps<{
  /** Shape of the placeholder. */
  shape?: UiSkeletonShape
  /** Number of text lines (for `text` only). */
  lines?: number
}>(), {
  shape: UiSkeletonShape.Text,
  lines: 1,
})

/** Shape classes. */
const shapeClasses: Record<UiSkeletonShape, string> = {
  text: 'h-3 rounded',
  block: 'h-24 rounded-xl',
  circle: 'h-10 w-10 rounded-full',
}

/** Indexes of the lines to draw. */
const rows = computed<number[]>(() => Array.from({ length: props.shape === UiSkeletonShape.Text ? Math.max(1, props.lines) : 1 }, (_, i) => i))

/**
 * Tells whether a line is the last of several, which is drawn shorter.
 *
 * @param index - Zero-based line index.
 * @returns True for the final line of a multi-line placeholder.
 */
const isShortLine = (index: number): boolean => props.shape === UiSkeletonShape.Text && props.lines > 1 && index === rows.value.length - 1

/**
 * Width class of one placeholder: circles keep their intrinsic size, a final text line is shorter.
 *
 * @param index - Zero-based line index.
 * @returns A Tailwind width class, or an empty string for a circle.
 */
const widthClass = (index: number): string => {
  if (props.shape === UiSkeletonShape.Circle) {
    return ''
  }
  return isShortLine(index) ? 'w-2/3' : 'w-full'
}
</script>

<template>
  <div class="flex flex-col gap-2" aria-hidden="true">
    <div
      v-for="row in rows"
      :key="row"
      class="animate-pulse bg-surface-hover"
      :class="[shapeClasses[shape], widthClass(row)]"
    />
  </div>
</template>
