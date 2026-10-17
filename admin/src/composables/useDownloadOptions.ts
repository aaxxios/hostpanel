/**
 * Option lists of the downloads page, shared by the add form and the edit dialogs so the two
 * can never disagree about what a file type is called or how a category is offered.
 */
import { computed } from 'vue'
import type { ComputedRef, Ref } from 'vue'
import { useI18n } from 'vue-i18n'
import type { DownloadCategory } from '@/types/downloadcategory'
import { DownloadType } from '@/types/downloaditem'

/** One entry of a select: the value it stands for and its label. */
export interface DownloadOption<T> {
  /** Value the select holds when this entry is chosen. */
  value: T
  /** Translated label. */
  label: string
}

/** What `useDownloadOptions` offers. */
export interface DownloadOptions {
  /** File type entries. */
  typeOptions: ComputedRef<DownloadOption<DownloadType>[]>
  /** Category entries, led by "top level" (value 0). */
  categoryOptions: ComputedRef<DownloadOption<number>[]>
}

/**
 * Builds the download type and category options in the operator's language.
 *
 * @param categories - The categories to offer (reactive, so new ones appear without a reload).
 * @returns Computed option lists for the selects.
 */
export const useDownloadOptions = (categories: Ref<DownloadCategory[]>): DownloadOptions => {
  const { t } = useI18n()

  /** File type entries. */
  const typeOptions = computed<DownloadOption<DownloadType>[]>(() => [
    { value: DownloadType.Zip, label: t('support.downloads.zipFile') },
    { value: DownloadType.Executable, label: t('support.downloads.executable') },
    { value: DownloadType.Pdf, label: t('support.downloads.pdf') },
    { value: DownloadType.Other, label: t('support.downloads.other') },
  ])

  /** Category entries, led by "top level" (value 0). */
  const categoryOptions = computed<DownloadOption<number>[]>(() => [
    { value: 0, label: t('support.predefinedReplies.topLevel') },
    ...categories.value.map((category) => ({ value: category.id, label: category.name })),
  ])

  return { typeOptions, categoryOptions }
}
