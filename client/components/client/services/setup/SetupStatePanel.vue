<template>
  <UiCard padding="xl" class="text-center" data-testid="setup-state">
    <UiStack align="center" gap="md">
      <Loader2 v-if="loading" :size="48" :stroke-width="2" class="text-primary-500 animate-spin" />
      <UiTile v-else-if="icon" :icon="icon" :tone="tone" size="xl" />
      <UiHeading :level="2" size="section">{{ title }}</UiHeading>
      <UiText tone="muted">{{ description }}</UiText>
      <slot />
    </UiStack>
  </UiCard>
</template>

<script setup lang="ts">
/**
 * SetupStatePanel - the centred card the setup page shows instead of the wizard: loading, the
 * service cannot be set up, or it is already set up. An icon (or a spinner), a headline, a
 * sentence and an optional action in the default slot.
 */
import type { Component } from 'vue'
import { Loader2 } from 'lucide-vue-next'
import { UiTone } from '~/utils/ui/enums'

/** Props of SetupStatePanel. */
interface Props {
  /** Headline. */
  title: string
  /** One supporting sentence. */
  description: string
  /** A lucide icon component, shown unless `loading`. */
  icon?: Component
  /** Colour of the icon tile. */
  tone?: `${UiTone}`
  /** Shows a spinner in place of the icon. */
  loading?: boolean
}

withDefaults(defineProps<Props>(), {
  icon: undefined,
  tone: UiTone.Primary,
  loading: false,
})
</script>
