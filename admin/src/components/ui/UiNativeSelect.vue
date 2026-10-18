<script setup lang="ts">
/**
 * Native dropdown whose `<option>` children come from the slot.
 *
 * For lists where the options need per-option markup (a preview font, a disabled entry) that the
 * searchable `UiSelect` cannot express. `placeholder` renders a disabled first option shown while
 * nothing is chosen. Emits the raw string, so `v-model.number` converts through Vue's own modifier
 * handling.
 */
import { computed } from 'vue'
import { fieldBaseClasses, fieldInvalidClasses, fieldWidthClasses } from '@/utils/ui/input'
import { fieldSizeClasses } from '@/utils/ui/size'
import type { UiSize } from '@/utils/ui/size'

/** Props for UiNativeSelect. */
const props = withDefaults(defineProps<{
  /** Currently selected option value (two-way bound with `v-model`). */
  modelValue?: string | number
  /**
   * Modifiers of `v-model` (`.number`, `.trim`). Declared so they are not forwarded to the native
   * element as an attribute; Vue applies them itself when the component emits the update.
   */
  modelModifiers?: Record<string, boolean>
  /** Padding and type size. */
  size?: UiSize
  /** Text of the disabled first option, shown while nothing is chosen. */
  placeholder?: string
  /** Marks the value as refused by the server. */
  invalid?: boolean
}>(), {
  modelValue: '',
  modelModifiers: undefined,
  size: 'md',
  placeholder: undefined,
  invalid: false,
})

const emit = defineEmits<{
  /** Emitted with the chosen option's value. */
  'update:modelValue': [value: string]
}>()

/** Field classes. */
const classes = computed<string[]>(() => [
  fieldWidthClasses,
  fieldBaseClasses,
  fieldSizeClasses[props.size],
  props.invalid ? fieldInvalidClasses : '',
])

/**
 * Forwards the native change event as a model update.
 *
 * @param event - Native change event.
 */
const onChange = (event: Event): void => {
  emit('update:modelValue', (event.target as HTMLSelectElement).value)
}
</script>

<template>
  <select
    :value="modelValue"
    :placeholder="placeholder"
    :aria-invalid="invalid || undefined"
    :class="classes"
    @change="onChange"
  >
    <option v-if="placeholder !== undefined" value="" disabled>
      {{ placeholder }}
    </option>
    <slot />
  </select>
</template>
