<script setup lang="ts">
/**
 * Displays a single integration as a clickable card with logo, title, description, and status.
 *
 * Clicking the card navigates to the integration's configuration page.
 */
import { useI18n } from 'vue-i18n'
import { INTEGRATION_META } from '@/types/integrationmeta'
import type { IntegrationDto, IntegrationSlug } from '@/types/integration'
import UiBadge from '@/components/ui/UiBadge.vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiHeading from '@/components/ui/UiHeading.vue'
import UiImage from '@/components/ui/UiImage.vue'
import UiLink from '@/components/ui/UiLink.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiText from '@/components/ui/UiText.vue'

/** Translation function. */
const { t } = useI18n()

/** Props for IntegrationCard. */
const props = defineProps<{
  /** The integration summary data from the store. */
  integration: IntegrationDto
}>()

/** Static metadata for this integration (logo, color, description). */
const meta = INTEGRATION_META[props.integration.slug as IntegrationSlug]
</script>

<template>
  <UiLink :to="`/integrations/${integration.slug}/configure`" class="block" :data-testid="`integration-card-${integration.slug}`">
    <UiCard padding="md" class="h-full">
      <UiStack gap="md" class="h-full">
        <UiStack direction="row" align="start" justify="between">
          <UiStack align="center" justify="center" class="h-12 w-12 rounded-xl p-2.5" :class="meta?.color ?? 'bg-text-muted'">
            <UiImage
              v-if="meta?.logo"
              :src="meta.logo"
              :alt="integration.name"
              class="h-full w-full object-contain brightness-0 invert"
            />
          </UiStack>
          <UiBadge :tone="integration.isEnabled ? 'success' : 'neutral'" small :data-testid="`integration-status-${integration.slug}`">
            {{ integration.isEnabled ? t('shared.active') : t('shared.inactive') }}
          </UiBadge>
        </UiStack>

        <UiStack direction="row" align="center" gap="sm">
          <UiHeading :level="3" variant="card">{{ integration.name }}</UiHeading>
          <UiBadge v-if="integration.isPlugin" tone="primary" small>
            {{ t('integrations.integrationCard.plugin') }}
          </UiBadge>
        </UiStack>

        <UiText tone="muted" size="md" class="flex-1">
          {{ meta?.shortDescription ?? integration.description }}
        </UiText>

        <UiText tone="primary" size="sm" class="font-medium">
          {{ t('integrations.integrationCard.configure') }}
        </UiText>
      </UiStack>
    </UiCard>
  </UiLink>
</template>
