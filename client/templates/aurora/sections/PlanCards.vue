<template>
  <UiSection id="hosting" padding="lg" :contained="false" class="relative border-t border-line">
    <UiStack direction="row" wrap align="end" :justify="showHeader ? 'between' : 'end'" gap="xl">
      <UiStack v-if="showHeader" gap="none">
        <UiText tone="inherit" size="inherit" :class="[EYEBROW_CLASS, 'text-ac1']">{{ t('aurora.plans.eyebrow') }}</UiText>
        <UiHeading :level="2" size="bare" :class="[SECTION_TITLE_CLASS, 'mt-3']">
          {{ t('aurora.plans.title') }}
        </UiHeading>
      </UiStack>

      <UiCard tone="bare" padding="none" data-testid="billing-toggle" class="rounded-full border-line2 p-1.5">
        <UiStack direction="row" align="center" gap="sm">
          <UiButton
            variant="bare"
            data-testid="billing-monthly"
            :aria-pressed="!yearly"
            :class="[yearly ? PILL_IDLE : PILL_ACTIVE, 'px-[18px] py-2.5 text-sm font-semibold']"
            @click="emit('update:yearly', false)"
          >
            {{ t('aurora.plans.monthly') }}
          </UiButton>
          <UiButton
            variant="bare"
            data-testid="billing-yearly"
            :aria-pressed="yearly"
            :class="[yearly ? PILL_ACTIVE : PILL_IDLE, 'px-[18px] py-2.5 text-sm font-semibold']"
            @click="emit('update:yearly', true)"
          >
            {{ t('aurora.plans.yearly') }}
          </UiButton>
        </UiStack>
      </UiCard>
    </UiStack>

    <UiText v-if="plans.length === 0" tone="inherit" class="mt-10 text-[15px] text-mut2" data-testid="plans-empty">
      {{ t('aurora.plans.empty') }}
    </UiText>

    <!--
      A plan that costs nothing is not competing with the others on price, so it does
      not belong in a row that compares prices. It gets the full width above them.
    -->
    <UiCard
      v-if="freePlan"
      tone="bare"
      padding="none"
      data-testid="plan-free"
      class="mt-10 border-ac1/25 bg-surf p-[clamp(24px,4vw,40px)]"
    >
      <UiStack direction="row" wrap align="center" justify="between" gap="xl">
        <UiStack gap="none" class="min-w-[260px] flex-1">
          <UiBadge variant="primary" rounded size="sm" class="self-start uppercase tracking-[0.1em]">
            {{ t('aurora.plans.freeBadge') }}
          </UiBadge>
          <UiHeading :level="3" size="bare" class="mt-3.5 font-display text-[clamp(24px,3.4vw,32px)] font-bold -tracking-[0.02em] text-tx">
            {{ freePlan.name }}
          </UiHeading>
          <UiText v-if="freePlan.description" tone="inherit" class="mt-2 max-w-[560px] text-[15px] leading-relaxed text-mut">
            {{ freePlan.description }}
          </UiText>

          <UiList v-if="freePlan.features.length" list-style="bare" class="mt-5 flex flex-wrap gap-x-6 gap-y-2.5">
            <UiListItem v-for="line in freePlan.features" :key="line" class="flex items-center gap-2">
              <Icon name="lucide:check" class="h-4 w-4 shrink-0 text-ok" />
              <UiText as="span" size="sm" tone="inherit" class="text-tx2">{{ line }}</UiText>
            </UiListItem>
          </UiList>
        </UiStack>

        <UiStack align="start" gap="md" class="min-w-[220px]">
          <UiStack gap="none">
            <UiText
              as="span"
              size="inherit"
              tone="inherit"
              weight="bold"
              class="font-display text-[clamp(40px,6vw,56px)] -tracking-[0.03em] text-tx"
            >
              {{ freePlan.priceMonthly }}
            </UiText>
            <UiText as="span" size="inherit" tone="inherit" class="mt-1 text-[13px] uppercase tracking-[0.1em] text-mut2">
              {{ t('aurora.plans.freeForever') }}
            </UiText>
          </UiStack>
          <UiButton
            variant="bare"
            full-width
            :to="freePlan.href"
            :data-testid="`plan-choose-${freePlan.id}`"
            :class="[BRAND_BUTTON, 'px-6 py-3.5 text-[15px]']"
          >
            {{ t('aurora.plans.cta') }}
          </UiButton>
        </UiStack>
      </UiStack>
    </UiCard>

    <UiGrid v-if="paidPlans.length" cols="auto" gap="lg" class="mt-10">
      <UiCard
        v-for="plan in paidPlans"
        :key="plan.id"
        tone="bare"
        padding="xl"
        :data-testid="`plan-card-${plan.id}`"
        class="flex h-full flex-col border-line bg-surf"
      >
        <UiText as="span" size="inherit" tone="inherit" weight="bold" class="text-[17px] text-tx">{{ plan.name }}</UiText>
        <UiText v-if="plan.description" size="sm" tone="inherit" class="mt-1.5 text-mut2">{{ plan.description }}</UiText>

        <UiList v-if="plan.features.length" list-style="bare" class="mt-3.5 flex flex-col gap-2">
          <UiListItem v-for="line in plan.features" :key="line" class="flex items-start gap-2">
            <Icon name="lucide:check" class="mt-0.5 h-3.5 w-3.5 shrink-0 text-ok" />
            <UiText as="span" size="inherit" tone="inherit" class="min-w-0 text-[13px] text-tx2">{{ line }}</UiText>
          </UiListItem>
        </UiList>

        <!-- mt-auto pins price and button to the card bottom so they align across cards of unequal height. -->
        <UiStack gap="none" class="mt-auto pt-[22px]">
          <UiStack direction="row" align="end" gap="xs">
            <UiText as="span" size="inherit" tone="inherit" weight="bold" class="text-[38px] -tracking-[0.02em] text-tx">
              {{ yearly ? plan.priceAnnual : plan.priceMonthly }}
            </UiText>
            <UiText as="span" size="sm" tone="inherit" class="pb-2 text-mut2">
              {{ yearly ? t('aurora.plans.perMonthAnnual') : t('aurora.plans.perMonth') }}
            </UiText>
          </UiStack>

          <UiButton
            variant="bare"
            full-width
            :to="plan.href"
            :data-testid="`plan-choose-${plan.id}`"
            :class="[LINE_BUTTON, 'mt-[26px] py-3.5 text-[15px] font-bold']"
          >
            {{ t('aurora.plans.cta') }}
          </UiButton>
        </UiStack>
      </UiCard>
    </UiGrid>

    <!--
      A fallback, not a companion to the per-card lists above. An operator whose
      descriptions are plain summaries gets no feature lines at all, and this keeps
      the section from looking bare; where the plans do carry their own features,
      repeating a single "included in every plan" list beside them would contradict
      the cards the moment two plans differed.
    -->
    <UiCard
      v-if="showIncluded"
      tone="bare"
      data-testid="plans-included"
      class="mt-9 border-line bg-surf"
    >
      <UiText as="span" size="inherit" tone="inherit" :class="[EYEBROW_CLASS, 'tracking-[0.1em] text-ac2']">
        {{ t('aurora.plans.includedTitle') }}
      </UiText>
      <UiGrid cols="auto" gap="md" class="mt-4">
        <UiStack v-for="line in included" :key="line" direction="row" align="center" gap="xs">
          <Icon name="lucide:check" class="h-4 w-4 shrink-0 text-ok" />
          <UiText as="span" size="inherit" tone="inherit" class="text-[15px] text-tx2">{{ line }}</UiText>
        </UiStack>
      </UiGrid>
    </UiCard>
  </UiSection>
