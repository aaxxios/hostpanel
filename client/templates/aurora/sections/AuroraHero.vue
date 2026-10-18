<template>
  <UiStack gap="none" class="min-w-0">
    <UiBadge variant="primary" rounded class="gap-2.5 self-start" data-testid="hero-badge">
      <span class="h-[7px] w-[7px] rounded-full bg-ok" />
      {{ t('aurora.hero.badge') }}
    </UiBadge>

    <UiHeading :level="1" size="display" class="mt-[22px] font-display leading-[1.08]" data-testid="hero-title">
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

    <UiStack direction="row" wrap gap="none" class="mt-11 gap-x-10 gap-y-7">
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
 * Carries the page's only visible h1.
 */
import { BRAND_BUTTON, LINE_BUTTON } from '~/templates/aurora/buttons'

/** Translator for the active locale. */
const { t } = useI18n()
/** Prefixes a path with the active locale. */
const localePath = useLocalePath()

/** Headline figures; the numbers are marketing copy, only their captions are translated. */
const stats = computed(() => [
  { value: '2 400+', label: t('aurora.hero.statDomains') },
  { value: '180+', label: t('aurora.hero.statProjects') },
  { value: '24/7', label: t('aurora.hero.statSupport') },
])
</script>
