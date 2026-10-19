<script setup lang="ts">
/**
 * Detail/config page for a single integration.
 *
 * Route: /integrations/:slug
 * Layout: breadcrumb + 2/3 config form + 1/3 status sidebar.
 * Special cases: slug "cwp" renders CwpIntegrationView and "cwp7" renders Cwp7IntegrationView.
 */
import { computed, defineAsyncComponent, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useIntegrationsStore } from '@/stores/integrations'
import { INTEGRATION_META } from '@/types/integrationmeta'
import { IntegrationSlug } from '@/types/integration'
import type { IntegrationConfigPayload } from '@/types/integration'
import IntegrationBreadcrumb from '@/components/integrations/IntegrationBreadcrumb.vue'
import IntegrationConfigForm from '@/components/integrations/IntegrationConfigForm.vue'
import IntegrationStatusSidebar from '@/components/integrations/IntegrationStatusSidebar.vue'
import UiAlert from '@/components/ui/UiAlert.vue'
import UiSkeleton from '@/components/ui/UiSkeleton.vue'
import UiSplit from '@/components/ui/UiSplit.vue'
import UiStack from '@/components/ui/UiStack.vue'
import { AppRoute } from '@/lib/routes'

/** Current route, whose param is the slug. */
const route = useRoute()

/** Router, to leave the CWP page when the plugin is not installed. */
const router = useRouter()

/** Integrations store: the detail, the test state and the refusal sentence. */
const store = useIntegrationsStore()

/** Slug from the current route (e.g. "stripe"). */
const slug = route.params.slug as string

/** True when the current integration is the CWP plugin. */
const isCwp = slug === IntegrationSlug.Cwp

/** True when the current integration is CWP7. */
const isCwp7 = slug === IntegrationSlug.Cwp7

/** Async CWP integration page — only loaded when slug is "cwp". */
const CwpIntegrationView = defineAsyncComponent(
  () => import('@/views/CwpIntegrationView.vue'),
)

/** Async CWP7 integration page — only loaded when slug is "cwp7". */
const Cwp7IntegrationView = defineAsyncComponent(
  () => import('@/views/Cwp7IntegrationView.vue'),
)

/** Static metadata hint for this integration. */
const hint = INTEGRATION_META[slug as IntegrationSlug]?.hint

/**
 * Host the integration talks to, for the status card. Only hosted-gateway and server-backed
 * integrations carry a URL field; for the rest this is undefined and the card omits the row.
 * The scheme, path, query and fragment are cut off so a pasted full URL still reads cleanly.
 */
const endpointHost = computed<string | undefined>(() => {
  const cfg = store.current?.config ?? {}
  const raw = cfg.gateway_url ?? cfg.host ?? cfg.api_url
  if (!raw) {
    return undefined
  }
  return raw.replace(/^[a-z][a-z0-9+.-]*:\/\//i, '').split(/[/?#]/)[0] || undefined
})

/**
 * Handles save event from IntegrationConfigForm.
 *
 * @param payload - Updated config and enabled state.
 * @returns Promise that resolves when save is complete.
 */
const handleSave = async (payload: IntegrationConfigPayload): Promise<void> => {
  await store.saveConfig(slug, payload)
}

/**
 * Handles test event from IntegrationConfigForm.
 *
 * @returns Promise that resolves when test is complete.
 */
const handleTest = async (): Promise<void> => {
  await store.testConnection(slug)
}

/** Loads the integration, or leaves for the plugins page when the CWP plugin is not installed. */
onMounted(() => {
  if (isCwp) {
    if (store.integrations.length > 0 && !store.integrations.some(i => i.slug === IntegrationSlug.Cwp)) {
      void router.replace({ path: AppRoute.Plugins })
    }
    return
  }
  if (isCwp7) {
    return
  }
  void store.fetchOne(slug)
})
</script>

<template>
  <CwpIntegrationView v-if="isCwp" />
  <Cwp7IntegrationView v-else-if="isCwp7" />

  <UiStack v-else gap="md" class="w-full">
    <IntegrationBreadcrumb :name="store.current?.name ?? slug" />

    <!-- Loading -->
    <UiSkeleton v-if="store.loading && !store.current" :lines="4" data-testid="integration-loading" />

    <!-- Error -->
    <UiAlert v-else-if="store.error" variant="error" data-testid="integration-error">
      {{ store.error }}
    </UiAlert>

    <!-- Content -->
    <UiSplit v-else-if="store.current">
      <IntegrationConfigForm
        :integration="store.current"
        :loading="store.loading"
        :testing="store.testing"
        @save="handleSave"
        @test="handleTest"
      />
      <IntegrationStatusSidebar
        :last-tested-at="store.current.lastTestedAt"
        :test-result="store.testResult"
        :testing="store.testing"
        :endpoint-host="endpointHost"
        :hint="hint"
      />
    </UiSplit>
  </UiStack>
</template>
