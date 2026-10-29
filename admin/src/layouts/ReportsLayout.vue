<script setup lang="ts">
/**
 * Shell of the reports area: the report list as an inner sidebar and the chosen report beside it.
 */
import RoutedView from '@/components/layout/RoutedView.vue'
import ReportsInnerSidebar from '@/components/reports/ReportsInnerSidebar.vue'
import UiSidebarLayout from '@/components/ui/UiSidebarLayout.vue'
</script>

<template>
  <UiSidebarLayout data-testid="reports-layout">
    <template #sidebar>
      <ReportsInnerSidebar />
    </template>
    <RoutedView />
  </UiSidebarLayout>
</template>
