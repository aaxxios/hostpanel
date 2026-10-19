<template>
  <UiSkeleton shape="block" class="h-32" />
</template>

<script setup lang="ts">
/**
 * Profile entry point: the details live in the account page's details tab, so this route only
 * forwards there.
 */
import { clientAccountTabPath } from '~/constants/clientRoutes'
import { AccountTab } from '~/types/accounttab'

definePageMeta({ layout: 'client', middleware: 'client-auth' })
await navigateTo(clientAccountTabPath(AccountTab.Details), { replace: true })
</script>
