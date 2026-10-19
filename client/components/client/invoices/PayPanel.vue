<template>
  <UiCard data-testid="invoice-pay-panel">
    <UiStack gap="lg">
      <UiHeading :level="1" size="subsection">{{ $t('invoicePay.makePayment') }}</UiHeading>

      <UiAlert v-if="paySuccess" variant="success" :title="$t('invoicePay.paymentSuccess')" data-testid="invoice-pay-success">
        <UiLink data-testid="pay-panel-invoice-link" :to="invoicePath">{{ $t('invoicePay.viewInvoice') }}</UiLink>
      </UiAlert>

      <UiAlert v-if="payError" variant="error" data-testid="invoice-pay-error">{{ payError }}</UiAlert>

      <template v-if="!paySuccess">
        <!-- Suppressed entirely when nothing is selectable, so the row is not an empty box. -->
        <ClientInvoicesPayMethodSelector
          v-if="hasSelectableMethod"
          v-model="selectedMethodId"
          :saved-methods="savedMethods"
          :can-use-new-card="canUseNewCard"
        />

        <!-- Hosted-gateway payment: whichever gateway plugin the deployment enabled. -->
        <UiButton v-if="hostedGatewayModule" full-width :loading="submitting" data-testid="invoice-pay-hosted" @click="payWithHostedGateway">
          {{ $t('invoicePay.payByCardHosted') }}
        </UiButton>

        <!-- These fields are only a prelude to the billing-portal hand-off, so without that
             hand-off the form is not shown -- collecting a card number that goes nowhere would
             be worse than not asking. -->
        <ClientInvoicesPayCardForm
          v-if="showNewCardForm"
          v-model:card-number="cardNumber"
          v-model:expiry-date="expiryDate"
          v-model:cvv="cvv"
          v-model:card-description="cardDescription"
        />

        <!-- Submit -- only when a method is actually selected. -->
        <UiButton v-if="selectedMethodId !== null" size="lg" :loading="submitting" data-testid="invoice-pay-submit" @click="submitPayment">
          <CreditCard v-if="!submitting" :size="18" :stroke-width="2" class="mr-2" />
          {{ submitting ? $t('invoicePay.processing') : $t('invoicePay.submitPayment') }}
        </UiButton>
      </template>
    </UiStack>
  </UiCard>
</template>

<script setup lang="ts">
/**
 * PayPanel - the "Make payment" card of the invoice payment page.
 *
 * Offers the saved cards, a new-card hand-off to the billing portal when the deployment has one,
 * and a hosted-gateway button when a gateway plugin is enabled. The in-flight flag, the success
 * flag and the refusal sentence belong to the invoice payment store; this component only renders
 * them and calls its actions.
 */
import { clientInvoicePath } from '~/constants/clientRoutes'
import { CreditCard } from 'lucide-vue-next'
import { storeToRefs } from 'pinia'
import { NEW_CARD } from '~/constants/payMethod'
import { useInvoicePaymentStore } from '~/stores/invoicepayment'

/** Props of PayPanel. */
interface Props {
  /** The id of the invoice being paid. */
  invoiceId: string
}

/** The props of the component. */
const props = defineProps<Props>()

/** Prefixes a path with the active locale. */
const localePath = useLocalePath()
/** The runtime configuration of the app. */
const config = useRuntimeConfig()

/**
 * Base URL of the billing-portal instance this deployment fronts, or an empty string.
 *
 * Empty is a legitimate configuration -- a deployment that runs no billing portal -- so every
 * link built from it is guarded.
 */
const billingPortalUrl = config.public.billingPortalUrl

/** The invoice payment store, loaded by the page before this panel is shown. */
const paymentStore = useInvoicePaymentStore()
/** Reactive state of the store this component renders. */
const { submitting, paySuccess, error: payError, savedMethods, hostedGatewayModule } = storeToRefs(paymentStore)

/** The invoice's own page. */
const invoicePath = computed(() => localePath(clientInvoicePath(props.invoiceId)))

/**
 * Whether paying with a card that is not saved is possible at all.
 *
 * That path is a hand-off to the billing portal's hosted card page, so it exists only when this
 * deployment has a billing-portal instance. Without one the redirect would leave this page for a
 * 404, so the option is not offered rather than offered and broken.
 */
const canUseNewCard = computed(() => Boolean(billingPortalUrl))

/**
 * The chosen method: a saved method's id, {@link NEW_CARD}, or null when no method is
 * selectable. Paying through a hosted gateway is a separate button and is unaffected.
 */
const selectedMethodId = ref<string | null>(savedMethods.value[0]?.id ?? (canUseNewCard.value ? NEW_CARD : null))

/** The new-card number as shown. */
const cardNumber = ref('')

/** The new-card expiry as shown. */
const expiryDate = ref('')

/** The new-card security code. */
const cvv = ref('')

/** The optional name for the new card. */
const cardDescription = ref('')

/** Whether the card-entry form renders: it only ever feeds the billing-portal hand-off. */
const showNewCardForm = computed(() => canUseNewCard.value && selectedMethodId.value === NEW_CARD)

/** Whether the page offers any card-based way to pay (the hosted-gateway button aside). */
const hasSelectableMethod = computed(() => savedMethods.value.length > 0 || canUseNewCard.value)

/**
 * Pays the invoice with the selected method.
 *
 * A saved card is charged through the API. "New card" instead hands off to the billing portal's
 * hosted card page, which navigates the browser away, so it is only taken when
 * {@link canUseNewCard} holds; the guards here are a second line of defence.
 *
 * @returns Resolves when the attempt has ended; a refusal is rendered from `payError`.
 */
const submitPayment = async (): Promise<void> => {
  paymentStore.clearError()

  // Nothing selectable: the submit button is not rendered in this state.
  if (selectedMethodId.value === null) {
    return
  }

  if (selectedMethodId.value === NEW_CARD) {
    if (!canUseNewCard.value) {
      return
    }
    window.location.href = `${billingPortalUrl}/index.php?rp=/invoice/${props.invoiceId}/pay`
    return
  }

  await paymentStore.payWithSavedMethod(props.invoiceId, selectedMethodId.value)
}

/**
 * Starts a hosted-gateway payment and redirects the browser to the bank.
 *
 * @returns Resolves when the attempt has ended; a refusal is rendered from `payError`.
 */
const payWithHostedGateway = async (): Promise<void> => {
  if (!hostedGatewayModule.value) {
    return
  }
  const returnUrl = new URL(localePath(`/payment/result?invoice=${props.invoiceId}`), window.location.origin).toString()
  const redirectUrl = await paymentStore.startGatewayPayment(props.invoiceId, hostedGatewayModule.value as string, returnUrl)
  if (redirectUrl) {
    window.location.href = redirectUrl
  }
}
</script>
