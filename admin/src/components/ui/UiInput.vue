<script setup lang="ts">
/**
 * Single-line text field.
 *
 * Draws the themed field (surface, border, focus ring, invalid state) from tokens, so it follows
 * light/dark. With a `label` or an `icon` slot it also renders the caption and the leading icon;
 * otherwise the native `<input>` is the root. Attributes (`class`, `data-testid`, `maxlength`,
 * `min`, `readonly`, `autocomplete`...) always land on the `<input>`. Emits the raw string, so
 * `v-model.number` and `.trim` convert through Vue's own modifier handling.
 */
import { computed, useAttrs, useSlots } from 'vue'
import {
  fieldBaseClasses,
  fieldInvalidClasses,
  fieldWidthClasses,
  UNSTYLED_INPUT_TYPES,
} from '@/utils/ui/input'
import type { UiInputType } from '@/utils/ui/input'
import { fieldSizeClasses } from '@/utils/ui/size'
import type { UiSize } from '@/utils/ui/size'

defineOptions({ inheritAttrs: false })

/** Props for UiInput. */
const props = withDefaults(defineProps<{
  /** Current value (two-way bound with `v-model`). */
  modelValue?: string | number | null
  /**
   * Modifiers of `v-model` (`.number`, `.trim`). Declared so they are not forwarded to the native
   * element as an attribute; Vue applies them itself when the component emits the update.
   */
  modelModifiers?: Record<string, boolean>
  /** Native input type. */
  type?: UiInputType
  /** Padding and type size. */
  size?: UiSize
  /** Caption shown above the field. */
  label?: string
  /** Hint shown while the field is empty. */
  placeholder?: string
  /** Marks the value as refused by the server (red border, `aria-invalid`). */
  invalid?: boolean
}>(), {
  modelValue: '',
  modelModifiers: undefined,
  type: 'text',
  size: 'md',
  label: undefined,
  placeholder: undefined,
  invalid: false,
})

const emit = defineEmits<{
  /** Emitted with the new text on every keystroke. */
  'update:modelValue': [value: string]
}>()

/** Attributes forwarded to the native input. */
const attrs = useAttrs()

/** Slots, to detect the optional leading icon. */
const slots = useSlots()

/** Whether a leading icon is present, which decides the left padding. */
const hasIcon = computed<boolean>(() => slots.icon !== undefined)

/** Whether the label/icon wrapper is rendered around the input. */
const wrapped = computed<boolean>(() => props.label !== undefined || hasIcon.value)

/** Field classes; skipped for types the browser draws itself (a colour well). */
const fieldClasses = computed<string[]>(() => {
  if (UNSTYLED_INPUT_TYPES.includes(props.type)) {
    return []
  }
  return [
    fieldWidthClasses,
    fieldBaseClasses,
    fieldSizeClasses[props.size],
    props.invalid ? fieldInvalidClasses : '',
    hasIcon.value ? 'pl-9' : '',
  ]
})

/**
 * Forwards the native input event as a model update.
 *
 * @param event - Native input event.
 */
const onInput = (event: Event): void => {
  emit('update:modelValue', (event.target as HTMLInputElement).value)
}
</script>

<template>
  <div v-if="wrapped" class="flex flex-col gap-1.5">
    <span
      v-if="label !== undefined"
      class="text-[0.72rem] font-semibold uppercase tracking-[0.08em] text-text-secondary"
    >
      {{ label }}
    </span>
    <div class="relative">
      <span
        v-if="hasIcon"
        class="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-text-secondary flex items-center"
      >
        <slot name="icon" />
      </span>
      <input
        v-bind="attrs"
        :type="type"
        :value="modelValue ?? ''"
        :placeholder="placeholder"
        :aria-label="label"
        :aria-invalid="invalid || undefined"
        :class="fieldClasses"
        @input="onInput"
      >
    </div>
  </div>
  <input
    v-else
    v-bind="attrs"
    :type="type"
    :value="modelValue ?? ''"
    :placeholder="placeholder"
    :aria-invalid="invalid || undefined"
    :class="fieldClasses"
    @input="onInput"
  >
</template>
