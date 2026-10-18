/**
 * Unit tests for DownloadCategoryEditDialog: closed without a category, filled from the category
 * when opened, saves a trimmed payload (top level as null) and blocks an empty name.
 */
import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import DownloadCategoryEditDialog from '@/components/support/DownloadCategoryEditDialog.vue'
import type { DownloadCategory } from '@/types/downloadcategory'
import { createTestI18n } from '../../helpers/i18n'

/**
 * Builds a category.
 *
 * @param id - Category id.
 * @param name - Category name.
 * @returns A top-level category with two downloads.
 */
const category = (id: number, name: string): DownloadCategory => ({
  id,
  name,
  description: 'Drivers and tools',
  isHidden: false,
  parentCategoryId: null,
  downloadCount: 2,
})

/**
 * Mounts the dialog with the teleport inlined so its content is queryable.
 *
 * @param current - The category being edited, or null for a closed dialog.
 * @returns The wrapper.
 */
const mountDialog = (current: DownloadCategory | null) =>
  mount(DownloadCategoryEditDialog, {
    props: { category: current, categories: [category(1, 'Drivers'), category(2, 'Tools')], saving: false },
    global: { plugins: [createTestI18n()], stubs: { teleport: true } },
  })

describe('DownloadCategoryEditDialog', () => {
  it('stays closed without a category', () => {
    expect(mountDialog(null).find('[role="dialog"]').exists()).toBe(false)
  })

  it('fills the form from the category', () => {
    const wrapper = mountDialog(category(1, 'Drivers'))

    expect((wrapper.get('[data-testid="downloads-edit-category-name"]').element as HTMLInputElement).value).toBe('Drivers')
    expect((wrapper.get('[data-testid="downloads-edit-category-description"]').element as HTMLInputElement).value).toBe('Drivers and tools')
  })

  it('saves the trimmed payload with top level as a null parent', async () => {
    const wrapper = mountDialog(category(1, 'Drivers'))

    await wrapper.get('[data-testid="downloads-edit-category-name"]').setValue('  Drivers 2  ')
    await wrapper.get('[data-testid="downloads-edit-category-save"]').trigger('click')

    expect(wrapper.emitted('save')?.[0]).toEqual([
      { name: 'Drivers 2', description: 'Drivers and tools', isHidden: false, parentCategoryId: null },
    ])
  })

  it('blocks saving an empty name and reports close', async () => {
    const wrapper = mountDialog(category(1, 'Drivers'))

    await wrapper.get('[data-testid="downloads-edit-category-name"]').setValue('   ')
    expect(wrapper.get('[data-testid="downloads-edit-category-save"]').attributes('disabled')).toBeDefined()

    await wrapper.get('[data-testid="downloads-edit-category-cancel"]').trigger('click')
    expect(wrapper.emitted('close')).toHaveLength(1)
  })
})
