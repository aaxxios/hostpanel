<template>
  <UiCard
    tone="bare"
    padding="none"
    data-testid="trust-bar"
    class="relative w-full border-line bg-surf px-[clamp(18px,3vw,32px)] py-5 shadow-panel"
  >
    <UiGrid cols="1" gap="md" class="md:grid-cols-3">
      <UiStack
        v-for="item in badges"
        :key="item.label"
        direction="row"
        align="center"
        justify="center"
        gap="sm"
        data-testid="trust-item"
      >
        <UiTile size="lg" :tone="item.tone" data-testid="trust-chip">
          <Icon :name="item.icon" class="h-5 w-5" aria-hidden="true" />
        </UiTile>
        <UiText as="span" size="sm" tone="inherit" weight="semibold" class="text-tx2">{{ item.label }}</UiText>
      </UiStack>
    </UiGrid>
  </UiCard>
</template>

<script setup lang="ts">
/**
 * aurora trust bar — the guarantees, spread evenly across the container width, each with its
 * icon in a tinted chip. Accepted payment methods are not listed: which gateways are live is the
 * operator's configuration, and a fixed list of brands would be a claim nothing backs.
 */
import { UiTone } from '~/utils/ui/enums'

const { t } = useI18n()

/** Guarantees with their icon and the tone of the chip behind it. */
const badges = computed(() => [
  { icon: 'lucide:lock', label: t('aurora.trust.ssl'), tone: UiTone.Success },
  { icon: 'lucide:shield-check', label: t('aurora.trust.refund'), tone: UiTone.Primary },
  { icon: 'lucide:badge-percent', label: t('aurora.trust.noHidden'), tone: UiTone.Secondary },
])
</script>
