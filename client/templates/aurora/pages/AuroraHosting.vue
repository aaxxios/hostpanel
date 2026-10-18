<template>
  <UiStack gap="none" class="relative overflow-hidden font-aurora text-tx">
    <UiBackdrop variant="aurora" />

    <UiContainer class="relative">
      <UiStack as="section" align="center" gap="none" class="relative pb-9 pt-[clamp(48px,7vw,80px)] text-center">
        <UiText tone="inherit" size="inherit" :class="[EYEBROW_CLASS, 'text-ac1']">{{ t('aurora.hostingPage.eyebrow') }}</UiText>
        <UiHeading :level="1" size="bare" :class="[PAGE_TITLE_CLASS, 'mt-3.5']" data-testid="hosting-title">
          {{ t('aurora.hostingPage.title') }}
        </UiHeading>
        <UiText size="inherit" tone="inherit" class="mx-auto mt-4 max-w-[580px] text-[17px] leading-relaxed text-mut">
          {{ t('aurora.hostingPage.lead') }}
        </UiText>
      </UiStack>

      <AuroraPlanCards
        :plans="plans"
        :yearly="yearly"
        :show-header="false"
        @update:yearly="value => emit('update:yearly', value)"
      />

      <AuroraComparisonTable :plans="plans" :rows="comparisonRows" />

      <AuroraCta />
    </UiContainer>
  </UiStack>
</template>

<script setup lang="ts">
/**
 * aurora hosting page.
 *
 * The design's plan comparison matrix renders from the specification lines an
 * operator enters against each product. It disappears when none are entered.
 */
import AuroraPlanCards from '~/templates/aurora/sections/PlanCards.vue'
import AuroraComparisonTable from '~/templates/aurora/sections/AuroraComparisonTable.vue'
import AuroraCta from '~/templates/aurora/sections/AuroraCta.vue'
import { EYEBROW_CLASS, PAGE_TITLE_CLASS } from '~/templates/aurora/typography'
import type { PlanCard } from '~/types/plancard'
import type { ComparisonRow } from '~/types/comparisonrow'

withDefaults(defineProps<{
  /** Hosting plans for the cards and the comparison columns. */
  plans?: PlanCard[]
  /** Whether prices are shown per month billed annually. */
  yearly?: boolean
  /** Comparison rows, already aligned to the plan columns. */
  comparisonRows?: ComparisonRow[]
}>(), { plans: () => [], yearly: false, comparisonRows: () => [] })

/** Events this component raises. */
const emit = defineEmits<{
  /** The visitor switched the billing period. */
  'update:yearly': [value: boolean]
}>()

/** Translator for the active locale. */
const { t } = useI18n()
</script>
