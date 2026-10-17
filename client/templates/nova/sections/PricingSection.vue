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

      <UiStack
        direction="row"
        align="center"
        gap="none"
        role="group"
        :aria-label="t('nova.plans.billingLabel')"
        class="gap-1 rounded-full border border-nova-border p-1"
      >
        <UiButton
          v-for="option in options"
          :key="option.key"
          variant="bare"
          class="min-h-[44px] rounded-full px-5 text-sm font-semibold"
          :class="option.selected ? 'bg-nova-accent text-nova-on-accent' : 'text-nova-muted hover:text-nova-ink'"
          :aria-pressed="option.selected"
          :data-testid="`nova-billing-${option.key}`"
          @click="emit('update:yearly', option.value)"
        >
          {{ t(option.labelKey) }}
        </UiButton>
      </UiStack>
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

    <UiGrid v-else cols="3" gap="lg" class="mt-10">
      <NovaPricingCard
        v-for="plan in novaPlans"
        :key="plan.id"
        :plan="plan"
        :yearly="yearly"
      />
    </UiGrid>
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

/** The two billing options and which one is selected. */
const options = computed(() => [
  { key: 'monthly', labelKey: 'nova.plans.monthly', value: false, selected: !props.yearly },
  { key: 'yearly', labelKey: 'nova.plans.yearly', value: true, selected: props.yearly },
])
</script>
