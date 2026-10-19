<script setup lang="ts">
/**
 * Layout wrapper for client detail pages.
 * Shows a compact header with client info and the inner sidebar alongside the active sub-page.
 */
import { onMounted, watch, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useClientsStore } from '@/stores/clients'
import { clientStatusTone } from '@/lib/tones'
import ClientInnerSidebar from '@/components/clients/ClientInnerSidebar.vue'
import ClientExportModal from '@/components/clients/ClientExportModal.vue'
import UiAlert from '@/components/ui/UiAlert.vue'
import UiBadge from '@/components/ui/UiBadge.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiHeading from '@/components/ui/UiHeading.vue'
import UiIcon from '@/components/ui/UiIcon.vue'
import UiIconButton from '@/components/ui/UiIconButton.vue'
import UiSkeleton from '@/components/ui/UiSkeleton.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiText from '@/components/ui/UiText.vue'
import { UiButtonVariant } from '@/utils/ui/button'
import { UiAlignItems } from '@/utils/ui/layout'
import { UiSize } from '@/utils/ui/size'
import { UiTextSize, UiTextTone } from '@/utils/ui/text'
import { UiTone } from '@/utils/ui/tone'
import { AppRoute } from '@/lib/routes'

const { t } = useI18n()

/** Icon markup (shape tags) of the back button. */
const BACK_ICON = '<polyline points="15 18 9 12 15 6" />'

/** Icon markup (shape tags) of the export button. */
const EXPORT_ICON = '<path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="7 10 12 15 17 10"/><line x1="12" y1="15" x2="12" y2="3"/>'

/** Whether the export dialog is open. */
const showExportModal = ref(false)

/** Current route, source of the client id. */
const route = useRoute()

/** Router, used to leave for the clients list. */
const router = useRouter()

/** Clients store, owner of the loaded client. */
const store = useClientsStore()

/**
 * Loads the client data and currencies for the current route param.
 *
 * @param id - The client identifier from route params.
 * @returns Promise that resolves when both fetches complete.
 */
const loadClient = async (id: string): Promise<void> => {
  await store.fetchById(id)
  store.fetchCurrencies()
}

/**
 * Leaves the client for the clients list.
 *
 * @returns Nothing.
 */
const goBack = (): void => {
  router.push(AppRoute.Clients)
}

// Re-fetch when client ID changes in the route
watch(() => route.params.id, (newId) => {
  if (newId) {
    loadClient(String(newId))
  }
})

onMounted(() => {
  loadClient(String(route.params.id))
})
</script>

<template>
  <UiStack gap="md" class="h-full w-full">
    <UiStack v-if="store.loading && !store.current" :align="UiAlignItems.Center" data-testid="client-layout-loading">
      <UiSkeleton :lines="2" />
      <UiText :tone="UiTextTone.Secondary">{{ t('clients.clientLayout.loadingClient') }}</UiText>
    </UiStack>

    <UiAlert v-else-if="store.error && !store.current" variant="error" data-testid="client-layout-error">
      {{ store.error }}
    </UiAlert>

    <template v-else-if="store.current">
      <UiStack direction="row" :align="UiAlignItems.Center" data-testid="client-layout-header">
        <UiIconButton :label="t('shared.back')" data-testid="client-layout-back" @click="goBack">
          <UiIcon :markup="BACK_ICON" />
        </UiIconButton>
        <UiBadge :tone="UiTone.Primary">
          {{ store.current.firstName.charAt(0) }}{{ store.current.lastName.charAt(0) }}
        </UiBadge>
        <UiStack gap="xs" class="flex-1">
          <UiHeading :level="1" variant="section" data-testid="client-header-name">
            {{ store.current.firstName }} {{ store.current.lastName }}
          </UiHeading>
          <UiStack direction="row" :align="UiAlignItems.Center" gap="sm">
            <UiBadge :tone="clientStatusTone(store.current.status)" small>{{ store.current.status }}</UiBadge>
            <UiText v-if="store.current.companyName" :tone="UiTextTone.Secondary" :size="UiTextSize.Sm">
              {{ store.current.companyName }}
            </UiText>
          </UiStack>
        </UiStack>
        <UiButton
          :variant="UiButtonVariant.Secondary"
          :size="UiSize.Sm"
          data-testid="client-layout-export"
          @click="showExportModal = true"
        >
          <UiIcon :markup="EXPORT_ICON" :size="UiSize.Sm" />
          {{ t('clients.clientLayout.exportData') }}
        </UiButton>
      </UiStack>

      <ClientExportModal
        v-if="showExportModal"
        :client-id="Number(route.params.id)"
        :client-name="`${store.current.firstName} ${store.current.lastName}`"
        @close="showExportModal = false"
      />

      <UiStack direction="row" gap="none" class="flex-1 min-h-0">
        <ClientInnerSidebar :client-id="String(route.params.id)" />
        <UiStack gap="none" class="flex-1 min-w-0 overflow-y-auto">
          <RouterView />
        </UiStack>
      </UiStack>
    </template>
  </UiStack>
</template>
