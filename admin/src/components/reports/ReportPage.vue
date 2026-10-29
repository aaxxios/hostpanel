<script setup lang="ts">
/**
 * Frame every report screen sits in: title and description, the filters, then the result.
 *
 * Loading and the refusal sentence come from the reports store, so a screen passes neither. While
 * a report loads the result is replaced by a placeholder; a failed load shows the API sentence
 * (with a Retry button when the screen passes `retry`) instead of the result, so an empty-state
 * text never stands in for data that could not be read; otherwise the default slot and the
 * "generated on" line show.
 */
import { useI18n } from 'vue-i18n'
import ReportTimestamp from '@/components/reports/ReportTimestamp.vue'
import UiAlert from '@/components/ui/UiAlert.vue'
import UiErrorState from '@/components/ui/UiErrorState.vue'
import UiPageHeader from '@/components/ui/UiPageHeader.vue'
import UiSkeleton from '@/components/ui/UiSkeleton.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiText from '@/components/ui/UiText.vue'
import { useReportsStore } from '@/stores/reports'

const { t } = useI18n()

/** Props for ReportPage. */
defineProps<{
  /** Report title (already translated). */
  title: string
  /** Line under the title (already translated). */
  description?: string
  /** Loads the report again after a failed load; without it the failure shows no Retry button. */
  retry?: () => unknown
}>()

const store = useReportsStore()
</script>

<template>
  <UiStack gap="none" data-testid="report-page">
    <UiPageHeader :title="title" :subtitle="description" />

    <slot name="filters" />

    <UiStack v-if="store.loading" gap="sm" aria-busy="true" data-testid="report-loading">
      <UiSkeleton shape="block" />
      <UiSkeleton :lines="3" />
      <UiText class="sr-only">{{ t('shared.loading') }}</UiText>
    </UiStack>
    <template v-else-if="store.error">
      <UiErrorState v-if="retry" :message="store.error" testid="report-error" @retry="retry()" />
      <UiAlert v-else variant="danger" data-testid="report-error">
        {{ store.error }}
      </UiAlert>
    </template>
    <UiStack v-else gap="lg">
      <slot />
      <ReportTimestamp />
    </UiStack>
  </UiStack>
</template>
