<script setup lang="ts">
/**
 * Announcements list: every announcement with create, edit and delete actions, one page of
 * records at a time.
 */
import { computed, onMounted, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import SupportRowActions from '@/components/support/SupportRowActions.vue'
import UiAlert from '@/components/ui/UiAlert.vue'
import UiBadge from '@/components/ui/UiBadge.vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiDialog from '@/components/ui/UiDialog.vue'
import UiEmptyState from '@/components/ui/UiEmptyState.vue'
import UiLink from '@/components/ui/UiLink.vue'
import UiPageHeader from '@/components/ui/UiPageHeader.vue'
import UiPagination from '@/components/ui/UiPagination.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiTable from '@/components/ui/UiTable.vue'
import UiTableBody from '@/components/ui/UiTableBody.vue'
import UiTableCell from '@/components/ui/UiTableCell.vue'
import UiTableHead from '@/components/ui/UiTableHead.vue'
import UiTableHeader from '@/components/ui/UiTableHeader.vue'
import UiTableRow from '@/components/ui/UiTableRow.vue'
import UiText from '@/components/ui/UiText.vue'
import { formatDateTimeCompact } from '@/lib/format'
import { useAnnouncementsStore } from '@/stores/announcements'
import { pageCount } from '@/lib/paging'
import { UiButtonVariant } from '@/utils/ui/button'
import { UiTableDensity } from '@/utils/ui/injection'
import { UiTone } from '@/utils/ui/tone'

const { t } = useI18n()

const store = useAnnouncementsStore()

/** Number of columns of the table (width of the status row). */
const COLUMN_COUNT = 4

/** ID of the announcement waiting for delete confirmation; null when no dialog is open. */
const deleteTarget = ref<number | null>(null)

/** Whether a delete request is in flight. */
const deleting = ref(false)

/** Number of pages for the record count. */
const totalPages = computed<number>(() => pageCount(store.totalCount, store.pageSize))

/**
 * Loads another page.
 *
 * @param page - The page to show, 1-based.
 */
const goToPage = (page: number): void => {
  store.page = page
  store.fetchAll()
}

/**
 * Deletes the announcement the dialog asked about, then reloads the list.
 */
const handleDelete = async (): Promise<void> => {
  if (deleteTarget.value === null) {
    return
  }
  deleting.value = true
  const removed = await store.remove(deleteTarget.value)
  if (removed) {
    deleteTarget.value = null
    await store.fetchAll()
  }
  deleting.value = false
}

onMounted(() => {
  store.fetchAll()
})
</script>

<template>
  <UiStack>
    <UiPageHeader
      :title="t('shared.announcements')"
      :subtitle="t('support.announcementsList.manageClientFacingAnnouncements')"
    >
      <template #actions>
        <UiLink
          to="/support/announcements/new"
          :variant="UiButtonVariant.Primary"
          data-testid="announcements-add"
        >
          {{ t('support.announcementsList.addNewAnnouncement') }}
        </UiLink>
      </template>
    </UiPageHeader>

    <UiStack direction="row" justify="between" wrap>
      <UiText tone="muted" size="sm" data-testid="announcements-count">
        {{ t('shared.totalcountRecordsFound', { totalCount: store.totalCount }) }}
      </UiText>
      <UiText tone="muted" size="sm">
        {{ t('shared.pagePageOfTotalpages', { page: store.page, totalPages }) }}
      </UiText>
    </UiStack>

    <UiAlert v-if="store.error" variant="error" data-testid="announcements-error">{{ store.error }}</UiAlert>

    <UiCard class="overflow-x-auto">
      <UiTable
        :density="UiTableDensity.Comfortable"
        :loading="store.loading && store.announcements.length === 0"
        :empty="!store.loading && store.announcements.length === 0"
        :colspan="COLUMN_COUNT"
        data-testid="announcements-table"
      >
        <UiTableHead>
          <UiTableRow>
            <UiTableHeader>{{ t('shared.date') }}</UiTableHeader>
            <UiTableHeader>{{ t('shared.title') }}</UiTableHeader>
            <UiTableHeader>{{ t('shared.published') }}</UiTableHeader>
            <UiTableHeader align="right">{{ t('shared.actions') }}</UiTableHeader>
          </UiTableRow>
        </UiTableHead>
        <UiTableBody divided>
          <UiTableRow
            v-for="announcement in store.announcements"
            :key="announcement.id"
            hoverable
            :data-testid="`announcements-row-${announcement.id}`"
          >
            <UiTableCell muted>{{ formatDateTimeCompact(announcement.createdAt) }}</UiTableCell>
            <UiTableCell>{{ announcement.title }}</UiTableCell>
            <UiTableCell>
              <UiBadge :tone="announcement.isPublished ? UiTone.Success : UiTone.Neutral">
                {{ announcement.isPublished ? t('shared.yes') : t('shared.no') }}
              </UiBadge>
            </UiTableCell>
            <UiTableCell align="right">
              <SupportRowActions
                :edit-to="`/support/announcements/${announcement.id}/edit`"
                :testid-prefix="`announcements-${announcement.id}`"
                @delete="deleteTarget = announcement.id"
              />
            </UiTableCell>
          </UiTableRow>
        </UiTableBody>
        <template #empty>
          <UiEmptyState
            :title="t('support.announcementsList.noAnnouncementsFound')"
            :description="t('support.announcementsList.createYourFirstAnnouncementTo')"
          />
        </template>
      </UiTable>
    </UiCard>

    <UiPagination
      :page="store.page"
      :total="store.totalCount"
      :page-size="store.pageSize"
      testid="announcements-pagination"
      @update:page="goToPage"
    />

    <UiDialog
      variant="confirm"
      :open="deleteTarget !== null"
      :title="t('support.announcementsList.deleteAnnouncement')"
      :message="t('support.announcementsList.areYouSureYouWant')"
      :confirm-label="t('shared.delete')"
      :loading-label="t('shared.deleting')"
      :loading="deleting"
      tone="danger"
      @confirm="handleDelete"
      @close="deleteTarget = null"
    />
  </UiStack>
</template>
