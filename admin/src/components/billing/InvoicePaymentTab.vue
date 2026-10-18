<script setup lang="ts">
/**
 * "Add payment" tab of the invoice screen: records a payment received outside the gateway, with
 * the line items and the transaction ledgers below. A draft or cancelled invoice takes no
 * payment, so the form is replaced by the reason.
 */
import { computed, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import InvoiceItemsSection from '@/components/billing/InvoiceItemsSection.vue'
import InvoiceTransactionsSection from '@/components/billing/InvoiceTransactionsSection.vue'
import UiAlert from '@/components/ui/UiAlert.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiDatePicker from '@/components/ui/UiDatePicker.vue'
import UiField from '@/components/ui/UiField.vue'
import UiForm from '@/components/ui/UiForm.vue'
import UiGrid from '@/components/ui/UiGrid.vue'
import UiHeading from '@/components/ui/UiHeading.vue'
import UiInput from '@/components/ui/UiInput.vue'
import UiNumberInput from '@/components/ui/UiNumberInput.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiText from '@/components/ui/UiText.vue'
import { GATEWAY_OPTIONS } from '@/lib/constants'
import { InvoiceStatus } from '@/types/invoice'
import { useBillingStore } from '@/stores/billing'
import type { Invoice } from '@/types/invoice'
import { PaymentGateway } from '@/types/gateway'

const { t } = useI18n()

const props = defineProps<{
  /** The invoice to add payments to. */
  invoice: Invoice
  /** True to show the line items without editing controls (the form stays usable). */
  readonly?: boolean
}>()

const emit = defineEmits<{
  /** Emitted after a payment was recorded or the items were saved. */
  updated: []
}>()

const store = useBillingStore()

/** Gateway preselected in the form: the first real one the options list. */
const DEFAULT_GATEWAY = GATEWAY_OPTIONS.find((option) => option.value !== PaymentGateway.None)?.value ?? PaymentGateway.None

/** Payment date (ISO day), null until picked. */
const paymentDate = ref<string | null>(null)

/** Gateway the payment came through. */
const gateway = ref<string>(DEFAULT_GATEWAY)

/** Gateway transaction reference. */
const transactionId = ref('')

/** Amount received. */
const amount = ref(0)

/** Gateway fees taken from the amount. */
const fees = ref(0)

/** True while the payment is being recorded. */
const submitting = ref(false)

/** True when the invoice is a draft, which takes no payment. */
const isDraft = computed<boolean>(() => props.invoice.status === InvoiceStatus.Draft)

/** True when the invoice is cancelled, which takes no payment. */
const isCancelled = computed<boolean>(() => props.invoice.status === InvoiceStatus.Cancelled)

/** True when the form has what the API needs. */
const canSubmit = computed<boolean>(() => paymentDate.value !== null && paymentDate.value !== '' && amount.value > 0)

/** Records the payment, then clears the form. */
const submit = async (): Promise<void> => {
  if (!canSubmit.value || paymentDate.value === null) {
    return
  }
  submitting.value = true
  const ok = await store.addPayment(props.invoice.id, {
    date: new Date(paymentDate.value).toISOString(),
    gateway: gateway.value,
    transactionId: transactionId.value,
    amount: amount.value,
    fees: fees.value,
  })
  submitting.value = false
  if (ok) {
    paymentDate.value = null
    gateway.value = DEFAULT_GATEWAY
    transactionId.value = ''
    amount.value = 0
    fees.value = 0
    emit('updated')
  }
}
</script>

<template>
  <UiStack data-testid="billing-invoice-payment">
    <UiCard v-if="isDraft" padding="md">
      <UiText tone="muted" size="md" data-testid="billing-invoice-payment-draft">
        {{ t('billing.invoicePaymentTab.publishTheInvoiceBeforeAdding') }}
      </UiText>
    </UiCard>

    <UiAlert v-else-if="isCancelled" variant="warning" data-testid="billing-invoice-payment-cancelled">
      {{ t('billing.invoicePaymentTab.invoiceIsCancelledPleaseMark') }}
    </UiAlert>

    <template v-else>
      <UiCard padding="md">
        <UiForm stacked data-testid="billing-invoice-payment-form" @submit="submit">
          <UiHeading :level="3" variant="card">
            {{ t('billing.invoicePaymentTab.addPayment') }}
          </UiHeading>
          <UiGrid :columns="2">
            <UiField :label="t('shared.date')" :error="store.fieldErrors.date">
              <UiDatePicker v-model="paymentDate" :placeholder="t('shared.selectDate')" />
            </UiField>
            <UiField :label="t('shared.amount')" :error="store.fieldErrors.amount">
              <UiNumberInput v-model="amount" :step="0.01" :min="0" placeholder="0.00" data-testid="billing-invoice-payment-amount" />
            </UiField>
            <UiField :label="t('shared.paymentMethod')" :error="store.fieldErrors.gateway">
              <UiSelect v-model="gateway" :options="GATEWAY_OPTIONS" :placeholder="t('shared.paymentMethod')" />
            </UiField>
            <UiField :label="t('shared.transactionFees')" :error="store.fieldErrors.fees">
              <UiNumberInput v-model="fees" :step="0.01" :min="0" placeholder="0.00" />
            </UiField>
            <UiField :label="t('shared.transactionId')" :error="store.fieldErrors.transactionId">
              <template #default="{ id, invalid }">
                <UiInput
                  :id="id"
                  v-model="transactionId"
                  type="text"
                  :invalid="invalid"
                  :placeholder="t('billing.invoicePaymentTab.gatewayReference')"
                  data-testid="billing-invoice-payment-reference"
                />
              </template>
            </UiField>
          </UiGrid>
          <UiStack direction="row" justify="center">
            <UiButton
              type="submit"
              variant="primary"
              :loading="submitting"
              :disabled="!canSubmit"
              data-testid="billing-invoice-payment-submit"
            >
              {{ t('billing.invoicePaymentTab.addPayment') }}
            </UiButton>
          </UiStack>
        </UiForm>
      </UiCard>

      <InvoiceItemsSection :invoice="invoice" :readonly="readonly" @updated="emit('updated')" />
      <InvoiceTransactionsSection :invoice="invoice" />
    </template>
  </UiStack>
</template>
