<script setup lang="ts">
/**
 * "Nothing here" block for an empty list or an unconfigured area.
 *
 * `icon` slot above the title, `actions` slot below the description for the call to action.
 */
import UiHeading from '@/components/ui/UiHeading.vue'
import UiText from '@/components/ui/UiText.vue'

/** Props for UiEmptyState. */
defineProps<{
  /** Headline (already translated). */
  title: string
  /** Explanation under the headline. */
  description?: string
}>()
</script>

<template>
  <div class="flex flex-col items-center justify-center gap-3 py-12 text-center">
    <div class="text-text-secondary">
      <slot name="icon" />
    </div>
    <UiHeading :level="3" variant="card">
      {{ title }}
    </UiHeading>
    <UiText v-if="description" tone="secondary" size="md" class="max-w-md">
      {{ description }}
    </UiText>
    <div class="flex items-center gap-2 mt-1">
      <slot name="actions" />
    </div>
  </div>
</template>
