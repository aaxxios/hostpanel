<script setup lang="ts">
/**
 * Read-only table displaying domain reminder history.
 * Shows date, reminder type, recipient, and sent timestamp for each reminder.
 */
import { useI18n } from 'vue-i18n'
import UiCard from '@/components/ui/UiCard.vue'
import UiHeading from '@/components/ui/UiHeading.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiTable from '@/components/ui/UiTable.vue'
import UiTableBody from '@/components/ui/UiTableBody.vue'
import UiTableCell from '@/components/ui/UiTableCell.vue'
import UiTableHead from '@/components/ui/UiTableHead.vue'
import UiTableHeader from '@/components/ui/UiTableHeader.vue'
import UiTableRow from '@/components/ui/UiTableRow.vue'
import { formatDate } from '@/lib/format'
import type { DomainReminderItem } from '@/types/domain'

const { t } = useI18n()

/** Props for DomainRemindersTable. */
defineProps<{
  /** List of domain reminder history entries. */
  reminders: DomainReminderItem[]
}>()
</script>

<template>
  <UiCard padding="md">
    <UiStack>
      <UiHeading :level="2" variant="section">
        {{ t('clients.domainRemindersTable.domainReminderHistory') }}
      </UiHeading>
      <UiTable
        density="compact"
        :empty="reminders.length === 0"
        :colspan="4"
        data-testid="clients-domain-reminders-table"
      >
        <UiTableHead>
          <UiTableRow>
            <UiTableHeader>{{ t('shared.date') }}</UiTableHeader>
            <UiTableHeader>{{ t('clients.domainRemindersTable.reminder') }}</UiTableHeader>
            <UiTableHeader>{{ t('shared.to') }}</UiTableHeader>
            <UiTableHeader>{{ t('clients.domainRemindersTable.sent') }}</UiTableHeader>
          </UiTableRow>
        </UiTableHead>
        <UiTableBody divided>
          <UiTableRow v-for="reminder in reminders" :key="reminder.id" hoverable>
            <UiTableCell muted>{{ formatDate(reminder.sentAt) }}</UiTableCell>
            <UiTableCell>{{ reminder.reminderType }}</UiTableCell>
            <UiTableCell>{{ reminder.sentTo }}</UiTableCell>
            <UiTableCell muted>{{ formatDate(reminder.sentAt) }}</UiTableCell>
          </UiTableRow>
        </UiTableBody>
        <template #empty>
          {{ t('shared.noRecordsFound') }}
        </template>
      </UiTable>
    </UiStack>
  </UiCard>
</template>
