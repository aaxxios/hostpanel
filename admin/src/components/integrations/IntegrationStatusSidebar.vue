<script setup lang="ts">
/**
 * Right sidebar for the integration detail page.
 *
 * Displays the connection test state (in flight, last result, or last persisted test time) and an
 * optional contextual hint.
 */
import { useI18n } from 'vue-i18n'
import { formatClock, formatRelativeFromNow } from '@/lib/format'
import type { IntegrationTestResult } from '@/types/integration'
import UiAlert from '@/components/ui/UiAlert.vue'
import UiBadge from '@/components/ui/UiBadge.vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiSkeleton from '@/components/ui/UiSkeleton.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiText from '@/components/ui/UiText.vue'
import { MS_PER_SECOND } from '@/lib/limits'

/** Translation function and the active locale (for time wording). */
const { t, locale } = useI18n()

/** Props for IntegrationStatusSidebar. */
const props = defineProps<{
  /** ISO 8601 timestamp of last successful test, or null if never tested. */
  lastTestedAt: string | null
  /** Result of the most recent in-session test, or null. */
  testResult: IntegrationTestResult | null
  /** True while a connection test is in flight. */
  testing?: boolean
  /** Host the integration talks to (e.g. "gateway.example.com"), when it has one. */
  endpointHost?: string
  /** Optional hint text shown in a callout box. */
  hint?: string
}>()

/**
 * Formats a round-trip duration for the detail row.
 *
 * @param ms - Duration in milliseconds.
 * @returns "812 ms" below a second, "2.4 s" above it.
 */
const formatDuration = (ms: number): string =>
  ms < MS_PER_SECOND ? `${ms} ms` : `${(ms / MS_PER_SECOND).toFixed(1)} s`

/**
 * Describes the persisted last test time.
 *
 * @returns Relative wording such as "5 minutes ago", or the translated "Never".
 */
const formatLastTested = (): string =>
  props.lastTestedAt ? formatRelativeFromNow(props.lastTestedAt, locale.value) : t('integrations.common.never')
</script>

<template>
  <UiStack gap="md" data-testid="integration-status">
    <!-- Status card -->
    <UiCard padding="md">
      <UiStack gap="sm">
        <UiText tone="muted" size="xs" class="font-semibold uppercase tracking-[0.08em]">
          {{ t('integrations.integrationStatusSidebar.connectionStatus') }}
        </UiText>

        <!-- Test in flight. Checked first: the store clears the previous result when a test
             starts, so without this branch the card fell straight through to "Not tested yet"
             for the duration of the probe and gave no sign anything was happening. -->
        <UiStack v-if="testing" gap="xs" data-testid="integration-status-testing">
          <UiText tone="primary" size="md" class="font-medium">
            {{ t('integrations.integrationStatusSidebar.testingConnection') }}
          </UiText>
          <UiSkeleton :lines="2" />
        </UiStack>

        <!-- In-session test result -->
        <UiStack v-else-if="testResult" gap="xs" data-testid="integration-status-result">
          <UiBadge :tone="testResult.success ? 'success' : 'danger'" dot>
            {{ testResult.success ? t('integrations.integrationStatusSidebar.connectionOk') : t('integrations.integrationStatusSidebar.connectionFailed') }}
          </UiBadge>
          <UiText tone="muted" size="sm">{{ testResult.message }}</UiText>

          <!-- What the probe actually did. Each row is shown only when its value exists, so a
               built-in integration without a URL still gets a tidy card. -->
          <UiStack v-if="endpointHost" direction="row" gap="sm">
            <UiText tone="muted" size="xs">{{ t('integrations.integrationStatusSidebar.endpoint') }}</UiText>
            <UiText tone="secondary" size="xs" class="truncate font-mono">{{ endpointHost }}</UiText>
          </UiStack>
          <UiStack v-if="testResult.testedAt" direction="row" gap="sm">
            <UiText tone="muted" size="xs">{{ t('integrations.integrationStatusSidebar.tested') }}</UiText>
            <UiText tone="secondary" size="xs">{{ formatClock(testResult.testedAt, locale) }}</UiText>
          </UiStack>
          <UiStack v-if="testResult.durationMs !== undefined" direction="row" gap="sm">
            <UiText tone="muted" size="xs">{{ t('integrations.integrationStatusSidebar.roundTrip') }}</UiText>
            <UiText tone="secondary" size="xs">{{ formatDuration(testResult.durationMs) }}</UiText>
          </UiStack>
        </UiStack>

        <!-- Persisted last-tested -->
        <UiStack v-else gap="xs" data-testid="integration-status-idle">
          <UiBadge :tone="lastTestedAt ? 'success' : 'neutral'" dot>
            {{ lastTestedAt ? t('integrations.integrationStatusSidebar.previouslyTestedOk') : t('integrations.integrationStatusSidebar.notTestedYet') }}
          </UiBadge>
          <UiText tone="muted" size="sm">
            {{ t('integrations.integrationStatusSidebar.lastTestedFormatlasttested', { formatLastTested: formatLastTested() }) }}
          </UiText>
        </UiStack>
      </UiStack>
    </UiCard>

    <!-- Hint callout -->
    <UiAlert v-if="hint" variant="warning" data-testid="integration-status-hint">
      <UiStack gap="xs">
        <UiText tone="warning" size="xs" class="font-semibold uppercase tracking-[0.08em]">
          {{ t('integrations.integrationStatusSidebar.setupNote') }}
        </UiText>
        <UiText tone="secondary" size="sm">{{ hint }}</UiText>
      </UiStack>
    </UiAlert>
  </UiStack>
</template>
