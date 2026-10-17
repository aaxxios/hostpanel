<template>
  <UiStack gap="sm" data-testid="pay-method-selector">
    <UiText as="span" size="sm" weight="medium">{{ $t('invoicePay.paymentMethod') }}</UiText>

    <template v-if="savedMethods.length > 0">
      <UiCard v-for="method in savedMethods" :key="method.id" padding="sm" :class="cardClass(method.id)">
        <UiRadio
          name="pay-method"
          :value="method.id"
          :label="methodLabel(method)"
          :description="method.card_expiry ?? undefined"
          :model-value="modelValue ?? undefined"
          :testid="`pay-method-${method.id}`"
          @update:model-value="select(method.id)"
        />
      </UiCard>

      <!-- New card option -- hidden when there is no billing-portal page to hand off to. -->
      <UiCard v-if="canUseNewCard" padding="sm" :class="cardClass(NEW_CARD)">
        <UiRadio
          name="pay-method"
          :value="NEW_CARD"
          :label="$t('invoicePay.newCard')"
          :model-value="modelValue ?? undefined"
          testid="pay-method-new"
          @update:model-value="select(NEW_CARD)"
        />
      </UiCard>
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
 * Lists the client's saved cards as radio options, plus a "new card" option when the deployment
 * can hand a new card off to the hosted card page. With no saved card it only announces that the
 * card form follows. The chosen id is a saved method's id, or {@link NEW_CARD}.
 */
import { NEW_CARD } from '~/constants/payMethod'
import type { PaymentMethod } from '~/types/payment'

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
const props = defineProps<Props>()

/** Events the component raises. */
const emit = defineEmits<{
  /** The reader chose another method. */
  'update:modelValue': [value: string]
}>()

/**
 * Frame colours of an option, highlighted while it is the chosen one.
 *
 * @param id - The option's method id.
 * @returns The classes for the option's card.
 */
const cardClass = (id: string): string => (props.modelValue === id ? 'border-primary-500/50 bg-primary-500/5' : '')

/**
 * The text of a saved card's option: its network and last four digits.
 *
 * @param method - The saved payment method.
 * @returns For example `visa •••• 4242`.
 */
const methodLabel = (method: PaymentMethod): string => `${method.card_type ?? ''} •••• ${method.card_last_four ?? ''}`.trim()

/**
 * Reports a chosen option.
 *
 * @param id - The chosen method id.
 */
const select = (id: string): void => {
  emit('update:modelValue', id)
}
</script>
