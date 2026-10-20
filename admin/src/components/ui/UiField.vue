<script setup lang="ts">
/**
 * One form row: caption, the control, and the message under it.
 *
 * The default slot receives `{ id, invalid, describedby }` to bind on the control, so the caption
 * is wired to it (`for`/`id`) and the message is announced (`aria-describedby`). `error` is the
 * sentence the server returned for this field and always wins over `hint`.
 */
import { computed, useId } from 'vue'
import UiLabel from '@/components/ui/UiLabel.vue'
import UiText from '@/components/ui/UiText.vue'

/** Props for UiField. */
const props = withDefaults(defineProps<{
  /** Caption above the control. */
  label?: string
  /** Help line shown while there is no error. */
  hint?: string
  /** Server sentence for this field; turns the row red. */
  error?: string
  /** Marks the caption with an asterisk. */
  required?: boolean
  /** `data-testid` of the error message, for e2e specs. */
  errorTestid?: string
}>(), {
  label: undefined,
  hint: undefined,
  error: undefined,
  required: false,
  errorTestid: undefined,
})

/** Id of the control, generated per instance. */
const id = useId()

/** Id of the message element, referenced by the control's `aria-describedby`. */
const messageId = computed<string>(() => `${id}-message`)

/** Whether the field currently shows an error. */
const invalid = computed<boolean>(() => props.error !== undefined && props.error !== '')

/** Value for the control's `aria-describedby`; absent when there is no message. */
const describedby = computed<string | undefined>(() => {
  if (invalid.value || props.hint !== undefined) {
    return messageId.value
  }
  return undefined
})
</script>

<template>
  <div class="flex flex-col gap-1.5">
    <UiLabel v-if="label !== undefined" :for="id" variant="field">
      {{ label }}
      <span v-if="required" class="text-status-red">*</span>
    </UiLabel>
    <slot :id="id" :invalid="invalid" :describedby="describedby" />
    <UiText
      v-if="invalid"
      :id="messageId"
      role="alert"
      :data-testid="errorTestid"
      tone="danger"
      size="sm"
    >
      {{ error }}
    </UiText>
    <UiText
      v-else-if="hint !== undefined"
      :id="messageId"
      tone="secondary"
      size="xs"
    >
      {{ hint }}
    </UiText>
  </div>
</template>
