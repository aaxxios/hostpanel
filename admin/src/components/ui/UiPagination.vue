<script setup lang="ts">
/**
 * Page switcher for a paged list.
 *
 * Shows previous/next and a window of page numbers with gaps. The parent owns the current page
 * (`v-model:page`) and loads it; this component only reports the choice. Renders nothing for a
 * single page.
 */
import { computed } from 'vue'
import { pageCount as countPages } from '@/lib/paging'
import { useI18n } from 'vue-i18n'
import UiButton from '@/components/ui/UiButton.vue'

const { t } = useI18n()

/** Props for UiPagination. */
const props = withDefaults(defineProps<{
  /** Current page, starting at 1 (two-way bound with `v-model:page`). */
  page: number
  /** Total number of items. */
  total: number
  /** Items per page. */
  pageSize: number
  /** Prefix of the `data-testid`s (`<prefix>-prev`, `<prefix>-next`, `<prefix>-page-<n>`). */
  testid?: string
}>(), {
  testid: 'pagination',
})

const emit = defineEmits<{
  /** Emitted with the page the user chose. */
  'update:page': [page: number]
}>()

/** Number of pages. */
const pageCount = computed<number>(() => countPages(props.total, props.pageSize))

/** Page numbers to show; `null` marks a gap between non-adjacent numbers. */
const entries = computed<(number | null)[]>(() => {
  const wanted = new Set<number>([1, pageCount.value, props.page - 1, props.page, props.page + 1])
  const pages = [...wanted].filter(n => n >= 1 && n <= pageCount.value).sort((a, b) => a - b)
  const result: (number | null)[] = []
  pages.forEach((n, index) => {
    const previous = pages[index - 1]
    if (previous !== undefined && n - previous > 1) {
      result.push(null)
    }
    result.push(n)
  })
  return result
})

/**
 * Reports a page choice if it differs from the current one and is in range.
 *
 * @param next - Requested page.
 */
const go = (next: number): void => {
  if (next !== props.page && next >= 1 && next <= pageCount.value) {
    emit('update:page', next)
  }
}
</script>

<template>
  <nav
    v-if="pageCount > 1"
    class="flex items-center gap-1"
    :aria-label="t('ui.pagination.navigation')"
  >
    <UiButton
      variant="ghost"
      size="sm"
      :disabled="page <= 1"
      :data-testid="`${testid}-prev`"
      @click="go(page - 1)"
    >
      {{ t('ui.pagination.previous') }}
    </UiButton>
    <template v-for="(entry, index) in entries" :key="index">
      <span v-if="entry === null" class="px-1 text-text-secondary" aria-hidden="true">…</span>
      <UiButton
        v-else
        :variant="entry === page ? 'primary' : 'ghost'"
        size="sm"
        :aria-current="entry === page ? 'page' : undefined"
        :aria-label="`${t('ui.pagination.page')} ${entry}`"
        :data-testid="`${testid}-page-${entry}`"
        @click="go(entry)"
      >
        {{ entry }}
      </UiButton>
    </template>
    <UiButton
      variant="ghost"
      size="sm"
      :disabled="page >= pageCount"
      :data-testid="`${testid}-next`"
      @click="go(page + 1)"
    >
      {{ t('ui.pagination.next') }}
    </UiButton>
  </nav>
</template>
