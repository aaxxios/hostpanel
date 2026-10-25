<template>
  <UiStack gap="sm" data-testid="pay-method-selector">
    <UiText as="span" size="sm" weight="medium">{{ $t('invoicePay.paymentMethod') }}</UiText>

    <template v-if="savedMethods.length > 0">
      <UiOptionCard
        v-for="method in savedMethods"
        :key="method.id"
        name="pay-method"
        :value="method.id"
        :label="methodLabel(method)"
        :description="method.card_expiry ?? undefined"
        :icon="CARD_ICON"
        :model-value="modelValue"
        :testid="`pay-method-${method.id}`"
        @update:model-value="select(String($event))"
      />

      <!-- New card option -- hidden when there is no billing-portal page to hand off to. -->
      <UiOptionCard
        v-if="canUseNewCard"
        name="pay-method"
        :value="NEW_CARD"
        :label="$t('invoicePay.newCard')"
        :icon="CARD_ICON"
        :model-value="modelValue"
        testid="pay-method-new"
        @update:model-value="select(NEW_CARD)"
      />
    </template>

    <!-- No saved methods: only announces the card form when that form leads somewhere. -->
    <UiAlert v-else-if="canUseNewCard" variant="info" data-testid="pay-method-enter-card">
      {{ $t('invoicePay.enterCardBelow') }}
    </UiAlert>
  </UiStack>
</template>

<script setup lang="ts">
/**
 * PayMethodSelector - the "Payment method" choice of the invoice payment page.
 *
 * Lists the client's saved cards as selectable rows, plus a "new card" option when the deployment
 * can hand a new card off to the hosted card page. With no saved card it only announces that the
 * card form follows. The chosen id is a saved method's id, or {@link NEW_CARD}.
 */
import { PAYMENT_GATEWAY_KIND_ICON } from '~/constants/paymentGatewayKind'
import { NEW_CARD } from '~/constants/payMethod'
import type { PaymentMethod } from '~/types/payment'
import { PaymentGatewayKind } from '~/types/paymentgatewaykind'
import { brandName } from '~/utils/paymentMethodMapper'

/** Props of PayMethodSelector. */
interface Props {
  /** The client's saved payment methods. */
  savedMethods: PaymentMethod[]
  /** Whether a card that is not saved can be paid with (the hosted-page hand-off exists). */
  canUseNewCard: boolean
  /** The chosen method id, {@link NEW_CARD}, or null when nothing is selectable (v-model). */
  modelValue: string | null
}

/** The props of the component. */
defineProps<Props>()

/** The icon every card option wears. */
const CARD_ICON = PAYMENT_GATEWAY_KIND_ICON[PaymentGatewayKind.Card]

/** Events the component raises. */
const emit = defineEmits<{
  /** The reader chose another method. */
  'update:modelValue': [value: string]
}>()

/**
 * The text of a saved card's option: its network and last four digits.
 *
 * @param method - The saved payment method.
 * @returns For example `Visa •••• 4242`.
 */
const methodLabel = (method: PaymentMethod): string => `${method.card_type ? brandName(method.card_type) : ''} •••• ${method.card_last_four ?? ''}`.trim()

/**
 * Reports a chosen option.
 *
 * @param id - The chosen method id.
 */
const select = (id: string): void => {
  emit('update:modelValue', id)
}
</script>
