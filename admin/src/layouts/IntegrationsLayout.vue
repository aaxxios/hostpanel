<script setup lang="ts">
/**
 * Shell of the Integrations section: the category sidebar on the left, the routed page beside it.
 *
 * Built on the kit's sidebar layout, which cancels the app layout's gutter so the sidebar meets
 * the shell edge.
 */
import { RouterView } from 'vue-router'
import IntegrationsInnerSidebar from '@/components/integrations/IntegrationsInnerSidebar.vue'
import UiSidebarLayout from '@/components/ui/UiSidebarLayout.vue'
</script>

<template>
  <UiSidebarLayout data-testid="integrations-layout">
    <template #sidebar>
      <IntegrationsInnerSidebar />
    </template>
    <RouterView />
  </UiSidebarLayout>
</template>
