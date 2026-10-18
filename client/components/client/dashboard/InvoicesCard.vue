<template>
  <UiCard data-testid="dashboard-invoices">
    <UiCardHeader :title="$t('client.dashboard.recentInvoices')">
      <UiButton size="sm" variant="subtle" to="/client/invoices" data-testid="dashboard-invoices-all">
        {{ $t('client.dashboard.viewAll') }}
        <ArrowRight :size="12" :stroke-width="2" class="ml-1.5" />
      </UiButton>
    </UiCardHeader>

    <UiStack v-if="store.invoicesLoading" gap="sm">
      <UiSkeleton v-for="i in 3" :key="i" shape="block" class="h-12" />
    </UiStack>

    <UiText v-else-if="!store.invoices.length" size="sm" tone="muted" class="py-4 text-center" data-testid="dashboard-invoices-empty">
      {{ $t('client.dashboard.noInvoices') }}
    </UiText>

    <UiStack v-else gap="sm">
      <ClientDashboardListRow
        v-for="invoice in store.invoices.slice(0, 4)"
        :key="invoice.id"
        :to="`/client/invoices/${invoice.id}`"
        :testid="`dashboard-invoice-${invoice.id}`"
      >
        <div>
          <UiText size="sm" weight="medium">#{{ invoice.id }}</UiText>
          <UiText size="xs" tone="subtle">{{ $t('client.dashboard.due') }} {{ formatDate(invoice.dueDate) }}</UiText>
        </div>
        <UiStack direction="row" align="center" gap="sm">
          <UiText as="span" size="sm" weight="semibold">{{ formatCurrency(invoice.total, { code: invoice.currency }) }}</UiText>
          <ClientStatusBadge :status="invoice.status" />
        </UiStack>
      </ClientDashboardListRow>
    </UiStack>
  </UiCard>
</template>

<script setup lang="ts">
/**
 * The dashboard's "Recent Invoices" card: the latest four invoices with due date, total and
 * status, each opening the invoice.
 */
import { ArrowRight } from 'lucide-vue-next'
import { useClientStore } from '~/stores/client'
import { formatCurrency } from '~/utils/formatCurrency'
import { formatDate } from '~/utils/formatDate'

/** Client store: the signed-in person and their services, invoices, domains and tickets. */
const store = useClientStore()
</script>
