<template>
  <div data-testid="account-payment-tab">
    <UiStack v-if="paymentLoading" gap="sm" data-testid="account-payment-loading">
      <UiSkeleton v-for="index in 3" :key="index" shape="block" class="h-16" />
    </UiStack>

    <UiStack v-else gap="md">
      <UiCard>
        <UiCardHeader :title="$t('client.payment.title')">
          <!-- Only offered when a billing-portal instance exists to add the card on. -->
          <UiButton v-if="canAddPaymentMethod" size="sm" variant="subtle" data-testid="account-payment-add" @click="openAddForm">
            <Plus :size="12" :stroke-width="2" class="mr-1" />
            {{ $t('client.payment.addNew') }}
          </UiButton>
        </UiCardHeader>

        <!-- A failed read is checked before the empty state, which it is otherwise
             indistinguishable from: the list is empty either way. -->
        <UiAlert v-if="paymentLoadError" variant="error" data-testid="account-payment-load-error">
          {{ paymentLoadError }}
        </UiAlert>

        <UiEmptyState
          v-else-if="!paymentMethods.length"
          :icon="CreditCard"
          :title="$t('client.payment.empty')"
          :description="$t('client.payment.emptyDesc')"
        >
          <UiButton v-if="canAddPaymentMethod" data-testid="payment-methods-add" size="sm" variant="subtle" @click="openAddForm">
            {{ $t('client.payment.addNew') }}
          </UiButton>
        </UiEmptyState>

        <UiList v-else list-style="divided">
          <UiListItem v-for="method in paymentMethods" :key="method.id">
            <AccountPaymentMethodRow
              :method="method"
              :setting-default="settingDefaultId === method.id"
              :removing="removingId === method.id"
              @set-default="onSetDefault"
              @remove="pendingRemovalId = $event"
            />
          </UiListItem>
        </UiList>
      </UiCard>

      <UiAlert v-if="paymentActionError" variant="error" data-testid="account-payment-action-error">
        {{ paymentActionError }}
      </UiAlert>
      <UiAlert v-if="success" variant="success" data-testid="account-payment-success">{{ success }}</UiAlert>
    </UiStack>

    <UiConfirmDialog
      :open="pendingRemovalId !== null"
      :title="$t('client.payment.removeConfirm')"
      :description="$t('client.payment.removeConfirmDesc')"
      :confirm-label="$t('client.payment.remove')"
      :loading="removingId !== null"
      variant="danger"
      @confirm="onConfirmRemoval"
      @cancel="pendingRemovalId = null"
    />
  </div>
</template>

<script setup lang="ts">
/**
 * PaymentMethodsTab - the "Payment Methods" tab: the saved methods, set-default and remove.
 *
 * Adding a card is handed off to the billing portal's own page, because tokenised gateways
 * cannot take a card through this API; the action is therefore offered only when a portal URL
 * is configured. Every refusal is the store's sentence.
 *
 * @module components/account/PaymentMethodsTab
 */

import { CreditCard, Plus } from 'lucide-vue-next'
import { storeToRefs } from 'pinia'
import { useAccountStore } from '~/stores/account'
import { ActionOutcome } from '~/types/actionoutcome'

/** Translator for the success sentences. */
const { t } = useI18n()

/** The account store that holds and changes the saved methods. */
const account = useAccountStore()

/** The methods, their loading and error state, and the per-action progress and refusal. */
const {
  paymentMethods, paymentLoading, paymentLoadError, paymentActionError, settingDefaultId, removingId
} = storeToRefs(account)

/** Success sentence after a change; empty otherwise. */
const success = ref('')

/** Id of the method the removal confirmation is about, or null when it is closed. */
const pendingRemovalId = ref<string | null>(null)

/**
 * Base URL of the billing-portal instance this deployment fronts, or an empty string.
 *
 * Empty is a legitimate configuration -- a deployment that runs no billing portal, or one where
 * `BILLING_PORTAL_URL` has not been set yet -- so every link built from it must be guarded.
 */
const billingPortalUrl = useRuntimeConfig().public.billingPortalUrl

/**
 * Whether the "add a card" action can actually do anything.
 *
 * With no `billingPortalUrl` the button would open a path on this app's own origin, i.e. a 404
 * in a new tab, so the action is not offered at all rather than offered and broken.
 */
const canAddPaymentMethod = computed<boolean>(() => Boolean(billingPortalUrl))

/** Opens the billing portal's native credit-card page in a new tab. */
const openAddForm = (): void => {
  // Callers are gated on canAddPaymentMethod; this is the second line of defence.
  if (!canAddPaymentMethod.value) {
    return
  }
  window.open(`${billingPortalUrl}/clientarea.php?action=creditcard`, '_blank')
}

/**
 * Makes a method the default and reads the list again when it worked.
 *
 * @param id - Id of the method.
 */
const onSetDefault = async (id: string): Promise<void> => {
  success.value = ''
  if (await account.setDefaultPaymentMethod(id) === ActionOutcome.Succeeded) {
    success.value = t('client.payment.defaultSet')
    await account.loadPaymentMethods()
  }
}

/** Removes the confirmed method and reads the list again when it worked. */
const onConfirmRemoval = async (): Promise<void> => {
  const id = pendingRemovalId.value
  if (id === null) {
    return
  }
  success.value = ''
  if (await account.removePaymentMethod(id) === ActionOutcome.Succeeded) {
    success.value = t('client.payment.removeSuccess')
    await account.loadPaymentMethods()
  }
  pendingRemovalId.value = null
}
</script>
