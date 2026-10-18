/**
 * Unit tests for DownloadItemEditDialog: closed without a download, filled from the download when
 * opened, saves a trimmed payload and shows the read-only download count.
 */
import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import DownloadItemEditDialog from '@/components/support/DownloadItemEditDialog.vue'
import { DownloadType } from '@/types/downloaditem'
import type { DownloadItem } from '@/types/downloaditem'
import { createTestI18n } from '../../helpers/i18n'

/** A download used by the tests. */
const download: DownloadItem = {
  id: 7,
  title: 'Setup',
  description: 'Installer',
  type: DownloadType.Zip,
  filename: 'setup.zip',
  categoryId: 3,
  categoryName: 'Drivers',
  downloadCount: 12,
  clientsOnly: true,
  productDownload: false,
  isHidden: false,
  createdAt: '2026-10-01T00:00:00Z',
}

/**
 * Mounts the dialog with the teleport inlined so its content is queryable.
 *
 * @param current - The download being edited, or null for a closed dialog.
 * @returns The wrapper.
 */
const mountDialog = (current: DownloadItem | null) =>
  mount(DownloadItemEditDialog, {
    props: { download: current, categories: [], saving: false },
    global: { plugins: [createTestI18n()], stubs: { teleport: true } },
  })

describe('DownloadItemEditDialog', () => {
  it('stays closed without a download', () => {
    expect(mountDialog(null).find('[role="dialog"]').exists()).toBe(false)
  })

  it('fills the form and shows the read-only count', () => {
    const wrapper = mountDialog(download)

    expect((wrapper.get('[data-testid="downloads-edit-download-title"]').element as HTMLInputElement).value).toBe('Setup')
    expect((wrapper.get('[data-testid="downloads-edit-download-count"]').element as HTMLInputElement).value).toBe('12')
    expect(wrapper.get('[data-testid="downloads-edit-download-count"]').attributes('disabled')).toBeDefined()
    expect(wrapper.get('[data-testid="downloads-edit-download-clients-only"]').attributes('aria-checked')).toBe('true')
  })

  it('saves the trimmed payload', async () => {
    const wrapper = mountDialog(download)

    await wrapper.get('[data-testid="downloads-edit-download-title"]').setValue('  Setup 2  ')
    await wrapper.get('[data-testid="downloads-edit-download-save"]').trigger('click')

    expect(wrapper.emitted('save')?.[0]).toEqual([
      {
        title: 'Setup 2',
        description: 'Installer',
        type: DownloadType.Zip,
        filename: 'setup.zip',
        categoryId: 3,
        clientsOnly: true,
        productDownload: false,
        isHidden: false,
      },
    ])
  })

  it('reports close from cancel', async () => {
    const wrapper = mountDialog(download)

    await wrapper.get('[data-testid="downloads-edit-download-cancel"]').trigger('click')

    expect(wrapper.emitted('close')).toHaveLength(1)
  })
})
