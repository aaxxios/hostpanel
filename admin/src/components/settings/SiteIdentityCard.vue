<script setup lang="ts">
/**
 * Site identity card — the name the storefront calls itself and a one-line tagline.
 *
 * The name reaches the browser title's `og:site_name`, every logo's alt text and the structured
 * data; the tagline is the default meta description. Each field is saved on its own, so a mistake
 * in one cannot ride along with the other.
 */
import { toRef } from 'vue'
import { useI18n } from 'vue-i18n'
import { useSettingDraft } from '@/composables/useSettingDraft'
import SettingNotSeeded from '@/components/settings/SettingNotSeeded.vue'
import SettingSaveButton from '@/components/settings/SettingSaveButton.vue'
import SettingsSection from '@/components/settings/SettingsSection.vue'
import UiInput from '@/components/ui/UiInput.vue'
import UiStack from '@/components/ui/UiStack.vue'
import type { Setting } from '@/types/setting'

const { t } = useI18n()

/** Props for SiteIdentityCard. */
const props = defineProps<{
  /** All system settings, as loaded from the backend. */
  settings: Setting[]
  /** True while a save is in flight. */
  saving?: boolean
}>()

const emit = defineEmits<{
  /** Emitted with the row id and the new value when the operator saves a field. */
  save: [id: number, value: string]
}>()

/** The settings list as a ref, which is what the draft composable watches. */
const settings = toRef(props, 'settings')

/** The two identity fields, each with its own draft. */
const FIELDS = [
  {
    key: 'portal.site.name',
    env: 'NUXT_PUBLIC_PORTAL_SITE_NAME',
    testid: 'name',
    labelKey: 'settings.siteIdentity.name.label',
    hintKey: 'settings.siteIdentity.name.hint',
    draft: useSettingDraft(settings, 'portal.site.name'),
  },
  {
    key: 'portal.site.tagline',
    env: 'NUXT_PUBLIC_PORTAL_SITE_NAME',
    testid: 'tagline',
    labelKey: 'settings.siteIdentity.tagline.label',
    hintKey: 'settings.siteIdentity.tagline.hint',
    draft: useSettingDraft(settings, 'portal.site.tagline'),
  },
]
</script>

<template>
  <SettingsSection
    :title="t('settings.siteIdentity.title')"
    :description="t('settings.siteIdentity.description')"
  >
    <template v-for="field in FIELDS" :key="field.key">
      <SettingNotSeeded v-if="!field.draft.setting.value" :setting-key="field.key" :env-name="field.env" />
      <UiStack v-else direction="row" align="end" wrap>
        <UiInput
          v-model="field.draft.value.value"
          :label="t(field.labelKey)"
          :placeholder="t(field.hintKey)"
          :maxlength="200"
          :data-testid="`site-identity-${field.testid}`"
          class="min-w-72 flex-1"
          @keyup.enter="field.draft.submit((id, value) => emit('save', id, value))"
        />
        <SettingSaveButton
          :dirty="field.draft.dirty.value"
          :saving="saving"
          :data-testid="`site-identity-save-${field.testid}`"
          @click="field.draft.submit((id, value) => emit('save', id, value))"
        />
      </UiStack>
    </template>
  </SettingsSection>
</template>
