<script setup lang="ts">
/**
 * Cancellation requests — paged table of every service cancellation request.
 *
 * Shows date, product/service, client, reason, type, status and a delete action that asks first.
 * Reads and writes `stores/cancellationRequests`; a refusal shows the API's sentence.
 */
import { computed, onMounted, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import UiAlert from '@/components/ui/UiAlert.vue'
import UiBadge from '@/components/ui/UiBadge.vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiDialog from '@/components/ui/UiDialog.vue'
import UiEmptyState from '@/components/ui/UiEmptyState.vue'
import UiIcon from '@/components/ui/UiIcon.vue'
import UiIconButton from '@/components/ui/UiIconButton.vue'
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
import { formatDate } from '@/lib/format'
import { Icon } from '@/lib/icons'
import { cancellationStatusLabel, cancellationStatusTone, cancellationTypeLabel, cancellationTypeTone } from '@/lib/tones'
import { useCancellationRequestsStore } from '@/stores/cancellationRequests'
import { UiTone } from '@/utils/ui/tone'

const { t } = useI18n()

/** Cancellation requests store: the page, its paging and the load state. */
const store = useCancellationRequestsStore()

/** Longest reason shown in the table before it is cut with an ellipsis. */
const REASON_MAX_LENGTH = 60

/** ID of the request awaiting delete confirmation, null when nothing is staged. */
const deleteTarget = ref<number | null>(null)

/** True while the staged request is being deleted. */
const deleting = ref(false)

/** Whether the current page has any request to list. */
const hasRows = computed<boolean>(() => store.cancellations.length > 0)

/**
 * Shortens a reason for the table row, keeping the full text for the tooltip.
 *
 * @param text - The reason as the client wrote it.
 * @returns The text, cut with an ellipsis when longer than the limit.
 */
const shorten = (text: string): string =>
  text.length <= REASON_MAX_LENGTH ? text : `${text.slice(0, REASON_MAX_LENGTH)}…`

/**
 * Moves to a page of the list and loads it.
 *
 * @param page - Target page, 1-based.
 */
const goToPage = (page: number): void => {
  store.page = page
  void store.fetchAll()
}

/**
 * Stages a cancellation request for deletion. Only opens the prompt; {@link handleDelete} does the
 * work, so the question is asked by the app's own dialog rather than the browser's.
 *
 * @param id - The cancellation request ID to delete.
 */
const confirmDelete = (id: number): void => {
  deleteTarget.value = id
}

/**
 * Deletes the staged cancellation request and clears the prompt.
 *
 * The store puts a refusal into its own `error`, which the page renders, so nothing is caught here.
 *
 * @returns Promise that resolves when the delete attempt has finished.
 */
const handleDelete = async (): Promise<void> => {
  const id = deleteTarget.value
  if (id === null) {
    return
  }
  deleting.value = true
  await store.deleteRequest(id)
  deleting.value = false
  deleteTarget.value = null
}

onMounted(() => store.fetchAll())
</script>

<template>
  <UiStack>
    <UiPageHeader
      :title="t('clients.cancellationRequests.cancellationRequests')"
      :subtitle="t('clients.cancellationRequests.reviewAndProcessServiceCancellation')"
      data-testid="cancellations-header"
    />

    <UiAlert v-if="store.error" variant="error" data-testid="cancellations-error">
      {{ store.error }}
    </UiAlert>

    <UiCard v-if="hasRows || store.loading">
      <UiTable density="comfortable" :loading="store.loading && !hasRows" :colspan="7" data-testid="cancellations-table">
        <UiTableHead>
          <UiTableRow bordered>
            <UiTableHeader>{{ t('shared.date') }}</UiTableHeader>
            <UiTableHeader>{{ t('shared.productService') }}</UiTableHeader>
            <UiTableHeader>{{ t('shared.client') }}</UiTableHeader>
            <UiTableHeader>{{ t('shared.reason') }}</UiTableHeader>
            <UiTableHeader>{{ t('shared.type') }}</UiTableHeader>
            <UiTableHeader>{{ t('shared.status') }}</UiTableHeader>
            <UiTableHeader align="right">{{ t('shared.actions') }}</UiTableHeader>
          </UiTableRow>
        </UiTableHead>
        <UiTableBody>
          <UiTableRow
            v-for="item in store.cancellations"
            :key="item.id"
            hoverable
            bordered
            :data-testid="`cancellations-row-${item.id}`"
          >
            <UiTableCell muted>{{ formatDate(item.createdAt) }}</UiTableCell>
            <UiTableCell>{{ item.serviceName }}</UiTableCell>
            <UiTableCell>
              <UiLink variant="link" :to="`/clients/${item.clientId}`" :data-testid="`cancellations-client-${item.id}`">
                {{ item.clientName }}
              </UiLink>
            </UiTableCell>
            <UiTableCell>
              <UiText tone="secondary" :title="item.reason ?? ''">
                {{ item.reason ? shorten(item.reason) : '—' }}
              </UiText>
            </UiTableCell>
            <UiTableCell>
              <UiBadge :tone="cancellationTypeTone(item.type)" small>{{ cancellationTypeLabel(item.type) }}</UiBadge>
            </UiTableCell>
            <UiTableCell>
              <UiBadge :tone="cancellationStatusTone(item.status)" small>
                {{ cancellationStatusLabel(item.status) }}
              </UiBadge>
            </UiTableCell>
            <UiTableCell align="right">
              <UiIconButton
                :label="t('clients.cancellationRequests.deleteRequest')"
                size="sm"
                :data-testid="`cancellations-delete-${item.id}`"
                @click="confirmDelete(item.id)"
              >
                <UiIcon :markup="Icon.Trash" size="sm" />
              </UiIconButton>
            </UiTableCell>
          </UiTableRow>
        </UiTableBody>
      </UiTable>

      <UiStack direction="row" justify="between" align="center" class="p-3">
        <UiText tone="muted" size="xs">
          {{ t('clients.cancellationRequests.totalcountRequestValue', { totalCount: store.totalCount }) }}
        </UiText>
        <UiPagination
          :page="store.page"
          :total="store.totalCount"
          :page-size="store.pageSize"
          testid="cancellations-pagination"
          @update:page="goToPage"
        />
      </UiStack>
    </UiCard>

    <UiCard v-else-if="!store.error">
      <UiEmptyState
        :title="t('clients.cancellationRequests.noCancellationRequestsFound')"
        data-testid="cancellations-empty"
      >
        <template #icon>
          <UiIcon :markup="Icon.Remove" size="lg" />
        </template>
      </UiEmptyState>
    </UiCard>

    <UiDialog
      v-if="deleteTarget !== null"
      variant="confirm"
      :title="t('clients.cancellationRequests.deleteCancellationRequest')"
      :message="t('clients.cancellationRequests.areYouSureYouWant')"
      :confirm-label="t('shared.delete')"
      :loading-label="t('shared.deleting')"
      :loading="deleting"
      :tone="UiTone.Danger"
      data-testid="cancellations-delete-dialog"
      @confirm="handleDelete"
      @close="deleteTarget = null"
    />
  </UiStack>
</template>
