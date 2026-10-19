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
    </UiStack>
  </UiCard>
</template>

<script setup lang="ts">
/**
 * aurora trust bar — the guarantees. Accepted payment methods are not listed: which gateways are
 * live is the operator's configuration, and a fixed list of brands would be a claim nothing backs.
 */
const { t } = useI18n()

/** Guarantees with their icon and accent colour. */
const badges = computed(() => [
  { icon: 'lucide:lock', label: t('aurora.trust.ssl'), tone: 'text-ok' },
  { icon: 'lucide:shield-check', label: t('aurora.trust.refund'), tone: 'text-ac1' },
  { icon: 'lucide:badge-percent', label: t('aurora.trust.noHidden'), tone: 'text-ac2' },
])
</script>
