<script setup lang="ts">
/**
 * Radio button bound with `v-model`.
 *
 * Renders the native `<input type="radio">` so grouping by `name` and arrow-key navigation come
 * from the browser; the accent colour is the primary token and can be overridden by a screen
 * (its `accent-*` class has zero-specificity competition). Emits the value it stands for when the
 * user selects it.
 */

/** Props for UiRadio. */
const props = defineProps<{
  /** Currently selected value of the group (two-way bound with `v-model`). */
  modelValue?: string | number | boolean
  /** Value this radio stands for. */
  value: string | number | boolean
  /** Blocks interaction and dims the radio. */
  disabled?: boolean
}>()

const emit = defineEmits<{
  /** Emitted with this radio's value when the user selects it. */
  'update:modelValue': [value: string | number | boolean]
}>()

/** Reports this radio's value as the group's new selection. */
const onChange = (): void => {
  emit('update:modelValue', props.value)
}
</script>

<template>
  <input
    type="radio"
    :checked="modelValue === value"
    :disabled="disabled"
    class="[:where(&)]:w-4 [:where(&)]:h-4 [:where(&)]:accent-primary-500 cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
    @change="onChange"
  >
</template>
