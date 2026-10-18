<script setup lang="ts">
/**
 * Modal for adding a single billable item to a client.
 * Supports various invoice actions including recurring billing.
 */
import { ref, computed, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { useClientBillableItemsStore } from '@/stores/clientBillableItems'
import { INVOICE_ACTION_OPTIONS, RECURRENCE_PERIOD_OPTIONS } from '@/lib/constants'
import { toDateInputValue } from '@/lib/format'
import { BillableInvoiceAction, RecurrencePeriod } from '@/types/billableitem'
import UiAlert from '@/components/ui/UiAlert.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiDatePicker from '@/components/ui/UiDatePicker.vue'
import { UiDatePickerDirection } from '@/utils/ui/datePicker'
import UiDialog from '@/components/ui/UiDialog.vue'
import UiField from '@/components/ui/UiField.vue'
import UiForm from '@/components/ui/UiForm.vue'
import UiGrid from '@/components/ui/UiGrid.vue'
import UiLabel from '@/components/ui/UiLabel.vue'
import UiNumberInput from '@/components/ui/UiNumberInput.vue'
import UiRadio from '@/components/ui/UiRadio.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiTextarea from '@/components/ui/UiTextarea.vue'
import { UiButtonVariant } from '@/utils/ui/button'
import { UiAlignItems, UiJustify } from '@/utils/ui/layout'
import { UiLabelVariant } from '@/utils/ui/text'

const { t } = useI18n()

/** Props for AddBillableItemModal. */
const props = defineProps<{
  /** Client ID for fetching services. */
  clientId: string
  /** External saving state indicator. */
  saving: boolean
}>()

const emit = defineEmits<{
  /** Emitted after a successful save. */
  saved: []
  /** Emitted when the user closes the modal. */
  close: []
}>()

/** Invoice action value that turns on the recurrence fields. */
const RECUR_ACTION = BillableInvoiceAction.Recur

/** Billable items store, the only caller of the billable item endpoints. */
const store = useClientBillableItemsStore()

/** True while save is in progress. */
const internalSaving = ref(false)

/** Error message from the save operation (owned by the store). */
const saveError = computed(() => store.saveError)

/** Available services for the dropdown. */
const serviceOptions = ref<{ value: number; label: string }[]>([])

/** Selected service ID. */
const serviceId = ref<number>(0)

/** Item description. */
const description = ref('')

/** Hours or quantity value. */
const hoursQty = ref(0)

/** Whether the hoursQty field represents hours. */
const isHours = ref(true)

/** Charge amount. */
const amount = ref(0)

/** Selected invoice action. */
const invoiceAction = ref<string>(BillableInvoiceAction.DontInvoice)

/** Due date in YYYY-MM-DD format. */
const dueDate = ref(toDateInputValue(new Date().toISOString()))

/** Invoice count (read-only for new items). */
const invoiceCount = ref(0)

/** Recurrence interval (every N). */
const recurrenceInterval = ref(1)

/** Recurrence period unit. */
const recurrencePeriod = ref<string>(RecurrencePeriod.Months)

/** Recurrence limit (number of times). */
const recurrenceLimit = ref(0)

/** Whether a recurrence action is selected. */
const isRecurring = computed(() => invoiceAction.value === RECUR_ACTION)

/**
 * Fetches client services for the product/service dropdown.
 *
 * @returns Promise that resolves when services are loaded.
 */
const fetchServices = async (): Promise<void> => {
  serviceOptions.value = await store.fetchServiceOptions(props.clientId)
}

/**
 * Submits the billable item to the backend.
 *
 * @returns Promise that resolves when the item is saved.
 */
const handleSave = async (): Promise<void> => {
  internalSaving.value = true
  const saved = await store.addItem(props.clientId, {
    serviceId: serviceId.value || null,
    description: description.value,
    hoursQty: hoursQty.value,
    isHours: isHours.value,
    amount: amount.value,
    invoiceAction: invoiceAction.value,
    dueDate: dueDate.value || null,
    invoiceCount: invoiceCount.value,
    recurrenceInterval: isRecurring.value ? recurrenceInterval.value : null,
    recurrencePeriod: isRecurring.value ? recurrencePeriod.value : null,
    recurrenceLimit: isRecurring.value ? (recurrenceLimit.value || null) : null,
  })
  internalSaving.value = false
  if (saved) {
    emit('saved')
  }
}

onMounted(() => fetchServices())
</script>

<template>
  <UiDialog :title="t('shared.addBillableItem')" size="lg" @close="emit('close')">
    <UiStack>
      <UiAlert v-if="saveError" variant="error" data-testid="clients-add-billable-item-error">
        {{ saveError }}
      </UiAlert>

      <UiForm stacked @submit="handleSave">
        <UiField :label="t('shared.productService')">
          <UiSelect
            v-model="serviceId"
            :options="[{ value: 0, label: t('shared.none') }, ...serviceOptions]"
            :placeholder="t('clients.addBillableItem.selectAService')"
          />
        </UiField>

        <UiField :label="t('shared.description')">
          <template #default="{ id }">
            <UiTextarea
              :id="id"
              v-model="description"
              rows="3"
              :placeholder="t('clients.addBillableItem.describeTheCharge')"
              data-testid="clients-add-billable-item-description"
            />
          </template>
        </UiField>

        <UiGrid :columns="2">
          <UiStack gap="sm">
            <UiField :label="t('shared.hoursQty')">
              <UiNumberInput v-model="hoursQty" :step="0.01" :min="0" placeholder="0" data-testid="clients-add-billable-item-hours-qty" />
            </UiField>
            <UiStack direction="row" :align="UiAlignItems.Center">
              <UiLabel :variant="UiLabelVariant.Inline">
                <UiRadio v-model="isHours" :value="true" name="hoursType" data-testid="clients-add-billable-item-hours" />
                {{ t('shared.hours') }}
              </UiLabel>
              <UiLabel :variant="UiLabelVariant.Inline">
                <UiRadio v-model="isHours" :value="false" name="hoursType" data-testid="clients-add-billable-item-qty" />
                {{ t('shared.qty') }}
              </UiLabel>
            </UiStack>
          </UiStack>
          <UiField :label="t('shared.amount')">
            <UiNumberInput v-model="amount" :step="0.01" :min="0" placeholder="0.00" data-testid="clients-add-billable-item-amount" />
          </UiField>
        </UiGrid>

        <UiField :label="t('shared.invoiceAction')">
          <UiStack gap="sm">
            <UiLabel
              v-for="opt in INVOICE_ACTION_OPTIONS"
              :key="opt.value"
              :variant="UiLabelVariant.Inline"
            >
              <UiRadio
                v-model="invoiceAction"
                :value="opt.value"
                name="invoiceAction"
                :data-testid="`clients-add-billable-item-action-${opt.value}`"
              />
              <template v-if="opt.value !== RECUR_ACTION">
                {{ opt.label }}
              </template>
              <template v-else>
                {{ t('clients.addBillableItem.recurEvery') }}
                <UiNumberInput v-model="recurrenceInterval" :min="1" :disabled="!isRecurring" placeholder="1" data-testid="clients-add-billable-item-recur-interval" />
                <UiSelect
                  v-model="recurrencePeriod"
                  :options="RECURRENCE_PERIOD_OPTIONS"
                  :disabled="!isRecurring"
                  :placeholder="t('clients.addBillableItem.recurEvery')"
                />
                {{ t('clients.addBillableItem.for') }}
                <UiNumberInput v-model="recurrenceLimit" :min="0" :disabled="!isRecurring" placeholder="0" data-testid="clients-add-billable-item-recur-limit" />
                {{ t('clients.addBillableItem.times') }}
              </template>
            </UiLabel>
          </UiStack>
        </UiField>

        <UiGrid :columns="2">
          <UiField :label="t('shared.dueDate')">
            <UiDatePicker v-model="dueDate" :direction="UiDatePickerDirection.Future" />
          </UiField>
          <UiField :label="t('clients.addBillableItem.invoiceCount')">
            <UiNumberInput v-model="invoiceCount" :min="0" :disabled="true" placeholder="0" data-testid="clients-add-billable-item-invoice-count" />
          </UiField>
        </UiGrid>

        <UiStack direction="row" :justify="UiJustify.End" :align="UiAlignItems.Center" gap="sm">
          <UiButton
            type="button"
            :variant="UiButtonVariant.Secondary"
            data-testid="clients-add-billable-item-cancel"
            @click="emit('close')"
          >
            {{ t('shared.cancel') }}
          </UiButton>
          <UiButton
            type="submit"
            :variant="UiButtonVariant.Primary"
            :disabled="internalSaving"
            data-testid="clients-add-billable-item-save"
          >
            {{ internalSaving ? t('shared.saving') : t('shared.save') }}
          </UiButton>
        </UiStack>
      </UiForm>
    </UiStack>
  </UiDialog>
</template>
