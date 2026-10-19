<script setup lang="ts">
/**
 * CWP7 module landing page.
 *
 * Shows module info and quick-action buttons to create servers or products
 * linked to the CWP7 provisioning module.
 * Lists existing CWP7 servers with their status.
 */
import { computed, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRouter } from 'vue-router'
import { useServersStore } from '@/stores/servers'
import { INTEGRATION_META } from '@/types/integrationmeta'
import { IntegrationSlug } from '@/types/integration'
import { ServerModule } from '@/types/server'
import type { ServerDto } from '@/types/server'
import type { UiTone } from '@/utils/ui/tone'
import IntegrationBreadcrumb from '@/components/integrations/IntegrationBreadcrumb.vue'
import UiBadge from '@/components/ui/UiBadge.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiGrid from '@/components/ui/UiGrid.vue'
import UiHeading from '@/components/ui/UiHeading.vue'
import UiImage from '@/components/ui/UiImage.vue'
import UiLink from '@/components/ui/UiLink.vue'
import UiSkeleton from '@/components/ui/UiSkeleton.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiText from '@/components/ui/UiText.vue'
import { AppRoute } from '@/lib/routes'

/** Translation function. */
const { t } = useI18n()

/** Router, for the two quick actions. */
const router = useRouter()

/** Servers store: the server list. */
const serversStore = useServersStore()

/** Static metadata of the CWP7 integration (logo and tile colour). */
const meta = INTEGRATION_META[IntegrationSlug.Cwp7]

/** CWP7 servers only. */
const cwp7Servers = computed<ServerDto[]>(() =>
  serversStore.servers.filter(s => s.module === ServerModule.Cwp7),
)

/**
 * Tone of a server's last known state.
 *
 * @param server - The server.
 * @returns Success when online, danger when offline, neutral when never tested.
 */
const stateTone = (server: ServerDto): UiTone => {
  if (server.isOnline === true) {
    return 'success'
  }
  return server.isOnline === false ? 'danger' : 'neutral'
}

/** Navigates to the servers page with CWP7 module pre-selected. */
const goToCreateServer = (): void => {
  void router.push({ path: AppRoute.Servers, query: { create: ServerModule.Cwp7 } })
}

/** Navigates to the product creation page. */
const goToCreateProduct = (): void => {
  void router.push({ path: AppRoute.SettingsProducts, query: { create: 'true', module: ServerModule.Cwp7 } })
}

/** Loads the servers. */
onMounted(() => {
  void serversStore.fetchServers()
})
</script>

