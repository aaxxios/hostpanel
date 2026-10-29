<script setup lang="ts">
/**
 * Home sections card: one switch per section of the storefront home page.
 *
 * Each switch is its own settings row (`portal.sections.*`) and saves on its own, so turning one
 * section off cannot ride along with another change. The hero is not listed because it cannot be
 * hidden. A section the template does not draw is simply not affected by its switch.
 */
import { useI18n } from 'vue-i18n'
import SettingToggleField from '@/components/settings/SettingToggleField.vue'
import SettingsSection from '@/components/settings/SettingsSection.vue'
import { HOME_SECTION_DEFAULT, HOME_SECTION_SETTING, HomeSection } from '@/types/homesection'
import type { Setting, SettingSave } from '@/types/setting'

const { t } = useI18n()

/** Props for HomeSectionsCard. */
defineProps<{
  /** All system settings, as loaded from the backend. */
  settings: Setting[]
  /** Persists one row and answers how it went; each control shows its own outcome. */
  save: SettingSave
}>()

/** Every section, in the order the card lists them. */
const SECTIONS: HomeSection[] = Object.values(HomeSection)
</script>

<template>
  <SettingsSection
    :title="t('settings.homeSections.title')"
    :description="t('settings.homeSections.description')"
  >
    <SettingToggleField
      v-for="section in SECTIONS"
      :key="section"
      :settings="settings"
      :setting-key="HOME_SECTION_SETTING[section]"
      :label="t(`settings.homeSections.sections.${section}.label`)"
      :hint="t(`settings.homeSections.sections.${section}.hint`)"
      :default-on="HOME_SECTION_DEFAULT[section]"
      :testid="`home-section-${section}`"
      :save="save"
    />
  </SettingsSection>
</template>
