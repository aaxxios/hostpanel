<script setup lang="ts">
/**
 * Plugin Manager page.
 *
 * Lists installed plugins, provides ZIP upload for installation,
 * and shows a restart banner when pending changes need to take effect.
 */
import { onMounted, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { usePluginsStore } from '@/stores/plugins'
import PluginCard from '@/components/plugins/PluginCard.vue'
import PluginUploader from '@/components/plugins/PluginUploader.vue'
import RestartBanner from '@/components/plugins/RestartBanner.vue'
import UiAlert from '@/components/ui/UiAlert.vue'
import UiEmptyState from '@/components/ui/UiEmptyState.vue'
import UiHeading from '@/components/ui/UiHeading.vue'
import UiPageHeader from '@/components/ui/UiPageHeader.vue'
import UiSkeleton from '@/components/ui/UiSkeleton.vue'
import UiStack from '@/components/ui/UiStack.vue'

/** Translation function. */
const { t } = useI18n()

/** Plugins store: the installed list, the restart flag and the refusal sentence. */
const store = usePluginsStore()

/** Id of the plugin whose removal is in flight. */
const removingId = ref<string | null>(null)

/** True while the server restart request is in flight. */
const restarting = ref(false)

/**
 * Removes a plugin.
 *
 * @param id - Id of the plugin to remove.
 */
const handleRemove = async (id: string): Promise<void> => {
  removingId.value = id
  await store.remove(id)
  removingId.value = null
}

/** Restarts the server so installed or removed plugins take effect. */
const handleRestart = async (): Promise<void> => {
  restarting.value = true
  await store.restart()
  restarting.value = false
}

/**
 * Installs an uploaded plugin archive.
 *
 * @param file - The chosen `.zip` archive.
 */
const handleUpload = async (file: File): Promise<void> => {
  await store.install(file)
}

/** Loads the installed plugins. */
onMounted(store.fetchAll)
</script>

<template>
  <UiStack gap="lg" class="w-full">
    <UiPageHeader
      :title="t('plugins.plugins.pluginManager')"
      :subtitle="store.plugins.length > 0
        ? `${t('plugins.plugins.installAndManageProviderPlugins')} ${t('plugins.plugins.lengthInstalled', { length: store.plugins.length })}`
        : t('plugins.plugins.installAndManageProviderPlugins')"
    />

    <!-- Restart banner -->
    <RestartBanner v-if="store.requiresRestart" :restarting="restarting" @restart="handleRestart" />

    <!-- Upload -->
    <PluginUploader @upload="handleUpload" />

    <!-- Error -->
    <UiAlert v-if="store.error" variant="error" data-testid="plugins-error">
      {{ store.error }}
    </UiAlert>

    <!-- Loading -->
    <UiSkeleton v-if="store.loading && store.plugins.length === 0" :lines="3" data-testid="plugins-loading" />

    <!-- Empty state -->
    <UiEmptyState
      v-else-if="!store.loading && store.plugins.length === 0"
      :title="t('plugins.plugins.noPluginsInstalledYet')"
      :description="`${t('plugins.plugins.uploadA')} ${t('plugins.plugins.zip')} ${t('plugins.plugins.fileAboveToGetStarted')}`"
      data-testid="plugins-empty"
    />

    <!-- Plugin list -->
    <UiStack v-else gap="sm" data-testid="plugins-list">
      <UiHeading :level="2" variant="section">{{ t('plugins.plugins.installedPlugins') }}</UiHeading>
      <PluginCard
        v-for="plugin in store.plugins"
        :key="plugin.id"
        :plugin="plugin"
        :removing="removingId === plugin.id"
        @remove="handleRemove"
      />
    </UiStack>
  </UiStack>
</template>
