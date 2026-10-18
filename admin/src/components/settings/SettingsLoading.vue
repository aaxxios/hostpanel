<script setup lang="ts">
/**
 * "Loading" state of a settings list or page: a skeleton with a caption announced politely.
 *
 * The caption is the screen's own sentence ("Loading slides..."); the skeleton keeps the layout
 * from jumping when the data arrives.
 */
import UiSkeleton from '@/components/ui/UiSkeleton.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiText from '@/components/ui/UiText.vue'

/** Props for SettingsLoading. */
defineProps<{
  /** What is loading (already translated). */
  message: string
}>()
</script>

<template>
  <UiStack gap="sm" role="status" aria-busy="true" data-testid="settings-loading">
    <UiText tone="secondary" size="md">
      {{ message }}
    </UiText>
    <UiSkeleton :lines="3" />
  </UiStack>
</template>
