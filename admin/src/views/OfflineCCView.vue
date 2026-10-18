<script setup lang="ts">
/**
 * Offline credit card processing — card payments entered by hand.
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
    :title="t('billing.offlineCC.offlineCreditCardProcessing')"
    :subtitle="t('billing.offlineCC.processOfflineCreditCardPayments')"
    :heading="t('billing.offlineCC.comingSoon')"
    :description="t('billing.offlineCC.comingSoonDescription')"
    :icon="Icon.Dollar"
  />
</template>
