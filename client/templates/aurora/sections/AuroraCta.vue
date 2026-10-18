<template>
  <UiCard
    tone="bare"
    padding="none"
    data-testid="aurora-cta"
    class="relative mb-[clamp(56px,8vw,84px)] border-line2 bg-card-hi px-[clamp(22px,4vw,64px)] py-[clamp(36px,5vw,64px)] text-center"
  >
    <UiHeading :level="2" size="bare" class="font-display text-[clamp(28px,4.4vw,42px)] font-bold -tracking-[0.02em] text-tx">
      {{ t('aurora.cta.title') }}
    </UiHeading>
    <UiText size="inherit" tone="inherit" class="mx-auto mt-4 max-w-[560px] text-[18px] leading-relaxed text-tx2">
      {{ t('aurora.cta.lead') }}
    </UiText>
    <UiStack direction="row" wrap justify="center" class="mt-8">
      <UiButton
        variant="bare"
        :to="localePath('/contact')"
        data-testid="cta-primary"
        :class="[BRAND_BUTTON, 'px-7 py-[15px] text-base']"
      >
        {{ t('aurora.cta.primary') }}
      </UiButton>
      <UiButton
        variant="bare"
        :to="localePath('/hosting')"
        data-testid="cta-secondary"
        :class="[LINE_BUTTON, 'px-7 py-[15px] text-base']"
      >
        {{ t('aurora.cta.secondary') }}
      </UiButton>
    </UiStack>
  </UiCard>
</template>

<script setup lang="ts">
/** aurora closing call to action. */
import { BRAND_BUTTON, LINE_BUTTON } from '~/templates/aurora/buttons'

/** Translator for the active locale. */
const { t } = useI18n()
/** Prefixes a path with the active locale. */
const localePath = useLocalePath()
</script>
