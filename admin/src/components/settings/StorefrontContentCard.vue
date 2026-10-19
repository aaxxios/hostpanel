<script setup lang="ts">
/**
 * Storefront card — the home-page figures, the hero badge and the contact details the portal shows.
 *
 * The figures (clients, uptime, regions) and the badge are the numbers and the line in the hero;
 * the contact details feed the footer and the contact page. Each field is saved on its own, so a
 * mistake in one cannot ride along with another. The site name and tagline sit in the identity
 * card above.
 */
import { toRef } from 'vue'
import { useI18n } from 'vue-i18n'
import { useSettingDraft } from '@/composables/useSettingDraft'
import type { SettingDraft } from '@/composables/useSettingDraft'
import SettingNotSeeded from '@/components/settings/SettingNotSeeded.vue'
import SettingSaveButton from '@/components/settings/SettingSaveButton.vue'
import SettingsSection from '@/components/settings/SettingsSection.vue'
import UiInput from '@/components/ui/UiInput.vue'
import UiStack from '@/components/ui/UiStack.vue'
import { SettingEnv, SettingKey } from '@/types/setting'
import type { Setting } from '@/types/setting'
import { SHORT_TEXT_MAX_LENGTH } from '@/lib/limits'

const { t } = useI18n()

/** Props for StorefrontContentCard. */
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

/** One editable field of the card. */
interface StorefrontField {
  /** Settings key the field edits. */
  key: SettingKey
  /** Environment variable the portal falls back to until the row exists. */
  env: string
  /** Suffix of the field's test ids. */
  testid: string
  /** Locale key of the label. */
  labelKey: string
  /** Locale key of the placeholder. */
  hintKey: string
  /** The field's draft state. */
  draft: SettingDraft
}

/** The editable fields, each with its own draft. */
const FIELDS: StorefrontField[] = [
  {
    key: SettingKey.StatsClients,
    env: SettingEnv[SettingKey.StatsClients],
    testid: 'stats-clients',
    labelKey: 'settings.storefrontContent.statsClients.label',
    hintKey: 'settings.storefrontContent.statsClients.hint',
    draft: useSettingDraft(settings, SettingKey.StatsClients),
  },
  {
    key: SettingKey.StatsUptime,
    env: SettingEnv[SettingKey.StatsUptime],
    testid: 'stats-uptime',
    labelKey: 'settings.storefrontContent.statsUptime.label',
    hintKey: 'settings.storefrontContent.statsUptime.hint',
    draft: useSettingDraft(settings, SettingKey.StatsUptime),
  },
  {
    key: SettingKey.StatsRegions,
    env: SettingEnv[SettingKey.StatsRegions],
    testid: 'stats-regions',
    labelKey: 'settings.storefrontContent.statsRegions.label',
    hintKey: 'settings.storefrontContent.statsRegions.hint',
    draft: useSettingDraft(settings, SettingKey.StatsRegions),
  },
  {
    key: SettingKey.HeroBadge,
    env: SettingEnv[SettingKey.HeroBadge],
    testid: 'hero-badge',
    labelKey: 'settings.storefrontContent.heroBadge.label',
    hintKey: 'settings.storefrontContent.heroBadge.hint',
    draft: useSettingDraft(settings, SettingKey.HeroBadge),
  },
  {
    key: SettingKey.ContactEmail,
    env: SettingEnv[SettingKey.ContactEmail],
    testid: 'contact-email',
    labelKey: 'settings.storefrontContent.contactEmail.label',
    hintKey: 'settings.storefrontContent.contactEmail.hint',
    draft: useSettingDraft(settings, SettingKey.ContactEmail),
  },
  {
    key: SettingKey.ContactPhone,
    env: SettingEnv[SettingKey.ContactPhone],
    testid: 'contact-phone',
    labelKey: 'settings.storefrontContent.contactPhone.label',
    hintKey: 'settings.storefrontContent.contactPhone.hint',
    draft: useSettingDraft(settings, SettingKey.ContactPhone),
  },
  {
    key: SettingKey.ContactWhatsapp,
    env: SettingEnv[SettingKey.ContactWhatsapp],
    testid: 'contact-whatsapp',
    labelKey: 'settings.storefrontContent.contactWhatsapp.label',
    hintKey: 'settings.storefrontContent.contactWhatsapp.hint',
    draft: useSettingDraft(settings, SettingKey.ContactWhatsapp),
  },
  {
    key: SettingKey.ContactTelegram,
    env: SettingEnv[SettingKey.ContactTelegram],
    testid: 'contact-telegram',
    labelKey: 'settings.storefrontContent.contactTelegram.label',
    hintKey: 'settings.storefrontContent.contactTelegram.hint',
    draft: useSettingDraft(settings, SettingKey.ContactTelegram),
  },
  {
    key: SettingKey.ContactAddress,
    env: SettingEnv[SettingKey.ContactAddress],
    testid: 'contact-address',
    labelKey: 'settings.storefrontContent.contactAddress.label',
    hintKey: 'settings.storefrontContent.contactAddress.hint',
    draft: useSettingDraft(settings, SettingKey.ContactAddress),
  },
  {
    key: SettingKey.ContactMapUrl,
    env: SettingEnv[SettingKey.ContactMapUrl],
    testid: 'contact-map-url',
    labelKey: 'settings.storefrontContent.contactMapUrl.label',
    hintKey: 'settings.storefrontContent.contactMapUrl.hint',
    draft: useSettingDraft(settings, SettingKey.ContactMapUrl),
  },
]
</script>

<template>
  <SettingsSection
    :title="t('settings.storefrontContent.title')"
    :description="t('settings.storefrontContent.description')"
  >
    <template v-for="field in FIELDS" :key="field.key">
      <SettingNotSeeded v-if="!field.draft.setting.value" :setting-key="field.key" :env-name="field.env" />
      <UiStack v-else direction="row" align="end" wrap>
        <UiInput
          v-model="field.draft.value.value"
          :label="t(field.labelKey)"
          :placeholder="t(field.hintKey)"
          :maxlength="SHORT_TEXT_MAX_LENGTH"
          :data-testid="`storefront-${field.testid}`"
          class="min-w-72 flex-1"
          @keyup.enter="field.draft.submit((id, value) => emit('save', id, value))"
        />
        <SettingSaveButton
          :dirty="field.draft.dirty.value"
          :saving="saving"
          :data-testid="`storefront-save-${field.testid}`"
          @click="field.draft.submit((id, value) => emit('save', id, value))"
        />
      </UiStack>
    </template>
  </SettingsSection>
</template>
