<script setup lang="ts">
/**
 * Invoice detail: the invoice loaded by id and shown in six tabs (summary, add payment, options,
 * credit, refund, notes). The route decides the mode — `/view` is read-only, `/edit` lets the
 * operator change the invoice — and a button on the page toggles it.
 */
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import InvoiceCreditTab from '@/components/billing/InvoiceCreditTab.vue'
import InvoiceNotesTab from '@/components/billing/InvoiceNotesTab.vue'
import InvoiceOptionsTab from '@/components/billing/InvoiceOptionsTab.vue'
import InvoicePaymentTab from '@/components/billing/InvoicePaymentTab.vue'
import InvoiceRefundTab from '@/components/billing/InvoiceRefundTab.vue'
import InvoiceSummaryTab from '@/components/billing/InvoiceSummaryTab.vue'
import UiAlert from '@/components/ui/UiAlert.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiIcon from '@/components/ui/UiIcon.vue'
import UiPageHeader from '@/components/ui/UiPageHeader.vue'
import UiSkeleton from '@/components/ui/UiSkeleton.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiTabs from '@/components/ui/UiTabs.vue'
import { useResultBanner } from '@/composables/useResultBanner'
import { Icon } from '@/lib/icons'
import { useBillingStore } from '@/stores/billing'
import { InvoiceOpenAction, InvoiceTab, InvoiceStatus } from '@/types/invoice'
import type { Invoice } from '@/types/invoice'
import type { UiTabItem } from '@/utils/ui/tabs'

const { t } = useI18n()

/** Route action that opens the invoice for editing. */
const EDIT_ACTION = InvoiceOpenAction.Edit

const route = useRoute()
const router = useRouter()
const store = useBillingStore()

/** The success line shown after a tab saved its changes. */
const banner = useResultBanner()

/** The tab on show. */
const activeTab = ref<string>(InvoiceTab.Summary)

/** True when the operator can change the invoice (the `/edit` route, or toggled on). */
const manageMode = ref(route.params.action === EDIT_ACTION)

/** Id of the invoice from the route. */
const invoiceId = computed<number>(() => Number(route.params.id))

/** The invoice on show, once the store holds the one the route names. */
const invoice = computed<Invoice | null>(() =>
  store.currentInvoice !== null && store.currentInvoice.id === invoiceId.value ? store.currentInvoice : null,
)

/** True when the draft notice applies: a draft invoice opened for editing. */
const showDraftNotice = computed<boolean>(() => manageMode.value && invoice.value?.status === InvoiceStatus.Draft)

/** The tabs; "Options" exists only while managing. */
const tabs = computed<UiTabItem[]>(() => {
  const all: UiTabItem[] = [
    { value: InvoiceTab.Summary, label: t('billing.invoiceDetail.tabSummary'), testid: 'billing-invoice-tab-summary' },
    { value: InvoiceTab.Payment, label: t('billing.invoiceDetail.addPayment'), testid: 'billing-invoice-tab-payment' },
    { value: InvoiceTab.Options, label: t('billing.invoiceDetail.options'), testid: 'billing-invoice-tab-options' },
    { value: InvoiceTab.Credit, label: t('billing.invoiceDetail.tabCredit'), testid: 'billing-invoice-tab-credit' },
    { value: InvoiceTab.Refund, label: t('billing.invoiceDetail.tabRefund'), testid: 'billing-invoice-tab-refund' },
    { value: InvoiceTab.Notes, label: t('shared.notes'), testid: 'billing-invoice-tab-notes' },
  ]
  return manageMode.value ? all : all.filter((tab) => tab.value !== InvoiceTab.Options)
})

/**
 * Loads the invoice the route names.
 *
 * @returns Resolves when the request has settled.
 */
const load = (): Promise<void> => store.fetchById(invoiceId.value)

/** Toggles between the read-only view and the editing mode. */
const toggleMode = (): void => {
  manageMode.value = !manageMode.value
  if (!manageMode.value && activeTab.value === InvoiceTab.Options) {
    activeTab.value = InvoiceTab.Summary
  }
}

/**
 * Confirms a saved change; the store has already reloaded the invoice, so the tabs show the result.
 */
const onSaved = (): void => {
  banner.show(t('shared.changesSavedSuccessfully'), false)
}

/** Goes back to the previous screen. */
const goBack = (): void => {
  router.back()
}

/** Opens the browser print dialog for the page. */
const print = (): void => {
  window.print()
}

/** Opens the invoice's PDF in a new tab, where the browser downloads it. */
const downloadPdf = (): void => {
  window.open(store.pdfUrl(invoiceId.value), '_blank')
}

onMounted(load)
watch(invoiceId, load)
</script>

<template>
  <UiStack data-testid="billing-invoice-detail-view">
    <UiPageHeader
      :title="t('billing.invoiceDetail.invoiceId', { id: invoiceId })"
      :subtitle="t('billing.invoiceDetail.manageAndViewInvoiceDetails')"
      data-testid="invoice-detail-title"
    >
      <template #actions>
        <UiButton variant="ghost" data-testid="billing-invoice-back" @click="goBack">
          <UiIcon :markup="Icon.Back" size="sm" />
          {{ t('shared.back') }}
        </UiButton>
        <UiButton variant="primary" data-testid="billing-invoice-toggle-mode" @click="toggleMode">
          {{ manageMode ? t('billing.invoiceDetail.viewInvoice') : t('billing.invoiceDetail.manageInvoice') }}
        </UiButton>
        <UiButton variant="secondary" data-testid="billing-invoice-print" @click="print">
          {{ t('shared.print') }}
        </UiButton>
        <UiButton variant="secondary" data-testid="billing-invoice-download" @click="downloadPdf">
          {{ t('billing.invoiceDetail.downloadPdf') }}
        </UiButton>
      </template>
    </UiPageHeader>

    <UiAlert v-if="store.error" variant="error" data-testid="billing-invoice-error">
      {{ store.error }}
    </UiAlert>

    <UiAlert v-if="banner.message.value" variant="success" data-testid="billing-invoice-saved">
      {{ banner.message.value }}
    </UiAlert>

    <UiSkeleton v-if="invoice === null && store.loading" shape="block" data-testid="billing-invoice-loading" />

    <template v-if="invoice !== null">
      <UiAlert v-if="showDraftNotice" variant="info" data-testid="billing-invoice-draft-banner">
        {{ t('billing.invoiceDetail.thisIsADraftInvoice') }} {{ t('billing.invoiceDetail.theClientIsNotAble') }}
      </UiAlert>

      <UiTabs v-model="activeTab" :items="tabs">
        <InvoiceSummaryTab v-if="activeTab === InvoiceTab.Summary" :invoice="invoice" :readonly="!manageMode" @updated="onSaved" />
        <InvoicePaymentTab v-else-if="activeTab === InvoiceTab.Payment" :invoice="invoice" :readonly="!manageMode" @updated="onSaved" />
        <InvoiceOptionsTab v-else-if="activeTab === InvoiceTab.Options" :invoice="invoice" @updated="onSaved" />
        <InvoiceCreditTab v-else-if="activeTab === InvoiceTab.Credit" :invoice="invoice" :readonly="!manageMode" @updated="onSaved" />
        <InvoiceRefundTab v-else-if="activeTab === InvoiceTab.Refund" :invoice="invoice" :readonly="!manageMode" @updated="onSaved" />
        <InvoiceNotesTab v-else-if="activeTab === InvoiceTab.Notes" :invoice="invoice" :readonly="!manageMode" @updated="onSaved" />
      </UiTabs>
    </template>
  </UiStack>
</template>
