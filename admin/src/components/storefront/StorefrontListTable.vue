<script setup lang="ts" generic="T extends StorefrontEntity">
/**
 * The list page shell of a storefront content type: header with the Add button, a draggable table
 * ordered by sort order, edit and delete actions per row, and the delete confirmation.
 *
 * The page supplies the column captions and, per row, the cells between the drag handle and the
 * actions (`row` slot). Dragging reports the new order as id + sortOrder pairs; saving it, loading
 * the list and deleting stay with the page's store.
 */
import { ref, watch } from 'vue'
import type { Ref } from 'vue'
import { useI18n } from 'vue-i18n'
import draggable from 'vuedraggable'
import SettingsLoading from '@/components/settings/SettingsLoading.vue'
import UiAlert from '@/components/ui/UiAlert.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiDialog from '@/components/ui/UiDialog.vue'
import UiIcon from '@/components/ui/UiIcon.vue'
import UiIconButton from '@/components/ui/UiIconButton.vue'
import UiPageHeader from '@/components/ui/UiPageHeader.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiTable from '@/components/ui/UiTable.vue'
import UiTableCell from '@/components/ui/UiTableCell.vue'
import UiTableHead from '@/components/ui/UiTableHead.vue'
import UiTableHeader from '@/components/ui/UiTableHeader.vue'
import UiTableRow from '@/components/ui/UiTableRow.vue'
import UiText from '@/components/ui/UiText.vue'
import { Icon } from '@/lib/icons'
import type { StorefrontEntity, StorefrontListTestIds, StorefrontOrderItem } from '@/types/storefront'

const { t } = useI18n()

/** Props for StorefrontListTable. */
const props = defineProps<{
  /** Page title (already translated). */
  title: string
  /** Line under the title (already translated). */
  subtitle: string
  /** Label of the Add button (already translated). */
  addLabel: string
  /** Caption of the loading state (already translated). */
  loadingLabel: string
  /** Text shown when the list is empty (already translated). */
  emptyLabel: string
  /** Title of the delete question (already translated). */
  deleteTitle: string
  /** Label of the edit action (already translated). */
  editLabel: string
  /** Label of the delete action (already translated). */
  deleteLabel: string
  /** Label of the move-up action (already translated). */
  moveUpLabel: string
  /** Label of the move-down action (already translated). */
  moveDownLabel: string
  /** Test ids of the page, named in full by the page that uses the table. */
  testids: StorefrontListTestIds
  /** Column captions between the drag handle and the actions (already translated). */
  columns: string[]
  /** The entries, in any order; the table sorts them by sort order. */
  items: T[]
  /** True while the list is loading. */
  loading: boolean
  /** Sentence for a failed load, null when none. */
  error: string | null
  /** Sentence for a failed create, update, delete or reorder, null when none. */
  actionError: string | null
  /** True while a new order is being saved. */
  savingOrder: boolean
  /** The entry staged for deletion, null when no question is open. */
  deleteTarget: T | null
  /** True while the staged entry is being deleted. */
  deleting: boolean
  /** The question text for the staged entry (already translated). */
  deleteMessage: string
}>()

const emit = defineEmits<{
  /** Emitted when the operator presses Add. */
  add: []
  /** Emitted with the entry the operator wants to edit. */
  edit: [item: T]
  /** Emitted with the entry the operator wants to delete (opens the question). */
  stageDelete: [item: T]
  /** Emitted when the operator confirms the delete question. */
  confirmDelete: []
  /** Emitted when the operator dismisses the delete question. */
  cancelDelete: []
  /** Emitted with the new order after a drag ends. */
  reorder: [order: StorefrontOrderItem[]]
}>()

/** Local mutable copy of the entries, which the drag library reorders in place. */
const local = ref<T[]>([]) as Ref<T[]>

/** Copies the entries from the props, sorted by sort order. */
const sync = (): void => {
  local.value = [...props.items].sort((a, b) => a.sortOrder - b.sortOrder)
}

watch(() => props.items, sync, { immediate: true })

/**
 * Moves an entry one place up or down and reports the new order; the keyboard alternative to dragging.
 *
 * @param index - Current position of the entry.
 * @param step - -1 to move up, 1 to move down.
 */
const move = (index: number, step: number): void => {
  const target = index + step
  const moved = local.value[index]
  const other = local.value[target]
  if (moved === undefined || other === undefined) {
    return
  }
  local.value[index] = other
  local.value[target] = moved
  onDragEnd()
}

