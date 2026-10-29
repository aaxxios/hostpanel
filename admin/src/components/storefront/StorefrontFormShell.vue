<script setup lang="ts">
/**
 * The page shell of a storefront content form: breadcrumb, loading state, the failed-load state with
 * a retry, the Cancel / Save bar with the refusal of a save next to it, and the question asked when
 * the operator leaves with unsaved edits.
 *
 * The sections of the form go in the default slot. Loading, saving and the dirty flag come from
 * `useStorefrontForm`; the shell only draws them and reports Save and Cancel.
 */
import { useI18n } from 'vue-i18n'
import SettingsBreadcrumb from '@/components/settings/SettingsBreadcrumb.vue'
import SettingsFormFooter from '@/components/settings/SettingsFormFooter.vue'
import SettingsLoading from '@/components/settings/SettingsLoading.vue'
import UiDialog from '@/components/ui/UiDialog.vue'
import UiErrorState from '@/components/ui/UiErrorState.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiStatusMessage from '@/components/ui/UiStatusMessage.vue'
import { UiTone } from '@/utils/ui/tone'
import { useUnsavedChangesGuard } from '@/composables/useUnsavedChangesGuard'

const { t } = useI18n()

/** One step of the breadcrumb trail. */
interface TrailStep {
  /** Visible caption (already translated). */
  label: string
  /** Router destination; omitted for the current page. */
  to?: string
}

/** Props for StorefrontFormShell. */
const props = withDefaults(defineProps<{
  /** The trail above the form. */
  breadcrumb: TrailStep[]
  /** Prefix of the test ids: `<prefix>-form-view`, `<prefix>-form-load-error`, `<prefix>-form-error`. */
  testid: string
  /** Caption of the loading state (already translated). */
  loadingLabel: string
  /** Label of the save button (already translated). */
  saveLabel: string
  /** True while the stored entry is loading. */
  loading: boolean
  /** True while the form is being saved. */
  saving: boolean
  /** True when the form has edits not yet saved. */
  dirty: boolean
  /** Sentence for a refused save, null when none. */
  error: string | null
  /** Sentence for a failed load of the stored entry, null when none; shown instead of the form. */
  loadError?: string | null
}>(), {
  loadError: null,
})

const emit = defineEmits<{
  /** Emitted when the operator presses Save. */
  save: []
  /** Emitted when the operator presses Cancel. */
  cancel: []
  /** Emitted when the operator asks to load the stored entry again. */
  retry: []
}>()

/** Parks a route change while the reader is asked about unsaved edits. */
const leaveGuard = useUnsavedChangesGuard(() => props.dirty && !props.saving)
</script>

<template>
  <UiStack gap="none" class="pb-24" :data-testid="`${testid}-form-view`">
    <SettingsBreadcrumb :items="breadcrumb" />

    <SettingsLoading v-if="loading" :message="loadingLabel" />

    <UiErrorState v-else-if="loadError" :message="loadError" :testid="`${testid}-form-load-error`" @retry="emit('retry')" />

    <template v-else>
      <slot />

      <UiStatusMessage :message="error" :tone="UiTone.Danger" class="mb-5" :data-testid="`${testid}-form-error`" />

      <SettingsFormFooter :save-label="saveLabel" :saving="saving" @cancel="emit('cancel')" @save="emit('save')" />
    </template>

    <!-- Unsaved-changes question, asked while the route change waits -->
    <UiDialog
      v-if="leaveGuard.pending.value"
      variant="confirm"
      :title="t('storefront.form.unsavedChanges')"
      :message="t('storefront.form.unsavedChangesMessage')"
      :confirm-label="t('storefront.form.leave')"
      tone="warning"
      @confirm="leaveGuard.confirmLeave"
      @close="leaveGuard.cancelLeave"
    />
  </UiStack>
</template>
