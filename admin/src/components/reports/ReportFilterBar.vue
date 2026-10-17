<script setup lang="ts">
/**
 * Filter card of a report: the controls in the `fields` slot, extra rows in the default slot, and
 * a row of actions (generate, clear, export, print) the screen switches on.
 */
import { useI18n } from 'vue-i18n'
import UiButton from '@/components/ui/UiButton.vue'
import UiFilterCard from '@/components/ui/UiFilterCard.vue'
import UiStack from '@/components/ui/UiStack.vue'

const { t } = useI18n()

/** Props for ReportFilterBar. */
withDefaults(defineProps<{
  /** Caption of the main button; "Generate" when omitted. */
  generateLabel?: string
  /** Shows a Clear button. */
  clearable?: boolean
  /** Shows an Export CSV button. */
  exportable?: boolean
  /** Shows a Print button. */
  printable?: boolean
}>(), {
  generateLabel: undefined,
  clearable: false,
  exportable: false,
  printable: false,
})

const emit = defineEmits<{
  /** The operator asked for the report with the chosen filters. */
  generate: []
  /** The operator cleared the filters. */
  clear: []
  /** The operator asked for the CSV export. */
  export: []
  /** The operator asked to print. */
  print: []
}>()
</script>

<template>
  <UiFilterCard data-testid="report-filters">
    <template #fields>
      <UiStack direction="row" gap="md" align="end" wrap>
        <slot name="fields" />
      </UiStack>
    </template>

    <slot />

    <template #actions>
      <UiStack direction="row" gap="sm" justify="center" align="center" wrap>
        <UiButton variant="primary" data-testid="report-generate" @click="emit('generate')">
          {{ generateLabel ?? t('shared.generate') }}
        </UiButton>
        <UiButton v-if="clearable" variant="ghost" data-testid="report-clear" @click="emit('clear')">
          {{ t('shared.clear') }}
        </UiButton>
        <UiButton v-if="exportable" variant="secondary" data-testid="report-export" @click="emit('export')">
          {{ t('shared.exportCsv') }}
        </UiButton>
        <UiButton v-if="printable" variant="secondary" data-testid="report-print" @click="emit('print')">
          {{ t('shared.print') }}
        </UiButton>
      </UiStack>
    </template>
  </UiFilterCard>
</template>
