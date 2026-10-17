<script setup lang="ts">
/**
 * Client statement: the entries, debits, credits and running balance of one client account.
 */
import { computed, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import ReportField from '@/components/reports/ReportField.vue'
import ReportFilterBar from '@/components/reports/ReportFilterBar.vue'
import ReportPage from '@/components/reports/ReportPage.vue'
import ReportTable from '@/components/reports/ReportTable.vue'
import UiClientSelect from '@/components/ui/UiClientSelect.vue'
import UiDateRangePicker from '@/components/ui/UiDateRangePicker.vue'
import UiText from '@/components/ui/UiText.vue'
import { useReport } from '@/composables/useReport'
import { useReportClients } from '@/composables/useReportClients'
import { formatReportAmount } from '@/lib/money'
import { reportPath } from '@/lib/reportsQuery'
import type { ReportClientStatement } from '@/types/reportclientstatement'
import { ReportCellFormat } from '@/types/reportcolumn'
import type { ReportColumn } from '@/types/reportcolumn'
import { UiAlign } from '@/utils/ui/table'

const { t } = useI18n()

const { clients } = useReportClients()

/** The client chosen in the picker; empty until chosen. */
const selectedClient = ref<string | number | null>('')

/** The date range chosen, or none for the whole account. */
const dateRange = ref<[string, string] | null>(null)

const { data, load } = useReport<ReportClientStatement>(
  () => reportPath('/reports/client-statement', {
    clientId: selectedClient.value,
    from: dateRange.value?.[0],
    to: dateRange.value?.[1],
  }),
  false,
)

/** Loads the statement once a client is chosen. */
const generate = async (): Promise<void> => {
  if (selectedClient.value) {
    await load()
  }
}

/** Opens the print dialog. */
const print = (): void => {
  window.print()
}

/** Statement columns. */
const columns = computed<ReportColumn[]>(() => [
  { key: 'type', label: t('shared.type'), format: ReportCellFormat.Status },
  { key: 'date', label: t('shared.date') },
  { key: 'description', label: t('shared.description') },
  { key: 'debit', label: t('reports.clientStatement.debit'), align: UiAlign.Right, format: ReportCellFormat.Money },
  { key: 'credit', label: t('reports.directDebit.credit'), align: UiAlign.Right, format: ReportCellFormat.Money },
  { key: 'balance', label: t('shared.balance'), align: UiAlign.Right, format: ReportCellFormat.Money },
])
</script>

<template>
  <ReportPage
    :title="t('reports.clientStatement.clientAccountRegisterBalance')"
    :description="t('reports.clientStatement.thisReportProvidesAStatement')"
  >
    <template #filters>
      <ReportFilterBar printable @generate="generate" @print="print">
        <template #fields>
          <ReportField :label="t('reports.clientStatement.selectClient')" width="grow">
            <UiClientSelect v-model="selectedClient" :clients="clients" :placeholder="t('shared.selectAClient')" />
          </ReportField>
          <ReportField :label="t('shared.dateRange')" width="grow">
            <UiDateRangePicker v-model="dateRange" :placeholder="t('shared.selectDateRange')" />
          </ReportField>
        </template>
      </ReportFilterBar>
    </template>

    <template v-if="data">
      <UiText tone="secondary" size="md" class="text-right" data-testid="report-previous-balance">
        {{ t('reports.clientStatement.previousBalance') }} {{ formatReportAmount(data.previousBalance) }}
      </UiText>
      <ReportTable
        :columns="columns"
        :rows="data.lines"
        :empty-text="t('reports.clientStatement.noEntriesForThisPeriod')"
        :totals="{ balance: data.endingBalance }"
        :totals-label="t('reports.clientStatement.endingBalance')"
      />
    </template>
  </ReportPage>
</template>
