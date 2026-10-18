<template>
  <section class="py-20 bg-ui-page relative overflow-hidden" data-testid="sections-why-choose-us">
    <!-- Background -->
    <UiDecor class="inset-0">
      <UiDecor class="inset-0 bg-gradient-to-br from-primary-500/5 via-transparent to-cyan-500/5" />
      <UiDecor class="top-1/2 left-0 w-1/3 h-1/2 bg-primary-500/10 rounded-full blur-[150px]" />
      <UiDecor class="bottom-0 right-0 w-1/4 h-1/3 bg-primary-500/10 dark:bg-secondary-500/10 rounded-full blur-[120px]" />
      <UiDecor class="top-1/4 right-1/4 w-2 h-2 bg-primary-400/50 rounded-full animate-float [animation-delay:0.5s]" />
      <UiDecor class="top-1/3 left-1/4 w-3 h-3 bg-primary-400/40 dark:bg-secondary-400/40 rounded-full animate-float [animation-delay:1.2s]" />
      <UiDecor class="bottom-1/3 right-1/3 w-2 h-2 bg-cyan-400/50 rounded-full animate-float [animation-delay:2s]" />
      <UiDecor class="inset-0 opacity-[0.015] pattern-grid-50" />
    </UiDecor>

    <UiContainer class="relative z-10">
      <MarketingSectionHeader class="mb-16" :badge="$t('whyChooseUs.badge')" :subtitle="$t('whyChooseUs.subtitle')">
        <template #title>
          <i18n-t keypath="whyChooseUs.title" tag="span">
            <template #results>
              <span class="text-primary-600 dark:text-primary-400">{{ $t('whyChooseUs.titleHighlight') }}</span>
            </template>
          </i18n-t>
        </template>
      </MarketingSectionHeader>

      <!-- Advantages -->
      <UiGrid cols="2" gap="lg" class="lg:grid-cols-4">
        <UiCard
          v-for="(advantage, index) in advantages"
          :key="advantage.key"
          v-motion
          :initial="{ opacity: 0, y: 40 }"
          :visible-once="{ opacity: 1, y: 0, transition: { delay: index * 100, duration: 500 } }"
          class="relative flex flex-col items-center text-center group hover:border-primary-500/30 hover:-translate-y-2 transition-all duration-300"
          :data-testid="`advantage-${advantage.key}`"
        >
          <!-- Hover glow -->
          <UiDecor class="inset-0 rounded-2xl opacity-0 group-hover:opacity-100 transition-opacity duration-500 bg-gradient-to-br from-primary-500/10 to-cyan-500/5" />

          <!-- Icon -->
          <div class="relative w-16 h-16 rounded-xl bg-gradient-to-br from-primary-500/20 to-cyan-500/10 flex items-center justify-center mb-4 group-hover:scale-110 transition-transform duration-300">
            <UiDecor class="inset-0 rounded-xl border border-primary-500/30 opacity-0 group-hover:opacity-100 group-hover:scale-125 transition-all duration-500" />
            <Icon :name="advantage.icon" class="relative text-3xl text-primary-600 dark:text-primary-400 group-hover:rotate-12 transition-transform duration-300" />
          </div>

          <UiHeading :level="3" size="subsection" class="relative mb-2">{{ advantage.title }}</UiHeading>
          <UiText tone="muted" size="sm" class="relative leading-relaxed">{{ advantage.description }}</UiText>

          <UiDecor class="bottom-0 left-1/2 -translate-x-1/2 h-1 w-0 group-hover:w-16 bg-gradient-to-r from-primary-500 to-cyan-500 rounded-full transition-all duration-500" />
          <UiDecor class="top-0 left-0 w-10 h-10 border-l-2 border-t-2 border-primary-500/50 rounded-tl-xl opacity-0 group-hover:opacity-100 transition-opacity duration-300" />
        </UiCard>
      </UiGrid>
    </UiContainer>

    <!-- Section decorative corners -->
    <UiDecor class="top-0 right-0 w-32 h-32 border-r-2 border-t-2 border-primary-500/25 dark:border-secondary-500/25" />
    <UiDecor class="bottom-0 left-0 w-32 h-32 border-l-2 border-b-2 border-primary-500/25" />
  </section>
</template>

<script setup lang="ts">
/**
 * Why Choose Us section highlighting the company's four advantages.
 */
const { t } = useI18n()

/** The four advantages, each with the icon that stands for it and its translated copy. */
const advantages = computed(() => [
  {
    key: 'fastDelivery',
    icon: 'mdi:rocket-launch',
    title: t('whyChooseUs.advantages.fastDelivery.title'),
    description: t('whyChooseUs.advantages.fastDelivery.description'),
  },
  {
    key: 'qualityGuaranteed',
    icon: 'mdi:shield-check',
    title: t('whyChooseUs.advantages.qualityGuaranteed.title'),
    description: t('whyChooseUs.advantages.qualityGuaranteed.description'),
  },
  {
    key: 'support',
    icon: 'mdi:headset',
    title: t('whyChooseUs.advantages.support.title'),
    description: t('whyChooseUs.advantages.support.description'),
  },
  {
    key: 'provenResults',
    icon: 'mdi:trending-up',
    title: t('whyChooseUs.advantages.provenResults.title'),
    description: t('whyChooseUs.advantages.provenResults.description'),
  },
])
</script>
