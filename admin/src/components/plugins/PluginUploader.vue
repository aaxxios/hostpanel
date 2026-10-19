<script setup lang="ts">
/**
 * File upload zone for installing a plugin ZIP.
 *
 * The picker is limited to `.zip` through its `accept` filter; whether the archive is a valid
 * plugin is the server's call, and a refusal is shown by the screen as the sentence it returned.
 * A file dropped on the card is taken the same way as one chosen with the picker.
 */
import { ref } from 'vue'
import { useI18n } from 'vue-i18n'
import UiButton from '@/components/ui/UiButton.vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiFileInput from '@/components/ui/UiFileInput.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiText from '@/components/ui/UiText.vue'
import { BYTES_PER_KB } from '@/lib/limits'

/** Translation function. */
const { t } = useI18n()

/** Events emitted by PluginUploader. */
const emit = defineEmits<{
  /** Emitted with the chosen archive when the admin confirms the install. */
  upload: [file: File]
}>()

/** Files currently held by the picker (empty or one). */
const picked = ref<File[]>([])

/** True while a file is dragged over the card. */
const isDragging = ref(false)

/**
 * Takes a file dropped on the card as the selection.
 *
 * @param event - The drop event.
 */
const handleDrop = (event: DragEvent): void => {
  isDragging.value = false
  const file = event.dataTransfer?.files?.[0]
  picked.value = file === undefined ? [] : [file]
}

/** Emits the selected archive and clears the selection. */
const handleUpload = (): void => {
  const file = picked.value[0]
  if (file === undefined) {
    return
  }
  emit('upload', file)
  picked.value = []
}

/** Forgets the selected archive. */
const clearFile = (): void => {
  picked.value = []
}
</script>

<template>
  <UiCard
    padding="lg"
    class="border-2 border-dashed"
    :class="isDragging ? 'border-primary-500' : ''"
    data-testid="plugin-uploader"
    @dragover.prevent="isDragging = true"
    @dragleave.prevent="isDragging = false"
    @drop.prevent="handleDrop"
  >
    <UiStack gap="sm" align="center">
      <UiText tone="primary" size="lg" class="font-medium">
        {{ t('plugins.pluginUploader.dropYourPlugin') }}
        {{ t('plugins.pluginUploader.zip') }}
        {{ t('plugins.pluginUploader.here') }}
      </UiText>
      <UiText tone="muted" size="sm">{{ t('plugins.pluginUploader.orClickToBrowseFiles') }}</UiText>

      <UiFileInput v-model="picked" accept=".zip" data-testid="plugin-zip-input" />

      <UiStack v-if="picked.length > 0" direction="row" align="center" gap="md" wrap>
        <UiText tone="primary" size="md" class="font-mono" data-testid="plugin-selected-name">
          {{ picked[0]?.name }}
        </UiText>
        <UiText tone="muted" size="sm">
          {{ t('plugins.pluginUploader.valueKbReadyToInstall', { value: ((picked[0]?.size ?? 0) / BYTES_PER_KB).toFixed(1) }) }}
        </UiText>
        <UiButton type="button" variant="ghost" size="sm" data-testid="plugin-cancel" @click="clearFile">
          {{ t('shared.cancel') }}
        </UiButton>
        <UiButton type="button" variant="primary" size="sm" data-testid="plugin-install" @click="handleUpload">
          {{ t('plugins.pluginUploader.installPlugin') }}
        </UiButton>
      </UiStack>
    </UiStack>
  </UiCard>
</template>
