<script setup lang="ts">
/**
 * Refund tab of the invoice screen: pick a payment, the amount and the kind of refund, decide
 * what happens to the credit the payment created, and send it. A draft, cancelled or already
 * refunded invoice takes no refund, so the form is replaced by the reason. The line items and
 * the transaction ledgers sit below.
 */
import { computed, ref, watch } from 'vue'
import { formatUsd, formatUsdPlain } from '@/lib/money'
import { useI18n } from 'vue-i18n'
import InvoiceItemsSection from '@/components/billing/InvoiceItemsSection.vue'
import InvoiceTransactionsSection from '@/components/billing/InvoiceTransactionsSection.vue'
import UiAlert from '@/components/ui/UiAlert.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiField from '@/components/ui/UiField.vue'
import UiForm from '@/components/ui/UiForm.vue'
import UiInput from '@/components/ui/UiInput.vue'
import UiLabel from '@/components/ui/UiLabel.vue'
import UiNumberInput from '@/components/ui/UiNumberInput.vue'
import UiRadio from '@/components/ui/UiRadio.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiText from '@/components/ui/UiText.vue'
import { useBillingStore } from '@/stores/billing'
import { InvoiceRefundType, InvoiceTransactionType, InvoiceStatus } from '@/types/invoice'
import type { Invoice, InvoiceTransaction } from '@/types/invoice'

const { t } = useI18n()

const props = defineProps<{
  /** The invoice to refund a payment of. */
  invoice: Invoice
  /** True to show the notice and the items without the refund form. */
  readonly?: boolean
}>()

const emit = defineEmits<{
  /** Emitted after a refund was sent or the items were saved. */
  updated: []
}>()

/** What happens to the credit a refunded payment created. */
const CreditAction = {
  None: '',
  Remove: 'remove',
  Leave: 'leave',
} as const

const store = useBillingStore()

/** Id of the payment picked for refund, as the select holds it. */
const selectedTransactionId = ref('')

/** Amount to refund. */
const refundAmount = ref(0)

/** Kind of refund. */
const refundType = ref<string>(InvoiceRefundType.Gateway)

/** Reference of the refund, for a refund made outside the gateway. */
const refundReference = ref('')

/** The choice about the credit; the refund waits for it. */
const creditAction = ref<string>(CreditAction.None)

/** True while the refund is being sent. */
const processing = ref(false)

/** Payments that can still be refunded. */
const payments = computed<InvoiceTransaction[]>(() =>
  (props.invoice.transactions ?? []).filter((transaction) => transaction.type === InvoiceTransactionType.Payment),
)

/** The picked payment, if any. */
const selectedPayment = computed<InvoiceTransaction | undefined>(() =>
  payments.value.find((transaction) => String(transaction.id) === selectedTransactionId.value),
)

/** True when the refund is made outside the gateway and needs a reference. */
const isManual = computed<boolean>(() => refundType.value === InvoiceRefundType.Manual)

/** True when the invoice is a draft, which cannot be refunded. */
const isDraft = computed<boolean>(() => props.invoice.status === InvoiceStatus.Draft)

/** True when the invoice is cancelled, which cannot be refunded. */
const isCancelled = computed<boolean>(() => props.invoice.status === InvoiceStatus.Cancelled)

/** True when the invoice was already refunded. */
const isRefunded = computed<boolean>(() => props.invoice.status === InvoiceStatus.Refunded)

/** True when the refund form is replaced by a notice. */
const formBlocked = computed<boolean>(() => isDraft.value || isCancelled.value || isRefunded.value)

/** True when the refund can be sent: a payment is picked and the credit question answered. */
const canRefund = computed<boolean>(
  () => selectedTransactionId.value !== '' && creditAction.value !== CreditAction.None && !processing.value,
)

/** Options of the payment select. */
const transactionOptions = computed(() => {
  if (payments.value.length === 0) {
    return [{ value: '', label: t('billing.invoiceRefundTab.noTransactionsAppliedToThis') }]
  }
  return [
    { value: '', label: t('billing.invoiceRefundTab.selectATransaction') },
    ...payments.value.map((transaction) => ({
      value: String(transaction.id),
      label: `#${transaction.id} — ${formatUsdPlain(transaction.amount)} (${transaction.gateway})`,
    })),
  ]
})

/** Options of the refund-kind select. */
const refundTypeOptions = computed(() => [
  { value: InvoiceRefundType.Gateway, label: t('billing.invoiceRefundTab.refundThroughGateway') },
  { value: InvoiceRefundType.Manual, label: t('billing.invoiceRefundTab.manualRefundProcessedExternally') },
  { value: InvoiceRefundType.CreditBalance, label: t('billing.addTransaction.addToClientSCredit') },
])

/** The warning that explains what the credit question is about. */
const creditWarning = computed<string>(() => {
  const refunded = formatUsd(selectedPayment.value?.amount)
  const balance = formatUsd(props.invoice.credit)
  return `${t('billing.invoiceRefundTab.theseTransactionsResultedInA')} ${refunded}${t('billing.invoiceRefundTab.theCurrentCreditBalanceIs')} ${balance}.`
})

