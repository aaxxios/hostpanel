/**
 * Unit tests for `useDownloadOptions`: the type entries, and the category entries led by
 * "top level" that follow the reactive category list.
 */
import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import { defineComponent, ref } from 'vue'
import type { Ref } from 'vue'
import { useDownloadOptions } from '@/composables/useDownloadOptions'
import type { DownloadCategory } from '@/types/downloadcategory'
import { DownloadType } from '@/types/downloaditem'
import { createTestI18n } from '../helpers/i18n'

/** A category with the given id and name. */
const category = (id: number, name: string): DownloadCategory => ({
  id,
  name,
  description: '',
  isHidden: false,
  parentCategoryId: null,
  downloadCount: 0,
})

/**
 * Runs the composable inside a mounted component (it needs the i18n instance).
 *
 * @param categories - Reactive category list handed to the composable.
 * @returns The composable's result.
 */
const run = (categories: Ref<DownloadCategory[]>) => {
  // Assigned synchronously by `setup()` during the mount below, before it is read.
  let result!: ReturnType<typeof useDownloadOptions>
  mount(
    defineComponent({
      setup() {
        result = useDownloadOptions(categories)
        return () => null
      },
    }),
    { global: { plugins: [createTestI18n()] } },
  )
  return result
}

describe('useDownloadOptions', () => {
  it('offers every download type with its label', () => {
    /** Options built over an empty category list. */
    const { typeOptions } = run(ref<DownloadCategory[]>([]))

    expect(typeOptions.value.map((option) => option.value)).toEqual([
      DownloadType.Zip,
      DownloadType.Executable,
      DownloadType.Pdf,
      DownloadType.Other,
    ])
    expect(typeOptions.value.find((option) => option.value === DownloadType.Pdf)?.label).toBe('PDF')
  })

  it('leads the categories with top level and follows the list', () => {
    /** The reactive list the composable follows. */
    const categories = ref<DownloadCategory[]>([category(3, 'Drivers')])
    /** Options built over that list. */
    const { categoryOptions } = run(categories)

    expect(categoryOptions.value.map((option) => option.value)).toEqual([0, 3])

    categories.value = [...categories.value, category(4, 'Tools')]

    expect(categoryOptions.value.map((option) => option.label)).toContain('Tools')
  })
})
