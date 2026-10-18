<template>
  <UiCard
    tone="bare"
    padding="none"
    data-testid="trust-bar"
    class="relative border-line bg-surf px-[clamp(18px,3vw,28px)] py-[22px]"
  >
    <UiStack direction="row" wrap align="center" justify="between" gap="lg">
      <UiStack direction="row" wrap align="center" gap="lg">
        <UiStack v-for="item in badges" :key="item.label" direction="row" align="center" gap="xs" class="text-tx2">
          <Icon :name="item.icon" class="h-[18px] w-[18px]" :class="item.tone" />
          <UiText as="span" size="sm" tone="inherit">{{ item.label }}</UiText>
        </UiStack>
      </UiStack>

      <UiStack direction="row" align="center" gap="xs">
        <UiText as="span" size="inherit" tone="inherit" class="mr-1 text-[13px] text-mut2">{{ t('aurora.trust.paymentLabel') }}</UiText>
        <UiBadge v-for="method in payments" :key="method" size="sm" data-testid="trust-payment">{{ method }}</UiBadge>
      </UiStack>
    </UiStack>
  </UiCard>
</template>

<script setup lang="ts">
/** aurora trust bar — guarantees and accepted payment methods. */
const { t } = useI18n()

/** Guarantees with their icon and accent colour. */
const badges = computed(() => [
  { icon: 'lucide:lock', label: t('aurora.trust.ssl'), tone: 'text-ok' },
  { icon: 'lucide:shield-check', label: t('aurora.trust.refund'), tone: 'text-ac1' },
  { icon: 'lucide:badge-percent', label: t('aurora.trust.noHidden'), tone: 'text-ac2' },
])

// Card brands are proper nouns and are not translated.
/** Accepted payment methods. */
const payments = ['VISA', 'Mastercard', 'PayPal', 'Idram']
</script>
