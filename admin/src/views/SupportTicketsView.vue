<script setup lang="ts">
/**
 * Support tickets list: a paged, searchable table of all tickets with a status filter, flagging
 * and bulk actions.
 */
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import SupportTicketStatusBadge from '@/components/support/SupportTicketStatusBadge.vue'
import UiAlert from '@/components/ui/UiAlert.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiCheckbox from '@/components/ui/UiCheckbox.vue'
import UiEmptyState from '@/components/ui/UiEmptyState.vue'
import UiIcon from '@/components/ui/UiIcon.vue'
import UiIconButton from '@/components/ui/UiIconButton.vue'
import UiInput from '@/components/ui/UiInput.vue'
import UiLink from '@/components/ui/UiLink.vue'
import UiPageHeader from '@/components/ui/UiPageHeader.vue'
import UiPagination from '@/components/ui/UiPagination.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiTable from '@/components/ui/UiTable.vue'
import UiTableBody from '@/components/ui/UiTableBody.vue'
import UiTableCell from '@/components/ui/UiTableCell.vue'
import UiTableHead from '@/components/ui/UiTableHead.vue'
import UiTableHeader from '@/components/ui/UiTableHeader.vue'
import UiTableRow from '@/components/ui/UiTableRow.vue'
import UiText from '@/components/ui/UiText.vue'
import { formatElapsed } from '@/lib/format'
import { useSupportStore } from '@/stores/support'
import { TicketBulkAction, TicketFlagFilter, TicketStatus } from '@/types/ticket'
import type { TicketListFilter } from '@/types/ticket'
import { Icon } from '@/lib/icons'
import { pageCount } from '@/lib/paging'
import { UiButtonVariant } from '@/utils/ui/button'
import { UiTableDensity } from '@/utils/ui/injection'
import { UiSize } from '@/utils/ui/size'
import { toneTextClasses, UiTone } from '@/utils/ui/tone'
import { SEARCH_DEBOUNCE_MS } from '@/lib/limits'
import { AppRoute, supportTicketPath } from '@/lib/routes'

const { t } = useI18n()

const store = useSupportStore()

/** Number of columns of the table (width of the status row). */
const COLUMN_COUNT = 7

/** One status-filter button. */
interface FilterOption {
  /** Filter applied by the button; null shows every active ticket. */
  value: TicketListFilter | null
  /** Translated label. */
  label: string
  /** Suffix of the button's `data-testid`. */
  key: string
}

/** Status filter buttons in display order. */
const filterOptions = computed<FilterOption[]>(() => [
  { value: null, label: t('constants.ticketFilterOptions.allActiveTickets'), key: 'all' },
  { value: TicketFlagFilter.Flagged, label: t('constants.ticketFilterOptions.flagged'), key: 'flagged' },
  { value: TicketStatus.Open, label: t('support.networkIssues.open'), key: 'open' },
  { value: TicketStatus.AwaitingReply, label: t('constants.ticketFilterOptions.answered'), key: 'awaiting-reply' },
  { value: TicketStatus.Answered, label: t('constants.ticketFilterOptions.customerReply'), key: 'answered' },
  { value: TicketStatus.Closed, label: t('clients.clientForm.closed'), key: 'closed' },
])

/** Bulk actions offered for the selected tickets. */
const bulkActions = computed(() => [
  { action: TicketBulkAction.Close, label: t('shared.close'), variant: UiButtonVariant.Secondary },
  { action: TicketBulkAction.Delete, label: t('shared.delete'), variant: UiButtonVariant.Danger },
  { action: TicketBulkAction.Flag, label: t('support.supportTickets.flag'), variant: UiButtonVariant.Secondary },
  { action: TicketBulkAction.Unflag, label: t('support.supportTickets.unflag'), variant: UiButtonVariant.Secondary },
])

/** IDs of tickets selected via checkbox. */
const selectedIds = ref<number[]>([])

/** Whether every ticket on the page is selected. */
const allSelected = computed<boolean>(
  () => store.tickets.length > 0 && selectedIds.value.length === store.tickets.length,
)

/** Number of pages for the current filter. */
const totalPages = computed<number>(() => pageCount(store.totalCount, store.pageSize))

