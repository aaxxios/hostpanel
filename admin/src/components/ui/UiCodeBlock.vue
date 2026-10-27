<script setup lang="ts">
/**
 * A block of text meant to be copied exactly — a shell command, a public key — in a monospace
 * box with a copy button. The text wraps rather than scrolls, so nothing is hidden off screen.
 * `data-testid` falls through to the box; the button carries `<testid>-copy` when one is given.
 */
import { ref, useAttrs } from 'vue'
import UiButton from '@/components/ui/UiButton.vue'

defineOptions({ inheritAttrs: false })

/** Props for UiCodeBlock. */
const props = defineProps<{
  /** The text shown and copied. */
  code: string
  /** Label of the copy button. */
  copyLabel: string
  /** Label shown on the button for a moment after copying. */
  copiedLabel: string
}>()

/** How long the copied state lasts, in milliseconds. */
const COPIED_FOR_MS = 2000

/** Attributes passed by the parent, for the button's derived test id. */
const attrs = useAttrs()

/** Whether the text was just copied. */
const copied = ref(false)

/**
 * Copies the text. A refused clipboard (insecure context, permission) leaves the text selectable
 * in the box, so it needs no message of its own.
 *
 * @returns Resolves when the attempt has ended.
 */
const copy = async (): Promise<void> => {
  try {
    await navigator.clipboard.writeText(props.code)
    copied.value = true
    setTimeout(() => {
      copied.value = false
    }, COPIED_FOR_MS)
  } catch {
    copied.value = false
  }
}
</script>

<template>
  <div class="flex items-start gap-2">
    <code
      v-bind="$attrs"
      class="block min-w-0 flex-1 whitespace-pre-wrap break-all rounded-md border border-border bg-surface-field px-3 py-2 font-mono text-xs text-text-primary"
    >{{ props.code }}</code>
    <UiButton
      type="button"
      variant="secondary"
      size="sm"
      :data-testid="attrs['data-testid'] ? `${attrs['data-testid']}-copy` : undefined"
      @click="copy"
    >
      {{ copied ? props.copiedLabel : props.copyLabel }}
    </UiButton>
  </div>
</template>
