<template>
  <UiStack gap="none" class="min-w-0">
    <UiBadge v-if="badge" variant="primary" rounded class="gap-2.5 self-start" data-testid="hero-badge">
      <span class="h-[7px] w-[7px] rounded-full bg-ok" />
      {{ badge }}
    </UiBadge>

    <UiHeading :level="1" size="display" class="font-display leading-[1.08]" :class="badge ? 'mt-[22px]' : ''" data-testid="hero-title">
      {{ t('aurora.hero.titleLine1') }}<br>
      {{ t('aurora.hero.titleLine2') }}<br>
      <UiText as="span" size="inherit" tone="inherit" weight="bold" class="bg-hero-grad bg-clip-text text-transparent">
        {{ t('aurora.hero.titleAccent') }}
      </UiText>
    </UiHeading>

    <UiText size="lg" tone="inherit" class="mt-6 max-w-[520px] leading-[1.65] text-mut">
      {{ t('aurora.hero.lead') }}
    </UiText>

    <UiStack direction="row" wrap gap="md" class="mt-[34px]">
      <UiButton
        variant="bare"
        :to="localePath('/domains')"
        data-testid="hero-cta-primary"
        :class="[BRAND_BUTTON, 'px-[26px] py-[15px] text-base font-semibold']"
      >
        {{ t('aurora.hero.ctaPrimary') }}
      </UiButton>
      <UiButton
        variant="bare"
        :to="localePath('/contact')"
        data-testid="hero-cta-secondary"
        :class="[LINE_BUTTON, 'px-[26px] py-[15px] text-base']"
      >
        {{ t('aurora.hero.ctaSecondary') }}
      </UiButton>
    </UiStack>

    <UiStack v-if="stats.length > 0" direction="row" wrap gap="none" class="mt-11 gap-x-10 gap-y-7" data-testid="hero-stats">
      <UiStack v-for="stat in stats" :key="stat.label" gap="none">
        <UiText as="span" size="inherit" tone="inherit" weight="bold" class="text-[26px] text-tx">{{ stat.value }}</UiText>
        <UiText as="span" size="sm" tone="inherit" class="mt-1 text-mut2">{{ stat.label }}</UiText>
      </UiStack>
    </UiStack>
  </UiStack>
</template>

<script setup lang="ts">
/**
 * aurora hero — the homepage's headline, calls to action and headline figures.
 * The badge and the figures are operator settings and each is hidden while empty.
 * Carries the page's only visible h1.
 */
import { PortalSettingKey } from '~/constants/portalSettingKey'
import { BRAND_BUTTON, LINE_BUTTON } from '~/templates/aurora/buttons'

/** Translator for the active locale. */
const { t } = useI18n()
/** Prefixes a path with the active locale. */
const localePath = useLocalePath()
/** Operator settings, with their runtime-config fallbacks. */
const { get } = usePortalSettings()

/** Short line in the badge above the headline; empty hides the badge. */
const badge = computed(() => get(PortalSettingKey.HeroBadge, 'portalHeroBadge'))

/** Headline figures the operator configured; one that is empty is left out. */
const stats = computed(() => [
  { value: get(PortalSettingKey.StatsClients, 'portalStatsClients'), label: t('aurora.hero.statClients') },
  { value: get(PortalSettingKey.StatsUptime, 'portalStatsUptime'), label: t('aurora.hero.statUptime') },
  { value: get(PortalSettingKey.StatsRegions, 'portalStatsRegions'), label: t('aurora.hero.statRegions') },
].filter(stat => stat.value !== ''))
</script>
