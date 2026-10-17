<template>
  <form novalidate data-testid="ui-form" @submit="onSubmit">
    <!-- Error banner -->
    <Transition
      enter-active-class="transition duration-200 ease-out"
      leave-active-class="transition duration-200 ease-in"
      enter-from-class="opacity-0 -translate-y-1"
      leave-to-class="opacity-0 -translate-y-1"
    >
      <div
        v-if="error"
        role="alert"
        data-testid="ui-form-error"
        class="mb-5 p-4 rounded-xl border border-red-500/30 bg-red-500/10 flex items-start gap-3"
      >
        <AlertCircle :size="16" :stroke-width="2" class="text-red-400 flex-shrink-0 mt-0.5" />
        <p class="text-red-400 text-sm">{{ error }}</p>
      </div>
    </Transition>

    <!-- Success banner -->
    <Transition
      enter-active-class="transition duration-200 ease-out"
      leave-active-class="transition duration-200 ease-in"
      enter-from-class="opacity-0 -translate-y-1"
      leave-to-class="opacity-0 -translate-y-1"
    >
      <div
        v-if="success"
        role="status"
        data-testid="ui-form-success"
        class="mb-5 p-4 rounded-xl border border-green-500/30 bg-green-500/10 flex items-start gap-3"
      >
        <CheckCircle :size="16" :stroke-width="2" class="text-green-400 flex-shrink-0 mt-0.5" />
        <p class="text-green-400 text-sm">{{ success }}</p>
      </div>
    </Transition>

    <!-- Fields -->
    <div :class="spacingClass">
      <slot />
    </div>

    <!-- Actions row -->
    <div v-if="$slots.actions" class="flex gap-3 justify-end mt-6">
      <slot name="actions" />
    </div>
  </form>
</template>

<script setup lang="ts">
/**
 * UiForm - the standard form wrapper.
 *
 * Handles `submit.prevent`, an error and a success banner (the store's `error` / success text),
 * the vertical rhythm between fields, and a right-aligned actions row.
 */
import { computed } from 'vue'
import { AlertCircle, CheckCircle } from 'lucide-vue-next'
import { UiFormSpacing } from '~/utils/ui/enums'

/** Props of UiForm. */
interface Props {
  /** Red banner shown above the fields. */
  error?: string
  /** Green banner shown above the fields. */
  success?: string
  /** Vertical spacing between top-level fields; `none` when the caller lays them out. */
  spacing?: `${UiFormSpacing}`
  /** Lets the browser submit to the form's own `action` (a hosted endpoint); the default intercepts the submit. */
  nativeSubmit?: boolean
}

/** Props of the component with their defaults applied. */
const props = withDefaults(defineProps<Props>(), {
  error: undefined,
  success: undefined,
  spacing: UiFormSpacing.Md,
  nativeSubmit: false,
})

/** Events the form emits. */
const emit = defineEmits<{
  /** Fires on submit; `preventDefault` has already been called unless `nativeSubmit` is set. */
  submit: [event: SubmitEvent]
}>()

/**
 * Handles the form's submit event: intercepts it unless the browser is meant to post natively.
 *
 * @param event The submit event.
 */
const onSubmit = (event: SubmitEvent): void => {
  if (!props.nativeSubmit) {
    event.preventDefault()
  }
  emit('submit', event)
}

/** Spacing utility per step. */
const SPACING_CLASSES: Record<UiFormSpacing, string> = {
  [UiFormSpacing.None]: '',
  [UiFormSpacing.Sm]: 'space-y-3',
  [UiFormSpacing.Md]: 'space-y-4',
  [UiFormSpacing.Lg]: 'space-y-5',
}

/** Class list of the field wrapper. */
const spacingClass = computed(() => SPACING_CLASSES[props.spacing as UiFormSpacing])
</script>
