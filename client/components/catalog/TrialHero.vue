<template>
  <UiAccent :color="color">
    <section class="relative py-20 md:py-32 overflow-hidden" data-testid="trial-hero">
      <UiDecor class="inset-0">
        <UiDecor :class="[accent.tintStrong, 'top-0 right-1/4 w-[500px] h-[500px] rounded-full blur-[140px] animate-blob']" />
        <UiDecor class="bottom-0 left-1/3 w-[400px] h-[400px] bg-cyan-500/10 rounded-full blur-[130px] animate-blob animation-delay-2000" />
      </UiDecor>

      <UiContainer class="relative z-10 text-center">
        <UiAccentTile :icon="icon" size="xl" class="mx-auto mb-6" />

        <span :class="[accent.borderSoft, accent.tint, 'inline-flex items-center gap-2 px-4 py-2 mb-6 rounded-full border backdrop-blur-sm']">
          <Zap :size="14" :stroke-width="2" :class="accent.text" />
          <UiText as="span" size="sm" weight="medium" tone="muted">{{ $t('trial.badge') }}</UiText>
        </span>

        <UiHeading :level="1" size="display" class="mb-4 leading-tight" data-testid="trial-title">
          {{ $t('trial.title', { product: name }) }}
        </UiHeading>
        <UiText tone="muted" size="lg" class="mb-10 max-w-2xl mx-auto">{{ $t('trial.subtitle') }}</UiText>

        <div class="flex flex-col sm:flex-row items-center justify-center gap-4">
          <UiAccentButton :href="trialUrl" size="lg" class="px-8 py-4 font-bold" data-testid="trial-cta">
            <Rocket :size="20" :stroke-width="2" />
            {{ $t('trial.cta') }}
          </UiAccentButton>

          <UiAccentButton appearance="outline" size="lg" :to="localePath(learnMoreUrl)" class="px-8 py-4" data-testid="trial-learn-more">
            {{ $t('trial.learnMore') }}
          </UiAccentButton>
        </div>

        <UiText size="sm" tone="subtle" class="mt-4 flex items-center justify-center gap-2">
          <ShieldCheck :size="16" :stroke-width="2" class="text-green-600 dark:text-green-400" />
          {{ $t('trial.noCc') }}
        </UiText>
      </UiContainer>
    </section>
  </UiAccent>
</template>

<script setup lang="ts">
/**
 * Hero of the trial page: the product's mark, a badge, the title, the trial and learn-more calls
 * to action and the "no credit card" note, all painted in the product's own accent colour.
 */
import { Rocket, ShieldCheck, Zap } from 'lucide-vue-next'
import { ACCENT_CLASSES } from '~/utils/ui/accent'

/** Props of TrialHero. */
interface Props {
  /** Display name of the product. */
  name: string
  /** Full icon name of the product, e.g. `mdi:server`. */
  icon: string
  /** Accent colour of the product, as a hex string. */
  color: string
  /** Where the trial call to action leads. */
  trialUrl: string
  /** The product's catalogue page (the learn-more link, before localisation). */
  learnMoreUrl: string
}

defineProps<Props>()

/** Prefixes a path with the active language. */
const localePath = useLocalePath()

/** Accent classes the hero reads (the colour itself is set by the surrounding UiAccent). */
const accent = ACCENT_CLASSES
</script>
