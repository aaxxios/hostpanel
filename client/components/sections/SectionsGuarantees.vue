<template>
  <section v-if="guarantees.length" class="py-12 md:py-20 bg-ui-page" data-testid="sections-guarantees">
    <UiContainer>
      <MarketingSectionHeader
        class="mb-12"
        :badge="$t('hostingSections.guarantees.badge')"
        :title="$t('hostingSections.guarantees.title')"
        :subtitle="$t('hostingSections.guarantees.subtitle')"
      />

      <UiBalancedGrid :count="guarantees.length" data-testid="sections-guarantees-grid">
        <UiCard
          v-for="guarantee in guarantees"
          :key="guarantee.key"
          hover
          :data-testid="`guarantee-${guarantee.key}`"
        >
          <UiStack gap="sm">
            <UiAccentTile :icon="GUARANTEE_ICONS[guarantee.key]" :class="ACCENT_BRAND_CLASS" />
            <UiHeading :level="3" size="subsection">{{ $t(`hostingSections.guarantees.items.${guarantee.key}.title`, { days: guarantee.days ?? 0 }) }}</UiHeading>
            <UiText tone="muted" size="sm">{{ $t(`hostingSections.guarantees.items.${guarantee.key}.description`, { days: guarantee.days ?? 0 }) }}</UiText>
          </UiStack>
        </UiCard>
      </UiBalancedGrid>
    </UiContainer>
  </section>
</template>

<script setup lang="ts">
/**
 * Guarantees section: free migration, the money-back period and no hidden fees, each as a card
 * when the operator set it. With none set the section renders nothing.
 */
import { GuaranteeKey } from '~/constants/guaranteeKey'
import { ACCENT_BRAND_CLASS } from '~/utils/ui/accent'

/** The guarantees on offer, in display order. */
const guarantees = useGuarantees()

/** Icon of each guarantee. */
const GUARANTEE_ICONS: Record<GuaranteeKey, string> = {
  [GuaranteeKey.FreeMigration]: 'mdi:truck-fast-outline',
  [GuaranteeKey.MoneyBack]: 'mdi:cash-refund',
  [GuaranteeKey.NoHiddenFees]: 'mdi:receipt-text-check-outline',
}
</script>
