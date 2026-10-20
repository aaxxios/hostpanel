<template>
  <div ref="rootEl" class="relative">
    <slot name="trigger" :open="modelValue" :toggle="toggle" :close="close" />

    <div
      v-if="modelValue"
      role="menu"
      data-testid="ui-popover-panel"
      :class="panelClasses"
    >
      <slot :close="close" />
    </div>
  </div>
</template>

<script setup lang="ts">
/**
 * UiPopover - a floating panel opened from a trigger, for menus and tile launchers.
 *
 * The trigger lives in the `trigger` slot and receives `open`, `toggle` and `close`; the default
 * slot is the panel body and receives `close`. The panel closes on Escape and on a click anywhere
 * outside the component. Where the panel sits and how wide it is stays with the screen, through
 * `panelClass`, because that depends on the header it hangs from.
 */
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'

/** Props of UiPopover. */
interface Props {
  /** Whether the panel is open (v-model). */
  modelValue?: boolean
  /** Position and width utilities of the panel (`fixed right-4 top-16 w-72`). */
  panelClass?: string
}

/** Props of the component with their defaults applied. */
const props = withDefaults(defineProps<Props>(), {
  modelValue: false,
  panelClass: '',
})

/** Events this component raises. */
const emit = defineEmits<{
  /** Fires when the panel opens or closes. */
  'update:modelValue': [value: boolean]
}>()

/** Root element, used to tell an inside click from an outside one. */
const rootEl = ref<HTMLElement | null>(null)

/** Class list of the panel: the surface, plus the screen's placement. */
const panelClasses = computed(() => [
  'z-40 flex flex-col rounded-2xl border border-gray-200 bg-ui-popover p-2.5 shadow-2xl dark:border-white/15',
  props.panelClass,
].filter(Boolean).join(' '))

/** Opens a closed panel and closes an open one. */
const toggle = (): void => {
  emit('update:modelValue', !props.modelValue)
}

/** Closes the panel. */
const close = (): void => {
  emit('update:modelValue', false)
}

/**
 * Closes the panel when the click landed outside this component.
 *
 * @param event The document click.
 */
const onClickOutside = (event: MouseEvent): void => {
  if (props.modelValue && rootEl.value && !rootEl.value.contains(event.target as Node)) { // target is always a Node for a document click
    close()
  }
}

/**
 * Closes the panel on Escape.
 *
 * @param event The document key press.
 */
const onEscape = (event: KeyboardEvent): void => {
  if (event.key === 'Escape') {
    close()
  }
}

onMounted(() => {
  document.addEventListener('click', onClickOutside)
  document.addEventListener('keydown', onEscape)
})

onBeforeUnmount(() => {
  document.removeEventListener('click', onClickOutside)
  document.removeEventListener('keydown', onEscape)
})
</script>
