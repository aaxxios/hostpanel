/**
 * One function per endpoint on the users linked to a client (`/api/clients/{id}/users`) and
 * the Identity user search that feeds the add-user panel. No state lives here, no error
 * handling.
 *
 * Callers are the client users store, never a `.vue` file.
 */

import { buildQuery, useApi } from '@/composables/useApi'
import type { ClientUserItem, UserSearchHit } from '@/types/clientuser'

/**
 * Endpoint functions for the users of a client.
 *
 * @returns One typed function per endpoint the client users page uses.
 */
export const useClientUsersApi = () => {
  const { request } = useApi()

  /**
   * Lists the users linked to a client.
   *
   * @param clientId - The client id.
   * @returns The linked users.
   */
  const list = (clientId: string): Promise<ClientUserItem[]> =>
    request<ClientUserItem[]>(`/clients/${clientId}/users`)

  /**
   * Searches Identity users for the add-user panel.
   *
   * @param term - Search text.
   * @param pageSize - Maximum hits.
   * @returns The matching users, wrapped in the paged envelope.
   */
  const search = (term: string, pageSize: number): Promise<{ items: UserSearchHit[] }> =>
    request<{ items: UserSearchHit[] }>(`/admin/users${buildQuery({ search: term, pageSize, page: 1 })}`)

  /**
   * Links an existing user to the client.
   *
   * @param clientId - The client id.
   * @param userId - Identity user id.
   * @param permissions - Permission bit-flags.
   * @returns Nothing.
   */
  const add = (clientId: string, userId: string, permissions: number): Promise<void> =>
    request<void>(`/clients/${clientId}/users`, { method: 'POST', body: JSON.stringify({ memberUserId: userId, permissions }) })

  /**
   * Invites a new user by email and links them to the client.
   *
   * @param clientId - The client id.
   * @param payload - Email, names and permission bit-flags.
   * @returns Nothing.
   */
  const invite = (clientId: string, payload: Record<string, unknown>): Promise<void> =>
    request<void>(`/clients/${clientId}/users/invite`, { method: 'POST', body: JSON.stringify(payload) })

  /**
   * Replaces a non-owner user permissions.
   *
   * @param clientId - The client id.
   * @param userId - Identity user id.
   * @param permissions - New permission bit-flags.
   * @returns Nothing.
   */
  const updatePermissions = (clientId: string, userId: string, permissions: number): Promise<void> =>
    request<void>(`/clients/${clientId}/users/${userId}/permissions`, {
      method: 'PUT',
      body: JSON.stringify({ permissions }),
    })

  /**
   * Transfers ownership of the client to a linked user.
   *
   * @param clientId - The client id.
   * @param userId - Identity user id of the new owner.
   * @returns Nothing.
   */
  const makeOwner = (clientId: string, userId: string): Promise<void> =>
    request<void>(`/clients/${clientId}/users/${userId}/make-owner`, { method: 'POST' })

  /**
   * Unlinks a user from the client.
   *
   * @param clientId - The client id.
   * @param userId - Identity user id.
   * @returns Nothing.
   */
  const remove = (clientId: string, userId: string): Promise<void> =>
    request<void>(`/clients/${clientId}/users/${userId}`, { method: 'DELETE' })

  return { list, search, add, invite, updatePermissions, makeOwner, remove }
}
