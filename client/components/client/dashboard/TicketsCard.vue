<template>
  <UiCard data-testid="dashboard-tickets">
    <UiCardHeader :title="$t('client.dashboard.recentTickets')">
      <UiButton size="sm" variant="primary" to="/client/tickets/new" data-testid="dashboard-ticket-new">
        <Plus :size="12" :stroke-width="2" class="mr-1" />
        {{ $t('client.dashboard.openNewTicket') }}
      </UiButton>
    </UiCardHeader>

    <UiStack v-if="store.ticketsLoading" gap="sm">
      <UiSkeleton v-for="i in 3" :key="i" shape="block" class="h-12" />
    </UiStack>

    <UiStack v-else-if="!store.tickets.length" align="center" gap="xs" class="py-6" data-testid="dashboard-tickets-empty">
      <UiText size="sm" tone="muted">{{ $t('client.dashboard.noTickets') }}</UiText>
      <UiButton size="sm" variant="ghost" to="/client/tickets/new" data-testid="dashboard-ticket-open">
        {{ $t('client.dashboard.openTicketLink') }}
      </UiButton>
    </UiStack>

    <UiStack v-else gap="sm">
      <ClientDashboardListRow
        v-for="ticket in store.tickets.slice(0, 4)"
        :key="ticket.id"
        :to="`/client/tickets/${ticket.id}`"
        :testid="`dashboard-ticket-${ticket.id}`"
      >
        <div class="min-w-0 flex-1">
          <UiText size="sm" weight="medium" truncate>{{ ticket.subject }}</UiText>
          <UiText size="xs" tone="subtle">{{ ticket.departmentName }}</UiText>
        </div>
        <ClientStatusBadge :status="ticket.status" class="flex-shrink-0" />
      </ClientDashboardListRow>
    </UiStack>
  </UiCard>
</template>

<script setup lang="ts">
/**
 * The dashboard's "Recent Tickets" card: the latest four support tickets with their status, or
 * an invitation to open the first one.
 */
import { Plus } from 'lucide-vue-next'
import { useClientStore } from '~/stores/client'

/** Client store: the signed-in person and their services, invoices, domains and tickets. */
const store = useClientStore()
</script>
