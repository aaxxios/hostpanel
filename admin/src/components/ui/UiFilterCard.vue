<script setup lang="ts">
/**
 * Filter card wrapper with named slots for the filter sections (`fields`, default, `dates`,
 * `actions`), so every list screen frames its filters the same way.
 */
import UiCard from '@/components/ui/UiCard.vue'
</script>

<template>
  <UiCard class="p-4 mb-6 space-y-4">
    <slot name="fields" />
    <slot />
    <slot name="dates" />
    <slot name="actions" />
  </UiCard>
</template>
