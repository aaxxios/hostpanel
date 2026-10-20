<template>
  <section class="relative overflow-hidden py-12 md:py-24" data-testid="ui-page-hero">
    <UiBackdrop variant="aurora" />
    <div
      aria-hidden="true"
      class="pointer-events-none absolute right-0 top-0 h-48 w-48 border-r-2 border-t-2 border-primary-500/30 dark:border-secondary-500/30"
    />
    <div
      aria-hidden="true"
      class="pointer-events-none absolute bottom-0 left-0 h-48 w-48 border-b-2 border-l-2 border-primary-500/30 dark:border-secondary-500/30"
    />

    <UiContainer class="relative z-10 text-center">
      <div
      class="animate-reveal-up motion-reduce:animate-none"
      >
        <slot name="breadcrumbs" />
        <UiBadge
          v-if="badge"
          variant="primary"
          rounded
          class="mb-6"
          data-testid="ui-page-hero-badge"
        >
          {{ badge }}
        </UiBadge>
        <UiHeading
          :level="1"
          size="display"
          class="mb-6 break-words"
          data-testid="ui-page-hero-title"
        >
          {{ title }}
          <UiGradientText
            v-if="highlight"
            class="bg-gradient-to-r from-primary-600 to-primary-600 dark:from-primary-400 dark:to-secondary-400"
          >
            {{ highlight }}
          </UiGradientText>
        </UiHeading>
        <UiText
          v-if="subtitle"
          size="lg"
          tone="muted"
          class="mx-auto max-w-3xl"
          data-testid="ui-page-hero-subtitle"
        >
          {{ subtitle }}
        </UiText>
        <slot />
      </div>
    </UiContainer>
  </section>
</template>

<script setup lang="ts">
/**
 * UiPageHero - the opening band of a public page: aurora backdrop, corner accents, an optional
 * badge, the h1 title (with an optional gradient-highlighted word), a lead sentence, and a
 * default slot for what belongs under them (category links, a "last updated" line).
 *
 * The `breadcrumbs` slot sits above the badge. The whole block fades in on first paint.
 */

/** Props of UiPageHero. */
interface Props {
  /** Small pill above the title. */
  badge?: string
  /** The page title, rendered as the h1. */
  title: string
  /** Word or phrase painted with the brand gradient after the title. */
  highlight?: string
  /** One lead sentence under the title. */
  subtitle?: string
}

withDefaults(defineProps<Props>(), {
  badge: undefined,
  highlight: undefined,
  subtitle: undefined,
})
</script>
