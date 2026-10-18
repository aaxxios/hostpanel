/**
 * One function per endpoint on the API client surface (`/api/clients`), plus the reference
 * lookups the client pickers read and the few list endpoints the client summary reuses.
 * No state lives here, no error handling: a refusal rejects with an `ApiError` and the
 * clients stores turn it into a sentence.
 *
 * Callers are the clients stores, never a `.vue` file.
 */

import { buildQuery, useApi } from '@/composables/useApi'
import type { AdminCreateClientPayload } from '@/types/admincreateclientpayload'
import type { ClientDetail, ClientFilters, ClientListItem, ClientSummaryData, ConfiguredCurrency } from '@/types/client'
import type { CountryOption } from '@/types/countryoption'
import type { DomainRegistration } from '@/types/domain'
import type { PagedResult } from '@/types/pagedresult'
import type { ServiceListItem } from '@/types/service'
import type { UserSearchHit } from '@/types/clientuser'

/**
 * Endpoint functions for clients.
 *
 * @returns One typed function per client endpoint the admin panel uses.
 */
export const useClientsApi = () => {
  const { request } = useApi()

  /**
   * Lists clients, filtered and paged.
   *
   * @param page - Page number (1-based).
   * @param pageSize - Items per page.
   * @param filters - Optional search, email, phone and status filters.
   * @returns One page of clients.
   */
  const list = (page: number, pageSize: number, filters: ClientFilters = {}): Promise<PagedResult<ClientListItem>> =>
    request<PagedResult<ClientListItem>>(`/clients${buildQuery({ page, pageSize, ...filters })}`)

  /**
   * Loads one client with its detail.
   *
   * @param id - The client id.
   * @returns The client.
   */
  const get = (id: number | string): Promise<ClientDetail> => request<ClientDetail>(`/clients/${id}`)

  /**
   * Creates a client through the admin endpoint.
   *
   * @param payload - Client creation fields.
   * @returns Nothing.
   */
  const create = (payload: AdminCreateClientPayload): Promise<void> =>
    request<void>('/clients/admin', { method: 'POST', body: JSON.stringify(payload) })

  /**
   * Updates a client profile, address, notification and settings.
   *
   * @param id - The client id.
   * @param payload - Updated fields.
   * @returns Nothing.
   */
  const update = (id: number | string, payload: Record<string, unknown>): Promise<void> =>
    request<void>(`/clients/${id}`, { method: 'PUT', body: JSON.stringify(payload) })

  /**
   * Searches Identity users by name or email.
   *
   * @param search - Search text.
   * @param pageSize - Maximum hits.
   * @returns One page of matching users.
   */
  const searchUsers = (search: string, pageSize: number): Promise<PagedResult<UserSearchHit>> =>
    request<PagedResult<UserSearchHit>>(`/admin/users${buildQuery({ search, pageSize, page: 1 })}`)

  /**
   * Loads the country reference list.
   *
   * @returns The countries.
   */
  const countries = (): Promise<CountryOption[]> => request<CountryOption[]>('/reference/countries')

  /**
   * Loads the operator currency configuration.
   *
   * @returns Every configured currency, enabled or not.
   */
  const configuredCurrencies = (): Promise<ConfiguredCurrency[]> =>
    request<ConfiguredCurrency[]>('/admin/currencies')

  /**
   * Adds a contact to a client.
   *
   * @param clientId - The client id.
   * @param payload - Contact fields.
   * @returns Nothing.
   */
  const addContact = (clientId: number | string, payload: Record<string, unknown>): Promise<void> =>
    request<void>(`/clients/${clientId}/contacts`, { method: 'POST', body: JSON.stringify(payload) })

  /**
   * Updates a contact of a client.
   *
   * @param clientId - The client id.
   * @param contactId - The contact id.
   * @param payload - Contact fields.
   * @returns Nothing.
   */
  const updateContact = (clientId: number | string, contactId: number, payload: Record<string, unknown>): Promise<void> =>
    request<void>(`/clients/${clientId}/contacts/${contactId}`, { method: 'PUT', body: JSON.stringify(payload) })

  /**
   * Removes a contact from a client.
   *
   * @param clientId - The client id.
   * @param contactId - The contact id.
   * @returns Nothing.
   */
  const removeContact = (clientId: number | string, contactId: number): Promise<void> =>
    request<void>(`/clients/${clientId}/contacts/${contactId}`, { method: 'DELETE' })

  /**
   * Exports the chosen sections of a client data.
   *
   * @param clientId - The client id.
   * @param fields - Section keys to include; sent as repeated `fields` keys.
   * @returns The exported document.
   */
  const exportData = (clientId: number | string, fields: string[]): Promise<object> =>
    request<object>(`/clients/${clientId}/export${buildQuery({ fields })}`)

  /**
   * Loads the aggregated summary of a client.
   *
   * @param clientId - The client id.
   * @returns The summary figures.
   */
  const summary = (clientId: number): Promise<ClientSummaryData> =>
    request<ClientSummaryData>(`/clients/${clientId}/summary`)

  /**
   * Lists a few services of a client for the summary table.
   *
   * @param clientId - The client id.
   * @param pageSize - Items to return.
   * @returns One page of services.
   */
  const recentServices = (clientId: number, pageSize: number): Promise<PagedResult<ServiceListItem>> =>
    request<PagedResult<ServiceListItem>>(`/services${buildQuery({ clientId, page: 1, pageSize })}`)

  /**
   * Lists a few domains of a client for the summary table.
   *
   * @param clientId - The client id.
   * @param pageSize - Items to return.
   * @returns One page of domains.
   */
  const recentDomains = (clientId: number, pageSize: number): Promise<PagedResult<DomainRegistration>> =>
    request<PagedResult<DomainRegistration>>(`/domains${buildQuery({ clientId, page: 1, pageSize })}`)

  return {
    list,
    get,
    create,
    update,
    searchUsers,
    countries,
    configuredCurrencies,
    addContact,
    updateContact,
    removeContact,
    exportData,
    summary,
    recentServices,
    recentDomains,
  }
}
