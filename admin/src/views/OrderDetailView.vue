<script setup lang="ts">
/**
 * Order detail view — one order with its items, linked invoice and the admin actions
 * (accept, cancel, delete).
 *
 * Reads `stores/orders`; a refused load shows `store.error`, a refused accept, cancel or delete
 * shows `store.actionError` above the order.
 */
import { onMounted, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { useFallbackLabels } from '@/composables/useFallbackLabels'
import { useRoute, useRouter } from 'vue-router'
import OrderFact from '@/components/orders/OrderFact.vue'
import UiAlert from '@/components/ui/UiAlert.vue'
import UiBadge from '@/components/ui/UiBadge.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiDialog from '@/components/ui/UiDialog.vue'
import UiGrid from '@/components/ui/UiGrid.vue'
import UiHeading from '@/components/ui/UiHeading.vue'
import UiIcon from '@/components/ui/UiIcon.vue'
import UiLink from '@/components/ui/UiLink.vue'
import UiPageHeader from '@/components/ui/UiPageHeader.vue'
import UiSkeleton from '@/components/ui/UiSkeleton.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiTable from '@/components/ui/UiTable.vue'
import UiTableBody from '@/components/ui/UiTableBody.vue'
import UiTableCell from '@/components/ui/UiTableCell.vue'
import UiTableHead from '@/components/ui/UiTableHead.vue'
import UiTableHeader from '@/components/ui/UiTableHeader.vue'
import UiTableRow from '@/components/ui/UiTableRow.vue'
import UiText from '@/components/ui/UiText.vue'
import UiTextarea from '@/components/ui/UiTextarea.vue'
import { Icon } from '@/lib/icons'
import { INVOICE_STATUS_OPTIONS, ORDER_STATUS_OPTIONS } from '@/lib/constants'
import { capitalize, formatDateTime } from '@/lib/format'
import { formatMoney } from '@/lib/money'
import { invoiceStatusTone, orderItemStatusTone, orderStatusTone, statusLabel } from '@/lib/tones'
import { OrderStatus } from '@/types/order'
import { useOrdersStore } from '@/stores/orders'
import { AppRoute, clientPath, invoicePath } from '@/lib/routes'

const { t } = useI18n()


/** Fallback text for fields the API leaves unresolved. */

const { clientLabel } = useFallbackLabels()

const route = useRoute()
const router = useRouter()
const store = useOrdersStore()

/** Whether the delete confirmation dialog is open. */
const showDeleteConfirm = ref(false)

/** Admin notes text area value. */
const notesText = ref('')

/**
 * Reads the order id from the route.
 *
 * @returns The order primary key.
 */
const orderId = (): number => Number(route.params.id)

/**
 * Accepts the current order.
 *
 * @returns Resolves when the store has finished.
 */
const handleAccept = async (): Promise<void> => {
  await store.acceptOrder(orderId())
}

/**
 * Cancels the current order.
 *
 * @returns Resolves when the store has finished.
 */
const handleCancel = async (): Promise<void> => {
  await store.cancelOrder(orderId())
}

/**
 * Deletes the current order and returns to the list; a refusal stays on the page and shows
 * `store.actionError`.
 *
 * @returns Resolves when the order is gone and the list is open, or the dialog is closed.
 */
const handleDelete = async (): Promise<void> => {
  const deleted = await store.deleteOrder(orderId())
  showDeleteConfirm.value = false
  if (deleted) {
    await router.push(AppRoute.Orders)
  }
}

onMounted(async () => {
  // A refusal from an earlier visit must not greet the reader.
  store.actionError = null
  await store.fetchOne(orderId())
  if (store.currentOrder) {
    notesText.value = store.currentOrder.notes ?? ''
  }
})
</script>

<template>
  <UiStack>
    <UiLink variant="link" :to="AppRoute.Orders" data-testid="order-back">
      <UiIcon :markup="Icon.Back" size="sm" />
      {{ t('orders.orderDetail.backToOrders') }}
    </UiLink>

    <UiStack v-if="store.loading && !store.currentOrder" aria-busy="true" data-testid="order-loading">
      <UiText tone="secondary" size="md">
        {{ t('orders.orderDetail.loadingOrder') }}
      </UiText>
      <UiSkeleton :lines="3" />
    </UiStack>

    <UiAlert v-else-if="store.error" variant="danger" data-testid="order-error">
      {{ store.error }}
    </UiAlert>

    <template v-else-if="store.currentOrder">
      <UiAlert v-if="store.actionError" variant="danger" data-testid="order-action-error">
        {{ store.actionError }}
      </UiAlert>

      <UiPageHeader :title="store.currentOrder.orderNumber" data-testid="order-header">
        <UiStack direction="row" gap="sm" align="center" wrap>
          <UiBadge :tone="orderStatusTone(store.currentOrder.status)" data-testid="order-status">
            {{ statusLabel(ORDER_STATUS_OPTIONS, store.currentOrder.status) }}
          </UiBadge>
          <UiText tone="muted" size="sm">
            {{ formatDateTime(store.currentOrder.createdAt) }}
          </UiText>
        </UiStack>
        <template #actions>
          <UiButton
            v-if="store.currentOrder.status === OrderStatus.Pending"
            variant="secondary"
            size="sm"
            data-testid="order-accept"
            @click="handleAccept"
          >
            <UiIcon :markup="Icon.Check" size="sm" />
            {{ t('orders.orderDetail.accept') }}
          </UiButton>
          <UiButton
            v-if="store.currentOrder.status === OrderStatus.Pending"
            variant="danger"
            size="sm"
            data-testid="order-cancel"
            @click="handleCancel"
          >
            <UiIcon :markup="Icon.Close" size="sm" />
            {{ t('shared.cancel') }}
          </UiButton>
          <UiButton
            v-if="store.currentOrder.status === OrderStatus.Pending || store.currentOrder.status === OrderStatus.Cancelled"
            variant="ghost"
            size="sm"
            data-testid="order-delete"
            @click="showDeleteConfirm = true"
          >
            <UiIcon :markup="Icon.Trash" size="sm" />
            {{ t('shared.delete') }}
          </UiButton>
        </template>
      </UiPageHeader>

      <UiGrid :columns="2">
        <UiCard padding="md" data-testid="order-info">
          <UiStack>
            <UiHeading :level="2" variant="section">
              {{ t('orders.orderDetail.orderInfo') }}
            </UiHeading>
            <OrderFact :label="t('shared.client')">
              <UiLink variant="link" :to="clientPath(store.currentOrder.clientId)" data-testid="order-client">
                {{ clientLabel(store.currentOrder.clientName, store.currentOrder.clientId) }}
              </UiLink>
            </OrderFact>
            <OrderFact :label="t('shared.paymentMethod')">
              {{ store.currentOrder.paymentMethod }}
            </OrderFact>
            <OrderFact :label="t('shared.ipAddress')">
              {{ store.currentOrder.ipAddress ?? '—' }}
            </OrderFact>
            <OrderFact :label="t('orders.orderDetail.created')">
              {{ formatDateTime(store.currentOrder.createdAt) }}
            </OrderFact>
          </UiStack>
        </UiCard>

        <UiCard padding="md" data-testid="order-invoice">
          <UiStack>
            <UiHeading :level="2" variant="section">
              {{ t('orders.orderDetail.invoice') }}
            </UiHeading>
            <template v-if="store.currentOrder.invoiceId">
              <OrderFact :label="t('shared.invoiceId')">
                <UiLink variant="link" :to="invoicePath(store.currentOrder.invoiceId)" data-testid="order-invoice-link">
                  #{{ store.currentOrder.invoiceId }}
                </UiLink>
              </OrderFact>
              <OrderFact :label="t('shared.status')">
                <UiBadge
                  v-if="store.currentOrder.invoiceStatus"
                  :tone="invoiceStatusTone(store.currentOrder.invoiceStatus)"
                  small
                >
                  {{ statusLabel(INVOICE_STATUS_OPTIONS, store.currentOrder.invoiceStatus) }}
                </UiBadge>
                <template v-else>
                  &mdash;
                </template>
              </OrderFact>
              <OrderFact :label="t('shared.total')">
                {{ formatMoney(store.currentOrder.total) }}
              </OrderFact>
            </template>
            <UiText v-else tone="muted" size="md">
              {{ t('orders.orderDetail.noInvoiceLinkedToThis') }}
            </UiText>
          </UiStack>
        </UiCard>
      </UiGrid>

      <UiCard data-testid="order-items">
        <UiTable density="comfortable">
          <UiTableHead>
            <UiTableRow bordered>
              <UiTableHeader>{{ t('orders.orderDetail.product') }}</UiTableHeader>
              <UiTableHeader>{{ t('shared.billingCycle') }}</UiTableHeader>
              <UiTableHeader>{{ t('shared.domain') }}</UiTableHeader>
              <UiTableHeader>{{ t('orders.orderDetail.firstPayment') }}</UiTableHeader>
              <UiTableHeader>{{ t('orders.orderDetail.recurring') }}</UiTableHeader>
              <UiTableHeader>{{ t('shared.status') }}</UiTableHeader>
            </UiTableRow>
          </UiTableHead>
          <UiTableBody>
            <UiTableRow
              v-for="item in store.currentOrder.items"
              :key="item.id"
              hoverable
              bordered
              :data-testid="`order-item-${item.id}`"
            >
              <UiTableCell>{{ item.productName }}</UiTableCell>
              <UiTableCell>{{ capitalize(item.billingCycle) }}</UiTableCell>
              <UiTableCell>{{ item.domain ?? item.hostname ?? '—' }}</UiTableCell>
              <UiTableCell>{{ formatMoney(item.firstPaymentAmount) }}</UiTableCell>
              <UiTableCell>{{ formatMoney(item.recurringAmount) }}</UiTableCell>
              <UiTableCell>
                <UiBadge :tone="orderItemStatusTone(item.status)" small>
                  {{ statusLabel(ORDER_STATUS_OPTIONS, item.status) }}
                </UiBadge>
              </UiTableCell>
            </UiTableRow>
          </UiTableBody>
        </UiTable>
      </UiCard>

      <UiCard padding="md" data-testid="order-notes">
        <UiStack gap="sm">
          <UiHeading :level="2" variant="section">
            {{ t('shared.adminNotes') }}
          </UiHeading>
          <UiTextarea
            v-model="notesText"
            :rows="4"
            :placeholder="t('orders.orderDetail.addNotesAboutThisOrder')"
            data-testid="order-notes-input"
          />
          <UiText tone="muted" size="xs">
            {{ t('orders.orderDetail.notesAreOnlyVisibleTo') }}
          </UiText>
        </UiStack>
      </UiCard>
    </template>

    <UiDialog
      v-if="showDeleteConfirm"
      variant="confirm"
      tone="danger"
      :title="t('orders.orderDetail.deleteOrder')"
      :message="t('orders.orderDetail.areYouSureYouWant')"
      :confirm-label="t('shared.delete')"
      @confirm="handleDelete"
      @close="showDeleteConfirm = false"
    />
  </UiStack>
</template>
