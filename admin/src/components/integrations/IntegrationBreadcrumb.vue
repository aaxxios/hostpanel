<script setup lang="ts">
/**
 * Breadcrumb of the integration pages: a link back to the integrations list, then the name of the
 * integration being configured.
 */
import { useI18n } from 'vue-i18n'
import UiLink from '@/components/ui/UiLink.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiText from '@/components/ui/UiText.vue'
import { AppRoute } from '@/lib/routes'

/** Translation function. */
const { t } = useI18n()

/** Props for IntegrationBreadcrumb. */
defineProps<{
  /** Name of the integration, already translated or as the API returned it. */
  name: string
}>()
</script>

<template>
  <UiStack direction="row" align="center" gap="sm" data-testid="integration-breadcrumb">
    <UiLink :to="AppRoute.Integrations" variant="link" data-testid="integration-breadcrumb-root">
      {{ t('shared.integrations') }}
    </UiLink>
    <UiText tone="muted" size="md">/</UiText>
    <UiText tone="primary" size="md" class="font-medium">{{ name }}</UiText>
  </UiStack>
</template>