/** Called after a drag ends: reports the new position of every entry. */
const onDragEnd = (): void => {
  emit('reorder', local.value.map((item, index) => ({ id: item.id, sortOrder: index })))
}
</script>

<template>
  <UiStack gap="none" :data-testid="testids.view">
    <UiAlert v-if="actionError" variant="danger" class="mb-4" :data-testid="testids.actionError">
      {{ actionError }}
    </UiAlert>

    <UiPageHeader :title="title" :subtitle="subtitle">
      <template #actions>
        <UiText v-if="savingOrder" tone="muted" size="sm" role="status" :data-testid="testids.savingOrder">
          {{ t('storefront.list.savingOrder') }}
        </UiText>
        <UiButton type="button" variant="primary" :data-testid="testids.add" @click="emit('add')">
          {{ addLabel }}
        </UiButton>
      </template>
    </UiPageHeader>

    <SettingsLoading v-if="loading" :message="loadingLabel" />

    <UiAlert v-else-if="error" variant="danger" :data-testid="testids.error">
      {{ error }}
    </UiAlert>

    <UiCard v-else class="overflow-hidden">
      <UiTable density="comfortable" :empty="local.length === 0" :colspan="columns.length + 2">
        <template #empty>
          {{ emptyLabel }}
        </template>
        <UiTableHead>
          <UiTableRow class="border-b border-border">
            <UiTableHeader class="w-10"><span class="sr-only">{{ t('storefront.list.reorder') }}</span></UiTableHeader>
            <UiTableHeader v-for="column in columns" :key="column" align="left">{{ column }}</UiTableHeader>
            <UiTableHeader align="right">{{ t('shared.actions') }}</UiTableHeader>
          </UiTableRow>
        </UiTableHead>
        <draggable
          v-model="local"
          tag="tbody"
          item-key="id"
          handle=".drag-handle"
          ghost-class="opacity-40"
          :animation="200"
          class="divide-y divide-border"
          @end="onDragEnd"
        >
          <template #item="{ element, index }">
            <UiTableRow hoverable :data-testid="`${testids.row}-${element.id}`">
              <UiTableCell>
                <UiStack align="center" justify="center" gap="none" class="drag-handle h-6 w-6 cursor-grab text-text-muted active:cursor-grabbing">
                  <UiIcon :markup="Icon.Grip" size="sm" />
                </UiStack>
              </UiTableCell>

              <slot name="row" :item="element" />

              <UiTableCell align="right">
                <UiIconButton
                  :label="moveUpLabel"
                  size="sm"
                  :disabled="index === 0"
                  :data-testid="`${testids.moveUp}-${element.id}`"
                  @click="move(index, -1)"
                >
                  <UiIcon :markup="Icon.ArrowUp" size="sm" />
                </UiIconButton>
                <UiIconButton
                  :label="moveDownLabel"
                  size="sm"
                  :disabled="index === local.length - 1"
                  :data-testid="`${testids.moveDown}-${element.id}`"
                  @click="move(index, 1)"
                >
                  <UiIcon :markup="Icon.ArrowDown" size="sm" />
                </UiIconButton>
                <UiIconButton
                  :label="editLabel"
                  size="sm"
                  :data-testid="`${testids.edit}-${element.id}`"
                  @click="emit('edit', element)"
                >
                  <UiIcon :markup="Icon.Edit" size="sm" />
                </UiIconButton>
                <UiIconButton
                  :label="deleteLabel"
                  size="sm"
                  :data-testid="`${testids.delete}-${element.id}`"
                  @click="emit('stageDelete', element)"
                >
                  <UiIcon :markup="Icon.Trash" size="sm" />
                </UiIconButton>
              </UiTableCell>
            </UiTableRow>
          </template>
        </draggable>
      </UiTable>
    </UiCard>

    <!-- Delete confirmation -->
    <UiDialog
      v-if="deleteTarget"
      variant="confirm"
      :title="deleteTitle"
      :message="deleteMessage"
      :confirm-label="t('shared.delete')"
      :loading-label="t('shared.deleting')"
      :loading="deleting"
      tone="danger"
      @confirm="emit('confirmDelete')"
      @close="emit('cancelDelete')"
    />
  </UiStack>
</template>
