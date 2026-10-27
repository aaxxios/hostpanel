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
import type { ServerSshSettingsDto, ServerSshSettingsPayload, SshAgentTestResultDto } from '@/types/serverssh'
import { ApiPath } from '@/lib/apiPaths'
import { HttpMethod } from '@/types/http'

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
  const listServers = (): Promise<ServerDto[]> => request<ServerDto[]>(ApiPath.AdminServers)

  /**
   * Creates a server.
   *
   * @param payload - Server data.
   * @returns Nothing the caller needs.
   */
  const createServer = (payload: ServerPayload): Promise<void> =>
    request<void>(ApiPath.AdminServers, { method: HttpMethod.Post, body: JSON.stringify(payload) })

  /**
   * Updates a server.
   *
   * @param id - Server id.
   * @param payload - Updated server data.
   * @returns Nothing; the API answers 204.
   */
  const updateServer = (id: number, payload: ServerPayload): Promise<void> =>
    request<void>(`${ApiPath.AdminServers}/${id}`, { method: HttpMethod.Put, body: JSON.stringify(payload) })

  /**
   * Deletes a server.
   *
   * @param id - Server id.
   * @returns Nothing; the API answers 204.
   */
  const deleteServer = (id: number): Promise<void> =>
    request<void>(`${ApiPath.AdminServers}/${id}`, { method: HttpMethod.Delete })

  /**
   * Probes a server's connection.
   *
   * @param id - Server id.
   * @returns The probe result.
   */
  const testServerConnection = (id: number): Promise<TestConnectionResultDto> =>
    request<TestConnectionResultDto>(`${ApiPath.AdminServers}/${id}/test-connection`, { method: HttpMethod.Post })

  /**
   * Lists the server groups with their servers.
   *
   * @returns Every group.
   */
  const listServerGroups = (): Promise<ServerGroupDto[]> =>
    request<ServerGroupDto[]>(ApiPath.AdminServerGroups)

  /**
   * Creates a server group.
   *
   * @param payload - Group data.
   * @returns Nothing the caller needs.
   */
  const createServerGroup = (payload: ServerGroupPayload): Promise<void> =>
    request<void>(ApiPath.AdminServerGroups, { method: HttpMethod.Post, body: JSON.stringify(payload) })

  /**
   * Updates a server group.
   *
   * @param id - Group id.
   * @param payload - Updated group data.
   * @returns Nothing; the API answers 204.
   */
  const updateServerGroup = (id: number, payload: ServerGroupPayload): Promise<void> =>
    request<void>(`${ApiPath.AdminServerGroups}/${id}`, { method: HttpMethod.Put, body: JSON.stringify(payload) })

  /**
   * Deletes a server group.
   *
   * @param id - Group id.
   * @returns Nothing; the API answers 204.
   */
  const deleteServerGroup = (id: number): Promise<void> =>
    request<void>(`${ApiPath.AdminServerGroups}/${id}`, { method: HttpMethod.Delete })

  /**
   * Reads a server's SSH key management settings. The private key is never part of the answer.
   *
   * @param id - Server id.
   * @returns The settings; disabled defaults when never configured.
   */
  const getServerSshSettings = (id: number): Promise<ServerSshSettingsDto> =>
    request<ServerSshSettingsDto>(`${ApiPath.AdminServers}/${id}/ssh-settings`)

  /**
   * Saves a server's SSH key management settings.
   *
   * @param id - Server id.
   * @param payload - The settings; a null private key keeps the stored one.
   * @returns The saved settings.
   */
  const saveServerSshSettings = (id: number, payload: ServerSshSettingsPayload): Promise<ServerSshSettingsDto> =>
    request<ServerSshSettingsDto>(`${ApiPath.AdminServers}/${id}/ssh-settings`, { method: HttpMethod.Put, body: JSON.stringify(payload) })

  /**
   * Pings a server's SSH key agent and records the outcome.
   *
   * @param id - Server id.
   * @returns Whether the agent answered, and its version or the reason it did not.
   */
  const testServerSshAgent = (id: number): Promise<SshAgentTestResultDto> =>
    request<SshAgentTestResultDto>(`${ApiPath.AdminServers}/${id}/ssh-settings/test`, { method: HttpMethod.Post })

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
    getServerSshSettings,
    saveServerSshSettings,
    testServerSshAgent,
  }
}
