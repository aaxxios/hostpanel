<template>
  <section class="py-8 md:py-20 bg-ui-page relative overflow-hidden" data-testid="products-benefits">
    <UiDecor class="inset-0">
      <UiDecor class="top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[700px] h-[500px] bg-primary-500/15 dark:bg-secondary-500/15 rounded-full blur-[180px]" />
      <UiDecor class="top-20 left-1/4 w-2 h-2 bg-primary-400/60 dark:bg-secondary-400/60 rounded-full animate-float [animation-delay:0.7s]" />
      <UiDecor class="bottom-1/4 right-1/3 w-3 h-3 bg-cyan-400/50 rounded-full animate-float [animation-delay:1.6s]" />
    </UiDecor>

    <UiContainer class="relative z-10">
      <MarketingSectionHeader class="mb-16" :subtitle="$t('products.benefits.subtitle')">
        <template #title>
          <i18n-t keypath="products.benefits.title" tag="span">
            <template #benefitsHighlight>
              <span class="bg-gradient-to-r from-primary-400 dark:from-secondary-400 to-cyan-400 bg-clip-text text-transparent">{{ $t('products.benefits.benefitsHighlight') }}</span>
            </template>
          </i18n-t>
        </template>
      </MarketingSectionHeader>

      <!-- Benefits -->
      <UiGrid cols="2" gap="lg" class="mb-16 lg:grid-cols-4">
        <UiCard
          v-for="(benefit, index) in benefits"
          :key="benefit.key"
          v-motion
          :initial="{ opacity: 0, y: 40 }"
          :visible-once="{ opacity: 1, y: 0, transition: { delay: index * 100, duration: 500 } }"
          padding="xl"
          class="relative group h-full text-center hover:border-primary-500/40 dark:hover:border-secondary-500/40 hover:-translate-y-2 transition-all duration-300"
          :data-testid="`benefit-${benefit.key}`"
        >
          <UiDecor class="inset-0 rounded-2xl opacity-0 group-hover:opacity-100 transition-opacity duration-500 bg-gradient-to-br from-primary-500/10 dark:from-secondary-500/10 to-cyan-500/5" />

          <span class="relative inline-flex w-16 h-16 rounded-2xl bg-gradient-to-br from-primary-500/25 dark:from-secondary-500/25 to-cyan-500/15 items-center justify-center mb-4 group-hover:scale-110 group-hover:rotate-6 transition-all duration-300 border-2 border-primary-500/30 dark:border-secondary-500/30 shadow-xl shadow-primary-500/20">
            <Icon :name="benefit.icon" class="text-3xl text-primary-700 dark:text-secondary-400" />
          </span>

          <UiHeading :level="3" class="relative text-lg mb-3">{{ benefit.title }}</UiHeading>
          <UiText tone="muted" size="sm" class="relative leading-relaxed">{{ benefit.description }}</UiText>

          <UiDecor class="bottom-0 left-1/2 -translate-x-1/2 h-1 w-0 group-hover:w-24 bg-gradient-to-r from-primary-500 dark:from-secondary-500 to-cyan-500 transition-all duration-500 rounded-full" />
        </UiCard>
      </UiGrid>

      <!-- Reassurances -->
      <UiGrid cols="1" gap="lg" class="max-w-4xl mx-auto md:grid-cols-3">
        <UiCard
          v-for="(item, index) in reassurances"
          :key="item.key"
          v-motion
          :initial="{ opacity: 0, y: 30 }"
          :visible-once="{ opacity: 1, y: 0, transition: { delay: (index + 1) * 100, duration: 400 } }"
          padding="md"
          :class="[item.hoverBorder, 'group text-center transition-colors']"
          :data-testid="`reassurance-${item.key}`"
        >
          <component :is="item.icon" :size="32" :stroke-width="2" :class="[item.iconColor, 'mx-auto mb-3 group-hover:scale-110 transition-transform']" />
          <UiText size="sm" weight="semibold" class="mb-1">{{ $t(`products.benefits.additional.${item.key}.title`) }}</UiText>
          <UiText size="xs" tone="subtle">{{ $t(`products.benefits.additional.${item.key}.description`) }}</UiText>
        </UiCard>
      </UiGrid>
    </UiContainer>

    <!-- Corner decorations -->
    <UiDecor class="top-0 right-0 w-48 h-48 border-r-2 border-t-2 border-cyan-500/25" />
    <UiDecor class="bottom-0 left-0 w-48 h-48 border-l-2 border-b-2 border-primary-500/25 dark:border-secondary-500/25" />
  </section>
</template>

<script setup lang="ts">
/**
 * The benefits block of the products page: four platform benefits and three reassurances (free
 * trial, guarantee, lifetime updates).
 */
import { CheckCircle, Infinity as InfinityIcon, ShieldCheck } from 'lucide-vue-next'

/** Translation helpers and the active language. */
const { t } = useI18n()

/** Operator-set site name that the copy interpolates as `{site}`. */
const { name: siteName } = useSiteIdentity()

/** The four platform benefits, in display order. */
const benefits = computed(() => [
  { key: 'whiteLabel', icon: 'mdi:palette', title: t('products.benefits.whiteLabel.title'), description: t('products.benefits.whiteLabel.description', { site: siteName.value }) },
  { key: 'support', icon: 'mdi:headset', title: t('products.benefits.support.title'), description: t('products.benefits.support.description') },
  { key: 'updates', icon: 'mdi:refresh', title: t('products.benefits.updates.title'), description: t('products.benefits.updates.description') },
  { key: 'api', icon: 'mdi:code-tags', title: t('products.benefits.api.title'), description: t('products.benefits.api.description') },
])

/** The three reassurances under the benefits; each names the `products.benefits.additional.*` group it reads. */
const reassurances = [
  { key: 'trial', icon: CheckCircle, iconColor: 'text-green-600 dark:text-green-400', hoverBorder: 'hover:border-green-500/30' },
  { key: 'guarantee', icon: ShieldCheck, iconColor: 'text-blue-600 dark:text-blue-400', hoverBorder: 'hover:border-blue-500/30' },
  { key: 'lifetime', icon: InfinityIcon, iconColor: 'text-blue-600 dark:text-purple-400', hoverBorder: 'hover:border-purple-500/30' },
]
</script>
