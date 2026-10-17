<script setup lang="ts">
/**
 * Advanced filter rows (up to a maximum, five by default).
 *
 * Each row is a field, a condition (exact / containing) and a value. Emits the rows that are
 * complete (field and value set) whenever any of them changes.
 */
import { ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import UiSelect from '@/components/ui/UiSelect.vue'
import { UiFilterCondition } from '@/utils/ui/filter'

const { t } = useI18n()

/** One filter row. */
export interface FilterRow {
  /** Key of the field being filtered; empty while unchosen. */
  field: string
  /** How the value is matched. */
  condition: UiFilterCondition
  /** Value to match. */
  value: string
}

/** One field the filters can target. */
export interface FieldOption {
  /** Field key sent to the screen. */
  value: string
  /** Caption shown in the dropdown. */
  label: string
}

/** Props for UiAdvancedFilters. */
const props = defineProps<{
  /** Available fields for the field dropdown. */
  fields: FieldOption[]
  /** Maximum number of filter rows. */
  max?: number
}>()

const emit = defineEmits<{
  /** Emitted with the complete rows on every change. */
  'update:filters': [filters: FilterRow[]]
}>()

/** Row limit, fixed when the component is created. */
const maxFilters = props.max ?? 5

/** The rows being edited; there is always at least one. */
const filters = ref<FilterRow[]>([
  { field: '', condition: UiFilterCondition.Exact, value: '' },
])

/** Options of the field dropdown: an empty "none" entry plus the caller's fields. */
const fieldOptions = [{ value: '', label: t('shared.none') }, ...props.fields]

/** Options of the condition dropdown. */
const conditionOptions = [
  { value: UiFilterCondition.Exact, label: t('ui.uiAdvancedFilters.exactMatch') },
  { value: UiFilterCondition.Contains, label: t('ui.uiAdvancedFilters.containing') },
]

/** Appends an empty row while below the limit. */
const addFilter = (): void => {
  if (filters.value.length < maxFilters) {
    filters.value.push({ field: '', condition: UiFilterCondition.Exact, value: '' })
  }
}

/**
 * Removes a row, keeping one empty row when the last is removed.
 *
 * @param i - Index of the row to remove.
 */
const removeFilter = (i: number): void => {
  filters.value.splice(i, 1)
  if (filters.value.length === 0) {
    filters.value.push({ field: '', condition: UiFilterCondition.Exact, value: '' })
  }
}

watch(filters, (val) => {
  emit('update:filters', val.filter(f => f.field && f.value))
}, { deep: true })
</script>

<template>
  <div class="space-y-3">
    <div
      v-for="(f, i) in filters"
      :key="i"
      class="bg-surface-field border border-border/50 rounded-xl p-3"
    >
      <!-- Header: label + remove -->
      <div class="flex items-center justify-between mb-2">
        <span class="text-[0.72rem] font-semibold uppercase tracking-[0.08em] text-text-muted">{{ t('ui.uiAdvancedFilters.filterValue', { value: i + 1 }) }}</span>
        <button
          v-if="filters.length > 1"
          type="button"
          :aria-label="t('shared.delete')"
          :data-testid="`advanced-filters-remove-${i}`"
          class="w-6 h-6 flex items-center justify-center text-text-muted hover:text-status-red transition-colors rounded-md hover:bg-surface-hover"
          @click="removeFilter(i)"
        >
          <svg class="w-3 h-3" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" aria-hidden="true"><line x1="18" y1="6" x2="6" y2="18" /><line x1="6" y1="6" x2="18" y2="18" /></svg>
        </button>
      </div>

      <!-- Controls: field + condition + value in an even grid -->
      <div class="grid grid-cols-3 gap-3">
        <UiSelect v-model="f.field" :options="fieldOptions" :placeholder="t('ui.uiAdvancedFilters.selectField')" />
        <UiSelect v-model="f.condition" :options="conditionOptions" :placeholder="t('ui.uiAdvancedFilters.exactMatch')" />
        <input
          v-model="f.value"
          type="text"
          :placeholder="t('ui.uiAdvancedFilters.enterValue')"
          :data-testid="`advanced-filters-value-${i}`"
          class="w-full bg-surface-field border border-border rounded-[10px] px-3 py-2 text-[0.82rem] text-text-primary placeholder:text-text-muted outline-none focus:border-primary-500/50 transition-colors"
        >
      </div>
    </div>

    <!-- Add filter -->
    <button
      v-if="filters.length < maxFilters"
      type="button"
      data-testid="advanced-filters-add"
      class="flex items-center gap-1.5 text-[0.78rem] text-primary-400 hover:text-primary-300 transition-colors px-1"
      @click="addFilter"
    >
      <svg class="w-3.5 h-3.5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true"><line x1="12" y1="5" x2="12" y2="19" /><line x1="5" y1="12" x2="19" y2="12" /></svg>
      {{ t('ui.uiAdvancedFilters.addFilter') }}
    </button>
  </div>
</template>
