<template>
  <UiSection id="plans" padding="lg" class="border-b border-nova-border">
    <UiStack direction="row" wrap align="end" justify="between" gap="lg">
      <NovaSectionHeader
        v-if="showHeader"
        :eyebrow="t('nova.plans.eyebrow')"
        :title="t('nova.plans.title')"
        :lead="t('nova.plans.lead')"
        class="min-w-0"
      />

      <UiSegmentedControl
        :model-value="yearly"
        :options="options"
        :label="t('nova.plans.billingLabel')"
        testid="nova-billing-toggle"
        @update:model-value="emit('update:yearly', $event)"
      />
    </UiStack>

    <!--
      The catalogue not answering is not the visitor's fault and not their
      problem to debug, so the empty state says what it is and stops.
    -->
    <UiCard
      v-if="novaPlans.length === 0"
      padding="xl"
      class="mt-10 !border-nova-border !bg-nova-surface"
      data-testid="nova-plans-empty"
    >
      <UiText size="inherit" tone="inherit" weight="semibold" class="text-[17px] text-nova-ink">
        {{ t('nova.plans.empty') }}
      </UiText>
      <UiText size="inherit" tone="inherit" class="mt-2 text-[15px] text-nova-muted">
        {{ t('nova.plans.emptyHint') }}
      </UiText>
    </UiCard>

    <UiBalancedGrid v-else :count="novaPlans.length" class="mt-10" data-testid="nova-plan-grid">
      <NovaPricingCard
        v-for="plan in novaPlans"
        :key="plan.id"
        :plan="plan"
        :yearly="yearly"
      />
    </UiBalancedGrid>
  </UiSection>
</template>

<script setup lang="ts">
/**
 * nova pricing section.
 *
 * Plans and their formatted prices arrive as props — templates never fetch —
 * and the billing toggle is reported upward rather than owned here, so the page
 * can keep the hosting page and the homepage on the same state.
 */
import NovaPricingCard from '~/templates/nova/sections/PricingCard.vue'
import NovaSectionHeader from '~/templates/nova/sections/NovaSectionHeader.vue'
import { toNovaPlans } from '~/templates/nova/pricing'
import type { PlanCard } from '~/types/plancard'
import type { SegmentedOption } from '~/types/segmentedoption'

/** Props of PricingSection. */
const props = withDefaults(defineProps<{
  /** Plans as the catalogue store formats them. */
  plans?: PlanCard[]
  /** Whether the yearly price is selected. */
  yearly?: boolean
  /** Off on the hosting page, whose own hero already says the same thing. */
  showHeader?: boolean
}>(), { plans: () => [], yearly: false, showHeader: true })

/** Emits the section raises. */
const emit = defineEmits<{
  /** The reader chose the monthly or the yearly price. */
  'update:yearly': [value: boolean]
}>()

/** Translator for the copy. */
const { t } = useI18n()

/** The plans in the shape the cards render. */
const novaPlans = computed(() => toNovaPlans(props.plans))

/** The two billing options; `true` is the yearly price. */
const options = computed<SegmentedOption<boolean>[]>(() => [
  { value: false, label: t('nova.plans.monthly'), testid: 'nova-billing-monthly' },
  { value: true, label: t('nova.plans.yearly'), testid: 'nova-billing-yearly' },
])
</script>
