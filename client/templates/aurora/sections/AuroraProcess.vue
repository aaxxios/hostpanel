<template>
  <UiSection id="process" padding="lg" :contained="false" class="relative border-t border-line">
    <UiSplit ratio="equal" gap="xl">
      <template #start>
        <UiText tone="inherit" size="inherit" :class="[EYEBROW_CLASS, 'text-ac1']">{{ t('aurora.process.eyebrow') }}</UiText>
        <UiHeading :level="2" size="bare" :class="[SECTION_TITLE_CLASS, 'mt-3']">
          {{ t('aurora.process.title') }}
        </UiHeading>
        <UiText tone="inherit" class="mt-[18px] text-[17px] leading-relaxed text-mut">{{ t('aurora.process.lead') }}</UiText>
      </template>

      <template #end>
        <UiStack gap="md">
          <UiCard
            v-for="step in steps"
            :key="step.n"
            tone="bare"
            padding="md"
            :data-testid="`process-step-${step.n}`"
            class="border-line bg-surf"
          >
            <UiStack direction="row" align="start" gap="md">
              <UiText
                as="span"
                size="inherit"
                tone="inherit"
                class="w-[46px] shrink-0 bg-hero-grad bg-clip-text font-mono text-[26px] font-medium text-transparent"
              >
                {{ step.n }}
              </UiText>
              <UiStack gap="none" class="min-w-0">
                <UiText as="span" size="inherit" tone="inherit" weight="bold" class="text-[17px] text-tx">{{ step.title }}</UiText>
                <UiText as="span" size="inherit" tone="inherit" class="mt-1.5 text-[15px] leading-relaxed text-mut">{{ step.body }}</UiText>
              </UiStack>
            </UiStack>
          </UiCard>
        </UiStack>
      </template>
    </UiSplit>
  </UiSection>
</template>

<script setup lang="ts">
/** aurora process section — four numbered steps. */
import { EYEBROW_CLASS, SECTION_TITLE_CLASS } from '~/templates/aurora/typography'

/** Translator for the active locale. */
const { t } = useI18n()

/** The four steps with their numbers; the numbers are ordinals, not copy. */
const steps = computed(() => [
  { n: '01', title: t('aurora.process.step1Title'), body: t('aurora.process.step1Body') },
  { n: '02', title: t('aurora.process.step2Title'), body: t('aurora.process.step2Body') },
  { n: '03', title: t('aurora.process.step3Title'), body: t('aurora.process.step3Body') },
  { n: '04', title: t('aurora.process.step4Title'), body: t('aurora.process.step4Body') },
])
</script>
