<template>
  <NuxtLink
    :to="to"
    class="flex items-center justify-between gap-3 rounded-xl bg-gray-50 p-3 transition-colors hover:bg-gray-100 dark:bg-white/5 dark:hover:bg-white/10"
    :data-testid="testid"
  >
    <slot />
  </NuxtLink>
</template>

<script setup lang="ts">
/**
 * One clickable row of a dashboard list (a service, an invoice, a domain or a ticket): the
 * content on the left and its status on the right, laid out by the slot.
 */

/** Props of ClientDashboardListRow. */
interface Props {
  /** Route the row opens. */
  to: string
  /** `data-testid` of the row. */
  testid?: string
}

withDefaults(defineProps<Props>(), {
  testid: undefined
})
</script>
