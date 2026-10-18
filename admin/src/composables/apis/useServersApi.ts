/**
 * Endpoints behind the servers store: provisioning servers and server groups.
 *
 * No state lives here; every function rejects with an `ApiError` carrying the problem+json body.
 */

import { useApi } from '@/composables/useApi'
import type {
  ServerDto,
  ServerGroupDto,
  ServerGroupPayload,
  ServerPayload,
  TestConnectionResultDto,
} from '@/types/server'

/**
 * Endpoint functions for `/admin/servers` and `/admin/server-groups`.
 *
 * @returns One typed function per endpoint.
 */
export const useServersApi = () => {
  const { request } = useApi()

  /**
   * Lists the provisioning servers.
   *
   * @returns Every server.
   */
  const listServers = (): Promise<ServerDto[]> => request<ServerDto[]>('/admin/servers')

  /**
   * Creates a server.
   *
   * @param payload - Server data.
   * @returns Nothing the caller needs.
   */
  const createServer = (payload: ServerPayload): Promise<void> =>
    request<void>('/admin/servers', { method: 'POST', body: JSON.stringify(payload) })

  /**
   * Updates a server.
   *
   * @param id - Server id.
   * @param payload - Updated server data.
   * @returns Nothing; the API answers 204.
   */
  const updateServer = (id: number, payload: ServerPayload): Promise<void> =>
    request<void>(`/admin/servers/${id}`, { method: 'PUT', body: JSON.stringify(payload) })

  /**
   * Deletes a server.
   *
   * @param id - Server id.
   * @returns Nothing; the API answers 204.
   */
  const deleteServer = (id: number): Promise<void> =>
    request<void>(`/admin/servers/${id}`, { method: 'DELETE' })

  /**
   * Probes a server's connection.
   *
   * @param id - Server id.
   * @returns The probe result.
   */
  const testServerConnection = (id: number): Promise<TestConnectionResultDto> =>
    request<TestConnectionResultDto>(`/admin/servers/${id}/test-connection`, { method: 'POST' })

  /**
   * Lists the server groups with their servers.
   *
   * @returns Every group.
   */
  const listServerGroups = (): Promise<ServerGroupDto[]> =>
    request<ServerGroupDto[]>('/admin/server-groups')

  /**
   * Creates a server group.
   *
   * @param payload - Group data.
   * @returns Nothing the caller needs.
   */
  const createServerGroup = (payload: ServerGroupPayload): Promise<void> =>
    request<void>('/admin/server-groups', { method: 'POST', body: JSON.stringify(payload) })

  /**
   * Updates a server group.
   *
   * @param id - Group id.
   * @param payload - Updated group data.
   * @returns Nothing; the API answers 204.
   */
  const updateServerGroup = (id: number, payload: ServerGroupPayload): Promise<void> =>
    request<void>(`/admin/server-groups/${id}`, { method: 'PUT', body: JSON.stringify(payload) })

  /**
   * Deletes a server group.
   *
   * @param id - Group id.
   * @returns Nothing; the API answers 204.
   */
  const deleteServerGroup = (id: number): Promise<void> =>
    request<void>(`/admin/server-groups/${id}`, { method: 'DELETE' })

  return {
    listServers,
    createServer,
    updateServer,
    deleteServer,
    testServerConnection,
    listServerGroups,
    createServerGroup,
    updateServerGroup,
    deleteServerGroup,
  }
}
