<script setup lang="ts">
/**
 * Form label.
 *
 * `variant` picks the look: `field` is the small uppercase caption above a control, `inline`
 * wraps a checkbox/radio with its text, `plain` (default) adds nothing. Attributes (`for`,
 * classes, listeners) land on the native `<label>`.
 */
import { computed } from 'vue'
import { labelVariantClasses } from '@/utils/ui/text'
import type { UiLabelVariant } from '@/utils/ui/text'

/** Props for UiLabel. */
const props = withDefaults(defineProps<{
  /** Visual look of the label. */
  variant?: UiLabelVariant
}>(), {
  variant: 'plain',
})

/** Classes of the chosen look. */
const classes = computed<string>(() => labelVariantClasses[props.variant])
</script>

<template>
  <label :class="classes"><slot /></label>
</template>
