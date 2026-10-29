<script setup lang="ts">
/**
 * Displays a single installed plugin with its status and a remove button; a plugin shipped with
 * the server build is marked built-in and has no remove button, since the server refuses it.
 */
import { useI18n } from 'vue-i18n'
import type { PluginListItemDto } from '@/types/plugin'
import UiBadge from '@/components/ui/UiBadge.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiText from '@/components/ui/UiText.vue'
import { UiTone } from '@/utils/ui/tone'

/** Translation function. */
const { t } = useI18n()

/** Props for PluginCard. */
defineProps<{
  /** The installed plugin to show. */
  plugin: PluginListItemDto
  /** True while this plugin's removal is in flight. */
  removing: boolean
}>()

/** Events emitted by PluginCard. */
const emit = defineEmits<{
  /** Emitted with the plugin id when the admin asks to remove it. */
  remove: [id: string]
}>()
</script>

<template>
  <UiCard padding="sm" :data-testid="`plugin-card-${plugin.id}`">
    <UiStack direction="row" align="center" gap="md">
      <UiStack gap="xs" class="min-w-0 flex-1">
        <UiStack direction="row" align="center" gap="sm" wrap>
          <UiText tone="primary" size="lg" class="font-semibold">{{ plugin.name }}</UiText>
          <UiText tone="muted" size="xs" class="font-mono">v{{ plugin.version }}</UiText>
          <UiBadge :tone="plugin.isLoaded ? UiTone.Success : UiTone.Warning" small>
            {{ plugin.isLoaded ? t('shared.active') : t('plugins.pluginCard.pendingRestart') }}
          </UiBadge>
          <UiBadge v-if="plugin.isBuiltIn" :tone="UiTone.Info" small :data-testid="`plugin-builtin-${plugin.id}`">
            {{ t('plugins.pluginCard.builtIn') }}
          </UiBadge>
        </UiStack>
        <UiText tone="muted" size="sm" class="truncate">{{ plugin.description }}</UiText>
        <UiText tone="muted" size="xs">
          {{ t('plugins.pluginCard.categoryByAuthor', { category: plugin.category, author: plugin.author }) }}
        </UiText>
      </UiStack>

      <UiButton
        v-if="!plugin.isBuiltIn"
        type="button"
        variant="danger"
        size="sm"
        :disabled="removing"
        :data-testid="`plugin-remove-${plugin.id}`"
        @click="emit('remove', plugin.id)"
      >
        {{ removing ? t('plugins.pluginCard.removing') : t('plugins.pluginCard.remove') }}
      </UiButton>
    </UiStack>
  </UiCard>
</template>
