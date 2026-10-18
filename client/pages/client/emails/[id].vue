<template>
  <UiStack gap="lg">
    <UiBackLink to="/client/emails" :label="$t('client.emails.backTo')" testid="email-back" />

    <UiStack v-if="pending">
      <UiSkeleton class="h-8 w-2/3" shape="block" />
      <UiSkeleton class="h-96" shape="block" />
    </UiStack>

    <UiEmptyState v-else-if="error || !email" :icon="AlertCircle" :title="$t('client.emails.notFound')" />

    <template v-else>
      <UiStack gap="xs">
        <UiHeading :level="1">{{ email.subject }}</UiHeading>
        <UiText size="sm" tone="subtle">{{ formatDateTime(email.date) }}</UiText>
      </UiStack>

      <UiCard padding="none">
        <ClientEmailsEmailFrame :html="email.message" :title="$t('client.emails.messageFrame')" />
      </UiCard>
    </template>
  </UiStack>
</template>

<script setup lang="ts">
/**
 * One email from the client's history: its subject and date and the message body, which is shown
 * in a sandboxed frame because it is untrusted markup. The message is read through the client
 * store.
 */
import { AlertCircle } from 'lucide-vue-next'
import { useClientStore } from '~/stores/client'
import { formatDateTime } from '~/utils/formatDate'

definePageMeta({ layout: 'client', middleware: 'client-auth' })

/** The current route; its params name the record shown. */
const route = useRoute()

/** The client store that owns the email and its failure sentence. */
const store = useClientStore()
/** Loading flag of the data this page reads. */
const { pending } = await useAsyncData(
  () => `client-email-${String(route.params.id)}`,
  () => store.loadEmail(String(route.params.id))
)

/** The message being read; null until loaded. */
const email = computed(() => store.email)

/** The sentence for a failed read. */
const error = computed(() => store.emailError)
</script>
