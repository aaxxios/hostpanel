<template>
  <UiStack gap="sm" data-testid="pay-method-selector">
    <UiText as="span" size="sm" weight="medium">{{ $t('invoicePay.paymentMethod') }}</UiText>

    <UiOptionCard
      v-for="method in savedMethods"
      :key="method.id"
      name="pay-method"
      :value="method.id"
      :label="methodLabel(method)"
      :description="method.cardExpiry ?? undefined"
      :icon="CARD_ICON"
      :model-value="modelValue"
      :testid="`pay-method-${method.id}`"
      @update:model-value="select(String($event))"
    />
  </UiStack>
</template>

<script setup lang="ts">
/**
 * PayMethodSelector - the "Payment method" choice of the invoice payment page.
 *
 * Lists the client's saved cards as selectable rows. The parent renders it only when there is at
 * least one saved card; the chosen id is that card's id.
 */
import { PAYMENT_GATEWAY_KIND_ICON } from '~/constants/paymentGatewayKind'
import type { PaymentMethod } from '~/types/payment'
import { PaymentGatewayKind } from '~/types/paymentgatewaykind'
import { brandName } from '~/utils/paymentMethodMapper'

/** Props of PayMethodSelector. */
interface Props {
  /** The client's saved payment methods. */
  savedMethods: PaymentMethod[]
  /** The chosen saved method's id, or null when nothing is selectable (v-model). */
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
const methodLabel = (method: PaymentMethod): string => `${method.cardType ? brandName(method.cardType) : ''} •••• ${method.cardLastFour ?? ''}`.trim()

/**
 * Reports a chosen option.
 *
 * @param id - The chosen method id.
 */
const select = (id: string): void => {
  emit('update:modelValue', id)
}
</script>
