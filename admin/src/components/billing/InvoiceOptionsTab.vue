<script setup lang="ts">
/**
 * Options tab of the invoice screen: invoice date, due date, payment method and the two tax
 * rates, saved together; below them the editable line items (previewed with the combined tax)
 * and the transaction ledgers.
 */
import { computed, onMounted, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import InvoiceItemsSection from '@/components/billing/InvoiceItemsSection.vue'
import InvoiceTransactionsSection from '@/components/billing/InvoiceTransactionsSection.vue'
import UiAlert from '@/components/ui/UiAlert.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiDatePicker from '@/components/ui/UiDatePicker.vue'
import { UiDatePickerDirection } from '@/utils/ui/datePicker'
import UiField from '@/components/ui/UiField.vue'
import UiForm from '@/components/ui/UiForm.vue'
import UiGrid from '@/components/ui/UiGrid.vue'
import UiNumberInput from '@/components/ui/UiNumberInput.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiText from '@/components/ui/UiText.vue'
import { GATEWAY_OPTIONS } from '@/lib/constants'
import { toDateInputValue } from '@/lib/format'
import { InvoiceStatus } from '@/types/invoice'
import { useBillingStore } from '@/stores/billing'
import type { Invoice } from '@/types/invoice'

const { t } = useI18n()

const props = defineProps<{
  /** The invoice whose options are edited. */
  invoice: Invoice
}>()

const emit = defineEmits<{
  /** Emitted after the options or the items were saved. */
  updated: []
}>()

const store = useBillingStore()

/** Editable invoice date (ISO day). */
const invoiceDate = ref<string | null>(null)

/** Editable due date (ISO day). */
const dueDate = ref<string | null>(null)

/** Editable payment method. */
const paymentMethod = ref('')

/** Primary tax rate, percent. */
const taxRate1 = ref(0)

/** Secondary tax rate, percent. */
const taxRate2 = ref(0)

/** True while the options are being saved. */
const saving = ref(false)

/** The rate the API stores: both taxes added together. */
const combinedTaxRate = computed<number>(() => taxRate1.value + taxRate2.value)

/** True when the invoice is a draft the client cannot see yet. */
const isDraft = computed<boolean>(() => props.invoice.status === InvoiceStatus.Draft)

/** Fills the form from the invoice. */
const populate = (): void => {
  invoiceDate.value = toDateInputValue(props.invoice.invoiceDate)
  dueDate.value = toDateInputValue(props.invoice.dueDate)
  paymentMethod.value = props.invoice.paymentMethod ?? GATEWAY_OPTIONS[0]?.value ?? ''
  taxRate1.value = props.invoice.taxRate ?? 0
  taxRate2.value = 0
}

/** Saves the options and tells the parent to reload. */
const save = async (): Promise<void> => {
  saving.value = true
  const ok = await store.updateOptions(props.invoice.id, {
    invoiceDate: invoiceDate.value ?? '',
    dueDate: dueDate.value ?? '',
    paymentMethod: paymentMethod.value,
    taxRate: combinedTaxRate.value,
  })
  saving.value = false
  if (ok) {
    emit('updated')
  }
}

onMounted(populate)
</script>

<template>
  <UiStack data-testid="billing-invoice-options">
    <UiAlert v-if="isDraft" variant="info" data-testid="billing-invoice-options-draft">
      {{ t('billing.invoiceOptionsTab.thisIsADraftInvoice') }}
    </UiAlert>

    <UiCard padding="md">
      <UiForm stacked data-testid="billing-invoice-options-form" @submit="save">
        <UiGrid :columns="2">
          <UiField :label="t('shared.invoiceDate')" :error="store.fieldErrors.invoiceDate">
            <UiDatePicker v-model="invoiceDate" :placeholder="t('shared.selectDate')" />
          </UiField>
          <UiField :label="t('shared.dueDate')" :error="store.fieldErrors.dueDate">
            <UiDatePicker v-model="dueDate" :direction="UiDatePickerDirection.Future" :placeholder="t('shared.selectDate')" />
          </UiField>
          <UiField :label="t('shared.paymentMethod')" :error="store.fieldErrors.paymentMethod">
            <UiSelect v-model="paymentMethod" :options="GATEWAY_OPTIONS" :placeholder="t('shared.paymentMethod')" />
          </UiField>
          <UiField :label="t('billing.invoiceOptionsTab.taxRate')" :error="store.fieldErrors.taxRate">
            <UiStack direction="row" gap="sm" align="center">
              <UiText tone="muted" size="sm">1</UiText>
              <UiNumberInput v-model="taxRate1" :step="0.01" :min="0" placeholder="0.00" data-testid="billing-invoice-tax-1" />
              <UiText tone="muted" size="sm">%</UiText>
              <UiText tone="muted" size="sm">2</UiText>
              <UiNumberInput v-model="taxRate2" :step="0.01" :min="0" placeholder="0.00" data-testid="billing-invoice-tax-2" />
              <UiText tone="muted" size="sm">%</UiText>
            </UiStack>
          </UiField>
        </UiGrid>
        <UiStack direction="row" justify="center">
          <UiButton
            type="submit"
            variant="primary"
            :loading="saving"
            data-testid="billing-invoice-options-save"
          >
            {{ t('shared.saveChanges') }}
          </UiButton>
        </UiStack>
      </UiForm>
    </UiCard>

    <InvoiceItemsSection :invoice="invoice" :tax-percent="combinedTaxRate" @updated="emit('updated')" />
    <InvoiceTransactionsSection :invoice="invoice" />
  </UiStack>
</template>