/** First record index (1-based) for display. */
const showingFrom = computed<number>(() => (store.totalCount === 0 ? 0 : (store.page - 1) * store.pageSize + 1))

/** Last record index (1-based) for display. */
const showingTo = computed<number>(() => Math.min(store.page * store.pageSize, store.totalCount))

/** Debounce timer of the search box. */
let searchTimer: ReturnType<typeof setTimeout> | undefined

/**
 * Forgets the selection (it belongs to the page that was loaded).
 */
const clearSelection = (): void => {
  selectedIds.value = []
}

/**
 * Selects every ticket on the page, or none.
 *
 * @param checked - Whether the select-all box is now checked.
 */
const handleSelectAll = (checked: boolean): void => {
  selectedIds.value = checked ? store.tickets.map((ticket) => ticket.id) : []
}

/**
 * Adds or removes one ticket from the selection.
 *
 * @param id - The ticket identifier.
 */
const handleToggleSelect = (id: number): void => {
  const index = selectedIds.value.indexOf(id)
  if (index === -1) {
    selectedIds.value.push(id)
  } else {
    selectedIds.value.splice(index, 1)
  }
}

/**
 * Applies a status filter and reloads the first page.
 *
 * @param value - The filter, or null for all active tickets.
 */
const applyFilter = (value: TicketListFilter | null): void => {
  store.statusFilter = value
  store.page = 1
  clearSelection()
  store.fetchAll()
}

/**
 * Flips the flag of one ticket and reloads on success.
 *
 * @param id - The ticket identifier.
 */
const handleToggleFlag = async (id: number): Promise<void> => {
  if (await store.toggleFlag(id)) {
    await store.fetchAll()
  }
}

/**
 * Runs a bulk action on the selected tickets and reloads on success.
 *
 * @param action - The action to run.
 */
const handleBulkAction = async (action: TicketBulkAction): Promise<void> => {
  if (selectedIds.value.length === 0) {
    return
  }
  if (await store.bulkAction(selectedIds.value, action)) {
    clearSelection()
    await store.fetchAll()
  }
}

/**
 * Loads another page.
 *
 * @param page - The page to show, 1-based.
 */
const goToPage = (page: number): void => {
  store.page = page
  clearSelection()
  store.fetchAll()
}

// The search box reloads from the first page once typing pauses.
watch(
  () => store.search,
  () => {
    clearTimeout(searchTimer)
    searchTimer = setTimeout(() => {
      store.page = 1
      clearSelection()
      store.fetchAll()
    }, SEARCH_DEBOUNCE_MS)
  },
)

onBeforeUnmount(() => {
  clearTimeout(searchTimer)
})

onMounted(() => {
  store.fetchAll()
})
</script>

