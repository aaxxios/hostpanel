<script setup lang="ts">
/**
 * Summary tab of the invoice screen: who is billed, the dates and amounts, the status, and the
 * actions that move the invoice along (publish, attempt capture, cancel, mark paid). The line
 * items and the transaction ledgers sit below.
 */
import { computed, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { useFallbackLabels } from '@/composables/useFallbackLabels'
import InvoiceItemsSection from '@/components/billing/InvoiceItemsSection.vue'
import InvoiceTransactionsSection from '@/components/billing/InvoiceTransactionsSection.vue'
import UiAlert from '@/components/ui/UiAlert.vue'
import UiBadge from '@/components/ui/UiBadge.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiGrid from '@/components/ui/UiGrid.vue'
import UiLink from '@/components/ui/UiLink.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiText from '@/components/ui/UiText.vue'
import { formatDate } from '@/lib/format'
import { invoiceStatusTone } from '@/lib/tones'
import { formatUsd } from '@/lib/money'
import { useBillingStore } from '@/stores/billing'
import { InvoiceBulkAction, InvoiceTransactionType, InvoiceStatus } from '@/types/invoice'
import type { Invoice } from '@/types/invoice'
import { UiTextTone } from '@/utils/ui/text'
import { ClientSection, clientSectionPath } from '@/lib/routes'

const { t } = useI18n()


/** Fallback text for fields the API leaves unresolved. */

const { clientLabel } = useFallbackLabels()

const props = defineProps<{
  /** The invoice to display. */
  invoice: Invoice
  /** True to hide the editing controls and the action buttons. */
  readonly?: boolean
}>()

const emit = defineEmits<{
  /** Emitted after any change (publish, capture, cancel, mark paid, items saved). */
  updated: []
}>()

/** The actions this tab can run; one runs at a time. */
const SummaryAction = {
  Publish: 'publish',
  Capture: 'capture',
  Cancel: 'cancel',
  MarkPaid: 'markPaid',
} as const

/** One of the {@link SummaryAction} values. */
type SummaryAction = (typeof SummaryAction)[keyof typeof SummaryAction]

/** One caption/value line of the invoice facts. */
interface FactRow {
  /** Stable key of the row. */
  key: string
  /** Row caption. */
  label: string
  /** Row value. */
  value: string
  /** Tone of the value. */
  tone: UiTextTone
}

const store = useBillingStore()

/** The action currently in flight, null when idle. */
const running = ref<SummaryAction | null>(null)

/** Statuses from which the invoice can still be paid. */
const PAYABLE: string[] = [InvoiceStatus.Unpaid, InvoiceStatus.Overdue]

/** Statuses from which the invoice can still be cancelled. */
const CANCELLABLE: string[] = [InvoiceStatus.Draft, InvoiceStatus.Unpaid, InvoiceStatus.Overdue]

/** True when the invoice is a draft the client cannot see yet. */
const isDraft = computed<boolean>(() => props.invoice.status === InvoiceStatus.Draft)

/** True when the invoice can still be paid. */
const canPay = computed<boolean>(() => PAYABLE.includes(props.invoice.status))

/** True when the invoice can still be cancelled. */
const canCancel = computed<boolean>(() => CANCELLABLE.includes(props.invoice.status))

/** Sum of the payments recorded against the invoice. */
const paymentsTotal = computed<number>(() =>
  (props.invoice.transactions ?? [])
    .filter((transaction) => transaction.type === InvoiceTransactionType.Payment)
    .reduce((sum, transaction) => sum + transaction.amount, 0),
)

/**
 * What is still owed. A Paid invoice owes nothing: marking one paid (bulk action, the client's own
 * payment, a completed gateway session) settles it through the ledger without a row in the
 * invoice's own payment list, so subtracting that list alone left a settled invoice showing its
 * whole total as the balance.
 */
const balance = computed<number>(() =>
  props.invoice.status === InvoiceStatus.Paid ? 0 : Math.max(0, (props.invoice.total ?? 0) - paymentsTotal.value),
)

/** The invoice facts shown beside the status. */
const facts = computed<FactRow[]>(() => [
  { key: 'invoiceDate', label: t('shared.invoiceDate'), value: formatDate(props.invoice.invoiceDate), tone: UiTextTone.Secondary },
  { key: 'dueDate', label: t('shared.dueDate'), value: formatDate(props.invoice.dueDate), tone: UiTextTone.Secondary },
  { key: 'amount', label: t('billing.invoiceSummaryTab.invoiceAmount'), value: formatUsd(props.invoice.total ?? 0), tone: UiTextTone.Secondary },
  { key: 'balance', label: t('shared.balance'), value: formatUsd(balance.value), tone: balance.value > 0 ? UiTextTone.Danger : UiTextTone.Success },
])

/**
 * Runs one invoice action, then tells the parent to reload.
 *
 * @param action - Which action is running.
 * @param task - The store call; true when the change was accepted.
 */
const run = async (action: SummaryAction, task: () => Promise<boolean>): Promise<void> => {
  running.value = action
  const ok = await task()
  running.value = null
  if (ok) {
    emit('updated')
  }
}

/**
 * Publishes the draft.
 *
 * @returns Resolves when the request has settled.
 */
const publish = (): Promise<void> => run(SummaryAction.Publish, () => store.publishInvoice(props.invoice.id))

/**
 * Attempts to capture the payment through the gateway.
 *
 * @returns Resolves when the request has settled.
 */
const capture = (): Promise<void> => run(SummaryAction.Capture, () => store.payInvoice(props.invoice.id))

/**
 * Cancels the invoice.
 *
 * @returns Resolves when the request has settled.
 */
const cancel = (): Promise<void> => run(SummaryAction.Cancel, () => store.cancelInvoice(props.invoice.id))

/**
 * Marks the invoice paid by hand and reloads it.
 *
 * @returns Resolves when the request has settled.
 */
const markPaid = (): Promise<void> =>
  run(SummaryAction.MarkPaid, async () => {
    const ok = await store.bulkAction([props.invoice.id], InvoiceBulkAction.MarkPaid)
    if (ok) {
      await store.fetchById(props.invoice.id)
    }
    return ok
  })
</script>

<template>
  <UiStack data-testid="billing-invoice-summary">
    <UiAlert v-if="isDraft && !readonly" variant="info" data-testid="billing-invoice-draft-notice">
      {{ t('billing.invoiceSummaryTab.thisIsADraftInvoice') }}
    </UiAlert>

    <UiCard padding="md">
      <UiGrid :columns="2" gap="lg">
        <UiStack gap="sm">
          <UiStack direction="row" gap="md" align="center" wrap>
            <UiText tone="muted" size="md">{{ t('shared.clientName') }}</UiText>
            <UiText tone="primary" size="md" data-testid="billing-invoice-client-name">{{ clientLabel(invoice.clientName, invoice.clientId) }}</UiText>
            <UiLink
              variant="link"
              :to="clientSectionPath(invoice.clientId, ClientSection.Invoices)"
              data-testid="billing-invoice-client-invoices"
            >
              {{ t('billing.invoiceSummaryTab.viewInvoices') }}
            </UiLink>
          </UiStack>
          <UiStack v-for="fact in facts" :key="fact.key" direction="row" gap="md" align="center">
            <UiText tone="muted" size="md">{{ fact.label }}</UiText>
            <UiText :tone="fact.tone" size="md" :data-testid="`billing-invoice-fact-${fact.key}`">{{ fact.value }}</UiText>
          </UiStack>
        </UiStack>

        <UiStack align="center" gap="sm">
          <UiBadge :tone="invoiceStatusTone(invoice.status)" data-testid="billing-invoice-status">
            {{ invoice.status }}
          </UiBadge>
          <UiText tone="muted" size="sm">
            {{ t('billing.invoiceSummaryTab.lastCaptureAttempt') }} {{ t('shared.none') }}
          </UiText>
          <UiText tone="muted" size="sm">
            {{ t('billing.invoiceSummaryTab.paymentMethod') }} {{ invoice.paymentMethod || t('shared.none') }}
          </UiText>

          <UiStack v-if="!readonly" direction="row" gap="sm" justify="center" wrap>
            <UiButton
              v-if="isDraft"
              variant="primary"
              :loading="running === SummaryAction.Publish"
              :disabled="running !== null"
              data-testid="billing-invoice-publish"
              @click="publish"
            >
              {{ t('billing.invoiceSummaryTab.publish') }}
            </UiButton>
            <UiButton
              v-if="canPay"
              variant="secondary"
              :loading="running === SummaryAction.Capture"
              :disabled="running !== null"
              data-testid="billing-invoice-capture"
              @click="capture"
            >
              {{ t('billing.invoiceSummaryTab.attemptCapture') }}
            </UiButton>
            <UiButton
              v-if="canCancel"
              variant="secondary"
              :loading="running === SummaryAction.Cancel"
              :disabled="running !== null"
              data-testid="billing-invoice-cancel"
              @click="cancel"
            >
              {{ t('shared.markCancelled') }}
            </UiButton>
            <UiButton
              v-if="canPay"
              variant="primary"
              :loading="running === SummaryAction.MarkPaid"
              :disabled="running !== null"
              data-testid="billing-invoice-mark-paid"
              @click="markPaid"
            >
              {{ t('billing.invoiceSummaryTab.markPaid') }}
            </UiButton>
          </UiStack>
        </UiStack>
      </UiGrid>
    </UiCard>

    <InvoiceItemsSection :invoice="invoice" :readonly="readonly" @updated="emit('updated')" />
    <InvoiceTransactionsSection :invoice="invoice" />
  </UiStack>
</template>
