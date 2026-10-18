<template>
  <UiStack gap="md" data-testid="pay-card-form">
    <UiInput
      :model-value="cardNumber"
      :label="$t('invoicePay.cardNumber')"
      :placeholder="$t('invoicePay.cardNumberPlaceholder')"
      autocomplete="cc-number"
      testid="pay-card-number"
      @update:model-value="cardNumber = formatCardNumber(String($event))"
    />
    <UiGrid cols="2">
      <UiInput
        :model-value="expiryDate"
        :label="$t('invoicePay.expiryDate')"
        :placeholder="$t('invoicePay.expiryPlaceholder')"
        autocomplete="cc-exp"
        testid="pay-card-expiry"
        @update:model-value="expiryDate = formatExpiry(String($event))"
      />
      <UiInput
        :model-value="cvv"
        :label="$t('invoicePay.cvv')"
        :placeholder="$t('invoicePay.cvvPlaceholder')"
        autocomplete="cc-csc"
        testid="pay-card-cvv"
        @update:model-value="cvv = formatCvv(String($event))"
      />
    </UiGrid>
    <UiInput
      v-model="cardDescription"
      :label="$t('invoicePay.cardDescription')"
      :placeholder="$t('invoicePay.cardDescriptionPlaceholder')"
      testid="pay-card-description"
    />
  </UiStack>
</template>

<script setup lang="ts">
/**
 * PayCardForm - the new-card fields of the invoice payment page: number, expiry, security code
 * and an optional name for the card.
 *
 * The fields are only a prelude to the hosted card page, so they format what is typed (digits,
 * grouping, `MM / YY`) and never validate it; the payment provider does that. Each value is a
 * two-way model so the page can read them.
 */
import { formatCardNumber, formatCvv, formatExpiry } from '~/utils/cardFormat'

/** The card number as shown, grouped in fours. */
const cardNumber = defineModel<string>('cardNumber', { default: '' })

/** The expiry date as shown, `MM / YY`. */
const expiryDate = defineModel<string>('expiryDate', { default: '' })

/** The security code. */
const cvv = defineModel<string>('cvv', { default: '' })

/** The optional name the client gives the card. */
const cardDescription = defineModel<string>('cardDescription', { default: '' })
</script>