<template>
  <UiStack>
    <UiPageHeader
      :title="t('support.supportTickets.supportTickets')"
      :subtitle="t('support.supportTickets.manageAllClientSupportRequests')"
    >
      <template #actions>
        <UiLink
          :to="AppRoute.SupportTicketsNew"
          :variant="UiButtonVariant.Primary"
          data-testid="support-tickets-open-new"
        >
          {{ t('shared.openNewTicket') }}
        </UiLink>
      </template>
    </UiPageHeader>

    <UiStack direction="row" gap="sm" wrap>
      <UiButton
        v-for="option in filterOptions"
        :key="option.key"
        :variant="store.statusFilter === option.value ? UiButtonVariant.Primary : UiButtonVariant.Secondary"
        :size="UiSize.Sm"
        :data-testid="`support-tickets-filter-${option.key}`"
        @click="applyFilter(option.value)"
      >
        {{ option.label }}
      </UiButton>
    </UiStack>

    <UiInput
      v-model="store.search"
      :placeholder="t('support.supportTickets.searchTicketsBySubject')"
      class="max-w-md"
      data-testid="support-tickets-search"
    />

    <UiStack direction="row" justify="between" wrap>
      <UiText tone="muted" size="sm" data-testid="support-tickets-count">
        {{ t('support.supportTickets.totalcountRecordsFoundShowingShowingfrom', { totalCount: store.totalCount, showingFrom, showingTo }) }}
      </UiText>
      <UiText tone="muted" size="sm">
        {{ t('shared.pagePageOfTotalpages', { page: store.page, totalPages }) }}
      </UiText>
    </UiStack>

    <UiCard v-if="selectedIds.length > 0" padding="sm" data-testid="support-tickets-bulk">
      <UiStack direction="row" gap="sm" align="center" wrap>
        <UiText tone="muted" size="md">{{ t('shared.withSelected') }}</UiText>
        <UiButton
          v-for="entry in bulkActions"
          :key="entry.action"
          :variant="entry.variant"
          :size="UiSize.Sm"
          :data-testid="`support-tickets-bulk-${entry.action}`"
          @click="handleBulkAction(entry.action)"
        >
          {{ entry.label }}
        </UiButton>
      </UiStack>
    </UiCard>

    <UiAlert v-if="store.error" variant="error" data-testid="support-tickets-error">{{ store.error }}</UiAlert>

    <UiCard class="overflow-x-auto">
      <UiTable
        :density="UiTableDensity.Comfortable"
        :loading="store.loading && store.tickets.length === 0"
        :empty="!store.loading && store.tickets.length === 0"
        :colspan="COLUMN_COUNT"
        data-testid="support-tickets-table"
      >
        <UiTableHead>
          <UiTableRow>
            <UiTableHeader>
              <UiCheckbox
                :model-value="allSelected"
                data-testid="support-tickets-select-all"
                @update:model-value="handleSelectAll"
              />
            </UiTableHeader>
            <UiTableHeader />
            <UiTableHeader>{{ t('shared.subject') }}</UiTableHeader>
            <UiTableHeader>{{ t('shared.department') }}</UiTableHeader>
            <UiTableHeader>{{ t('support.supportTickets.requestor') }}</UiTableHeader>
            <UiTableHeader>{{ t('shared.status') }}</UiTableHeader>
            <UiTableHeader>{{ t('shared.lastReply') }}</UiTableHeader>
          </UiTableRow>
        </UiTableHead>
        <UiTableBody divided>
          <UiTableRow
            v-for="ticket in store.tickets"
            :key="ticket.id"
            hoverable
            :selected="selectedIds.includes(ticket.id)"
            :data-testid="`support-tickets-row-${ticket.id}`"
          >
            <UiTableCell>
              <UiCheckbox
                :model-value="selectedIds.includes(ticket.id)"
                :data-testid="`support-tickets-select-${ticket.id}`"
                @update:model-value="handleToggleSelect(ticket.id)"
              />
            </UiTableCell>
            <UiTableCell>
              <UiIconButton
                :label="t('support.supportTickets.flag')"
                :size="UiSize.Sm"
                :class="ticket.isFlagged ? toneTextClasses[UiTone.Warning] : 'text-text-muted'"
                :data-testid="`support-tickets-flag-${ticket.id}`"
                @click="handleToggleFlag(ticket.id)"
              >
                <UiIcon :markup="Icon.Flag" :size="UiSize.Sm" />
              </UiIconButton>
            </UiTableCell>
            <UiTableCell>
              <UiLink
                :to="supportTicketPath(ticket.id)"
                :variant="UiButtonVariant.Link"
                :data-testid="`support-tickets-open-${ticket.id}`"
              >
                {{ t('shared.tktIdSubject', { id: ticket.id, subject: ticket.subject }) }}
              </UiLink>
            </UiTableCell>
            <UiTableCell muted>{{ ticket.departmentName ?? '—' }}</UiTableCell>
            <UiTableCell muted>{{ t('support.supportTickets.clientClientid', { clientId: ticket.clientId }) }}</UiTableCell>
            <UiTableCell><SupportTicketStatusBadge :status="ticket.status" /></UiTableCell>
            <UiTableCell muted>{{ formatElapsed(ticket.lastReplyAt) }}</UiTableCell>
          </UiTableRow>
        </UiTableBody>
        <template #empty>
          <UiEmptyState
            :title="t('support.supportTickets.noTicketsFound')"
            :description="t('support.supportTickets.tryAdjustingYourFiltersOr')"
          />
        </template>
      </UiTable>
    </UiCard>

    <UiPagination
      :page="store.page"
      :total="store.totalCount"
      :page-size="store.pageSize"
      testid="support-tickets-pagination"
      @update:page="goToPage"
    />
  </UiStack>
</template>
