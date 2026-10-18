<script setup lang="ts">
/**
 * Displays a paginated log of emails sent to the current client.
 *
 * Read-only view — no actions available.
 */
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useClientEmailStore } from '@/stores/clientEmail'
import { formatDate } from '@/lib/format'
import UiAlert from '@/components/ui/UiAlert.vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiHeading from '@/components/ui/UiHeading.vue'
import UiPagination from '@/components/ui/UiPagination.vue'
import UiSkeleton from '@/components/ui/UiSkeleton.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiTable from '@/components/ui/UiTable.vue'
import UiTableBody from '@/components/ui/UiTableBody.vue'
import UiTableCell from '@/components/ui/UiTableCell.vue'
import UiTableHead from '@/components/ui/UiTableHead.vue'
import UiTableHeader from '@/components/ui/UiTableHeader.vue'
import UiTableRow from '@/components/ui/UiTableRow.vue'
import UiText from '@/components/ui/UiText.vue'
import { UiAlignItems, UiJustify } from '@/utils/ui/layout'
import { UiTextSize, UiTextTone } from '@/utils/ui/text'

const { t } = useI18n()

/** Current route, source of the client id. */
const route = useRoute()

/** Client email store, the only caller of the email-log endpoint. */
const store = useClientEmailStore()

/** Current 1-based page number. */
const page = ref(1)

/** Number of entries per page. */
const pageSize = 20

/** Numeric client ID from route params. */
const clientId = computed(() => Number(route.params.id))

/**
 * Loads email logs for the current page.
 *
 * @returns Promise that resolves when data is loaded.
 */
const load = async (): Promise<void> => {
  await store.fetchClientEmails(clientId.value, page.value, pageSize)
}

/**
 * Navigates to a specific page and reloads.
 *
 * @param p - Target page number.
 * @returns Nothing.
 */
const goToPage = (p: number): void => {
  page.value = p
  load()
}

onMounted(load)
</script>

<template>
  <UiStack>
    <UiHeading :level="1" variant="card">{{ t('clients.clientEmails.emails') }}</UiHeading>

    <UiAlert v-if="store.error" variant="error" data-testid="client-emails-error">
      {{ store.error }}
    </UiAlert>

    <UiStack v-if="store.loading && store.emails.length === 0" data-testid="client-emails-loading">
      <UiSkeleton :lines="3" />
      <UiText :tone="UiTextTone.Secondary">{{ t('clients.clientEmails.loadingEmails') }}</UiText>
    </UiStack>

    <UiCard v-else>
      <UiTable
        density="comfortable"
        :empty="store.emails.length === 0"
        :colspan="2"
        data-testid="client-emails-table"
      >
        <UiTableHead>
          <UiTableRow>
            <UiTableHeader>{{ t('shared.date') }}</UiTableHeader>
            <UiTableHeader>{{ t('shared.subject') }}</UiTableHeader>
          </UiTableRow>
        </UiTableHead>
        <UiTableBody divided>
          <UiTableRow v-for="email in store.emails" :key="email.id" hoverable>
            <UiTableCell muted>{{ formatDate(email.sentAt) }}</UiTableCell>
            <UiTableCell>{{ email.subject }}</UiTableCell>
          </UiTableRow>
        </UiTableBody>
        <template #empty>
          {{ t('shared.noRecordsFound') }}
        </template>
      </UiTable>
      <UiStack
        v-if="store.totalCount > pageSize"
        direction="row"
        :justify="UiJustify.Between"
        :align="UiAlignItems.Center"
      >
        <UiText :tone="UiTextTone.Muted" :size="UiTextSize.Sm">
          {{ t('clients.clientEmails.totalcountEmailValue', { totalCount: store.totalCount }) }}
        </UiText>
        <UiPagination
          :page="page"
          :total="store.totalCount"
          :page-size="pageSize"
          testid="client-emails-pagination"
          @update:page="goToPage"
        />
      </UiStack>
    </UiCard>
  </UiStack>
</template>
