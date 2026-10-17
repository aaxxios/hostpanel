/**
 * Endpoints behind the announcements store.
 *
 * No state lives here; every function rejects with an `ApiError` carrying the problem+json body.
 */

import { buildQuery, useApi } from '@/composables/useApi'
import type { Announcement, AnnouncementPayload } from '@/types/announcement'
import type { PagedResult } from '@/types/pagedresult'

/**
 * Endpoint functions for `/announcements`.
 *
 * @returns One typed function per endpoint.
 */
export const useAnnouncementsApi = () => {
  const { request } = useApi()

  /**
   * Lists announcements, one page.
   *
   * @param page - Page, 1-based.
   * @param pageSize - Items per page.
   * @returns The page and the total count.
   */
  const listAnnouncements = (page: number, pageSize: number): Promise<PagedResult<Announcement>> =>
    request<PagedResult<Announcement>>(`/announcements${buildQuery({ page, pageSize })}`)

  /**
   * Reads one announcement.
   *
   * @param id - Announcement id.
   * @returns The announcement.
   */
  const getAnnouncement = (id: number | string): Promise<Announcement> =>
    request<Announcement>(`/announcements/${id}`)

  /**
   * Creates an announcement.
   *
   * @param payload - Title, content and published flag.
   * @returns The new announcement's id.
   */
  const createAnnouncement = (payload: AnnouncementPayload): Promise<number> =>
    request<number>('/announcements', { method: 'POST', body: JSON.stringify(payload) })

  /**
   * Replaces an announcement.
   *
   * @param id - Announcement id.
   * @param payload - Title, content and published flag.
   * @returns Nothing.
   */
  const updateAnnouncement = (id: number | string, payload: AnnouncementPayload): Promise<void> =>
    request<void>(`/announcements/${id}`, { method: 'PUT', body: JSON.stringify(payload) })

  /**
   * Deletes an announcement.
   *
   * @param id - Announcement id.
   * @returns Nothing.
   */
  const deleteAnnouncement = (id: number | string): Promise<void> =>
    request<void>(`/announcements/${id}`, { method: 'DELETE' })

  return { listAnnouncements, getAnnouncement, createAnnouncement, updateAnnouncement, deleteAnnouncement }
}
