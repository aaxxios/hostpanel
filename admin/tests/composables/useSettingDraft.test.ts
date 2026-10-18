/**
 * Tests for useSettingDraft: the edit shows until the stored value catches up, a missing row
 * saves nothing, and the fallback fills an empty control.
 */
import { describe, expect, it, vi } from 'vitest'
import { nextTick, ref } from 'vue'
import { useSettingDraft } from '@/composables/useSettingDraft'
import type { Setting } from '@/types/setting'

/**
 * Builds the settings list the composable watches.
 *
 * @param value - Stored value of the row under test.
 * @returns A ref holding one row.
 */
const settingsWith = (value: string) => ref<Setting[]>([{ id: 7, key: 'portal.template', value }])

describe('useSettingDraft', () => {
  it('shows the stored value, and the fallback when the row is empty or missing', () => {
    expect(useSettingDraft(settingsWith('nova'), 'portal.template').value.value).toBe('nova')
    expect(useSettingDraft(settingsWith(''), 'portal.template', 'aurora').value.value).toBe('aurora')
    expect(useSettingDraft(ref<Setting[]>([]), 'portal.template', 'aurora').value.value).toBe('aurora')
    expect(useSettingDraft(ref<Setting[]>([]), 'portal.template').setting.value).toBeUndefined()
  })

  it('shows an edit instead of the stored value and marks the draft dirty', () => {
    const draft = useSettingDraft(settingsWith('nova'), 'portal.template')
    expect(draft.dirty.value).toBe(false)

    draft.value.value = 'classic'

    expect(draft.value.value).toBe('classic')
    expect(draft.dirty.value).toBe(true)
  })

  it('is not dirty when the edit equals the stored value', () => {
    const draft = useSettingDraft(settingsWith('nova'), 'portal.template')

    draft.value.value = 'nova'

    expect(draft.dirty.value).toBe(false)
  })

  it('hands the row id and the edit to the saver, once there is something to save', () => {
    const draft = useSettingDraft(settingsWith('nova'), 'portal.template')
    const save = vi.fn()

    draft.submit(save)
    expect(save).not.toHaveBeenCalled()

    draft.value.value = 'classic'
    draft.submit(save)
    expect(save).toHaveBeenCalledWith(7, 'classic')
  })

  it('saves nothing for a row that was never seeded', () => {
    const draft = useSettingDraft(ref<Setting[]>([]), 'portal.template')
    const save = vi.fn()

    draft.value.value = 'classic'
    draft.submit(save)

    expect(save).not.toHaveBeenCalled()
  })

  it('keeps the edit across a failed save and drops it once the stored value catches up', async () => {
    const settings = settingsWith('nova')
    const draft = useSettingDraft(settings, 'portal.template')
    draft.value.value = 'classic'

    // The save was refused: the list is reloaded unchanged and the edit must survive.
    settings.value = [{ id: 7, key: 'portal.template', value: 'nova' }]
    await nextTick()
    expect(draft.value.value).toBe('classic')
    expect(draft.dirty.value).toBe(true)

    // The save went through: the stored value now equals the edit, so the draft clears.
    settings.value = [{ id: 7, key: 'portal.template', value: 'classic' }]
    await nextTick()
    expect(draft.dirty.value).toBe(false)
    expect(draft.value.value).toBe('classic')
  })
})
