<script setup lang="ts">
/**
 * The line items of an invoice: a read-only table with the totals, or — when `readonly` is off —
 * an editor that adds, removes and re-prices lines and saves them through the billing store.
 * Every invoice tab shows this block, so the logic lives here once.
 */
import { computed, onMounted, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import UiButton from '@/components/ui/UiButton.vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiCheckbox from '@/components/ui/UiCheckbox.vue'
import UiHeading from '@/components/ui/UiHeading.vue'
import UiIcon from '@/components/ui/UiIcon.vue'
import UiIconButton from '@/components/ui/UiIconButton.vue'
import UiNumberInput from '@/components/ui/UiNumberInput.vue'
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
import { formatUsdPlain } from '@/lib/money'
import { useBillingStore } from '@/stores/billing'
import type { Invoice, InvoiceItemUpdate } from '@/types/invoice'
import { MAX_PERCENT } from '@/lib/limits'

const { t } = useI18n()

const props = withDefaults(defineProps<{
  /** The invoice whose lines are shown. */
  invoice: Invoice
  /** True to show the lines without editing controls. */
  readonly?: boolean
  /** Tax percentage previewed on the taxed lines; 0 adds no tax row. */
  taxPercent?: number
}>(), {
  readonly: false,
  taxPercent: 0,
})

const emit = defineEmits<{
  /** Emitted after the edited lines were saved. */
  updated: []
}>()

/** One line while it is being edited. */
interface EditableLine {
  /** Existing line id; absent for a line added in the editor. */
  id?: number
  /** Charge description. */
  description: string
  /** Line amount. */
  amount: number
  /** Whether tax applies to the line. */
  taxed: boolean
  /** Whether the line is ticked for the bulk delete. */
  selected: boolean
}

/** A row of the totals block. */
interface TotalRow {
  /** Stable key of the row. */
  key: string
  /** Row caption. */
  label: string
  /** Row amount. */
  value: number
  /** True for the closing "total due" row. */
  strong: boolean
}

const store = useBillingStore()

/** The editable copy of the invoice lines. */
const lines = ref<EditableLine[]>([])

/** True while the lines are being saved. */
const saving = ref(false)

/** True when the invoice carries a tax rate, so a line defaults to taxed. */
const invoiceIsTaxed = computed<boolean>(() => (props.invoice.taxRate ?? 0) > 0)

/** True when every editable line is ticked (and there is at least one). */
const allSelected = computed<boolean>(() => lines.value.length > 0 && lines.value.every((line) => line.selected))

/** How many editable lines are ticked. */
const selectedCount = computed<number>(() => lines.value.filter((line) => line.selected).length)

/** Sub-total of the editable lines. */
const previewSubTotal = computed<number>(() => lines.value.reduce((sum, line) => sum + line.amount, 0))

/** Tax previewed on the taxed editable lines at `taxPercent`. */
const previewTax = computed<number>(() => {
  const taxable = lines.value.filter((line) => line.taxed).reduce((sum, line) => sum + line.amount, 0)
  return taxable * (props.taxPercent / MAX_PERCENT)
})

/** Total due after the invoice credit, as the editor previews it. */
const previewTotal = computed<number>(() => previewSubTotal.value + previewTax.value - (props.invoice.credit ?? 0))

/** The totals block: what the API computed when read-only, the live preview while editing. */
const totalRows = computed<TotalRow[]>(() => {
  const credit = props.invoice.credit ?? 0
  const rows: TotalRow[] = [
    {
      key: 'subtotal',
      label: t('shared.subTotal'),
      value: props.readonly ? (props.invoice.subTotal ?? 0) : previewSubTotal.value,
      strong: false,
    },
  ]
  if (!props.readonly && props.taxPercent > 0) {
    rows.push({ key: 'tax', label: t('billing.invoiceItems.tax'), value: previewTax.value, strong: false })
  }
  rows.push({ key: 'credit', label: t('shared.credit'), value: credit, strong: false })
  rows.push({
    key: 'total',
    label: t('shared.totalDue'),
    value: props.readonly ? (props.invoice.total ?? 0) : previewTotal.value,
    strong: true,
  })
  return rows
})

/** Resets the editable lines from the invoice. */
const populate = (): void => {
  lines.value = (props.invoice.items ?? []).map((item) => ({
    id: item.id,
    description: item.description,
    amount: item.amount,
    taxed: item.taxed ?? invoiceIsTaxed.value,
    selected: false,
  }))
}

/** Appends a blank line. */
const addLine = (): void => {
  lines.value.push({ description: '', amount: 0, taxed: false, selected: false })
}

/**
 * Removes one line.
 *
 * @param index - Position of the line.
 */
const removeLine = (index: number): void => {
  lines.value.splice(index, 1)
}

/** Ticks every line, or clears the ticks when all were ticked. */
const toggleAll = (): void => {
  const next = !allSelected.value
  lines.value.forEach((line) => {
    line.selected = next
  })
}

/** Drops every ticked line from the editor (saved with the rest). */
const deleteSelected = (): void => {
  lines.value = lines.value.filter((line) => !line.selected)
}

/** Saves the edited lines through the store and tells the parent when they were accepted. */
const save = async (): Promise<void> => {
  saving.value = true
  const items: InvoiceItemUpdate[] = lines.value.map((line) => ({
    id: line.id ?? null,
    description: line.description,
    unitPrice: line.amount,
    quantity: 1,
    isDeleted: false,
  }))
  const ok = await store.updateItems(props.invoice.id, items)
  saving.value = false
  if (ok) {
    emit('updated')
  }
}

/**
 * Whether tax applies to a read-only line.
 *
 * @param taxed - The flag the API sent for the line, when it sent one.
 * @returns True when the line is taxed.
 */
const isTaxed = (taxed: boolean | undefined): boolean => taxed ?? invoiceIsTaxed.value

onMounted(populate)
watch(() => props.invoice, populate)
</script>

<template>
  <UiStack>
    <UiCard padding="md" data-testid="billing-invoice-items">
      <UiStack gap="sm">
        <UiHeading :level="3" variant="card">
          {{ t('shared.invoiceItems') }}
        </UiHeading>

        <!-- Read-only lines -->
        <UiTable
          v-if="readonly"
          density="compact"
          :empty="(invoice.items ?? []).length === 0"
          :colspan="3"
          data-testid="billing-invoice-items-table"
        >
          <UiTableHead>
            <UiTableRow>
              <UiTableHeader>{{ t('shared.description') }}</UiTableHeader>
              <UiTableHeader align="right">{{ t('shared.amount') }}</UiTableHeader>
              <UiTableHeader align="center">{{ t('shared.taxed') }}</UiTableHeader>
            </UiTableRow>
          </UiTableHead>
          <UiTableBody divided>
            <UiTableRow v-for="item in invoice.items ?? []" :key="item.id" hoverable>
              <UiTableCell>{{ item.description }}</UiTableCell>
              <UiTableCell align="right">{{ formatUsdPlain(item.amount) }}</UiTableCell>
              <UiTableCell align="center" muted>
                {{ isTaxed(item.taxed) ? t('shared.yes') : t('shared.no') }}
              </UiTableCell>
            </UiTableRow>
          </UiTableBody>
          <template #empty>
            {{ t('shared.noItems') }}
          </template>
        </UiTable>

        <!-- Editable lines -->
        <UiTable
          v-else
          density="compact"
          :empty="lines.length === 0"
          :colspan="5"
          data-testid="billing-invoice-items-editor"
        >
          <UiTableHead>
            <UiTableRow>
              <UiTableHeader>
                <UiCheckbox
                  :model-value="allSelected"
                  :aria-label="t('shared.selectAll')"
                  data-testid="billing-invoice-items-select-all"
                  @update:model-value="toggleAll"
                />
              </UiTableHeader>
              <UiTableHeader>{{ t('shared.description') }}</UiTableHeader>
              <UiTableHeader align="right">{{ t('shared.amount') }}</UiTableHeader>
              <UiTableHeader align="center">{{ t('shared.taxed') }}</UiTableHeader>
              <UiTableHeader />
            </UiTableRow>
          </UiTableHead>
          <UiTableBody divided>
            <UiTableRow v-for="(line, index) in lines" :key="line.id ?? `new-${index}`">
              <UiTableCell>
                <UiCheckbox
                  v-model="line.selected"
                  :aria-label="t('shared.selectRow')"
                  :data-testid="`billing-invoice-item-select-${index}`"
                />
              </UiTableCell>
              <UiTableCell>
                <UiTextarea
                  v-model="line.description"
                  rows="1"
                  :placeholder="t('shared.description')"
                  :data-testid="`billing-invoice-item-description-${index}`"
                />
              </UiTableCell>
              <UiTableCell align="right">
                <UiNumberInput v-model="line.amount" :step="0.01" :min="0" placeholder="0.00" />
              </UiTableCell>
              <UiTableCell align="center">
                <UiCheckbox v-model="line.taxed" :aria-label="t('shared.taxed')" />
              </UiTableCell>
              <UiTableCell align="right">
                <UiIconButton
                  :label="t('billing.invoiceItems.removeItem')"
                  :data-testid="`billing-invoice-item-remove-${index}`"
                  @click="removeLine(index)"
                >
                  <UiIcon :markup="Icon.Close" size="sm" />
                </UiIconButton>
              </UiTableCell>
            </UiTableRow>
          </UiTableBody>
          <template #empty>
            {{ t('shared.noItems') }}
          </template>
        </UiTable>

        <UiStack v-if="!readonly" direction="row" justify="between" align="center" wrap>
          <UiStack direction="row" gap="sm" align="center">
            <UiText tone="secondary" size="md">{{ t('shared.withSelected') }}</UiText>
            <UiButton
              variant="danger"
              size="sm"
              :disabled="selectedCount === 0"
              data-testid="billing-invoice-items-delete-selected"
              @click="deleteSelected"
            >
              {{ t('shared.delete') }}
            </UiButton>
          </UiStack>
          <UiButton variant="secondary" size="sm" data-testid="billing-invoice-items-add" @click="addLine">
            {{ t('shared.addItem') }}
          </UiButton>
        </UiStack>

        <UiStack align="end" gap="xs" data-testid="billing-invoice-totals">
          <UiStack v-for="row in totalRows" :key="row.key" direction="row" gap="lg" align="center">
            <UiText :tone="row.strong ? 'primary' : 'muted'" size="md">{{ row.label }}</UiText>
            <UiText :tone="row.strong ? 'primary' : 'secondary'" size="md" :data-testid="`billing-invoice-total-${row.key}`">
              {{ formatUsdPlain(row.value) }}
            </UiText>
          </UiStack>
        </UiStack>
      </UiStack>
    </UiCard>

    <UiStack v-if="!readonly" direction="row" justify="center" gap="sm">
      <UiButton
        variant="primary"
        :loading="saving"
        :disabled="saving"
        data-testid="billing-invoice-items-save"
        @click="save"
      >
        {{ t('shared.saveChanges') }}
      </UiButton>
      <UiButton variant="secondary" data-testid="billing-invoice-items-cancel" @click="populate">
        {{ t('shared.cancelChanges') }}
      </UiButton>
    </UiStack>
  </UiStack>
</template>
