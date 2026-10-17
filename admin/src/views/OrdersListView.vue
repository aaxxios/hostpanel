<script setup lang="ts">
/**
 * Orders list view — paged table of all customer orders with a status filter.
 *
 * Shows order number, client, status, payment method, total, item count and date, with row
 * actions to accept or cancel a pending order and to open its detail. Reads `stores/orders`;
 * a refused load shows `store.error`, a refused accept or cancel shows `store.actionError`.
 */
import { onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import UiAlert from '@/components/ui/UiAlert.vue'
import UiBadge from '@/components/ui/UiBadge.vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiEmptyState from '@/components/ui/UiEmptyState.vue'
import UiIcon from '@/components/ui/UiIcon.vue'
import UiIconButton from '@/components/ui/UiIconButton.vue'
import UiLink from '@/components/ui/UiLink.vue'
import UiPageHeader from '@/components/ui/UiPageHeader.vue'
import UiPagination from '@/components/ui/UiPagination.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiTable from '@/components/ui/UiTable.vue'
import UiTableBody from '@/components/ui/UiTableBody.vue'
import UiTableCell from '@/components/ui/UiTableCell.vue'
import UiTableHead from '@/components/ui/UiTableHead.vue'
import UiTableHeader from '@/components/ui/UiTableHeader.vue'
import UiTableRow from '@/components/ui/UiTableRow.vue'
import UiText from '@/components/ui/UiText.vue'
import { Icon } from '@/lib/icons'
import { ORDER_STATUS_OPTIONS } from '@/lib/constants'
import { formatDate } from '@/lib/format'
import { formatMoney } from '@/lib/money'
import { OrderStatus } from '@/types/order'
import { orderStatusTone, statusLabel } from '@/lib/tones'
import { useOrdersStore } from '@/stores/orders'

const { t } = useI18n()

const store = useOrdersStore()

/**
 * Moves to a page of the list and loads it.
 *
 * @param page - Target page, 1-based.
 */
const goToPage = (page: number): void => {
  store.page = page
  void store.fetchAll()
}

/**
 * Applies the status filter and reloads from the first page.
 *
 * @param status - Chosen status, empty for all.
 */
const onStatusChange = (status: string): void => {
  store.statusFilter = status || null
  store.page = 1
  void store.fetchAll()
}

/**
 * Accepts a pending order, then reloads the list.
 *
 * @param id - The order primary key.
 * @returns Resolves when the list has been refreshed.
 */
const accept = async (id: number): Promise<void> => {
  if (await store.acceptOrder(id)) {
    await store.fetchAll()
  }
}

/**
 * Cancels a pending order, then reloads the list.
 *
 * @param id - The order primary key.
 * @returns Resolves when the list has been refreshed.
 */
const cancel = async (id: number): Promise<void> => {
  if (await store.cancelOrder(id)) {
    await store.fetchAll()
  }
}

onMounted(() => store.fetchAll())
</script>

<template>
  <UiStack>
    <UiPageHeader
      :title="t('orders.ordersList.orders')"
      :subtitle="t('orders.ordersList.manageCustomerOrders')"
      data-testid="orders-header"
    >
      <template #actions>
        <UiSelect
          class="w-44"
          :model-value="store.statusFilter ?? ''"
          :options="ORDER_STATUS_OPTIONS"
          :placeholder="t('orders.ordersList.filterByStatus')"
          data-testid="orders-filter-status"
          @update:model-value="onStatusChange"
        />
      </template>
    </UiPageHeader>

    <UiAlert v-if="store.actionError" variant="danger" data-testid="orders-action-error">
      {{ store.actionError }}
    </UiAlert>

    <UiAlert v-if="store.error" variant="danger" data-testid="orders-error">
      {{ store.error }}
    </UiAlert>

    <UiCard v-else-if="store.orders.length > 0 || store.loading">
      <UiTable density="comfortable" :loading="store.loading && store.orders.length === 0" :colspan="8" data-testid="orders-table">
        <UiTableHead>
          <UiTableRow bordered>
            <UiTableHeader>{{ t('orders.ordersList.order') }}</UiTableHeader>
            <UiTableHeader>{{ t('shared.client') }}</UiTableHeader>
            <UiTableHeader>{{ t('shared.status') }}</UiTableHeader>
            <UiTableHeader>{{ t('shared.paymentMethod') }}</UiTableHeader>
            <UiTableHeader>{{ t('shared.total') }}</UiTableHeader>
            <UiTableHeader>{{ t('orders.ordersList.items') }}</UiTableHeader>
            <UiTableHeader>{{ t('shared.date') }}</UiTableHeader>
            <UiTableHeader>{{ t('shared.actions') }}</UiTableHeader>
          </UiTableRow>
        </UiTableHead>
        <UiTableBody>
          <UiTableRow
            v-for="order in store.orders"
            :key="order.id"
            hoverable
            bordered
            :data-testid="`orders-row-${order.id}`"
          >
            <UiTableCell>{{ order.orderNumber }}</UiTableCell>
            <UiTableCell>
              <UiLink variant="link" :to="`/clients/${order.clientId}`" :data-testid="`orders-client-${order.id}`">
                {{ order.clientName }}
              </UiLink>
            </UiTableCell>
            <UiTableCell>
              <UiBadge :tone="orderStatusTone(order.status)" small>
                {{ statusLabel(ORDER_STATUS_OPTIONS, order.status) }}
              </UiBadge>
            </UiTableCell>
            <UiTableCell>{{ order.paymentMethod }}</UiTableCell>
            <UiTableCell>{{ formatMoney(order.total) }}</UiTableCell>
            <UiTableCell muted>{{ order.itemCount }}</UiTableCell>
            <UiTableCell muted>{{ formatDate(order.createdAt) }}</UiTableCell>
            <UiTableCell>
              <UiStack direction="row" gap="xs" align="center">
                <UiIconButton
                  v-if="order.status === OrderStatus.Pending"
                  :label="t('orders.ordersList.accept')"
                  size="sm"
                  :data-testid="`orders-accept-${order.id}`"
                  @click="accept(order.id)"
                >
                  <UiIcon :markup="Icon.Check" size="sm" />
                </UiIconButton>
                <UiIconButton
                  v-if="order.status === OrderStatus.Pending"
                  :label="t('shared.cancel')"
                  size="sm"
                  :data-testid="`orders-cancel-${order.id}`"
                  @click="cancel(order.id)"
                >
                  <UiIcon :markup="Icon.Close" size="sm" />
                </UiIconButton>
                <UiLink
                  :to="`/orders/${order.id}`"
                  variant="ghost"
                  size="sm"
                  :title="t('shared.view')"
                  :aria-label="t('shared.view')"
                  :data-testid="`orders-view-${order.id}`"
                >
                  <UiIcon :markup="Icon.Eye" size="sm" />
                </UiLink>
              </UiStack>
            </UiTableCell>
          </UiTableRow>
        </UiTableBody>
      </UiTable>

      <UiStack direction="row" justify="between" align="center" class="p-3">
        <UiText tone="muted" size="xs">
          {{ t('orders.ordersList.totalcountOrderValue', { totalCount: store.totalCount }) }}
        </UiText>
        <UiPagination
          :page="store.page"
          :total="store.totalCount"
          :page-size="store.pageSize"
          testid="orders-pagination"
          @update:page="goToPage"
        />
      </UiStack>
    </UiCard>

    <UiCard v-else>
      <UiEmptyState :title="t('orders.ordersList.noOrdersFound')" data-testid="orders-empty">
        <template #icon>
          <UiIcon :markup="Icon.Clipboard" size="lg" />
        </template>
      </UiEmptyState>
    </UiCard>
  </UiStack>
</template>
