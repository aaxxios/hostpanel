<script setup lang="ts">
/**
 * System settings view — the storefront's identity, content, appearance and branding cards, then the
 * key-value list of every other global setting.
 *
 * A secret row (an integration credential, the setup token) is write only: the API sends whether it
 * is set, never its value, so the row shows a masked placeholder with Replace and Clear instead of
 * the stored text.
 *
 * Only the first load replaces the page with a loading state: every save re-fetches the list, and
 * swapping the cards for a spinner and back would unmount them and drop every unsaved draft in the
 * other cards.
 */
import { onMounted, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import BrandColorsCard from '@/components/settings/BrandColorsCard.vue'
import BrandingCard from '@/components/settings/BrandingCard.vue'
import HomeSectionsCard from '@/components/settings/HomeSectionsCard.vue'
import HostingDetailsCard from '@/components/settings/HostingDetailsCard.vue'
import PortalAppearanceCard from '@/components/settings/PortalAppearanceCard.vue'
import SecretSettingField from '@/components/settings/SecretSettingField.vue'
import SettingsLoading from '@/components/settings/SettingsLoading.vue'
import SiteIdentityCard from '@/components/settings/SiteIdentityCard.vue'
import StorefrontContentCard from '@/components/settings/StorefrontContentCard.vue'
import TrackingChatCard from '@/components/settings/TrackingChatCard.vue'
import TypographyCard from '@/components/settings/TypographyCard.vue'
import UiAlert from '@/components/ui/UiAlert.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiInput from '@/components/ui/UiInput.vue'
import UiPageHeader from '@/components/ui/UiPageHeader.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiTable from '@/components/ui/UiTable.vue'
import UiTableBody from '@/components/ui/UiTableBody.vue'
import UiTableCell from '@/components/ui/UiTableCell.vue'
import UiTableHead from '@/components/ui/UiTableHead.vue'
import UiTableHeader from '@/components/ui/UiTableHeader.vue'
import UiTableRow from '@/components/ui/UiTableRow.vue'
import { useSettingsStore } from '@/stores/settings'
import type { Setting } from '@/types/setting'

const { t } = useI18n()

/** Settings store: the list, the loading flag and the refusal sentence. */
const store = useSettingsStore()

/** True while a save is in flight, so every card disables its Save button. */
const saving = ref(false)

/**
 * Unsaved edits of the table rows, keyed by setting id.
 *
 * The table used to be read-only, which left every seeded key — the storefront's contact details,
 * footer widgets and app launcher among them — changeable only through the API. Editing here is per
 * row rather than a bulk form so a mistake in one value cannot be saved along with the others.
 */
const drafts = ref<Record<number, string>>({})

/**
 * Bumped after every saved table row. It keys the secret controls, so a saved replacement closes
 * its input and shows the masked placeholder again even though the row's "set" state is unchanged.
 */
const savedVersion = ref(0)

onMounted(store.fetchSettings)

/**
 * Persists one setting.
 *
 * The store keeps the refusal sentence in `error` for the banner below. The outcome is returned
 * rather than swallowed, because the caller has to know whether it may drop the operator's text.
 *
 * @param id - Setting ID.
 * @param value - New value.
 * @returns True when the value was stored.
 */
const onSave = async (id: number, value: string): Promise<boolean> => {
  saving.value = true
  const saved = await store.updateSetting(id, value)
  saving.value = false
  return saved
}

/**
 * Current text for a row: the operator's unsaved edit, or the stored value. A secret row starts
 * empty, because its stored value never reaches the browser.
 *
 * @param setting - The settings row.
 * @returns The text the input shows.
 */
const draftOf = (setting: Setting): string => drafts.value[setting.id] ?? (setting.isSecret ? '' : setting.value ?? '')

/**
 * Whether a row has an edit worth saving. For a secret only a typed replacement counts: an empty
 * value would keep the stored one, so there is nothing to save.
 *
 * @param setting - The settings row.
 * @returns True when the draft differs from the stored value.
 */
const isDirty = (setting: Setting): boolean => {
  if (!(setting.id in drafts.value)) {
    return false
  }
  if (setting.isSecret) {
    return (drafts.value[setting.id] ?? '').trim().length > 0
  }
  return drafts.value[setting.id] !== (setting.value ?? '')
}

/**
 * Removes a secret row's stored value and drops any typed replacement.
 *
 * @param setting - The secret settings row.
 */
const clearRow = async (setting: Setting): Promise<void> => {
  saving.value = true
  const cleared = await store.clearSetting(setting.id)
  saving.value = false
  if (cleared) {
    delete drafts.value[setting.id]
  }
}

/**
 * Saves one row and drops its draft, so the input falls back to the stored value.
 *
 * The draft is dropped only on success: dropping it either way would replace whatever the operator
 * typed with the value already stored, leaving an error banner and no way back to the text that
 * caused it.
 *
 * @param setting - The settings row.
 */
const saveRow = async (setting: Setting): Promise<void> => {
  const draft = drafts.value[setting.id]
  if (!isDirty(setting) || draft === undefined) {
    return
  }
  if (await onSave(setting.id, draft)) {
    delete drafts.value[setting.id]
    savedVersion.value += 1
  }
}
</script>

<template>
  <UiStack gap="none" data-testid="system-settings-view">
    <UiPageHeader :title="t('settings.systemSettings')" />

    <SettingsLoading v-if="store.loading && store.settings.length === 0" :message="t('common.loading')" />
    <template v-else>
      <UiAlert v-if="store.error" variant="danger" class="mb-6" data-testid="system-settings-error">
        {{ store.error }}
      </UiAlert>

      <!-- Identity first: the name is the first thing an operator sets, and everything below renders it. -->
      <SiteIdentityCard :settings="store.settings" :saving="saving" @save="onSave" />
      <StorefrontContentCard :settings="store.settings" :saving="saving" @save="onSave" />
      <PortalAppearanceCard :settings="store.settings" :saving="saving" @save="onSave" />
      <HomeSectionsCard :settings="store.settings" :saving="saving" @save="onSave" />
      <HostingDetailsCard :settings="store.settings" :saving="saving" @save="onSave" />
      <BrandColorsCard :settings="store.settings" :saving="saving" @save="onSave" />
      <TypographyCard :settings="store.settings" :saving="saving" @save="onSave" />
      <BrandingCard :settings="store.settings" :saving="saving" @save="onSave" />
      <TrackingChatCard :settings="store.settings" :saving="saving" @save="onSave" />

      <UiCard class="overflow-hidden overflow-x-auto">
        <UiTable density="comfortable" :empty="store.settings.length === 0" :colspan="3">
          <template #empty>
            {{ t('settings.table.noSettings') }}
          </template>
          <UiTableHead>
            <UiTableRow>
              <UiTableHeader align="left">{{ t('settings.table.key') }}</UiTableHeader>
              <UiTableHeader align="left">{{ t('settings.table.value') }}</UiTableHeader>
              <UiTableHeader align="left">{{ t('settings.table.description') }}</UiTableHeader>
            </UiTableRow>
          </UiTableHead>
          <UiTableBody divided>
            <UiTableRow v-for="setting in store.settings" :key="setting.id" hoverable :data-testid="`setting-row-${setting.id}`">
              <UiTableCell class="font-mono text-text-primary">{{ setting.key }}</UiTableCell>
              <UiTableCell>
                <UiStack direction="row" align="center" gap="sm">
                  <SecretSettingField
                    v-if="setting.isSecret"
                    :key="`${setting.id}-${savedVersion}`"
                    :has-value="setting.hasValue"
                    :model-value="draftOf(setting)"
                    size="sm"
                    :disabled="saving"
                    :testid="`setting-secret-${setting.id}`"
                    class="min-w-48 flex-1"
                    @update:model-value="drafts[setting.id] = $event"
                    @clear="clearRow(setting)"
                  />
                  <UiInput
                    v-else
                    :model-value="draftOf(setting)"
                    type="text"
                    size="sm"
                    :placeholder="t('shared.value')"
                    :data-testid="`setting-value-${setting.id}`"
                    class="min-w-48 flex-1"
                    @update:model-value="drafts[setting.id] = $event"
                    @keyup.enter="saveRow(setting)"
                  />
                  <UiButton
                    v-if="isDirty(setting)"
                    type="button"
                    variant="primary"
                    size="sm"
                    :disabled="saving"
                    :data-testid="`setting-save-${setting.id}`"
                    @click="saveRow(setting)"
                  >
                    {{ saving ? t('common.saving') : t('common.save') }}
                  </UiButton>
                </UiStack>
              </UiTableCell>
              <UiTableCell muted>{{ setting.description ?? '—' }}</UiTableCell>
            </UiTableRow>
          </UiTableBody>
        </UiTable>
      </UiCard>
    </template>
  </UiStack>
</template>
