/**
 * Endpoints behind the plugins store: the installed list, ZIP install, removal and the restart.
 *
 * No state lives here; every function rejects with an `ApiError` carrying the problem+json body.
 */

import { useApi } from '@/composables/useApi'
import type { PluginActionResultDto, PluginListItemDto } from '@/types/plugin'

/**
 * Endpoint functions for `/admin/plugins`.
 *
 * @returns One typed function per endpoint.
 */
export const usePluginsApi = () => {
  const { request, upload } = useApi()

  /**
   * Lists the installed plugins.
   *
   * @returns Every installed plugin.
   */
  const listPlugins = (): Promise<PluginListItemDto[]> =>
    request<PluginListItemDto[]>('/admin/plugins')

  /**
   * Uploads a plugin ZIP for installation.
   *
   * @param file - The ZIP file.
   * @returns Whether a restart is needed.
   */
  const installPlugin = (file: File): Promise<PluginActionResultDto> =>
    upload<PluginActionResultDto>('/admin/plugins/install', file)

  /**
   * Removes an installed plugin.
   *
   * @param id - Plugin id as declared in plugin.json.
   * @returns Whether a restart is needed.
   */
  const removePlugin = (id: string): Promise<PluginActionResultDto> =>
    request<PluginActionResultDto>(`/admin/plugins/${id}`, { method: 'DELETE' })

  /**
   * Asks the server for a graceful restart.
   *
   * @returns Nothing; the server may drop the connection before it answers.
   */
  const restartServer = (): Promise<void> =>
    request<void>('/admin/plugins/restart', { method: 'POST' })

  return { listPlugins, installPlugin, removePlugin, restartServer }
}
