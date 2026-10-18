<template>
  <UiSection id="faq" padding="lg" :contained="false" class="relative border-t border-line">
    <UiSplit ratio="equal" gap="xl">
      <template #start>
        <UiHeading :level="2" size="bare" :class="BLOCK_TITLE_CLASS">
          {{ t('aurora.faq.title') }}
        </UiHeading>
      </template>
      <template #end>
        <!-- Answers stay in the DOM: this is indexable content and a crawler should read every one. -->
        <UiAccordion :items="entries" keep-content :default-open="[0]" data-testid="aurora-faq" />
      </template>
    </UiSplit>
  </UiSection>
</template>

<script setup lang="ts">
/** aurora FAQ accordion. Answers stay in the DOM so they remain indexable. */
import { BLOCK_TITLE_CLASS } from '~/templates/aurora/typography'

/** One question and its answer, read from the locale messages. */
interface FaqItem {
  /** The question. */
  q: string
  /** The answer. */
  a: string
}

/** Translator for the active locale. */
const { t, tm, rt } = useI18n()

/** Questions and answers of the active locale. */
const items = computed<FaqItem[]>(() => {
  const raw = tm('aurora.faq.items') as unknown[]
  if (!Array.isArray(raw)) {
    return []
  }
  return raw.map((entry) => {
    const row = entry as Record<string, unknown>
    return { q: rt(row.q as string), a: rt(row.a as string) }
  })
})

/** The same entries in the shape the accordion takes. */
const entries = computed(() => items.value.map(item => ({ title: item.q, content: item.a })))
</script>
