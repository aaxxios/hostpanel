<script setup lang="ts">
/**
 * Edit transaction: loads one ledger transaction into the shared form, saves the changes and
 * returns to the ledger once the API accepts them. The client of a transaction cannot change.
 */
import { onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import TransactionForm from '@/components/billing/TransactionForm.vue'
import UiPageHeader from '@/components/ui/UiPageHeader.vue'
import UiStack from '@/components/ui/UiStack.vue'
import { useClientOptions } from '@/composables/useClientOptions'
import { blankTransactionForm, firstInvoiceId, formFromTransaction } from '@/lib/transactionForm'
import { useTransactionsStore } from '@/stores/billingTransactions'
import type { TransactionFormValues } from '@/types/transaction'
import { AppRoute } from '@/lib/routes'

const { t } = useI18n()

const route = useRoute()
const router = useRouter()
const store = useTransactionsStore()
const { clients, load: loadClients } = useClientOptions()

/** The form values. */
const values = ref<TransactionFormValues>(blankTransactionForm())

/**
 * Reads the id of the transaction from the route.
 *
 * @returns The transaction id.
 */
const transactionId = (): number => Number.parseInt(String(route.params.id), 10)

/** Saves the changes and returns to the ledger when they were accepted. */
const submit = async (): Promise<void> => {
  const ok = await store.update(transactionId(), {
    date: values.value.date ?? '',
    description: values.value.description,
    transactionId: values.value.transactionId,
    invoiceId: firstInvoiceId(values.value.invoiceIds),
    paymentMethod: values.value.paymentMethod,
    amountIn: values.value.amountIn,
    amountOut: values.value.amountOut,
    fees: values.value.fees,
  })
  if (ok) {
    await router.push(AppRoute.BillingTransactions)
  }
}

watch(() => store.currentTransaction, (transaction) => {
  if (transaction !== null) {
    values.value = formFromTransaction(transaction)
  }
})

onMounted(() => {
  void loadClients()
  void store.fetchById(transactionId())
})
</script>

<template>
  <UiStack data-testid="billing-edit-transaction-view">
    <UiPageHeader
      :title="t('billing.editTransaction.editTransaction')"
      :subtitle="t('billing.editTransaction.updatePaymentTransactionDetails')"
    />
    <TransactionForm
      v-model="values"
      lock-client
      :clients="clients"
      :submit-label="t('billing.editTransaction.updateTransaction')"
      :loading="store.loading"
      :error="store.error"
      :field-errors="store.fieldErrors"
      @submit="submit"
    />
  </UiStack>
</template>
