<script setup lang="ts">
/**
 * The action bar pinned to the bottom of a full-page settings form (Cancel and Save).
 *
 * Both the slide form and the TLD form are long enough that the buttons would scroll away, so they
 * live in a bar that sticks to the bottom edge of the scrolling content area. It is sticky, not
 * fixed: a fixed bar is laid over the sidebar and the scrollbar as well, and hides the last fields
 * of the form. The negative margins cancel the layout's content gutter so the bar spans the whole
 * content column.
 */
import { useI18n } from 'vue-i18n'
import UiButton from '@/components/ui/UiButton.vue'
import UiStack from '@/components/ui/UiStack.vue'

const { t } = useI18n()

/** Props for SettingsFormFooter. */
defineProps<{
  /** Label of the save button (already translated), e.g. "Create slide" or "Save Changes". */
  saveLabel: string
  /** True while the save request is in flight; the button is disabled and shows the saving label. */
  saving: boolean
}>()

const emit = defineEmits<{
  /** Emitted when the operator presses Cancel. */
  cancel: []
  /** Emitted when the operator presses Save. */
  save: []
}>()
</script>

<template>
  <div class="sticky bottom-0 z-30 -mx-4 -mb-4 border-t border-border bg-surface-card/95 px-6 py-4 backdrop-blur-sm sm:-mx-6 sm:-mb-6 lg:-mx-8 lg:-mb-8">
    <UiStack direction="row" align="center" justify="end" gap="sm">
      <UiButton type="button" variant="secondary" data-testid="settings-form-cancel" @click="emit('cancel')">
        {{ t('shared.cancel') }}
      </UiButton>
      <UiButton
        type="button"
        variant="primary"
        :disabled="saving"
        data-testid="settings-form-save"
        @click="emit('save')"
      >
        {{ saving ? t('shared.saving') : saveLabel }}
      </UiButton>
    </UiStack>
  </div>
</template>
