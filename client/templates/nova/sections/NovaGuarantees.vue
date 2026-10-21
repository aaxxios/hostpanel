<template>
  <UiSection v-if="items.length" padding="lg" class="border-b border-nova-border bg-nova-surface" data-testid="nova-guarantees">
    <NovaSectionHeader
      :eyebrow="t('nova.guarantees.eyebrow')"
      :title="t('nova.guarantees.title')"
      :lead="t('nova.guarantees.lead')"
    />

    <UiBalancedGrid as="ul" :count="items.length" class="mt-10" data-testid="nova-guarantees-grid">
      <li v-for="item in items" :key="item.key" class="h-full" :data-testid="`nova-guarantee-${item.key}`">
        <UiCard padding="lg" class="flex h-full flex-col !border-nova-border !bg-nova-bg">
          <span class="grid h-11 w-11 place-items-center rounded-xl bg-nova-surface-2 text-nova-brand">
            <Icon :name="GUARANTEE_CARDS[item.key].icon" class="h-[22px] w-[22px]" aria-hidden="true" />
          </span>
          <UiHeading :level="3" size="bare" class="mt-5 text-[17px] font-bold text-nova-ink">
            {{ t(GUARANTEE_CARDS[item.key].titleKey) }}
          </UiHeading>
          <UiText size="inherit" tone="inherit" class="mt-2.5 text-[15px] leading-relaxed text-nova-muted">
            {{ t(GUARANTEE_CARDS[item.key].bodyKey ?? '', { days: item.days ?? 0 }) }}
          </UiText>
        </UiCard>
      </li>
    </UiBalancedGrid>
  </UiSection>
</template>

<script setup lang="ts">
/**
 * nova guarantees section: free migration, money back within N days and no hidden fees, each a
 * card when the operator has set it. The set cards come from `useGuarantees()`; with none the
 * section renders nothing.
 */
import NovaSectionHeader from '~/templates/nova/sections/NovaSectionHeader.vue'
import { GUARANTEE_CARDS } from '~/templates/nova/content'

/** Translator for the copy. */
const { t } = useI18n()
/** The guarantees the operator has set. */
const items = useGuarantees()
</script>
