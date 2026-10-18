<script setup lang="ts">
/**
 * Drop zone for a single file: drag a file onto it, or click to browse.
 *
 * It reports the picked file (`select`) and nothing else — the screen sends it through its store.
 * While `uploading` it shows a spinner and the progress caption instead of the invitation. The
 * hidden native input carries `inputTestid` so a flow test can attach a file without a drag.
 */
import { ref } from 'vue'
import { useI18n } from 'vue-i18n'
import UiFileInput from '@/components/ui/UiFileInput.vue'

const { t } = useI18n()

/** Props for UiUploadZone. */
withDefaults(defineProps<{
  /** Accepted MIME types, as the native `accept` attribute. */
  accept?: string
  /** Shows the progress state instead of the invitation. */
  uploading?: boolean
  /** Line under the invitation, e.g. the accepted formats and size limit (already translated). */
  hint?: string
  /** Lower zone, for forms with several of them. */
  compact?: boolean
  /** `data-testid` of the hidden file input. */
  inputTestid?: string
}>(), {
  accept: undefined,
  uploading: false,
  hint: undefined,
  compact: false,
  inputTestid: undefined,
})

const emit = defineEmits<{
  /** Emitted with the file the user picked or dropped. */
  select: [file: File]
}>()

/** Whether a file is currently being dragged over the zone. */
const dragging = ref(false)

/**
 * Reports a file chosen through the native picker and clears the picker so the same file can be
 * chosen again.
 *
 * @param event - The input change event.
 */
const onChange = (event: Event): void => {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]
  if (file) {
    emit('select', file)
  }
  input.value = ''
}

/**
 * Reports a file dropped on the zone.
 *
 * @param event - The drag event.
 */
const onDrop = (event: DragEvent): void => {
  dragging.value = false
  const file = event.dataTransfer?.files?.[0]
  if (file) {
    emit('select', file)
  }
}
</script>

<template>
  <label
    class="flex w-full cursor-pointer flex-col items-center justify-center rounded-lg border-2 border-dashed transition-colors"
    :class="[
      compact ? 'h-20' : 'h-24',
      dragging ? 'border-primary-500 bg-primary-500/5' : 'border-border bg-surface-field hover:border-border-strong',
    ]"
    @dragover.prevent="dragging = true"
    @dragleave.prevent="dragging = false"
    @drop.prevent="onDrop"
  >
    <span v-if="uploading" class="flex items-center gap-2 text-sm text-text-muted" role="status">
      <span class="h-4 w-4 animate-spin rounded-full border-2 border-primary-500/20 border-t-primary-500" />
      {{ t('ui.uploadZone.uploading') }}
    </span>
    <span v-else class="flex flex-col items-center gap-1">
      <svg class="h-5 w-5 text-text-muted" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" aria-hidden="true">
        <path d="M3 16.5v2.25A2.25 2.25 0 005.25 21h13.5A2.25 2.25 0 0021 18.75V16.5m-13.5-9L12 3m0 0l4.5 4.5M12 3v13.5" />
      </svg>
      <span class="text-xs text-text-muted">
        {{ t('ui.uploadZone.dropOr') }} <span class="text-primary-400">{{ t('ui.uploadZone.browse') }}</span>
      </span>
      <span v-if="hint" class="text-[0.65rem] text-text-muted">{{ hint }}</span>
    </span>
    <UiFileInput :accept="accept" class="hidden" :data-testid="inputTestid" @change="onChange" />
  </label>
</template>