/** Picking a payment resets the credit answer and proposes the full amount. */
watch(selectedTransactionId, () => {
  creditAction.value = CreditAction.None
  if (selectedPayment.value !== undefined) {
    refundAmount.value = selectedPayment.value.amount
  }
})

/** Sends the refund, then clears the form. */
const refund = async (): Promise<void> => {
  if (!canRefund.value) {
    return
  }
  processing.value = true
  const kind = creditAction.value === CreditAction.Remove ? InvoiceRefundType.CreditBalance : refundType.value
  const ok = await store.refundPayment(props.invoice.id, {
    transactionId: selectedTransactionId.value,
    amount: refundAmount.value,
    refundType: kind,
    gateway: props.invoice.paymentMethod ?? 'None',
    refundTransactionId: isManual.value ? refundReference.value : undefined,
  })
  processing.value = false
  if (ok) {
    selectedTransactionId.value = ''
    refundAmount.value = 0
    refundType.value = InvoiceRefundType.Gateway
    refundReference.value = ''
    creditAction.value = CreditAction.None
    emit('updated')
  }
}
</script>

<template>
  <UiStack data-testid="billing-invoice-refund">
    <UiAlert v-if="readonly && payments.length === 0" variant="warning" data-testid="billing-invoice-refund-none">
      {{ t('billing.invoiceRefundTab.thereAreNoTransactionsThat') }}
    </UiAlert>

    <template v-if="!readonly">
      <UiAlert v-if="isDraft" variant="warning" data-testid="billing-invoice-refund-draft">
        {{ t('billing.invoiceRefundTab.publishTheInvoiceBeforeRequesting') }}
      </UiAlert>
      <UiAlert v-else-if="isCancelled" variant="warning" data-testid="billing-invoice-refund-cancelled">
        {{ t('billing.invoiceRefundTab.cannotRefundACancelledInvoice') }}
      </UiAlert>
      <UiAlert v-else-if="isRefunded" variant="warning" data-testid="billing-invoice-refund-refunded">
        {{ t('billing.invoiceRefundTab.thisInvoiceHasAlreadyBeen') }}
      </UiAlert>

      <UiCard v-if="!formBlocked" padding="md">
        <UiForm stacked data-testid="billing-invoice-refund-form" @submit="refund">
          <UiField :label="t('shared.transactions')">
            <UiSelect
              v-model="selectedTransactionId"
              :options="transactionOptions"
              :placeholder="t('billing.invoiceRefundTab.selectATransaction')"
              data-testid="billing-invoice-refund-transaction"
            />
          </UiField>
          <UiField :label="t('shared.amount')" :hint="t('billing.invoiceRefundTab.leaveBlankForFullRefund')" :error="store.fieldErrors.amount">
            <UiNumberInput v-model="refundAmount" :step="0.01" :min="0" placeholder="0.00" data-testid="billing-invoice-refund-amount" />
          </UiField>
          <UiField :label="t('billing.invoiceRefundTab.refundType')" :error="store.fieldErrors.refundType">
            <UiSelect v-model="refundType" :options="refundTypeOptions" :placeholder="t('billing.invoiceRefundTab.refundType')" />
          </UiField>
          <UiField v-if="isManual" :label="t('shared.transactionId')" :error="store.fieldErrors.refundTransactionId">
            <template #default="{ id, invalid }">
              <UiInput
                :id="id"
                v-model="refundReference"
                type="text"
                :invalid="invalid"
                :placeholder="t('billing.invoiceRefundTab.externalRefundReference')"
                data-testid="billing-invoice-refund-reference"
              />
            </template>
          </UiField>

          <UiStack v-if="selectedPayment" gap="sm" data-testid="billing-invoice-refund-credit">
            <UiText tone="danger" size="md">{{ t('billing.invoiceRefundTab.warning') }}</UiText>
            <UiText tone="secondary" size="md">{{ creditWarning }}</UiText>
            <UiText tone="muted" size="md">{{ t('billing.invoiceRefundTab.weCanThereforeAutomaticallyRemove') }}</UiText>
            <UiLabel variant="inline">
              <UiRadio
                v-model="creditAction"
                :value="CreditAction.Remove"
                data-testid="billing-invoice-refund-credit-remove"
              />
              {{ t('billing.invoiceRefundTab.clickHereToRemoveThe') }}
            </UiLabel>
            <UiLabel variant="inline">
              <UiRadio
                v-model="creditAction"
                :value="CreditAction.Leave"
                data-testid="billing-invoice-refund-credit-leave"
              />
              {{ t('billing.invoiceRefundTab.clickHereToLeaveThe') }}
            </UiLabel>
          </UiStack>

          <UiStack direction="row" justify="center">
            <UiButton
              type="submit"
              variant="primary"
              :loading="processing"
              :disabled="!canRefund"
              data-testid="billing-invoice-refund-submit"
            >
              {{ t('billing.invoiceRefundTab.refund') }}
            </UiButton>
          </UiStack>
        </UiForm>
      </UiCard>
    </template>

    <InvoiceItemsSection :invoice="invoice" :readonly="readonly" @updated="emit('updated')" />
    <InvoiceTransactionsSection :invoice="invoice" />
  </UiStack>
</template>
