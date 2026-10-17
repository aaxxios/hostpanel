<script setup lang="ts">
/**
 * Credit tab of the invoice screen: apply credit to the invoice or take it back, with the line
 * items and the transaction ledgers below.
 */
import { computed, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import InvoiceItemsSection from '@/components/billing/InvoiceItemsSection.vue'
import InvoiceTransactionsSection from '@/components/billing/InvoiceTransactionsSection.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiGrid from '@/components/ui/UiGrid.vue'
import UiHeading from '@/components/ui/UiHeading.vue'
import UiNumberInput from '@/components/ui/UiNumberInput.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiText from '@/components/ui/UiText.vue'
import { useBillingStore } from '@/stores/billing'
import type { Invoice } from '@/types/invoice'

const { t } = useI18n()

const props = defineProps<{
  /** The invoice to manage credit for. */
  invoice: Invoice
  /** True to show the line items without editing controls. */
  readonly?: boolean
}>()

const emit = defineEmits<{
  /** Emitted after credit was applied or removed, or the items were saved. */
  updated: []
}>()

const store = useBillingStore()

/** Credit to apply to the invoice. */
const addAmount = ref(0)

/** Credit to take back from the invoice. */
const removeAmount = ref(0)

/** True while a credit change is in flight. */
const processing = ref(false)

/** Credit currently applied to the invoice. */
const availableCredit = computed<number>(() => props.invoice.credit ?? 0)

/** The credit available, formatted for the caption under each box. */
const availableText = computed<string>(() =>
  t('billing.invoiceCreditTab.valueUsdAvailable', { value: availableCredit.value.toFixed(2) }),
)

/** True when "apply credit" has an amount to apply. */
const canAdd = computed<boolean>(() => !processing.value && addAmount.value > 0)

/** True when "remove credit" has an amount and credit to take back. */
const canRemove = computed<boolean>(() => !processing.value && removeAmount.value > 0 && availableCredit.value > 0)

/** Applies the credit and tells the parent to reload. */
const addCredit = async (): Promise<void> => {
  processing.value = true
  const ok = await store.applyCredit(props.invoice.id, addAmount.value)
  processing.value = false
  if (ok) {
    addAmount.value = 0
    emit('updated')
  }
}

/** Takes the credit back and tells the parent to reload. */
const removeCredit = async (): Promise<void> => {
  processing.value = true
  const ok = await store.removeCredit(props.invoice.id, removeAmount.value)
  processing.value = false
  if (ok) {
    removeAmount.value = 0
    emit('updated')
  }
}
</script>

<template>
  <UiStack data-testid="billing-invoice-credit">
    <UiCard padding="md">
      <UiGrid :columns="2" gap="lg">
        <UiStack align="center" gap="sm">
          <UiHeading :level="3" variant="card">
            {{ t('billing.invoiceCreditTab.addCreditToInvoice') }}
          </UiHeading>
          <UiStack direction="row" gap="sm" align="center">
            <UiNumberInput v-model="addAmount" :step="0.01" :min="0" placeholder="0.00" data-testid="billing-invoice-credit-add-amount" />
            <UiButton
              variant="secondary"
              :disabled="!canAdd"
              data-testid="billing-invoice-credit-add"
              @click="addCredit"
            >
              {{ t('shared.go') }}
            </UiButton>
          </UiStack>
          <UiText tone="success" size="sm">{{ availableText }}</UiText>
        </UiStack>

        <UiStack align="center" gap="sm">
          <UiHeading :level="3" variant="card">
            {{ t('billing.invoiceCreditTab.removeCreditFromInvoice') }}
          </UiHeading>
          <UiStack direction="row" gap="sm" align="center">
            <UiNumberInput v-model="removeAmount" :step="0.01" :min="0" placeholder="0.00" data-testid="billing-invoice-credit-remove-amount" />
            <UiButton
              variant="secondary"
              :disabled="!canRemove"
              data-testid="billing-invoice-credit-remove"
              @click="removeCredit"
            >
              {{ t('shared.go') }}
            </UiButton>
          </UiStack>
          <UiText tone="success" size="sm">{{ availableText }}</UiText>
        </UiStack>
      </UiGrid>
    </UiCard>

    <InvoiceItemsSection :invoice="invoice" :readonly="readonly" @updated="emit('updated')" />
    <InvoiceTransactionsSection :invoice="invoice" />
  </UiStack>
</template>
