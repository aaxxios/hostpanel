<script setup lang="ts">
/**
 * Warning shown in place of a settings control whose row the API never seeded.
 *
 * The backend has no create endpoint for settings, so a missing key cannot be added from the
 * panel. Saying so beats a control that silently does nothing. Seeding runs on API start, so this
 * only ever appears against an API older than this panel.
 */
import { useI18n } from 'vue-i18n'
import UiAlert from '@/components/ui/UiAlert.vue'

const { t } = useI18n()

/** Props for SettingNotSeeded. */
defineProps<{
  /** The settings key that has no row. */
  settingKey: string
  /** The environment variable the portal falls back to until the row exists. */
  envName: string
}>()
</script>

<template>
  <UiAlert variant="warning" data-testid="setting-not-seeded">
    {{ t('settings.portalAppearance.notSeeded', { key: settingKey, env: envName }) }}
  </UiAlert>
</template>
