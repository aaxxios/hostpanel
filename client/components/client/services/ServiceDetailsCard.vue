<template>
  <UiCard data-testid="service-details">
    <UiCardHeader :title="$t('client.services.serviceDetails')" :icon="LayoutGrid" />
    <UiGrid as="dl" cols="2" gap="lg">
      <UiDescriptionItem :label="$t('client.services.plan')" :value="productNameOrFallback($t, service.name, service.id)" value-class="font-medium" />
      <UiDescriptionItem :label="$t('client.services.domain')" :value="service.domain || null" />
      <UiDescriptionItem :label="$t('client.services.status')">
        <ClientStatusBadge :status="service.status" />
      </UiDescriptionItem>
      <UiDescriptionItem :label="$t('client.services.billingCycle')" :value="service.billingcycle || null" value-class="capitalize" />
      <UiDescriptionItem :label="$t('client.services.regDate')" :value="formatDate(service.regdate)" />
      <UiDescriptionItem :label="$t('client.services.nextDueDate')" :value="formatDate(service.nextduedate)" />
      <UiDescriptionItem :label="$t('client.services.billingAmount')" :value="formatCurrency(service.recurringamount, currency)" value-class="font-medium" />
      <UiDescriptionItem :label="$t('client.services.firstPayment')" :value="formatCurrency(service.firstpaymentamount, currency)" />
      <UiDescriptionItem :label="$t('client.services.paymentMethod')" :value="service.paymentmethodname || service.paymentmethod || null" value-class="capitalize" />
    </UiGrid>
  </UiCard>
</template>

<script setup lang="ts">
/**
 * ServiceDetailsCard - the plan, domain, status, billing cycle, dates and amounts of a service as
 * a description list.
 */
import { LayoutGrid } from 'lucide-vue-next'
import type { ServiceDetail } from '~/types/servicedetail'
import type { CurrencyInfo } from '~/utils/formatCurrency'
import { productNameOrFallback } from '~/utils/displayFallback'
import { formatCurrency } from '~/utils/formatCurrency'
import { formatDate } from '~/utils/formatDate'

/** Props of ServiceDetailsCard. */
interface Props {
  /** The service being shown. */
  service: ServiceDetail
  /** What the account says the amounts are denominated in. */
  currency: CurrencyInfo
}

defineProps<Props>()
</script>