</template>

<script setup lang="ts">
/**
 * aurora hosting plan cards.
 *
 * Presentation only — plans and their formatted prices arrive as props, and the
 * billing-period toggle is reported upward rather than owned here.
 */
import { BRAND_BUTTON, LINE_BUTTON, PILL_ACTIVE, PILL_IDLE } from '~/templates/aurora/buttons'
import { EYEBROW_CLASS, SECTION_TITLE_CLASS } from '~/templates/aurora/typography'
import type { PlanCard } from '~/types/plancard'

/** Props with their defaults applied. */
const props = withDefaults(defineProps<{
  /** The plans to show. */
  plans?: PlanCard[]
  /** Whether prices are shown per month billed annually. */
  yearly?: boolean
  /** Off on the hosting page, whose own hero already states the same thing. */
  showHeader?: boolean
}>(), { plans: () => [], yearly: false, showHeader: true })

/** Events this component raises. */
const emit = defineEmits<{
  /** The visitor switched the billing period. */
  'update:yearly': [value: boolean]
}>()

/** Translator for the active locale. */
const { t } = useI18n()

// The free plan is pulled out of the row and shown on its own above it; the rest keep
// the grid. `find` rather than a filter: two free plans would be a catalogue mistake,
// and featuring the first is better than stacking banners.
/** The plan that costs nothing, if the catalogue has one. */
const freePlan = computed(() => props.plans.find(plan => plan.isFree))

/** Every other plan, compared on price in the grid. */
const paidPlans = computed(() => props.plans.filter(plan => !plan.isFree))

/** Whether the shared "included in every plan" list stands in for per-plan feature lines. */
const showIncluded = computed(() => props.plans.length > 0 && !props.plans.some(plan => plan.features.length))

/** What every plan includes. */
const included = computed(() => [
  t('aurora.plans.included1'),
  t('aurora.plans.included2'),
  t('aurora.plans.included3'),
  t('aurora.plans.included4'),
])
</script>
