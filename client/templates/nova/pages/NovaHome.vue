<template>
  <UiStack id="nova-main" as="div" gap="none" class="tpl-nova bg-nova-bg font-nova text-nova-ink" data-testid="nova-home">
    <NovaHero />
    <NovaTrustBar />
    <NovaPricingSection
      :plans="plans"
      :yearly="yearly"
      @update:yearly="value => emit('update:yearly', value)"
    />
    <NovaComparisonTable :plans="plans" :rows="comparisonRows" />
    <NovaWhyInnovayse />
    <NovaPerformance />
    <NovaSecurity />
    <NovaDashboard />
    <NovaMigration />
    <NovaUseCases />
    <NovaTestimonials />
    <NovaFaq />
    <NovaFinalCta />
  </UiStack>
</template>

<script setup lang="ts">
/**
 * nova template homepage.
 *
 * Presentation only — every value it displays arrives as a prop from
 * pages/index.vue, which owns the fetching, the SEO and the structured data.
 *
 * `id="nova-main"` is what the header's skip link points at. It sits here
 * rather than on the shared layout's <main>, because that element belongs to
 * every template and nova must not reach outside itself.
 *
 * The domain-search props are declared but unused: the page hands the same set
 * to whichever template is active, and declaring them keeps Vue from falling
 * them through onto this root element as stray DOM attributes. nova puts its
 * domain search on /domains, where the aurora components it reuses already are.
 */
import NovaHero from '~/templates/nova/sections/NovaHero.vue'
import NovaTrustBar from '~/templates/nova/sections/NovaTrustBar.vue'
import NovaPricingSection from '~/templates/nova/sections/PricingSection.vue'
import NovaComparisonTable from '~/templates/nova/sections/NovaComparisonTable.vue'
import NovaWhyInnovayse from '~/templates/nova/sections/WhyInnovayse.vue'
import NovaPerformance from '~/templates/nova/sections/NovaPerformance.vue'
import NovaSecurity from '~/templates/nova/sections/NovaSecurity.vue'
import NovaDashboard from '~/templates/nova/sections/NovaDashboard.vue'
import NovaMigration from '~/templates/nova/sections/NovaMigration.vue'
import NovaUseCases from '~/templates/nova/sections/UseCases.vue'
import NovaTestimonials from '~/templates/nova/sections/NovaTestimonials.vue'
import NovaFaq from '~/templates/nova/sections/NovaFaq.vue'
import NovaFinalCta from '~/templates/nova/sections/FinalCta.vue'
import type { DomainResult } from '~/templates/aurora/types/domainresult'
import type { PlanCard } from '~/types/plancard'
import type { ComparisonRow } from '~/types/comparisonrow'
import type { TldPriceRow } from '~/types/tldpricerow'

/** Props of NovaHome. */
withDefaults(defineProps<{
  /** Plans as the catalogue store formats them. */
  plans?: PlanCard[]
  /** Whether the yearly price is selected. */
  yearly?: boolean
  /** Specification lines, already aligned to the plan columns. */
  comparisonRows?: ComparisonRow[]
  /** Domain-search results; declared but unused, see the file header. */
  domainResults?: DomainResult[]
  /** Domain search in flight; declared but unused. */
  domainPending?: boolean
  /** Whether any zone is sold; declared but unused. */
  hasZones?: boolean
  /** TLD price hints; declared but unused. */
  priceHints?: TldPriceRow[]
}>(), {
  plans: () => [],
  yearly: false,
  comparisonRows: () => [],
  domainResults: () => [],
  domainPending: false,
  hasZones: true,
  priceHints: () => [],
})

/** Emits the page raises. */
const emit = defineEmits<{
  /** A domain search was requested; declared so the shared page can bind it. */
  search: [term: string]
  /** The reader chose the monthly or the yearly price. */
  'update:yearly': [value: boolean]
}>()
</script>
