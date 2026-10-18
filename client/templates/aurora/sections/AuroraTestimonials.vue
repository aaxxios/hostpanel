<template>
  <UiSection padding="lg" :contained="false" class="relative border-t border-line">
    <UiStack direction="row" wrap align="center" justify="between">
      <UiStack gap="none">
        <UiText tone="inherit" size="inherit" :class="[EYEBROW_CLASS, 'text-ac3']">{{ t('aurora.testimonials.eyebrow') }}</UiText>
        <UiHeading :level="2" size="bare" :class="[SECTION_TITLE_CLASS, 'mt-3']">
          {{ t('aurora.testimonials.title') }}
        </UiHeading>
      </UiStack>
      <UiStack direction="row" gap="xs">
        <UiButton
          variant="bare"
          :aria-label="t('aurora.testimonials.prev')"
          data-testid="testimonials-prev"
          class="h-[46px] w-[46px] rounded-full border border-line2 text-tx hover:border-ac1 hover:text-ac1"
          @click="go(-1)"
        >
          <ArrowLeft :size="18" />
        </UiButton>
        <UiButton
          variant="bare"
          :aria-label="t('aurora.testimonials.next')"
          data-testid="testimonials-next"
          class="h-[46px] w-[46px] rounded-full border border-line2 text-tx hover:border-ac1 hover:text-ac1"
          @click="go(1)"
        >
          <ArrowRight :size="18" />
        </UiButton>
      </UiStack>
    </UiStack>

    <UiCard
      v-if="current"
      tone="bare"
      padding="none"
      data-testid="testimonial-card"
      class="mt-9 min-h-[260px] border-line2 bg-card-hi p-[clamp(24px,3.5vw,44px)]"
    >
      <UiText
        as="blockquote"
        size="inherit"
        tone="inherit"
        class="max-w-[900px] font-display text-[clamp(19px,2.4vw,26px)] leading-snug -tracking-[0.01em] text-tx"
      >
        «{{ current.quote }}»
      </UiText>
      <UiStack direction="row" align="center" gap="md" class="mt-[30px]">
        <UiTile size="lg" rounded class="bg-brand font-bold text-on-tint">{{ current.author.charAt(0) }}</UiTile>
        <UiStack gap="none">
          <UiText as="span" tone="inherit" weight="semibold" class="text-tx">{{ current.author }}</UiText>
          <UiText as="span" size="sm" tone="inherit" class="text-mut2">{{ current.role }}</UiText>
        </UiStack>
      </UiStack>
      <UiStack direction="row" gap="xs" class="mt-[30px]">
        <UiButton
          v-for="(item, i) in items"
          :key="item.author"
          variant="bare"
          :aria-label="item.author"
          :data-testid="`testimonials-dot-${i}`"
          class="h-1.5 rounded-full p-0"
          :class="i === index ? 'w-[34px] bg-ac1' : 'w-3 bg-line2'"
          @click="index = i"
        />
      </UiStack>
    </UiCard>
  </UiSection>
</template>

<script setup lang="ts">
/** aurora testimonials carousel. Quotes are editorial copy — no API exists for them. */
import { ArrowLeft, ArrowRight } from 'lucide-vue-next'
import { EYEBROW_CLASS, SECTION_TITLE_CLASS } from '~/templates/aurora/typography'

/** One quote with its author, read from the locale messages. */
interface Testimonial {
  /** What the customer said. */
  quote: string
  /** Who said it. */
  author: string
  /** The author's role or company. */
  role: string
}

/** Translator for the active locale. */
const { t, tm, rt } = useI18n()

/** Quotes of the active locale. */
const items = computed<Testimonial[]>(() => {
  const raw = tm('aurora.testimonials.items') as unknown[]
  if (!Array.isArray(raw)) {
    return []
  }
  return raw.map((entry) => {
    const row = entry as Record<string, unknown>
    return {
      quote: rt(row.quote as string),
      author: rt(row.author as string),
      role: rt(row.role as string),
    }
  })
})

/** Index of the quote on show. */
const index = ref(0)

/** The quote on show; undefined when the locale has none. */
const current = computed(() => items.value[index.value])

/**
 * Moves the carousel, wrapping at both ends.
 *
 * @param step Positions to advance; negative moves back.
 */
const go = (step: number): void => {
  const total = items.value.length
  if (!total) {
    return
  }
  index.value = (index.value + step + total) % total
}
</script>
