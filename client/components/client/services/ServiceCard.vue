<template>
  <UiLinkCard :to="service.username ? `/client/services/${service.id}` : `/client/services/${service.id}/setup`" :highlight="!service.username" :testid="`service-card-${service.id}`">
    <UiStack direction="row" justify="between" align="start" class="mb-3">
      <UiStack direction="row" align="center" gap="sm" class="min-w-0">
        <UiTile size="lg" :icon="Server" tone="primary" />
        <UiStack gap="none" class="min-w-0">
          <UiText as="h3" size="sm" weight="semibold" truncate class="group-hover:text-primary-600 dark:group-hover:text-primary-400 transition-colors">
            {{ service.name }}
          </UiText>
          <UiText as="span" size="xs" tone="subtle" truncate>{{ service.domain || service.groupname }}</UiText>
        </UiStack>
      </UiStack>
      <div class="flex-shrink-0">
        <UiBadge v-if="!service.username" variant="info" size="sm" data-testid="service-needs-setup">{{ $t('client.dashboard.needsSetup') }}</UiBadge>
        <ClientStatusBadge v-else :status="service.status" />
      </div>
    </UiStack>

    <UiGrid cols="2" gap="sm" class="mt-4">
      <UiStack gap="none">
        <UiText as="span" size="xs" tone="subtle">{{ $t('client.services.nextDue') }}</UiText>
        <UiText as="span" size="xs" weight="medium" tone="muted">{{ formatDate(service.nextduedate) }}</UiText>
      </UiStack>
      <UiStack gap="none">
        <UiText as="span" size="xs" tone="subtle">{{ $t('client.services.amount') }}</UiText>
        <UiText as="span" size="xs" weight="medium" tone="muted">{{ formatCurrency(service.recurringamount, currency) }}</UiText>
      </UiStack>
    </UiGrid>

    <ClientServicesUsageBar
      v-if="hasUsageLimit(service.disklimit)"
      class="mt-4"
      :label="$t('client.services.diskUsage')"
      :used="service.diskusage"
      :limit="service.disklimit"
    />
  </UiLinkCard>
</template>

<script setup lang="ts">
/**
 * ServiceCard - one service of the list: name, domain, status (or a "needs setup" badge), next
 * due date, recurring amount and a disk-usage bar. The whole card links to the service, or to
 * its setup wizard while it has no control-panel account yet.
 */
import { Server } from 'lucide-vue-next'
import type { ClientService } from '~/types/clientservice'
import type { CurrencyInfo } from '~/utils/formatCurrency'
import { formatCurrency } from '~/utils/formatCurrency'
import { formatDate } from '~/utils/formatDate'
import { hasUsageLimit } from '~/utils/serviceUsage'

/** Props of ServiceCard. */
interface Props {
  /** The service the card shows. */
  service: ClientService
  /** What the account says the amounts are denominated in. */
  currency: CurrencyInfo
}

defineProps<Props>()
</script>
