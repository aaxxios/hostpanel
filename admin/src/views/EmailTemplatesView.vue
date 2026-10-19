<script setup lang="ts">
/**
 * Email templates view — read-only list of the notification templates.
 *
 * Reads `stores/settings`; a refused load shows the API's sentence.
 */
import { onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import UiAlert from '@/components/ui/UiAlert.vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiPageHeader from '@/components/ui/UiPageHeader.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiTable from '@/components/ui/UiTable.vue'
import UiTableBody from '@/components/ui/UiTableBody.vue'
import UiTableCell from '@/components/ui/UiTableCell.vue'
import UiTableHead from '@/components/ui/UiTableHead.vue'
import UiTableHeader from '@/components/ui/UiTableHeader.vue'
import UiTableRow from '@/components/ui/UiTableRow.vue'
import { useSettingsStore } from '@/stores/settings'

const { t } = useI18n()

/** Settings store: holds the templates and the load state. */
const store = useSettingsStore()

onMounted(() => store.fetchEmailTemplates())
</script>

<template>
  <UiStack>
    <UiPageHeader :title="t('settings.emailTemplates.emailTemplates')" data-testid="email-templates-header" />

    <UiAlert v-if="store.error" variant="error" data-testid="email-templates-error">
      {{ store.error }}
    </UiAlert>

    <UiCard v-else>
      <UiTable
        density="comfortable"
        :loading="store.loading"
        :empty="!store.loading && store.emailTemplates.length === 0"
        :colspan="3"
        data-testid="email-templates-table"
      >
        <template #empty>
          {{ t('settings.emailTemplates.noTemplatesFound') }}
        </template>
        <UiTableHead>
          <UiTableRow bordered>
            <UiTableHeader>{{ t('shared.id') }}</UiTableHeader>
            <UiTableHeader>{{ t('shared.name') }}</UiTableHeader>
            <UiTableHeader>{{ t('shared.subject') }}</UiTableHeader>
          </UiTableRow>
        </UiTableHead>
        <UiTableBody>
          <UiTableRow
            v-for="template in store.emailTemplates"
            :key="template.id"
            hoverable
            bordered
            :data-testid="`email-templates-row-${template.id}`"
          >
            <UiTableCell muted>{{ template.id }}</UiTableCell>
            <UiTableCell>{{ template.slug }}</UiTableCell>
            <UiTableCell muted>{{ template.subject }}</UiTableCell>
          </UiTableRow>
        </UiTableBody>
      </UiTable>
    </UiCard>
  </UiStack>
</template>
