<template>
  <UiSkeleton shape="block" class="h-32" />
</template>

<script setup lang="ts">
/**
 * Users entry point: the list lives in the account page's users tab, so this route only forwards
 * there.
 */
import { clientAccountTabPath } from '~/constants/clientRoutes'
import { AccountTab } from '~/types/accounttab'

definePageMeta({ layout: 'client', middleware: 'client-auth' })
await navigateTo(clientAccountTabPath(AccountTab.Users), { replace: true })
</script>
