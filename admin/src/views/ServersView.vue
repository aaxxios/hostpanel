<script setup lang="ts">
/**
 * Main Servers management page.
 *
 * Lists all provisioning servers grouped by module, then shows server groups.
 * Provides Add/Edit/Delete actions for both servers and groups. Every refusal sentence belongs to
 * the servers store and is shown as the API returned it.
 */
import { computed, onMounted, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { useServersStore } from '@/stores/servers'
import { FILL_TYPE_LABELS, MODULE_LABELS } from '@/types/server'
import type { ServerDto, ServerGroupDto, ServerGroupPayload, ServerModule, ServerPayload } from '@/types/server'
import type { UiTone } from '@/utils/ui/tone'
import GroupFormModal from '@/components/servers/GroupFormModal.vue'
import ServerFormModal from '@/components/servers/ServerFormModal.vue'
import UiAlert from '@/components/ui/UiAlert.vue'
import UiBadge from '@/components/ui/UiBadge.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiDialog from '@/components/ui/UiDialog.vue'
import UiEmptyState from '@/components/ui/UiEmptyState.vue'
import UiHeading from '@/components/ui/UiHeading.vue'
import UiPageHeader from '@/components/ui/UiPageHeader.vue'
import UiSkeleton from '@/components/ui/UiSkeleton.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiTable from '@/components/ui/UiTable.vue'
import UiTableBody from '@/components/ui/UiTableBody.vue'
import UiTableCell from '@/components/ui/UiTableCell.vue'
import UiTableHead from '@/components/ui/UiTableHead.vue'
import UiTableHeader from '@/components/ui/UiTableHeader.vue'
import UiTableRow from '@/components/ui/UiTableRow.vue'
import UiText from '@/components/ui/UiText.vue'

/** Translation function and the active locale (for date formatting). */
const { t, locale } = useI18n()

/** Servers store: the lists, the refusal sentences and the actions. */
const store = useServersStore()

/** Result of one connection test, as shown in the status column. */
interface TestOutcome {
  /** Whether the server answered. */
  ok: boolean
  /** The sentence to show: the summary on success, the server's reason otherwise. */
  message: string
}

/** True while a server save is in flight. */
const savingServer = ref(false)

/** True while a group save is in flight. */
const savingGroup = ref(false)

/** Ids of the servers currently being tested. */
const testingIds = ref<number[]>([])

/** Last test result per server id. */
const testResults = ref<Record<number, TestOutcome>>({})

/** The server being edited, or null when creating. */
const editingServer = ref<ServerDto | null>(null)

/** True when the server form dialog is open. */
const showServerModal = ref(false)

/** The group being edited, or null when creating. */
const editingGroup = ref<ServerGroupDto | null>(null)

/** True when the group form dialog is open. */
const showGroupModal = ref(false)

/** The server awaiting delete confirmation, or null when nothing is staged. */
const deleteServerTarget = ref<ServerDto | null>(null)

/** True while the staged server is being deleted. */
const deletingServer = ref(false)

/** The group awaiting delete confirmation, or null when nothing is staged. */
const deleteGroupTarget = ref<ServerGroupDto | null>(null)

/** True while the staged group is being deleted. */
const deletingGroup = ref(false)

/** Servers grouped by module, in first-seen order, for display. */
const serversByModule = computed<{ module: ServerModule; servers: ServerDto[] }[]>(() => {
  const groups: { module: ServerModule; servers: ServerDto[] }[] = []
  for (const server of store.servers) {
    const existing = groups.find(g => g.module === server.module)
    if (existing) {
      existing.servers.push(server)
    } else {
      groups.push({ module: server.module, servers: [server] })
    }
  }
  return groups
})

/** Whether the page-level refusal is shown (a dialog shows its own while it is open). */
const showPageError = computed<boolean>(() =>
  store.actionError !== null && !showServerModal.value && !showGroupModal.value,
)

/**
 * Tests connectivity to a server and shows the result in its row.
 *
 * @param id - Server identifier.
 */
const handleTestConnection = async (id: number): Promise<void> => {
  testingIds.value = [...testingIds.value, id]
  testResults.value = Object.fromEntries(
    Object.entries(testResults.value).filter(([key]) => Number(key) !== id),
  )
  const result = await store.testConnection(id)
  let outcome: TestOutcome
  if (result === null) {
    outcome = { ok: false, message: store.actionError ?? '' }
  } else if (result.connected) {
    outcome = {
      ok: true,
      message: t('servers.servers.onlineDetails', {
        accounts: result.accountsCount ?? 0,
        version: result.version ?? '',
      }),
    }
  } else {
    outcome = { ok: false, message: result.errorMessage ?? t('servers.servers.connectionFailed') }
  }
  testResults.value = { ...testResults.value, [id]: outcome }
  testingIds.value = testingIds.value.filter(i => i !== id)
}

/**
 * Tone and caption of a server's last known state.
 *
 * @param server - The server.
 * @returns The badge tone and the locale key of its caption.
 */
const stateOf = (server: ServerDto): { tone: UiTone; key: string } => {
  if (server.isOnline === true) {
    return { tone: 'success', key: 'servers.servers.online' }
  }
  if (server.isOnline === false) {
    return { tone: 'danger', key: 'servers.servers.offline' }
  }
  return { tone: 'neutral', key: 'servers.servers.notTested' }
}

/**
 * Formats the account usage of a server.
 *
 * @param server - The server.
 * @returns `count / max`, `count`, or a dash when unknown.
 */
const accountsLabel = (server: ServerDto): string => {
  if (server.accountsCount === null) {
    return '—'
  }
  return server.maxAccounts === null ? `${server.accountsCount}` : `${server.accountsCount} / ${server.maxAccounts}`
}

/** Opens the server create dialog. */
const openAddServer = (): void => {
  store.actionError = null
  editingServer.value = null
  showServerModal.value = true
}

/**
 * Opens the server edit dialog.
 *
 * @param server - The server to edit.
 */
const openEditServer = (server: ServerDto): void => {
  store.actionError = null
  editingServer.value = server
  showServerModal.value = true
}

/** Opens the group create dialog. */
const openAddGroup = (): void => {
  store.actionError = null
  editingGroup.value = null
  showGroupModal.value = true
}

/**
 * Opens the group edit dialog.
 *
 * @param group - The group to edit.
 */
const openEditGroup = (group: ServerGroupDto): void => {
  store.actionError = null
  editingGroup.value = group
  showGroupModal.value = true
}

/**
 * Saves a server (create or update).
 *
 * @param payload - Form data from ServerFormModal.
 */
const handleSaveServer = async (payload: ServerPayload): Promise<void> => {
  savingServer.value = true
  const saved = editingServer.value
    ? await store.updateServer(editingServer.value.id, payload)
    : await store.createServer(payload)
  savingServer.value = false
  if (saved) {
    showServerModal.value = false
  }
}

/** Deletes the staged server and clears the prompt. */
const confirmDeleteServer = async (): Promise<void> => {
  const target = deleteServerTarget.value
  if (!target) {
    return
  }
  deletingServer.value = true
  const deleted = await store.deleteServer(target.id)
  deletingServer.value = false
  if (deleted) {
    deleteServerTarget.value = null
  }
}

/**
 * Saves a group (create or update).
 *
 * @param payload - Form data from GroupFormModal.
 */
const handleSaveGroup = async (payload: ServerGroupPayload): Promise<void> => {
  savingGroup.value = true
  const saved = editingGroup.value
    ? await store.updateGroup(editingGroup.value.id, payload)
    : await store.createGroup(payload)
  savingGroup.value = false
  if (saved) {
    showGroupModal.value = false
  }
}

/** Deletes the staged group and clears the prompt. */
const confirmDeleteGroup = async (): Promise<void> => {
  const target = deleteGroupTarget.value
  if (!target) {
    return
  }
  deletingGroup.value = true
  const deleted = await store.deleteGroup(target.id)
  deletingGroup.value = false
  if (deleted) {
    deleteGroupTarget.value = null
  }
}

/** Loads the servers and the groups. */
onMounted(async () => {
  await Promise.all([store.fetchServers(), store.fetchGroups()])
})
</script>

<template>
  <UiStack gap="lg" class="w-full">
    <UiAlert v-if="showPageError" variant="error" data-testid="servers-error">{{ store.actionError }}</UiAlert>

    <UiPageHeader
      :title="t('servers.servers.servers')"
      :subtitle="t('servers.servers.manageProvisioningServersAndLoad')"
    >
      <template #actions>
        <UiButton variant="secondary" data-testid="servers-add-group" @click="openAddGroup">
          {{ t('servers.servers.createGroup') }}
        </UiButton>
        <UiButton variant="primary" data-testid="servers-add" @click="openAddServer">
          {{ t('servers.servers.addNewServer') }}
        </UiButton>
      </template>
    </UiPageHeader>

    <!-- Loading -->
    <UiSkeleton v-if="store.loading && store.servers.length === 0" :lines="4" data-testid="servers-loading" />

    <!-- Servers, one table per module -->
    <UiStack v-else-if="store.servers.length > 0" gap="md" data-testid="servers-list">
      <UiStack v-for="entry in serversByModule" :key="entry.module" gap="xs">
        <UiText tone="muted" size="sm" class="font-semibold">{{ MODULE_LABELS[entry.module] }}</UiText>
        <UiTable density="comfortable" :data-testid="`servers-table-${entry.module}`">
          <UiTableHead>
            <UiTableRow>
              <UiTableHeader>{{ t('servers.servers.serverName') }}</UiTableHeader>
              <UiTableHeader>{{ t('shared.ipAddress') }}</UiTableHeader>
              <UiTableHeader>{{ t('shared.accounts') }}</UiTableHeader>
              <UiTableHeader>{{ t('shared.status') }}</UiTableHeader>
              <UiTableHeader>{{ t('shared.actions') }}</UiTableHeader>
            </UiTableRow>
          </UiTableHead>
          <UiTableBody divided>
            <UiTableRow v-for="server in entry.servers" :key="server.id" hoverable :data-testid="`server-row-${server.id}`">
              <UiTableCell>
                <UiStack direction="row" align="center" gap="sm">
                  <UiText tone="primary" size="lg" class="font-medium">{{ server.name }}</UiText>
                  <UiBadge v-if="server.isDefault" tone="warning" small>{{ t('servers.servers.default') }}</UiBadge>
                </UiStack>
              </UiTableCell>
              <UiTableCell muted class="font-mono">{{ server.ipAddress ?? '—' }}</UiTableCell>
              <UiTableCell muted>{{ accountsLabel(server) }}</UiTableCell>
              <UiTableCell>
                <UiText v-if="testingIds.includes(server.id)" tone="muted" size="sm">{{ t('shared.testing') }}</UiText>
                <UiBadge
                  v-else-if="testResults[server.id]"
                  :tone="testResults[server.id]?.ok ? 'success' : 'danger'"
                  :data-testid="`server-test-result-${server.id}`"
                >
                  {{ testResults[server.id]?.message }}
                </UiBadge>
                <UiStack v-else gap="xs">
                  <UiBadge :tone="stateOf(server).tone" dot :data-testid="`server-state-${server.id}`">
                    {{ t(stateOf(server).key) }}
                  </UiBadge>
                  <UiText v-if="server.lastTestedAt" tone="muted" size="xs">
                    {{ new Date(server.lastTestedAt).toLocaleString(locale) }}
                  </UiText>
                </UiStack>
              </UiTableCell>
              <UiTableCell>
                <UiStack direction="row" gap="xs" wrap>
                  <UiButton
                    variant="ghost"
                    size="sm"
                    :disabled="testingIds.includes(server.id)"
                    :data-testid="`server-test-${server.id}`"
                    @click="handleTestConnection(server.id)"
                  >
                    {{ t('shared.testConnection') }}
                  </UiButton>
                  <UiButton variant="ghost" size="sm" :data-testid="`server-edit-${server.id}`" @click="openEditServer(server)">
                    {{ t('shared.edit') }}
                  </UiButton>
                  <UiButton variant="danger" size="sm" :data-testid="`server-delete-${server.id}`" @click="deleteServerTarget = server">
                    {{ t('shared.delete') }}
                  </UiButton>
                </UiStack>
              </UiTableCell>
            </UiTableRow>
          </UiTableBody>
        </UiTable>
      </UiStack>
    </UiStack>

    <!-- Empty state -->
    <UiEmptyState v-else :title="t('servers.servers.noServersYet')" data-testid="servers-empty">
      <template #actions>
        <UiButton variant="link" data-testid="servers-add-first" @click="openAddServer">
          {{ t('servers.servers.addYourFirstServer') }}
        </UiButton>
      </template>
    </UiEmptyState>

    <!-- Groups section -->
    <UiStack gap="sm">
      <UiHeading :level="2" variant="section">{{ t('servers.servers.groups') }}</UiHeading>
      <UiText tone="secondary" size="sm">{{ t('servers.servers.serverGroupsAllowYouTo') }}</UiText>

      <UiTable v-if="store.groups.length > 0" density="comfortable" data-testid="server-groups-table">
        <UiTableHead>
          <UiTableRow>
            <UiTableHeader>{{ t('servers.servers.groupName') }}</UiTableHeader>
            <UiTableHeader>{{ t('servers.servers.fillType') }}</UiTableHeader>
            <UiTableHeader>{{ t('servers.servers.servers') }}</UiTableHeader>
            <UiTableHeader>{{ t('shared.actions') }}</UiTableHeader>
          </UiTableRow>
        </UiTableHead>
        <UiTableBody divided>
          <UiTableRow v-for="group in store.groups" :key="group.id" hoverable :data-testid="`server-group-row-${group.id}`">
            <UiTableCell>{{ group.name }}</UiTableCell>
            <UiTableCell muted>{{ FILL_TYPE_LABELS[group.fillType] }}</UiTableCell>
            <UiTableCell muted>
              {{ group.servers.length > 0 ? group.servers.map(s => s.name).join(', ') : '—' }}
            </UiTableCell>
            <UiTableCell>
              <UiStack direction="row" gap="xs" wrap>
                <UiButton variant="ghost" size="sm" :data-testid="`server-group-edit-${group.id}`" @click="openEditGroup(group)">
                  {{ t('shared.edit') }}
                </UiButton>
                <UiButton variant="danger" size="sm" :data-testid="`server-group-delete-${group.id}`" @click="deleteGroupTarget = group">
                  {{ t('shared.delete') }}
                </UiButton>
              </UiStack>
            </UiTableCell>
          </UiTableRow>
        </UiTableBody>
      </UiTable>

      <!-- Groups empty -->
      <UiStack v-else direction="row" align="center" gap="xs" data-testid="server-groups-empty">
        <UiText tone="muted" size="md">{{ t('servers.servers.noGroupsYet') }}</UiText>
        <UiButton variant="link" data-testid="server-groups-create-first" @click="openAddGroup">
          {{ t('servers.servers.createOne') }}
        </UiButton>
      </UiStack>
    </UiStack>

    <!-- Server form dialog -->
    <ServerFormModal
      v-if="showServerModal"
      :server="editingServer"
      :saving="savingServer"
      :error="store.actionError"
      :field-errors="store.fieldErrors"
      @saved="handleSaveServer"
      @close="showServerModal = false"
    />

    <!-- Group form dialog -->
    <GroupFormModal
      v-if="showGroupModal"
      :group="editingGroup"
      :available-servers="store.servers"
      :saving="savingGroup"
      :error="store.actionError"
      :field-errors="store.fieldErrors"
      @saved="handleSaveGroup"
      @close="showGroupModal = false"
    />

    <UiDialog
      v-if="deleteServerTarget"
      variant="confirm"
      :title="t('servers.servers.deleteServer')"
      :message="t('servers.servers.deleteServerMessage', { name: deleteServerTarget.name })"
      :confirm-label="t('shared.delete')"
      :loading-label="t('shared.deleting')"
      :loading="deletingServer"
      tone="danger"
      @confirm="confirmDeleteServer"
      @close="deleteServerTarget = null"
    />

    <UiDialog
      v-if="deleteGroupTarget"
      variant="confirm"
      :title="t('servers.servers.deleteGroup')"
      :message="t('servers.servers.deleteGroupMessage', { name: deleteGroupTarget.name })"
      :confirm-label="t('shared.delete')"
      :loading-label="t('shared.deleting')"
      :loading="deletingGroup"
      tone="danger"
      @confirm="confirmDeleteGroup"
      @close="deleteGroupTarget = null"
    />
  </UiStack>
</template>
