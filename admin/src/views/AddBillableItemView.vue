<script setup lang="ts">
/**
 * Add billable item: a one-time or recurring charge for a client. Recurring charges also take
 * how often they recur. The screen returns to the list once the API accepts the item.
 */
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import UiAlert from '@/components/ui/UiAlert.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiClientSelect from '@/components/ui/UiClientSelect.vue'
import UiDatePicker from '@/components/ui/UiDatePicker.vue'
import { UiDatePickerDirection } from '@/utils/ui/datePicker'
import UiField from '@/components/ui/UiField.vue'
import UiForm from '@/components/ui/UiForm.vue'
import UiGrid from '@/components/ui/UiGrid.vue'
import UiInput from '@/components/ui/UiInput.vue'
import UiLink from '@/components/ui/UiLink.vue'
import UiNumberInput from '@/components/ui/UiNumberInput.vue'
import UiPageHeader from '@/components/ui/UiPageHeader.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import UiStack from '@/components/ui/UiStack.vue'
import { useClientOptions } from '@/composables/useClientOptions'
import { CURRENCY_OPTIONS } from '@/lib/constants'
import { toIsoDay } from '@/lib/format'
import { useBillableItemsStore } from '@/stores/billableItems'
import { BillableItemType } from '@/types/billableitem'
import { CurrencyCode } from '@/types/currency'
import { AppRoute } from '@/lib/routes'

const { t } = useI18n()

/** Units a recurring charge can recur in, as the API expects them in the period text. */
const RecurrenceUnit = {
  Day: 'day',
  Week: 'week',
  Month: 'month',
  Year: 'year',
} as const

const router = useRouter()
const store = useBillableItemsStore()
const { clients, load: loadClients } = useClientOptions()

/** Id of the client to bill, as the picker holds it. */
const clientId = ref('')

/** What the charge is for. */
const description = ref('')

/** Charge amount. */
const amount = ref(0)

/** Charge currency. */
const currency = ref<string>(CurrencyCode.Usd)

/** One-time or recurring. */
const type = ref<string>(BillableItemType.OneTime)

/** Next (or only) due date (ISO day). */
const nextDueDate = ref<string | null>(toIsoDay(new Date()))

/** How many units a recurring charge waits between invoices. */
const recurEvery = ref(1)

/** The unit of the recurrence. */
const recurUnit = ref<string>(RecurrenceUnit.Month)

/** True when the charge recurs, so the recurrence fields show. */
const isRecurring = computed<boolean>(() => type.value === BillableItemType.Recurring)

/** Options of the type select. */
const typeOptions = computed(() => [
  { value: BillableItemType.OneTime, label: t('billing.addBillableItem.oneTime') },
  { value: BillableItemType.Recurring, label: t('billing.addBillableItem.recurringEvery') },
])

/** Options of the recurrence-unit select. */
const unitOptions = computed(() => [
  { value: RecurrenceUnit.Day, label: t('billing.addBillableItem.dayS') },
  { value: RecurrenceUnit.Week, label: t('billing.addBillableItem.weekS') },
  { value: RecurrenceUnit.Month, label: t('billing.addBillableItem.monthS') },
  { value: RecurrenceUnit.Year, label: t('billing.addBillableItem.yearS') },
])

/** Creates the item and returns to the list when it was accepted. */
const submit = async (): Promise<void> => {
  const ok = await store.create({
    clientId: Number.parseInt(clientId.value, 10),
    description: description.value,
    amount: amount.value,
    currency: currency.value,
    type: type.value,
    recurringPeriod: isRecurring.value ? `${recurEvery.value} ${recurUnit.value}` : undefined,
    nextDueDate: nextDueDate.value || undefined,
  })
  if (ok) {
    await router.push(AppRoute.BillingBillableItems)
  }
}

onMounted(loadClients)
</script>

<template>
  <UiStack data-testid="billing-add-billable-item-view">
    <UiPageHeader :title="t('shared.addBillableItem')" :subtitle="t('billing.addBillableItem.createANewBillableItem')" />

    <UiCard padding="lg" class="max-w-3xl">
      <UiForm stacked @submit="submit">
        <UiAlert v-if="store.error" variant="error" data-testid="billing-add-billable-item-error">
          {{ store.error }}
        </UiAlert>

        <UiField :label="t('shared.client')" :error="store.fieldErrors.clientId">
          <UiClientSelect
            v-model="clientId"
            :clients="clients"
            :placeholder="t('shared.selectAClient')"
          />
        </UiField>

        <UiField :label="t('shared.description')" :error="store.fieldErrors.description">
          <template #default="{ id, invalid }">
            <UiInput
              :id="id"
              v-model="description"
              type="text"
              :invalid="invalid"
              :placeholder="t('billing.addBillableItem.whatIsThisChargeFor')"
              data-testid="billing-add-billable-item-description"
            />
          </template>
        </UiField>

        <UiGrid :columns="2">
          <UiField :label="t('shared.amount')" :error="store.fieldErrors.amount">
            <UiNumberInput v-model="amount" :step="0.01" :min="0" placeholder="0.00" data-testid="billing-add-billable-item-amount" />
          </UiField>
          <UiField :label="t('shared.currency')" :error="store.fieldErrors.currency">
            <UiSelect v-model="currency" :options="CURRENCY_OPTIONS" :placeholder="t('shared.currency')" />
          </UiField>
          <UiField :label="t('shared.type')" :error="store.fieldErrors.type">
            <UiSelect v-model="type" :options="typeOptions" :placeholder="t('shared.type')" data-testid="billing-add-billable-item-type" />
          </UiField>
          <UiField :label="t('billing.addBillableItem.nextDueDate')" :error="store.fieldErrors.nextDueDate">
            <UiDatePicker v-model="nextDueDate" :direction="UiDatePickerDirection.Future" :placeholder="t('shared.selectDate')" />
          </UiField>
        </UiGrid>

        <UiGrid v-if="isRecurring" :columns="2">
          <UiField :label="t('billing.addBillableItem.recurEvery')" :error="store.fieldErrors.recurringPeriod">
            <UiNumberInput v-model="recurEvery" :step="1" :min="1" placeholder="1" data-testid="billing-add-billable-item-every" />
          </UiField>
          <UiField :label="t('billing.addBillableItem.recurUnit')">
            <UiSelect v-model="recurUnit" :options="unitOptions" :placeholder="t('billing.addBillableItem.monthS')" />
          </UiField>
        </UiGrid>

        <UiStack direction="row" gap="md" align="center">
          <UiButton type="submit" variant="primary" :loading="store.loading" data-testid="billing-add-billable-item-submit">
            {{ t('shared.addBillableItem') }}
          </UiButton>
          <UiLink :to="AppRoute.BillingBillableItems" variant="ghost" data-testid="billing-add-billable-item-cancel">
            {{ t('shared.cancel') }}
          </UiLink>
        </UiStack>
      </UiForm>
    </UiCard>
  </UiStack>
</template>
