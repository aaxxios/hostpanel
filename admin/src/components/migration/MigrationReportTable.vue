<script setup lang="ts">
/**
 * Import report of a finished migration job: one row per imported, skipped or failed source row,
 * filterable by entity group and outcome, paged. The log comes from the migration store, which
 * owns the request and the refusal sentence.
 */
import { computed, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useMigrationStore } from '@/stores/migration'
import { formatClock } from '@/lib/format'
import { migrationActionLabelKey } from '@/lib/migrationStatus'
import { migrationActionTone } from '@/lib/tones'
import { MIGRATION_ENTITY_KEYS, MigrationLogAction } from '@/types/migration'
import type { MigrationLogEntry } from '@/types/migration'
import UiBadge from '@/components/ui/UiBadge.vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiEmptyState from '@/components/ui/UiEmptyState.vue'
import UiHeading from '@/components/ui/UiHeading.vue'
import UiPagination from '@/components/ui/UiPagination.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiTable from '@/components/ui/UiTable.vue'
import UiTableBody from '@/components/ui/UiTableBody.vue'
import UiTableCell from '@/components/ui/UiTableCell.vue'
import UiTableHead from '@/components/ui/UiTableHead.vue'
import UiTableHeader from '@/components/ui/UiTableHeader.vue'
import UiTableRow from '@/components/ui/UiTableRow.vue'

/** Translation function and the active locale (for time formatting). */
const { t, locale } = useI18n()

/** Props for MigrationReportTable. */
const props = defineProps<{
  /** Id of the job whose log is shown. */
  jobId: number
}>()

/** Migration store: owns the log request and its refusal sentence. */
const store = useMigrationStore()

/** Rows per page. */
const PAGE_SIZE = 20

/** Rows of the current page. */
const items = ref<MigrationLogEntry[]>([])

/** Rows matching the filters across all pages. */
const totalCount = ref(0)

/** True while a page is being fetched. */
const loading = ref(false)

/** Selected outcome filter; empty means all. */
const filterAction = ref('')

/** Selected entity-group filter; empty means all. */
const filterEntityType = ref('')

/** Current one-based page. */
const page = ref(1)

/**
 * Converts an entity key to the PascalCase name the log filter and rows use.
 *
 * @param key - Camel-case entity key such as `ticketReplies`.
 * @returns The PascalCase name such as `TicketReplies`.
 */
const toLogEntityType = (key: string): string => `${key.charAt(0).toUpperCase()}${key.slice(1)}`

/**
 * Converts a log entity name to the camel-case key of the locale catalogue.
 *
 * @param type - PascalCase name such as `TicketReplies`.
 * @returns The camel-case key such as `ticketReplies`.
 */
const toEntityKey = (type: string): string => `${type.charAt(0).toLowerCase()}${type.slice(1)}`

/** Outcome filter options. */
const actionOptions = computed(() => [
  { value: '', label: t('migration.migrationReportTable.allStatuses') },
  ...Object.values(MigrationLogAction).map(action => ({
    value: action,
    label: t(migrationActionLabelKey[action]),
  })),
])

/** Entity-group filter options. */
const entityTypeOptions = computed(() => [
  { value: '', label: t('migration.migrationReportTable.allTypes') },
  ...MIGRATION_ENTITY_KEYS.map(key => ({
    value: toLogEntityType(key),
    label: t(`migrationEntities.${key}`),
  })),
])

/** Fetches the current page of the log. */
const load = async (): Promise<void> => {
  loading.value = true
  const result = await store.fetchLogs(props.jobId, {
    action: filterAction.value || undefined,
    entityType: filterEntityType.value || undefined,
    page: page.value,
    pageSize: PAGE_SIZE,
  })
  loading.value = false
  if (result) {
    items.value = result.items
    totalCount.value = result.totalCount
  }
}

// A filter change restarts from the first page; the first run loads the report.
watch([filterAction, filterEntityType], () => {
  page.value = 1
  void load()
}, { immediate: true })

// Paging re-reads the log.
watch(page, () => {
  void load()
})
</script>

<template>
  <UiCard padding="md" data-testid="migration-report">
    <UiStack gap="md">
      <!-- Header + filters -->
      <UiStack direction="row" align="center" gap="sm" wrap>
        <UiHeading :level="3" variant="card" class="min-w-0 flex-1">
          {{ t('migration.migrationReportTable.importReport') }}
        </UiHeading>
        <UiBadge tone="success" small data-testid="migration-report-count">
          {{ t('migration.migrationReportTable.totalcountEntries', { totalCount }) }}
        </UiBadge>
        <UiSelect
          v-model="filterEntityType"
          :options="entityTypeOptions"
          :placeholder="t('migration.migrationReportTable.allTypes')"
          class="w-40"
          data-testid="migration-report-entity-filter"
        />
        <UiSelect
          v-model="filterAction"
          :options="actionOptions"
          :placeholder="t('migration.migrationReportTable.allStatuses')"
          class="w-40"
          data-testid="migration-report-action-filter"
        />
      </UiStack>

      <!-- Table -->
      <UiTable
        density="compact"
        :loading="loading && items.length === 0"
        :empty="!loading && items.length === 0"
        :colspan="5"
        data-testid="migration-report-table"
      >
        <UiTableHead>
          <UiTableRow>
            <UiTableHeader>{{ t('shared.type') }}</UiTableHeader>
            <UiTableHeader>{{ t('migration.migrationReportTable.identifier') }}</UiTableHeader>
            <UiTableHeader>{{ t('shared.status') }}</UiTableHeader>
            <UiTableHeader>{{ t('shared.reason') }}</UiTableHeader>
            <UiTableHeader>{{ t('migration.migrationReportTable.time') }}</UiTableHeader>
          </UiTableRow>
        </UiTableHead>
        <UiTableBody divided>
          <UiTableRow v-for="entry in items" :key="entry.id" hoverable>
            <UiTableCell muted>{{ t(`migrationEntities.${toEntityKey(entry.entityType)}`) }}</UiTableCell>
            <UiTableCell class="font-mono">{{ entry.identifier }}</UiTableCell>
            <UiTableCell>
              <UiBadge :tone="migrationActionTone[entry.action]" dot small>
                {{ t(migrationActionLabelKey[entry.action]) }}
              </UiBadge>
            </UiTableCell>
            <UiTableCell muted>{{ entry.reason ?? '—' }}</UiTableCell>
            <UiTableCell muted class="font-mono">{{ formatClock(entry.createdAt, locale) }}</UiTableCell>
          </UiTableRow>
        </UiTableBody>
        <template #empty>
          <UiEmptyState :title="t('migration.migrationReportTable.noRecordsFound')" />
        </template>
      </UiTable>

      <!-- Pagination -->
      <UiStack v-if="totalCount > PAGE_SIZE" direction="row" align="center" justify="between">
        <UiPagination
          v-model:page="page"
          :total="totalCount"
          :page-size="PAGE_SIZE"
          testid="migration-report-pagination"
        />
      </UiStack>
    </UiStack>
  </UiCard>
</template>
