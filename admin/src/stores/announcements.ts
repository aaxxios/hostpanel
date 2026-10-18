/**
 * Pinia store for announcements in the admin support module.
 *
 * Owns every request the announcements pages make and the sentence for a refusal, so the
 * pages hold no `catch`.
 */
import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useAnnouncementsApi } from '@/composables/apis/useAnnouncementsApi'
import { runAction } from '@/lib/runAction'
import type { Announcement, AnnouncementPayload } from '@/types/announcement'

/** Paginated listing and CRUD of announcements. */
export const useAnnouncementsStore = defineStore('announcements', () => {
  const api = useAnnouncementsApi()

  /** Paginated list of announcements. */
  const announcements = ref<Announcement[]>([])

  /** Total announcement count. */
  const totalCount = ref(0)

  /** Currently loaded announcement for editing. */
  const current = ref<Announcement | null>(null)

  /** True while a load or save request is in flight. */
  const loading = ref(false)

  /** Sentence for the last failed call, null when none. */
  const error = ref<string | null>(null)

  /** Per-field messages of the last refused save, keyed by camelCase field name. */
  const fieldErrors = ref<Record<string, string>>({})

  /** Current page (1-based). */
  const page = ref(1)

  /** Items per page. */
  const pageSize = ref(20)

  /**
   * Loads one page of announcements.
   *
   * @returns True when loaded; false when refused (see `error`).
   */
  const fetchAll = async (): Promise<boolean> => {
    loading.value = true
    const result = await runAction(error, () => api.listAnnouncements(page.value, pageSize.value))
    loading.value = false
    if (result.ok) {
      announcements.value = result.value.items
      totalCount.value = result.value.totalCount
    }
    return result.ok
  }

  /**
   * Loads one announcement into `current`.
   *
   * @param id - The announcement identifier.
   * @returns True when loaded; false when refused (see `error`).
   */
  const fetchById = async (id: number | string): Promise<boolean> => {
    loading.value = true
    current.value = null
    const result = await runAction(error, () => api.getAnnouncement(id))
    loading.value = false
    if (result.ok) {
      current.value = result.value
    }
    return result.ok
  }

  /**
   * Creates an announcement.
   *
   * @param payload - Announcement data.
   * @returns The new announcement's id, or null when refused (see `error` and `fieldErrors`).
   */
  const create = async (payload: AnnouncementPayload): Promise<number | null> => {
    loading.value = true
    const result = await runAction(error, () => api.createAnnouncement(payload), fieldErrors)
    loading.value = false
    return result.ok ? result.value : null
  }

  /**
   * Replaces an announcement.
   *
   * @param id - The announcement identifier.
   * @param payload - Updated data.
   * @returns True when saved; false when refused (see `error` and `fieldErrors`).
   */
  const update = async (id: number | string, payload: AnnouncementPayload): Promise<boolean> => {
    loading.value = true
    const result = await runAction(error, () => api.updateAnnouncement(id, payload), fieldErrors)
    loading.value = false
    return result.ok
  }

  /**
   * Deletes an announcement.
   *
   * @param id - The announcement identifier.
   * @returns True when deleted; false when refused (see `error`).
   */
  const remove = async (id: number | string): Promise<boolean> =>
    (await runAction(error, () => api.deleteAnnouncement(id))).ok

  return {
    announcements, totalCount, current, loading, error, fieldErrors, page, pageSize,
    fetchAll, fetchById, create, update, remove,
  }
})
