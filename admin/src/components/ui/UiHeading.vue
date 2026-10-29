<script setup lang="ts">
/**
 * Heading of a chosen level (1-6).
 *
 * `level` is the semantic level; `variant` is the look (`page`, `section`, `card`), so the visual
 * size does not have to follow the outline. The default `plain` look adds no classes.
 */
import { computed } from 'vue'
import { headingVariantClasses } from '@/utils/ui/text'
import type { UiHeadingVariant } from '@/utils/ui/text'

/** Props for UiHeading. */
const props = withDefaults(defineProps<{
  /** Semantic heading level, 1 to 6. */
  level?: 1 | 2 | 3 | 4 | 5 | 6
  /** Visual look of the heading. */
  variant?: UiHeadingVariant
}>(), {
  level: 2,
  variant: 'plain',
})

/** Classes of the chosen look. */
const classes = computed<string>(() => headingVariantClasses[props.variant])
</script>

<template>
  <component :is="`h${level}`" :class="classes"><slot /></component>
</template>
