<script setup lang="ts">
/**
 * Header of a screen: title, optional subtitle, and an actions area on the right.
 *
 * The `actions` slot takes the screen's primary buttons; the default slot renders under the title
 * (breadcrumbs, filters summary).
 */
import UiHeading from '@/components/ui/UiHeading.vue'
import UiText from '@/components/ui/UiText.vue'

/** Props for UiPageHeader. */
defineProps<{
  /** Screen title (already translated). */
  title: string
  /** Line under the title. */
  subtitle?: string
}>()
</script>

<template>
  <header class="flex flex-wrap items-start justify-between gap-4 mb-6">
    <div class="min-w-0">
      <UiHeading :level="1" variant="page" class="mb-1.5">
        {{ title }}
      </UiHeading>
      <UiText v-if="subtitle" tone="secondary" size="md">
        {{ subtitle }}
      </UiText>
      <slot />
    </div>
    <div class="flex items-center gap-2 shrink-0">
      <slot name="actions" />
    </div>
  </header>
</template>
