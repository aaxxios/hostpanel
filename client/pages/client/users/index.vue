<template>
  <UiSkeleton shape="block" class="h-32" />
</template>

<script setup lang="ts">
/**
 * Users entry point: the list lives in the account page's users tab, so this route only forwards
 * there.
 */
definePageMeta({ layout: 'client', middleware: 'client-auth' })
await navigateTo('/client/account?tab=users', { replace: true })
</script>
