<script setup lang="ts">
/**
 * Filter card for the reports that take a year, optionally with a month and a display currency.
 *
 * The chosen values are `v-model:year`, `v-model:month` and `v-model:currency`; the button asks
 * the screen to load.
 */
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import ReportField from '@/components/reports/ReportField.vue'
import ReportFilterBar from '@/components/reports/ReportFilterBar.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import { monthOptions, yearOptions } from '@/lib/reportsPeriod'
import { REPORT_CURRENCY_OPTIONS, ReportCurrency } from '@/lib/money'

const { t } = useI18n()

/** Props for ReportPeriodFilter. */
const props = withDefaults(defineProps<{
  /** Whether the month dropdown shows. */
  showMonth?: boolean
  /** Whether the currency dropdown shows. */
  showCurrency?: boolean
  /** Years before the current one on offer. */
  yearsBack?: number
  /** Years after the current one on offer. */
  yearsAhead?: number
  /** Caption of the main button; "Generate" when omitted. */
  generateLabel?: string
}>(), {
  showMonth: false,
  showCurrency: false,
  yearsBack: 4,
  yearsAhead: 0,
  generateLabel: undefined,
})

const emit = defineEmits<{
  /** The operator asked for the report. */
  generate: []
}>()

/** Chosen year. */
const year = defineModel<number>('year', { required: true })

/** Chosen month, 1 to 12. */
const month = defineModel<number>('month', { default: 1 })

/** Chosen display currency. */
const currency = defineModel<ReportCurrency>('currency', { default: ReportCurrency.Usd })

/** Month dropdown choices. */
const months = computed(() => monthOptions())

/** Year dropdown choices. */
const years = computed(() => yearOptions(props.yearsBack, props.yearsAhead))
</script>

<template>
  <ReportFilterBar :generate-label="generateLabel" @generate="emit('generate')">
    <template #fields>
      <ReportField v-if="showMonth" :label="t('shared.month')">
        <UiSelect v-model="month" :options="months" :placeholder="t('shared.month')" data-testid="report-month" />
      </ReportField>
      <ReportField :label="t('shared.year')" width="sm">
        <UiSelect v-model="year" :options="years" :placeholder="t('shared.year')" data-testid="report-year" />
      </ReportField>
      <ReportField v-if="showCurrency" :label="t('shared.currency')">
        <UiSelect v-model="currency" :options="REPORT_CURRENCY_OPTIONS" :placeholder="t('shared.currency')" data-testid="report-currency" />
      </ReportField>
    </template>
  </ReportFilterBar>
</template>
