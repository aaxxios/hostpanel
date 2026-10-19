<template>
  <UiAlert
    v-if="cancelStatus?.pending"
    variant="error"
    :title="$t('client.services.cancellationPending')"
    data-testid="service-cancel-pending"
  >
    <template v-if="cancelTypeKey">{{ $t(cancelTypeKey) }}</template>
  </UiAlert>

  <UiAlert
    v-if="service.status === ServiceStatus.Pending"
    variant="warning"
    :title="$t('client.services.setupRequired')"
    data-testid="service-setup-required"
  >
    {{ $t('client.services.setupRequiredNotice') }}
    <template #action>
      <UiButton :to="clientServiceSetupPath(serviceId)" size="sm" class="shrink-0" data-testid="service-complete-setup">
        <Zap :size="14" class="mr-1.5" />
        {{ $t('client.services.completeSetup') }}
      </UiButton>
    </template>
  </UiAlert>
</template>

<script setup lang="ts">
/**
 * ServiceNotices - the banners above the service detail: a pending cancellation (with its type
 * when the API named a known one) and a "setup required" call to action for a pending service.
 */
import { clientServiceSetupPath } from '~/constants/clientRoutes'
import { computed } from 'vue'
import { Zap } from 'lucide-vue-next'
import { CancellationType } from '~/types/cancellationtype'
import type { CancellationStatus } from '~/types/cancellationstatus'
import type { ServiceDetail } from '~/types/servicedetail'
import { ServiceStatus } from '~/types/servicestatus'

/** Props of ServiceNotices. */
interface Props {
  /** The service being shown. */
  service: ServiceDetail
  /** The id of the service, for the setup link. */
  serviceId: string
  /** Whether a cancellation is pending on the service. */
  cancelStatus: CancellationStatus
}

/** The props of the component. */
const props = defineProps<Props>()

/**
 * i18n key naming the pending cancellation's type, resolved from the enum member the API sends.
 * The API sends the member name and never a sentence, so the wording is chosen here, in the
 * reader's own language.
 *
 * `null` when the API sent a member this build does not know: the banner then shows the pending
 * notice without a type, which is preferable to guessing the wrong one.
 */
const cancelTypeKey = computed<string | null>(() => {
  switch (props.cancelStatus.type) {
    case CancellationType.Immediate:
      return 'client.services.cancelImmediate'
    case CancellationType.EndOfBillingPeriod:
      return 'client.services.cancelEndOfPeriod'
    default:
      return null
  }
})
</script>
