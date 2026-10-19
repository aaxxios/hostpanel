<template>
  <UiStack direction="row" align="center" justify="between" wrap data-testid="service-header">
    <UiStack direction="row" align="center">
      <UiTile :icon="Server" tone="primary" size="xl" />
      <UiStack gap="none">
        <UiHeading :level="1" size="subsection">{{ productNameOrFallback($t, service.name, service.id) }}</UiHeading>
        <UiText size="sm" tone="muted">{{ service.domain || service.groupname }}</UiText>
      </UiStack>
    </UiStack>
    <UiStack direction="row" align="center" gap="sm">
      <UiButton
        v-if="canLogin"
        size="sm"
        :loading="ssoLoading"
        class="hidden sm:flex"
        data-testid="service-cpanel-login"
        @click="emit('cpanel')"
      >
        <Monitor :size="14" class="mr-1.5" />
        {{ $t('client.services.actionLoginCpanel') }}
      </UiButton>
      <ClientStatusBadge :status="service.status" />
    </UiStack>
  </UiStack>
</template>

<script setup lang="ts">
/**
 * ServiceHeader - the title row of the service detail page: icon, plan name, domain, a "log in
 * to the control panel" button for an active service on a known server, and the status badge.
 */
import { computed } from 'vue'
import { Monitor, Server } from 'lucide-vue-next'
import type { ServiceDetail } from '~/types/servicedetail'
import { ServiceStatus } from '~/types/servicestatus'
import { productNameOrFallback } from '~/utils/displayFallback'

/** Props of ServiceHeader. */
interface Props {
  /** The service being shown. */
  service: ServiceDetail
  /** Whether the control-panel sign-in is in flight. */
  ssoLoading: boolean
}

/** The props of the component. */
const props = defineProps<Props>()

/** Events the component raises. */
const emit = defineEmits<{
  /** The reader asked to sign in to the control panel. */
  cpanel: []
}>()

/** Whether the control-panel button is offered: an active service with a known server. */
const canLogin = computed(() => Boolean(props.service.serverhostname) && props.service.status === ServiceStatus.Active)
</script>
