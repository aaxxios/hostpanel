<script setup lang="ts">
/**
 * Progress card of one migration job.
 *
 * Shows the job's state and overall percent, a per-entity checklist with imported/total counts,
 * the migrated/skipped/failed/total tiles, the translated failure reason, and, while the job can still
 * change, the plugin connection state with the "Check" and "Start Import" actions. A check tells
 * a plugin that did not answer apart from a request the API refused (whose sentence is shown), and a
 * refused import start shows the API's sentence beside its button. The job comes in as a prop; the
 * store owns the actions and their refusal sentences.
 */
import { computed, onUnmounted, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { useMigrationStore } from '@/stores/migration'
import { isJobActive, migrationJobFailureLabelKey, migrationStatusLabelKey } from '@/lib/migrationStatus'
import { migrationStatusTone } from '@/lib/tones'
import { MIGRATION_ENTITY_KEYS, MigrationStatus } from '@/types/migration'
import type { MigrationEntityKey, MigrationJob } from '@/types/migration'
import UiAlert from '@/components/ui/UiAlert.vue'
import UiBadge from '@/components/ui/UiBadge.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiGrid from '@/components/ui/UiGrid.vue'
import UiHeading from '@/components/ui/UiHeading.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiStatTile from '@/components/ui/UiStatTile.vue'
import UiText from '@/components/ui/UiText.vue'
import { NOTICE_VISIBLE_MS } from '@/lib/limits'

/** Translation function. */
const { t } = useI18n()

/** Props for MigrationProgressCard. */
const props = defineProps<{
  /** The job to show. */
  job: MigrationJob
}>()

/** Migration store: owns the connection check and the import start. */
const store = useMigrationStore()

/** True while the connection check is in flight. */
const checking = ref(false)

/** True while the import start request is in flight. */
const starting = ref(false)

/** What the last connection check found. */
enum CheckOutcome {
  /** The plugin answered. */
  Reachable = 'reachable',
  /** The API checked, and the plugin did not answer. */
  Unreachable = 'unreachable',
  /** The API refused the check itself; its sentence is in the store. */
  Refused = 'refused',
}

/** Outcome of the last connection check, null when none is shown. */
const checkOutcome = ref<CheckOutcome | null>(null)

/** The API's sentence for a refused import start, null when none. */
const startError = ref<string | null>(null)

/** The API's sentence for a refused connection check, shown with the refused outcome. */
const checkError = ref<string | null>(null)

/** Timer that hides the connection-check result. */
const checkResultTimer = ref<ReturnType<typeof setTimeout> | null>(null)

/**
 * Asks the server to test the source plugin. Whether the plugin answered is shown for a few
 * seconds; a refused request keeps the API's sentence until the next check.
 *
 * @returns Resolves when the check has settled.
 */
const checkConnection = async (): Promise<void> => {
  checking.value = true
  checkOutcome.value = null
  checkError.value = null
  if (checkResultTimer.value !== null) {
    clearTimeout(checkResultTimer.value)
    checkResultTimer.value = null
  }
  const job = await store.testConnection(props.job.id)
  checking.value = false
  if (job === null) {
    checkOutcome.value = CheckOutcome.Refused
    checkError.value = store.jobError ?? t('errors.noAnswer')
    return
  }
  checkOutcome.value = job.pluginConnected ? CheckOutcome.Reachable : CheckOutcome.Unreachable
  checkResultTimer.value = setTimeout(() => {
    checkOutcome.value = null
  }, NOTICE_VISIBLE_MS)
}

/**
 * Starts the import and follows the job until it finishes; a refusal shows the API's sentence.
 *
 * @returns Resolves when the request has settled.
 */
const startImport = async (): Promise<void> => {
  startError.value = null
  starting.value = true
  const job = await store.startImport(props.job.id)
  starting.value = false
  if (job === null) {
    startError.value = store.jobError ?? t('errors.noAnswer')
    return
  }
  store.startPolling(props.job.id)
}

/**
 * Queues a failed import again and follows the job; a refusal shows the API's sentence.
 *
 * @returns Resolves when the request has settled.
 */
const retryImport = async (): Promise<void> => {
  startError.value = null
  starting.value = true
  const job = await store.retryImport(props.job.id)
  starting.value = false
  if (job === null) {
    startError.value = store.jobError ?? t('errors.noAnswer')
    return
  }
  store.startPolling(props.job.id)
}

/** Entity groups this job exports. */
const activeKeys = computed<MigrationEntityKey[]>(() =>
  MIGRATION_ENTITY_KEYS.filter(key => props.job.entitySelection[key]),
)

/** Sum of imported records across the selected entity types. */
const totalMigrated = computed<number>(() =>
  activeKeys.value.reduce((sum, key) => sum + props.job.progress[key].imported, 0),
)

/** Sum of skipped records across the selected entity types. */
const totalSkipped = computed<number>(() =>
  activeKeys.value.reduce((sum, key) => sum + props.job.progress[key].skipped, 0),
)

/** Sum of source records across the selected entity types. */
const totalRecords = computed<number>(() =>
  activeKeys.value.reduce((sum, key) => sum + props.job.progress[key].total, 0),
)

/** Sum of records whose import failed, as the server counted them. */
const totalFailed = computed<number>(() =>
  activeKeys.value.reduce((sum, key) => sum + props.job.progress[key].failed, 0),
)

/** Stops the pending connection-check timer. */
onUnmounted(() => {
  if (checkResultTimer.value !== null) {
    clearTimeout(checkResultTimer.value)
  }
})
</script>

<template>
  <UiCard padding="md" data-testid="migration-progress-card">
    <UiStack gap="md">
      <!-- Header -->
      <UiStack direction="row" align="center" justify="between">
        <UiStack gap="xs">
          <UiStack direction="row" align="center" gap="sm">
            <UiHeading :level="3" variant="card">
              {{ job.label ?? t('migration.migrationProgressCard.migration') }}
            </UiHeading>
            <UiBadge :tone="migrationStatusTone[job.status]" small data-testid="migration-job-status">
              {{ t(migrationStatusLabelKey[job.status]) }}
            </UiBadge>
          </UiStack>
          <UiText tone="muted" size="xs">{{ t('migration.migrationProgressCard.jobId', { id: job.id }) }}</UiText>
        </UiStack>
        <UiText tone="primary" class="text-[2rem] font-black leading-none tabular-nums" data-testid="migration-job-percent">
          {{ job.overallPercent }}%
        </UiText>
      </UiStack>

      <!-- Entity checklist -->
      <UiStack gap="xs" data-testid="migration-entities">
        <UiStack
          v-for="key in activeKeys"
          :key="key"
          direction="row"
          align="center"
          justify="between"
          :data-testid="`migration-entity-${key}`"
        >
          <UiText :tone="job.progress[key].done ? 'primary' : 'secondary'" size="md">
            {{ t(`migrationEntities.${key}`) }}
          </UiText>
          <UiText :tone="job.progress[key].done ? 'success' : 'muted'" size="sm" class="font-semibold tabular-nums">
            <template v-if="job.progress[key].total > 0">
              {{ job.progress[key].imported + job.progress[key].skipped }}/{{ job.progress[key].total }}
            </template>
            <template v-else>—</template>
          </UiText>
        </UiStack>
      </UiStack>

      <!-- KPI tiles -->
      <UiGrid :columns="4" gap="sm">
        <UiStatTile :label="t('migration.migrationProgressCard.migrated')" :value="totalMigrated" tone="success" />
        <UiStatTile :label="t('migration.migrationProgressCard.skipped')" :value="totalSkipped" tone="warning" />
        <UiStatTile
          :label="t('migration.migrationProgressCard.failed')"
          :value="totalFailed"
          :tone="totalFailed > 0 ? 'danger' : 'neutral'"
        />
        <UiStatTile :label="t('shared.total')" :value="totalRecords" />
      </UiGrid>

      <!-- Failure message the server recorded for the job -->
      <UiAlert v-if="job.status === MigrationStatus.Failed && job.failure" variant="error" data-testid="migration-job-error">
        {{ t(migrationJobFailureLabelKey[job.failure]) }}
      </UiAlert>
      <UiStack v-if="job.status === MigrationStatus.Failed" direction="row" gap="sm">
        <UiButton
          type="button"
          variant="primary"
          size="sm"
          :loading="starting"
          data-testid="migration-retry"
          @click="retryImport"
        >
          {{ t('migration.migrationProgressCard.retryImport') }}
        </UiButton>
      </UiStack>

      <!-- Plugin connection + start -->
      <UiStack v-if="isJobActive(job.status)" direction="row" align="center" gap="sm" wrap>
        <UiBadge :tone="job.pluginConnected ? 'success' : 'neutral'" dot data-testid="migration-plugin-state">
          {{ job.pluginConnected ? t('migration.migrationProgressCard.pluginConnected') : t('migration.migrationProgressCard.waitingForPlugin') }}
        </UiBadge>
        <UiButton
          type="button"
          variant="secondary"
          size="sm"
          :disabled="checking"
          data-testid="migration-check"
          @click="checkConnection"
        >
          {{ checking ? t('migration.migrationProgressCard.checking') : t('migration.migrationProgressCard.check') }}
        </UiButton>
        <UiBadge
          v-if="checkOutcome === CheckOutcome.Reachable || checkOutcome === CheckOutcome.Unreachable"
          :tone="checkOutcome === CheckOutcome.Reachable ? 'success' : 'danger'"
          data-testid="migration-check-result"
        >
          {{ checkOutcome === CheckOutcome.Reachable ? t('shared.connected') : t('migration.migrationProgressCard.notReachable') }}
        </UiBadge>
        <UiButton
          v-if="job.status === MigrationStatus.Pending && job.pluginConnected"
          type="button"
          variant="primary"
          size="sm"
          :loading="starting"
          data-testid="migration-start"
          @click="startImport"
        >
          {{ t('migration.migrationProgressCard.startImport') }}
        </UiButton>
      </UiStack>
      <UiAlert v-if="checkOutcome === CheckOutcome.Refused && checkError" variant="error" data-testid="migration-check-error">
        {{ checkError }}
      </UiAlert>
      <UiAlert v-if="startError" variant="error" data-testid="migration-start-error">
        {{ startError }}
      </UiAlert>
    </UiStack>
  </UiCard>
</template>
