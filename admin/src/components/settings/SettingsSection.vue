<script setup lang="ts">
/**
 * A titled card that groups one part of a settings screen or form.
 *
 * The title and the optional description sit on top, the default slot carries the controls. Every
 * card on the system-settings page and every section of the slide and TLD forms is one of these,
 * so the spacing and type are defined once.
 */
import UiCard from '@/components/ui/UiCard.vue'
import UiHeading from '@/components/ui/UiHeading.vue'
import UiText from '@/components/ui/UiText.vue'

/** Props for SettingsSection. */
defineProps<{
  /** Section title (already translated). */
  title: string
  /** Line under the title (already translated). */
  description?: string
}>()
</script>

<template>
  <UiCard padding="md" class="mb-5">
    <UiHeading :level="2" variant="section">
      {{ title }}
    </UiHeading>
    <UiText v-if="description" tone="secondary" size="lg" class="mt-1">
      {{ description }}
    </UiText>
    <div class="mt-4 flex flex-col gap-4">
      <slot />
    </div>
  </UiCard>
</template>
