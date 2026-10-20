<template>
  <UiStack gap="none" class="relative overflow-hidden font-aurora text-tx" data-testid="aurora-home">
    <UiBackdrop variant="aurora" />

    <UiContainer class="relative">
      <UiSection id="domains" padding="lg" :contained="false" class="relative md:pb-14">
        <UiSplit gap="xl" class="items-center">
          <template #start>
            <AuroraHero />
          </template>
          <template #end>
            <!-- A brand glow behind the search card ties it to the headline and gives the
                 right column the same weight as the left. -->
            <UiStack gap="none" class="relative">
              <UiDecor class="-inset-8 rounded-[40px] bg-glow1 opacity-60 blur-3xl" data-testid="hero-search-glow" />
              <AuroraDomainSearch
                :results="domainResults"
                :pending="domainPending"
                :has-zones="hasZones"
                :price-hints="priceHints"
                @search="term => emit('search', term)"
              />
            </UiStack>
          </template>
        </UiSplit>
      </UiSection>

      <AuroraTrustBar />
      <AuroraPartners />

      <AuroraPlanCards
        :plans="plans"
        :yearly="yearly"
        @update:yearly="value => emit('update:yearly', value)"
      />

      <AuroraServices />
      <AuroraProcess v-if="showProcess" />
      <AuroraProjects />
      <AuroraTestimonials />
      <AuroraFaq v-if="showFaq" />
      <AuroraCta />
    </UiContainer>
  </UiStack>
</template>

<script setup lang="ts">
/**
 * aurora template homepage.
 * Presentation only — every value it displays arrives as a prop from pages/index.vue.
 */
import AuroraHero from '~/templates/aurora/sections/AuroraHero.vue'
import AuroraDomainSearch from '~/templates/aurora/sections/DomainSearch.vue'
import AuroraTrustBar from '~/templates/aurora/sections/AuroraTrustBar.vue'
import AuroraPlanCards from '~/templates/aurora/sections/PlanCards.vue'
import AuroraServices from '~/templates/aurora/sections/AuroraServices.vue'
import AuroraProcess from '~/templates/aurora/sections/AuroraProcess.vue'
import AuroraPartners from '~/templates/aurora/sections/AuroraPartners.vue'
import AuroraProjects from '~/templates/aurora/sections/AuroraProjects.vue'
import AuroraTestimonials from '~/templates/aurora/sections/AuroraTestimonials.vue'
import AuroraFaq from '~/templates/aurora/sections/AuroraFaq.vue'
import AuroraCta from '~/templates/aurora/sections/AuroraCta.vue'
import type { DomainResult } from '~/templates/aurora/types/domainresult'
import type { PlanCard } from '~/types/plancard'
import type { TldPriceRow } from '~/types/tldpricerow'

withDefaults(defineProps<{
  /** Lookup results of the search card. */
  domainResults?: DomainResult[]
  /** Whether a lookup is under way. */
  domainPending?: boolean
  /** False when the operator has priced no extensions. */
  hasZones?: boolean
  /** Extensions the search offers, shown as a price hint under the field. */
  priceHints?: TldPriceRow[]
  /** Hosting plans for the plan cards. */
  plans?: PlanCard[]
  /** Whether prices are shown per month billed annually. */
  yearly?: boolean
  /** Whether to show the process section; on by default because it carries indexable content. */
  showProcess?: boolean
  /** Whether to show the FAQ section; on by default because it carries indexable content. */
  showFaq?: boolean
}>(), {
  domainResults: () => [],
  domainPending: false,
  hasZones: true,
  priceHints: () => [],
  plans: () => [],
  yearly: false,
  showProcess: true,
  showFaq: true,
})

/** Events this component raises. */
const emit = defineEmits<{
  /** The visitor submitted a domain search term. */
  search: [term: string]
  /** The visitor switched the billing period. */
  'update:yearly': [value: boolean]
}>()
</script>
