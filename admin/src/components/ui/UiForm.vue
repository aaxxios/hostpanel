<script setup lang="ts">
/**
 * Form.
 *
 * Submitting never reloads the page: the native event is cancelled and re-emitted as `submit`
 * (so `@submit.prevent` on a screen keeps working). `stacked` lays the fields out as a column.
 */
import { computed } from 'vue'

/** Props for UiForm. */
const props = withDefaults(defineProps<{
  /** Lays the children out as a vertical stack with a gap. */
  stacked?: boolean
}>(), {
  stacked: false,
})

const emit = defineEmits<{
  /** Emitted with the (already cancelled) submit event. */
  submit: [event: Event]
}>()

/** Layout classes. */
const classes = computed<string>(() => (props.stacked ? 'flex flex-col gap-4' : ''))

/**
 * Cancels the native submit and re-emits it.
 *
 * @param event - Native submit event.
 */
const onSubmit = (event: Event): void => {
  event.preventDefault()
  emit('submit', event)
}
</script>

<template>
  <form :class="classes" @submit="onSubmit">
    <slot />
  </form>
</template>
