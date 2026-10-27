<template>
  <UiStack gap="lg">
    <UiBackLink :to="CLIENT_INVOICES_PATH" :label="$t('client.invoices.backTo')" testid="invoice-back" />

    <UiStack v-if="pending">
      <UiSkeleton shape="block" class="h-32" />
      <UiSkeleton shape="block" class="h-48" />
    </UiStack>

    <UiEmptyState v-else-if="error || !invoice" :icon="AlertCircle" :title="$t('client.invoices.notFound')" />

    <template v-else>
      <UiCard data-testid="invoice-header">
        <UiStack direction="row" justify="between" align="start" wrap class="mb-4">
          <UiStack gap="xs">
            <UiHeading :level="1" size="subsection" data-testid="invoice-heading">
              {{ $t('client.invoices.heading', { id: invoice.id }) }}
            </UiHeading>
            <UiText size="sm" tone="muted">{{ $t('client.invoices.issued') }} {{ formatDate(invoice.invoiceDate) }}</UiText>
          </UiStack>
          <UiStack direction="row" align="center" gap="sm">
            <ClientStatusBadge :status="invoice.status" />
            <UiButton v-if="isOutstanding" :to="payPath" data-testid="invoice-pay-now">
              <CreditCard :size="16" :stroke-width="2" class="mr-2" />
              {{ $t('client.invoices.payNow') }}
            </UiButton>
          </UiStack>
        </UiStack>

        <UiGrid as="dl" cols="3" gap="md">
          <UiDescriptionItem :label="$t('client.invoices.dueDate')" :value="formatDate(invoice.dueDate)" :value-class="isOverdue ? '!text-red-600 dark:!text-red-400' : ''" />
          <!-- `paidAt` is a moment, not a calendar day, so it is rendered with its time. -->
          <UiDescriptionItem v-if="formatDateTime(invoice.paidAt) !== EMPTY_DATE" :label="$t('client.invoices.datePaid')" :value="formatDateTime(invoice.paidAt)" />
          <UiDescriptionItem :label="$t('client.invoices.paymentMethod')" :value="invoice.paymentMethod || EMPTY_DATE" value-class="capitalize" />
        </UiGrid>
      </UiCard>

      <UiTable data-testid="invoice-items">
        <UiTableHead>
          <UiTableRow :hoverable="false">
            <UiTableTh>{{ $t('client.invoices.colDescription') }}</UiTableTh>
            <UiTableTh align="right">{{ $t('client.invoices.colAmount') }}</UiTableTh>
          </UiTableRow>
        </UiTableHead>
        <UiTableBody>
          <UiTableRow v-for="item in invoice.items" :key="item.id">
            <UiTableTd>
              <UiText as="span" size="sm" tone="muted">{{ item.description }}</UiText>
            </UiTableTd>
            <UiTableTd align="right">
              <UiText as="span" size="sm" weight="medium">{{ money(item.amount) }}</UiText>
            </UiTableTd>
          </UiTableRow>
        </UiTableBody>
        <UiTableFoot>
          <UiTableRow :hoverable="false">
            <UiTableTd><UiText as="span" size="sm" tone="muted">{{ $t('client.invoices.subtotal') }}</UiText></UiTableTd>
            <UiTableTd align="right"><UiText as="span" size="sm">{{ money(invoice.subTotal) }}</UiText></UiTableTd>
          </UiTableRow>
          <UiTableRow v-if="invoice.tax > 0" :hoverable="false">
            <UiTableTd><UiText as="span" size="sm" tone="muted">{{ $t('client.invoices.tax') }}</UiText></UiTableTd>
            <UiTableTd align="right"><UiText as="span" size="sm">{{ money(invoice.tax) }}</UiText></UiTableTd>
          </UiTableRow>
          <UiTableRow v-if="invoice.credit > 0" :hoverable="false">
            <UiTableTd><UiText as="span" size="sm" tone="muted">{{ $t('client.invoices.creditApplied') }}</UiText></UiTableTd>
            <UiTableTd align="right"><UiText as="span" size="sm" tone="success">-{{ money(invoice.credit) }}</UiText></UiTableTd>
          </UiTableRow>
          <UiTableRow :hoverable="false">
            <UiTableTd><UiText as="span" weight="bold">{{ $t('client.invoices.total') }}</UiText></UiTableTd>
            <UiTableTd align="right"><UiText as="span" weight="bold">{{ money(invoice.total) }}</UiText></UiTableTd>
          </UiTableRow>
        </UiTableFoot>
      </UiTable>

      <UiAlert v-if="isOutstanding" variant="info" data-testid="invoice-unpaid-banner">
        {{ $t('client.invoices.unpaidBanner') }}
        <template #action>
          <UiButton :to="payPath" size="sm" class="flex-shrink-0" data-testid="invoice-pay-amount">
            <CreditCard :size="16" :stroke-width="2" class="mr-2" />
            {{ $t('client.invoices.payAmount') }} {{ money(balanceDue(invoice)) }}
          </UiButton>
        </template>
      </UiAlert>
    </template>
  </UiStack>
</template>

<script setup lang="ts">
/**
 * One invoice: header with status and a pay button while money is owed, the line items with
 * totals, and a reminder banner. The invoice is read through the invoice payment store.
 */
import { CLIENT_INVOICES_PATH, clientInvoicePayPath } from '~/constants/clientRoutes'
import { AlertCircle, CreditCard } from 'lucide-vue-next'
import { useInvoicePaymentStore } from '~/stores/invoicepayment'
import { formatCurrency } from '~/utils/formatCurrency'
import { EMPTY_DATE, formatDate, formatDateTime } from '~/utils/formatDate'
import { balanceDue, isInvoiceOutstanding, isInvoiceOverdue } from '~/utils/invoice'

definePageMeta({ layout: 'client', middleware: 'client-auth' })

/** The current route; its params name the record shown. */
const route = useRoute()
/** Prefixes a path with the active locale. */
const localePath = useLocalePath()

// The invoice is read through the invoice store; the key follows the route, so a new invoice is read.
/** The invoice payment store that owns the invoice being read. */
const paymentStore = useInvoicePaymentStore()
/** Loading flag of the data this page reads. */
const { pending } = await useAsyncData(
  () => `client-invoice-${String(route.params.id)}`,
  () => paymentStore.loadInvoice(String(route.params.id))
)

/** The invoice being read; null until loaded or after a failed read. */
const invoice = computed(() => paymentStore.invoice)

/** The sentence for a failed read. */
const error = computed(() => paymentStore.invoiceError)

/** The payment page of this invoice. */
const payPath = computed(() => localePath(clientInvoicePayPath(String(route.params.id))))

/**
 * Formats one amount on this invoice in the invoice's own billing currency.
 *
 * An invoice's currency is fixed at creation (`InvoiceDto.Currency`) and is not always the
 * account's current one, so it is the authority. When it is absent (an invoice predating the
 * field) `formatCurrency` still shows a grouped number with no symbol rather than guessing.
 *
 * @param amount - The amount the API sent for this line.
 * @returns The formatted amount, or an em dash when the line carries no figure.
 */
const money = (amount: string | number | null | undefined): string =>
  formatCurrency(amount, { code: invoice.value?.currency })

/** Whether this invoice still has money owing -- one definition, in `utils/invoice.ts`. */
const isOutstanding = computed(() => invoice.value !== null && isInvoiceOutstanding(invoice.value))

/** Whether the due date should be shown in red -- one definition, in `utils/invoice.ts`. */
const isOverdue = computed(() => isInvoiceOverdue(invoice.value))
</script>
