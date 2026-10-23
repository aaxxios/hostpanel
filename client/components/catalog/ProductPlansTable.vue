<template>
  <UiTable scrollable :label="$t('hosting.comparison.title')" data-testid="product-compare-table">
    <UiTableHead>
      <UiTableRow :hoverable="false">
        <UiTableTh class="min-w-[160px] !p-4 md:!p-5">{{ $t('hosting.comparison.feature') }}</UiTableTh>
        <UiTableTh v-for="plan in plans" :key="plan.pid" align="center" class="min-w-[120px] !p-4 md:!p-5">
          <UiText size="sm" tone="inherit" weight="semibold" class="normal-case text-gray-900 dark:text-white [.tpl-classic_&]:text-classic-ink">
            {{ plan.tier }}
          </UiText>
          <UiText
            v-if="priceFor(plan)"
            size="xs"
            tone="muted"
            weight="medium"
            class="mt-1 normal-case tabular-nums [.tpl-classic_&]:text-classic-muted"
          >
            {{ priceFor(plan) }}
          </UiText>
        </UiTableTh>
      </UiTableRow>
    </UiTableHead>

    <UiTableBody>
      <!-- One row per specification line, aligned by the shared comparisonRows logic. -->
      <UiTableRow v-for="row in rows" :key="row.label" group :data-testid="`product-compare-row-${row.label}`">
        <UiTableTd class="!p-4 font-medium text-gray-600 dark:text-gray-400 md:!p-5 [.tpl-classic_&]:text-classic-muted">
          {{ row.label }}
        </UiTableTd>
        <UiTableTd
          v-for="(value, index) in row.values"
          :key="`${row.label}-${plans[index]?.pid ?? index}`"
          align="center"
          class="!p-4 md:!p-5"
          :class="value ? 'text-gray-900 dark:text-white [.tpl-classic_&]:text-classic-ink' : 'text-gray-500 dark:text-gray-500 [.tpl-classic_&]:text-classic-muted'"
        >
          <!-- A plan that does not carry this line reads as absent, not as zero. -->
          {{ value || $t('hosting.comparison.notIncluded') }}
        </UiTableTd>
      </UiTableRow>
    </UiTableBody>
  </UiTable>
</template>

<script setup lang="ts">
/**
 * The specification table of a product family: one column per plan, one row per specification
 * line, the cells aligned by the shared `buildComparisonRows` logic. A plan that lacks a line
 * reads "Not included". It is presentational; the page decides whether the plans are comparable
 * (`isComparable`) before it is drawn.
 */
import type { ComparisonRow } from '~/types/comparisonrow'
import type { ProductPlan } from '~/types/productplan'

/** Props of ProductPlansTable. */
interface Props {
  /** The plans, one column each, in the order the row values line up with. */
  plans: ProductPlan[]
  /** The specification rows aligned to {@link Props.plans}. */
  rows: ComparisonRow[]
  /** The price of a plan for the chosen cycle, formatted; empty when it has none. */
  priceFor: (plan: ProductPlan) => string
}

defineProps<Props>()
</script>
