<script setup lang="ts">
/**
 * Full CWP integration configuration page.
 *
 * Renders CwpBrandHeader, IntegrationConfigForm, and the provider status card in a two-column
 * layout. Fetches config and server-info in parallel on mount.
 */
import { computed, onMounted, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { useIntegrationsStore } from '@/stores/integrations'
import { useCwpServerInfo } from '@/composables/useCwpServerInfo'
import { IntegrationSlug } from '@/types/integration'
import type { IntegrationConfigPayload } from '@/types/integration'
import { ProviderConnectionState } from '@/types/providerstatus'
import type { ProviderMetric } from '@/types/providerstatus'
import CwpBrandHeader from '@/components/integrations/CwpBrandHeader.vue'
import IntegrationBreadcrumb from '@/components/integrations/IntegrationBreadcrumb.vue'
import IntegrationConfigForm from '@/components/integrations/IntegrationConfigForm.vue'
import ProviderStatusCard from '@/components/integrations/ProviderStatusCard.vue'
import UiAlert from '@/components/ui/UiAlert.vue'
import UiSplit from '@/components/ui/UiSplit.vue'
import UiStack from '@/components/ui/UiStack.vue'

/** Translation function. */
const { t } = useI18n()

/** Integrations store: the CWP config, the test state and the refusal sentence. */
const store = useIntegrationsStore()

/** Live server info and the call that refreshes it. */
const { info, fetch: fetchInfo } = useCwpServerInfo()

/** True while the test connection request is in progress. */
const testing = ref(false)

/** Refusal reported by a failed test, shown under the form. */
const testError = ref<string | null>(null)

/** True while the config save is in progress. */
const saving = ref(false)

/** Derived header status from server-info. */
const headerStatus = computed(() => {
  if (!info.value) {
    return ProviderConnectionState.Unknown
  }
  return info.value.connected ? ProviderConnectionState.Connected : ProviderConnectionState.Error
})

/** Figures of the status card: accounts on the server and the CWP version. */
const metrics = computed<ProviderMetric[]>(() => {
  const figures: ProviderMetric[] = [
    { label: t('shared.accounts'), value: info.value?.accountsCount ?? null, large: true },
  ]
  if (info.value?.cwpVersion) {
    figures.push({ label: t('integrations.providerStatus.cwpVersion'), value: info.value.cwpVersion, mono: true })
  }
  return figures
})

/**
 * Saves the current config to the backend, then re-fetches server-info.
 *
 * @param payload - Updated enabled state and config values from the form.
 * @returns Promise resolving when save and re-fetch complete.
 */
const handleSave = async (payload: IntegrationConfigPayload): Promise<void> => {
  saving.value = true
  await store.saveConfig(IntegrationSlug.Cwp, payload)
  await fetchInfo()
  saving.value = false
}

/**
 * Tests the CWP connection and updates the status card.
 *
 * @returns Promise resolving when test completes.
 */
const handleTest = async (): Promise<void> => {
  testing.value = true
  testError.value = null
  await store.testConnection(IntegrationSlug.Cwp)
  if (store.testResult && !store.testResult.success) {
    testError.value = store.testResult.message
  }
  await fetchInfo()
  testing.value = false
}

/** Loads the config and the live server info in parallel. */
onMounted(async () => {
  await Promise.all([
    store.fetchOne(IntegrationSlug.Cwp),
    fetchInfo(),
  ])
})
</script>

<template>
  <UiStack gap="md" class="w-full">
    <IntegrationBreadcrumb :name="t('integrations.cwpIntegration.centosWebPanel')" />

    <CwpBrandHeader :version="info?.cwpVersion ?? null" :status="headerStatus" />

    <UiSplit>
      <UiStack gap="md">
        <IntegrationConfigForm
          v-if="store.current"
          :integration="store.current"
          :loading="saving || store.loading"
          :testing="store.testing"
          @save="handleSave"
          @test="handleTest"
        />

        <!-- The refusal the test returned, in the server's words -->
        <UiAlert v-if="testError" variant="error" data-testid="cwp-test-error">
          {{ testError }}
        </UiAlert>
      </UiStack>

      <ProviderStatusCard :info="info" :metrics="metrics" :testing="testing" @test="handleTest" />
    </UiSplit>
  </UiStack>
</template>
