<script setup lang="ts">
/**
 * Side panel that slides over the page edge (filters, detail peek).
 *
 * Teleported to `<body>`; closes on Escape or a backdrop click, keeps Tab inside and returns focus
 * to the opener. The default slot is the body and `footer` an optional pinned action row.
 */
import { ref, useId } from 'vue'
import { useI18n } from 'vue-i18n'
import UiHeading from '@/components/ui/UiHeading.vue'
import UiIconButton from '@/components/ui/UiIconButton.vue'
import { useModalSurface } from '@/utils/ui/modalSurface'
import type { UiDrawerSide } from '@/utils/ui/drawer'

const { t } = useI18n()

/** Props for UiDrawer. */
const props = withDefaults(defineProps<{
  /** Whether the drawer is shown. */
  open: boolean
  /** Title in the header (already translated). */
  title: string
  /** Edge the drawer slides from. */
  side?: UiDrawerSide
}>(), {
  side: 'right',
})

const emit = defineEmits<{
  /** Emitted when the user presses Escape, clicks the backdrop or the close button. */
  close: []
}>()

/** Id of the title element, referenced by `aria-labelledby`. */
const titleId = useId()

/** The drawer panel, for focus handling. */
const panel = ref<HTMLElement | null>(null)

/** Placement classes per edge. */
const sideClasses: Record<UiDrawerSide, string> = {
  left: 'left-0 border-r',
  right: 'right-0 border-l',
}

/** Requests closing. */
const close = (): void => {
  emit('close')
}

/** Keyboard handler: Escape closes, Tab stays inside. */
const onKeydown = useModalSurface(panel, () => props.open, close)
</script>

<template>
  <Teleport to="body">
    <div v-if="open" class="fixed inset-0 z-50 bg-black/60" @click.self="close">
      <aside
        ref="panel"
        role="dialog"
        aria-modal="true"
        :aria-labelledby="titleId"
        tabindex="-1"
        class="absolute top-0 bottom-0 w-full max-w-md bg-surface-card border-border shadow-2xl flex flex-col overflow-hidden outline-none"
        :class="sideClasses[side]"
        @keydown="onKeydown"
      >
        <div class="flex items-center justify-between gap-3 px-6 py-4 border-b border-border shrink-0">
          <UiHeading :id="titleId" :level="2" variant="card">
            {{ title }}
          </UiHeading>
          <UiIconButton
            :label="t('ui.dialog.close')"
            size="sm"
            data-testid="drawer-close"
            @click="close"
          >
            <svg class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" aria-hidden="true">
              <line x1="18" y1="6" x2="6" y2="18" />
              <line x1="6" y1="6" x2="18" y2="18" />
            </svg>
          </UiIconButton>
        </div>
        <div class="flex-1 min-h-0 overflow-y-auto p-6" data-testid="drawer-body">
          <slot />
        </div>
        <div v-if="$slots.footer" class="flex justify-end gap-2 px-6 py-4 border-t border-border shrink-0">
          <slot name="footer" />
        </div>
      </aside>
    </div>
  </Teleport>
</template>
