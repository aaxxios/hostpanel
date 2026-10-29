<script setup lang="ts">
/**
 * The outcome line of one settings save, shown next to the control that was saved.
 *
 * A saved value reads "Changes saved successfully."; a refusal shows the API's own sentence, or the
 * generic no-answer sentence when the API gave none. Nothing shows until a save has finished.
 */
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import UiStatusMessage from '@/components/ui/UiStatusMessage.vue'
import { UiTone } from '@/utils/ui/tone'
import type { SettingSaveResult } from '@/types/setting'

const { t } = useI18n()

/** Props for SettingOutcome. */
const props = defineProps<{
  /** How the last save went; null shows nothing. */
  outcome: SettingSaveResult | null
}>()

/** The sentence to show, or null before a save has finished. */
const message = computed<string | null>(() => {
  if (props.outcome === null) {
    return null
  }
  return props.outcome.saved ? t('shared.changesSavedSuccessfully') : props.outcome.error ?? t('errors.noAnswer')
})

/** Danger for a refusal, success otherwise. */
const tone = computed<UiTone>(() => (props.outcome?.saved === false ? UiTone.Danger : UiTone.Success))
</script>

<template>
  <UiStatusMessage :message="message" :tone="tone" />
</template>
