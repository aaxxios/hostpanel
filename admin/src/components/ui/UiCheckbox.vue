<script setup lang="ts">
/**
 * Themed checkbox.
 *
 * Replaces the native `<input type="checkbox">`, whose appearance cannot be themed. Unchecked it is
 * a field-coloured box with a border; checked it fills with the primary colour and shows a tick.
 * It is a `role="checkbox"` button, so it stays keyboard- and screen-reader-operable. Give it an
 * `aria-label` (or wrap it in a `UiLabel`) so it has a name.
 */

/** Props for UiCheckbox. */
const props = withDefaults(defineProps<{
  /** Checked state (two-way bound with `v-model`). */
  modelValue: boolean
  /** Blocks interaction and dims the box. */
  disabled?: boolean
}>(), {
  disabled: false,
})

const emit = defineEmits<{
  /** Emitted with the flipped state when the user toggles the box. */
  'update:modelValue': [value: boolean]
  /** Emitted after `update:modelValue`, for callers that only need to know something changed. */
  change: []
}>()

/** Flips the state unless the checkbox is disabled. */
const toggle = (): void => {
  if (props.disabled) {
    return
  }
  emit('update:modelValue', !props.modelValue)
  emit('change')
}
</script>

<template>
  <button
    type="button"
    role="checkbox"
    :aria-checked="modelValue"
    :disabled="disabled"
    class="w-4 h-4 rounded-[4px] border flex items-center justify-center shrink-0 transition-colors outline-none focus-visible:ring-2 focus-visible:ring-primary-500/50 disabled:opacity-50 disabled:cursor-not-allowed cursor-pointer"
    :class="modelValue
      ? 'bg-primary-500 border-primary-500'
      : 'bg-surface-field border-border hover:border-text-secondary'"
    @click="toggle"
  >
    <svg
      v-if="modelValue"
      class="w-2.5 h-2.5 text-white"
      viewBox="0 0 12 10"
      fill="none"
      stroke="currentColor"
      stroke-width="2"
      stroke-linecap="round"
      stroke-linejoin="round"
    >
      <polyline points="1 5.5 4 8.5 11 1.5" />
    </svg>
  </button>
</template>
