<script setup lang="ts">
/**
 * Service Addons: add-on services attached to client accounts.
 *
 * The feature is not built yet, so the screen says so instead of showing records that do not exist.
 */
import { useI18n } from 'vue-i18n'
import ComingSoonPage from '@/components/layout/ComingSoonPage.vue'
import { Icon } from '@/lib/icons'

const { t } = useI18n()
</script>

<template>
  <ComingSoonPage
    :title="t('clients.serviceAddons.serviceAddons')"
    :subtitle="t('clients.serviceAddons.manageAddOnServicesFor')"
    :heading="t('clients.serviceAddons.comingSoon')"
    :description="t('clients.serviceAddons.serviceAddonsManagementWillBe')"
    :icon="Icon.Add"
  />
</template>
