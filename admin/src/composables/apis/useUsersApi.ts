/**
 * One function per endpoint on the admin user-management surface (`/api/admin/users`).
 * No state lives here, no error handling.
 *
 * Callers are the users store, never a `.vue` file.
 */

import { buildQuery, useApi } from '@/composables/useApi'
import type { PagedResult } from '@/types/pagedresult'
import type { UpdateUserPayload, UserDetail, UserListItem } from '@/types/useraccount'
import { ApiPath } from '@/lib/apiPaths'
import { HttpMethod } from '@/types/http'

/**
 * Endpoint functions for admin user management.
 *
 * @returns One typed function per user endpoint the admin panel uses.
 */
export const useUsersApi = () => {
  const { request } = useApi()

  /**
   * Lists users, optionally searched.
   *
   * @param page - Page number (1-based).
   * @param pageSize - Items per page.
   * @param search - Optional search text.
   * @returns One page of users.
   */
  const list = (page: number, pageSize: number, search?: string): Promise<PagedResult<UserListItem>> =>
    request<PagedResult<UserListItem>>(`${ApiPath.AdminUsers}${buildQuery({ page, pageSize, search })}`)

  /**
   * Loads one user with the linked client accounts.
   *
   * @param id - Identity user id.
   * @returns The user detail.
   */
  const get = (id: string): Promise<UserDetail> => request<UserDetail>(`${ApiPath.AdminUsers}/${id}`)

  /**
   * Updates a user profile.
   *
   * @param id - Identity user id.
   * @param payload - Updated profile fields.
   * @returns Nothing.
   */
  const update = (id: string, payload: UpdateUserPayload): Promise<void> =>
    request<void>(`${ApiPath.AdminUsers}/${id}`, { method: HttpMethod.Put, body: JSON.stringify(payload) })

  /**
   * Deletes a user; client records are preserved.
   *
   * @param id - Identity user id.
   * @returns Nothing.
   */
  const remove = (id: string): Promise<void> => request<void>(`${ApiPath.AdminUsers}/${id}`, { method: HttpMethod.Delete })

  /**
   * Sends a password reset email to the user.
   *
   * @param id - Identity user id.
   * @returns Nothing.
   */
  const sendPasswordReset = (id: string): Promise<void> =>
    request<void>(`${ApiPath.AdminUsers}/${id}/reset-password`, { method: HttpMethod.Post })

  /**
   * Sets a new password for the user.
   *
   * @param id - Identity user id.
   * @param password - The new password.
   * @returns Nothing.
   */
  const changePassword = (id: string, password: string): Promise<void> =>
    request<void>(`${ApiPath.AdminUsers}/${id}/change-password`, { method: HttpMethod.Post, body: JSON.stringify({ password }) })

  return { list, get, update, remove, sendPasswordReset, changePassword }
}