<template>
  <UiStack gap="lg" class="w-full">
    <IntegrationBreadcrumb :name="t('integrations.cwp7Integration.cwp7')" />

    <!-- Module header card -->
    <UiCard padding="md" data-testid="cwp7-header">
      <UiStack direction="row" align="start" gap="md">
        <UiStack align="center" justify="center" class="h-14 w-14 shrink-0 rounded-xl p-2" :class="meta.color">
          <UiImage :src="meta.logo" alt="" class="h-full w-full object-contain brightness-0 invert" />
        </UiStack>
        <UiStack gap="sm" class="min-w-0 flex-1">
          <UiHeading :level="1" variant="section">{{ t('integrations.cwp7Integration.cwp7') }}</UiHeading>
          <UiText tone="muted" size="md">{{ t('integrations.cwp7Integration.controlWebPanel7Provisioning') }}</UiText>

          <UiGrid :columns="3" gap="md">
            <UiStack gap="xs">
              <UiText tone="muted" size="sm">{{ t('shared.category') }}</UiText>
              <UiText tone="primary" size="sm" class="font-medium">{{ t('integrations.cwp7Integration.hostingProvisioning') }}</UiText>
            </UiStack>
            <UiStack gap="xs">
              <UiText tone="muted" size="sm">{{ t('integrations.cwp7Integration.moduleType') }}</UiText>
              <UiText tone="primary" size="sm" class="font-medium">{{ t('integrations.cwp7Integration.serverModule') }}</UiText>
            </UiStack>
            <UiStack gap="xs">
              <UiText tone="muted" size="sm">{{ t('integrations.cwp7Integration.developer') }}</UiText>
              <UiText tone="primary" size="sm" class="font-medium">{{ t('shared.productName') }}</UiText>
            </UiStack>
          </UiGrid>
        </UiStack>
      </UiStack>
    </UiCard>

    <!-- Action buttons -->
    <UiGrid :columns="2" gap="md">
      <UiCard padding="md">
        <UiStack gap="sm" align="start">
          <UiHeading :level="3" variant="card">{{ t('integrations.cwp7Integration.createNewServer') }}</UiHeading>
          <UiText tone="muted" size="sm">{{ t('integrations.cwp7Integration.addACwp7ServerWith') }}</UiText>
          <UiButton variant="secondary" data-testid="cwp7-create-server" @click="goToCreateServer">
            {{ t('integrations.cwp7Integration.createNewServer') }}
          </UiButton>
        </UiStack>
      </UiCard>
      <UiCard padding="md">
        <UiStack gap="sm" align="start">
          <UiHeading :level="3" variant="card">{{ t('integrations.cwp7Integration.createNewProduct') }}</UiHeading>
          <UiText tone="muted" size="sm">{{ t('integrations.cwp7Integration.createAHostingPlanLinked') }}</UiText>
          <UiButton variant="secondary" data-testid="cwp7-create-product" @click="goToCreateProduct">
            {{ t('integrations.cwp7Integration.createNewProduct') }}
          </UiButton>
        </UiStack>
      </UiCard>
    </UiGrid>

    <!-- CWP7 Servers list -->
    <UiStack gap="sm">
      <UiStack direction="row" align="center" justify="between">
        <UiHeading :level="2" variant="section">{{ t('integrations.cwp7Integration.cwp7Servers') }}</UiHeading>
        <UiText tone="muted" size="sm">{{ t('integrations.cwp7Integration.lengthServerValue', { length: cwp7Servers.length }) }}</UiText>
      </UiStack>

      <!-- Empty state -->
      <UiCard v-if="!serversStore.loading && cwp7Servers.length === 0" padding="md" data-testid="cwp7-servers-empty">
        <UiStack gap="sm" align="center">
          <UiText tone="muted" size="md">{{ t('integrations.cwp7Integration.noCwp7ServersConfiguredYet') }}</UiText>
          <UiButton variant="link" data-testid="cwp7-add-first-server" @click="goToCreateServer">
            {{ t('integrations.cwp7Integration.addYourFirstServer') }}
          </UiButton>
        </UiStack>
      </UiCard>

      <!-- Loading -->
      <UiSkeleton v-else-if="serversStore.loading" :lines="3" data-testid="cwp7-servers-loading" />

      <!-- Server cards -->
      <UiGrid v-else :columns="3" gap="md" data-testid="cwp7-servers">
        <UiLink v-for="server in cwp7Servers" :key="server.id" :to="AppRoute.Servers" class="block" :data-testid="`cwp7-server-${server.id}`">
          <UiCard padding="md" class="h-full">
            <UiStack gap="sm">
              <UiStack direction="row" align="center" gap="sm">
                <UiHeading :level="3" variant="card" class="min-w-0 flex-1 truncate">{{ server.name }}</UiHeading>
                <UiBadge v-if="server.isDisabled" tone="danger" small>{{ t('shared.disabled') }}</UiBadge>
                <UiBadge v-else :tone="stateTone(server)" dot small />
              </UiStack>
              <UiText tone="muted" size="sm">{{ server.hostname }}</UiText>
              <UiText v-if="server.ipAddress" tone="muted" size="sm">{{ server.ipAddress }}</UiText>
              <UiText v-if="server.accountsCount !== null" tone="muted" size="sm">
                {{ t('integrations.cwp7Integration.accountscountValueAccounts', { accountsCount: server.accountsCount, value: server.maxAccounts ?? '∞' }) }}
              </UiText>
            </UiStack>
          </UiCard>
        </UiLink>
      </UiGrid>
    </UiStack>
  </UiStack>
</template>
