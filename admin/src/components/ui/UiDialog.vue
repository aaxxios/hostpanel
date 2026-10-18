<script setup lang="ts">
/**
 * Modal dialog.
 *
 * `variant="plain"` is a titled panel around the default slot (with an optional `footer` slot).
 * `variant="confirm"` is the confirmation prompt: a `message`, a Cancel button and a confirm
 * button coloured by `tone`; it replaces the former confirmation modal. The panel is teleported to
 * `<body>`, closes on Escape or a backdrop click, keeps Tab inside and returns focus to the opener.
 * Callers mount it with `v-if`, or drive it with `open`.
 */
import { computed, ref, useId } from 'vue'
import { useI18n } from 'vue-i18n'
import UiButton from '@/components/ui/UiButton.vue'
import UiHeading from '@/components/ui/UiHeading.vue'
import UiIconButton from '@/components/ui/UiIconButton.vue'
import UiText from '@/components/ui/UiText.vue'
import { toneSolidClasses } from '@/utils/ui/tone'
import type { UiTone } from '@/utils/ui/tone'
import { useModalSurface } from '@/utils/ui/modalSurface'
import type { UiDialogVariant, UiDialogSize } from '@/utils/ui/dialog'

const { t } = useI18n()

/** Props for UiDialog. */
const props = withDefaults(defineProps<{
  /** Whether the dialog is shown; defaults to true so `v-if` mounting works. */
  open?: boolean
  /** Dialog kind. */
  variant?: UiDialogVariant
  /** Title in the header (already translated). */
  title?: string
  /** Prompt text of the confirm variant. */
  message?: string
  /** Label of the confirm button; defaults to a translated "Confirm". */
  confirmLabel?: string
  /** Label of the confirm button while `loading`; defaults to a translated "Processing...". */
  loadingLabel?: string
  /** Whether the confirmed action is running (disables the confirm button). */
  loading?: boolean
  /** Colour of the confirm button. */
  tone?: UiTone
  /** Panel width. */
  size?: UiDialogSize
  /** Whether a click on the backdrop closes the dialog. */
  closeOnBackdrop?: boolean
}>(), {
  open: true,
  variant: 'plain',
  title: undefined,
  message: undefined,
  confirmLabel: undefined,
  loadingLabel: undefined,
  loading: false,
  tone: 'primary',
  size: 'sm',
  closeOnBackdrop: true,
})

const emit = defineEmits<{
  /** Emitted when the user confirms (confirm variant). */
  confirm: []
  /** Emitted when the user cancels, presses Escape or clicks the backdrop. */
  close: []
}>()

/** Id of the title element, referenced by `aria-labelledby`. */
const titleId = useId()

/** The dialog panel, for focus handling. */
const panel = ref<HTMLElement | null>(null)

/** Panel widths. */
const sizeClasses: Record<UiDialogSize, string> = {
  sm: 'max-w-sm',
  md: 'max-w-md',
  lg: 'max-w-2xl',
  xl: 'max-w-4xl',
}

/** Classes of the confirm button's colour. */
const confirmClasses = computed<string>(() => toneSolidClasses[props.tone])

/** Visible label of the confirm button. */
const confirmText = computed<string>(() => {
  if (props.loading) {
    return props.loadingLabel ?? t('ui.dialog.processing')
  }
  return props.confirmLabel ?? t('ui.dialog.confirm')
})

/** Requests closing. */
const close = (): void => {
  emit('close')
}

/** Keyboard handler: Escape closes, Tab stays inside. */
const onKeydown = useModalSurface(panel, () => props.open, close)

/** Closes on a backdrop click when allowed. */
const onBackdrop = (): void => {
  if (props.closeOnBackdrop) {
    close()
  }
}
</script>

<template>
  <Teleport to="body">
    <div
      v-if="open"
      class="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4"
      @click.self="onBackdrop"
    >
      <div
        ref="panel"
        role="dialog"
        aria-modal="true"
        :aria-labelledby="title ? titleId : undefined"
        tabindex="-1"
        class="bg-surface-card border border-border rounded-xl shadow-2xl w-full max-h-[calc(100dvh-2rem)] overflow-hidden flex flex-col outline-none"
        :class="sizeClasses[size]"
        @keydown="onKeydown"
      >
        <div v-if="title" class="flex items-center justify-between gap-3 px-6 pt-6 pb-4 shrink-0">
          <UiHeading :id="titleId" :level="2" variant="card">
            {{ title }}
          </UiHeading>
          <UiIconButton
            :label="t('ui.dialog.close')"
            size="sm"
            data-testid="dialog-close"
            @click="close"
          >
            <svg class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" aria-hidden="true">
              <line x1="18" y1="6" x2="6" y2="18" />
              <line x1="6" y1="6" x2="18" y2="18" />
            </svg>
          </UiIconButton>
        </div>
        <div class="flex-1 min-h-0 overflow-y-auto px-6 py-2 flex flex-col gap-4" data-testid="dialog-body">
          <UiText v-if="variant === 'confirm' && message" tone="secondary" size="md">
            {{ message }}
          </UiText>
          <slot />
        </div>
        <div v-if="variant === 'confirm'" class="flex justify-end gap-2 px-6 pt-4 pb-6 shrink-0">
          <UiButton variant="secondary" data-testid="dialog-cancel" @click="close">
            {{ t('shared.cancel') }}
          </UiButton>
          <UiButton
            variant="plain"
            :disabled="loading"
            data-testid="dialog-confirm"
            class="px-4 py-2 text-[0.82rem] font-medium rounded-lg transition disabled:opacity-50"
            :class="confirmClasses"
            @click="emit('confirm')"
          >
            {{ confirmText }}
          </UiButton>
        </div>
        <div v-else-if="$slots.footer" class="flex justify-end gap-2 px-6 pt-4 pb-6 shrink-0">
          <slot name="footer" />
        </div>
      </div>
    </div>
  </Teleport>
</template>
