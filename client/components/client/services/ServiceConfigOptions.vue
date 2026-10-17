<template>
  <UiCard data-testid="service-config-options">
    <UiCardHeader :title="$t('client.services.configOptions')" :icon="SlidersHorizontal" />
    <UiStack gap="sm">
      <UiKeyValue v-for="option in options" :key="option.id" :label="option.option" :value="option.value" />
    </UiStack>
  </UiCard>
</template>

<script setup lang="ts">
/**
 * ServiceConfigOptions - the configurable options chosen when the service was ordered, one
 * name and value per line.
 */
import { SlidersHorizontal } from 'lucide-vue-next'

/** One chosen configurable option. */
interface ConfigOption {
  /** Option id, the list key. */
  id: number
  /** The option's name. */
  option: string
  /** The value chosen. */
  value: string
}

/** Props of ServiceConfigOptions. */
interface Props {
  /** The chosen options. */
  options: ConfigOption[]
}

defineProps<Props>()
</script>
