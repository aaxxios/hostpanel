/**
 * Endpoints under `/api/portal/client` — the signed-in client's own account and the four
 * collections hanging off it.
 *
 * This file is the list of what that backend area offers, and nothing else: no `ref`, no
 * `computed`, no caching, no error handling. Loading flags, cached results and error strings
 * live in `stores/client.ts`, which is the only intended caller.
 *
 * @module composables/apis/useClientApi
 */

import { apiFetch } from '~/composables/useApi'
import type { ClientEmail } from '~/types/clientemail'
import type { ClientEmailSummary } from '~/types/clientemailsummary'
import type { TeamMember } from '~/types/teammember'
import type { AccountContact } from '~/types/accountcontact'
import type { AccountContactInput } from '~/types/accountcontactinput'
import type { ClientUser } from '~/types/clientuser'
import type { ClientService } from '~/types/clientservice'
import type { ClientInvoice } from '~/types/clientinvoice'
import type { ClientDomain } from '~/types/clientdomain'
import type { ClientTicket } from '~/types/clientticket'
import type { CancellationStatus } from '~/types/cancellationstatus'
import type { CancellationType } from '~/types/cancellationtype'
import type { HostingInfo } from '~/types/hostinginfo'
import type { ServiceDetail } from '~/types/servicedetail'

/**
 * The `/api/portal/client` surface, one function per endpoint.
 *
 * @returns The client-area endpoint functions.
 */
export const useClientApi = () => {
  /**
   * Reads the signed-in client's profile.
   *
   * @returns The client profile.
   * @throws Whatever `apiFetch` throws — notably the `client_profile_not_found` answer for an
   * identity that has no client record. Callers decide what that means; this layer does not.
   */
  const fetchMe = (): Promise<ClientUser> =>
    apiFetch<ClientUser>('/api/portal/client/me')

  /**
   * Lists the client's hosting services.
   *
   * @returns Every service on the account.
   * @throws Whatever `apiFetch` throws.
   */
  const fetchServices = (): Promise<ClientService[]> =>
    apiFetch<ClientService[]>('/api/portal/client/services')

  /**
   * Lists the client's invoices.
   *
   * @returns Every invoice on the account, paid and unpaid alike.
   * @throws Whatever `apiFetch` throws.
   */
  const fetchInvoices = (): Promise<ClientInvoice[]> =>
    apiFetch<ClientInvoice[]>('/api/portal/client/invoices')

  /**
   * Lists the client's domains.
   *
   * @returns Every domain on the account.
   * @throws Whatever `apiFetch` throws.
   */
  const fetchDomains = (): Promise<ClientDomain[]> =>
    apiFetch<ClientDomain[]>('/api/portal/client/domains')

  /**
   * Lists the client's support tickets.
   *
   * @returns Every ticket on the account, open and closed alike.
   * @throws Whatever `apiFetch` throws.
   */
  const fetchTickets = (): Promise<ClientTicket[]> =>
    apiFetch<ClientTicket[]>('/api/portal/client/tickets')


  /**
   * Updates the signed-in client's profile.
   *
   * @param profile - The fields to change, including the email preference flags.
   * @returns Whatever the endpoint answers with.
   * @throws Whatever `apiFetch` throws.
   */
  const updateMe = (profile: Record<string, unknown>): Promise<unknown> =>
    apiFetch('/api/portal/client/me', { method: 'PUT', body: profile })

  /**
   * Reads the signed-in client's profile as a one-shot call.
   *
   * @returns The client profile.
   * @throws Whatever `apiFetch` throws.
   */
  const fetchMeOnce = (): Promise<Record<string, string>> =>
    apiFetch<Record<string, string>>('/api/portal/client/me')

  /**
   * Reads the people who may sign in to this client account.
   *
   * @returns The team members.
   * @throws Whatever `apiFetch` throws.
   */
  const fetchUsers = (): Promise<TeamMember[]> =>
    apiFetch<TeamMember[]>('/api/portal/client/users')

  /**
   * Invites somebody to this client account.
   *
   * @param invite - Email and the permissions to grant.
   * @returns Whatever the endpoint answers with.
   * @throws Whatever `apiFetch` throws.
   */
  const inviteUser = (invite: Record<string, unknown>): Promise<unknown> =>
    apiFetch('/api/portal/client/users/invite', { method: 'POST', body: invite })

  /**
   * Removes somebody's access to this client account.
   *
   * @param id - The user to remove.
   * @returns Whatever the endpoint answers with.
   * @throws Whatever `apiFetch` throws.
   */
  const removeUser = (id: string): Promise<unknown> =>
    apiFetch(`/api/portal/client/users/${id}`, { method: 'DELETE' })

  /**
   * Adds a sub-contact.
   *
   * @param contact - The contact's details, in the backend's `AddContactCommand` shape.
   * @returns Whatever the endpoint answers with.
   * @throws Whatever `apiFetch` throws.
   */
  const createContact = (contact: AccountContactInput): Promise<unknown> =>
    apiFetch('/api/portal/client/contacts', { method: 'POST', body: contact })

  /**
   * Updates a sub-contact.
   *
   * @param id - The contact to update.
   * @param contact - The whole contact, in the backend's `UpdateContactCommand` shape (an update replaces it).
   * @returns Whatever the endpoint answers with.
   * @throws Whatever `apiFetch` throws.
   */
  const updateContact = (id: number, contact: AccountContactInput): Promise<unknown> =>
    apiFetch(`/api/portal/client/contacts/${id}`, { method: 'PUT', body: contact })

  /**
   * Removes a sub-contact.
   *
   * @param id - The contact to delete.
   * @returns Whatever the endpoint answers with.
   * @throws Whatever `apiFetch` throws.
   */
  const removeContact = (id: number): Promise<unknown> =>
    apiFetch(`/api/portal/client/contacts/${id}`, { method: 'DELETE' })

  /**
   * Reads the sub-contacts as a one-shot call.
   *
   * @returns Every sub-contact on the account.
   * @throws Whatever `apiFetch` throws.
   */
  const fetchContacts = (): Promise<AccountContact[]> =>
    apiFetch<AccountContact[]>('/api/portal/client/contacts')

  /**
   * Reads the mail the system has sent this account.
   *
   * @returns The message list.
   * @throws Whatever `apiFetch` throws.
   */
  const fetchEmails = (): Promise<ClientEmailSummary[]> =>
    apiFetch<ClientEmailSummary[]>('/api/portal/client/emails')

  /**
   * Reads one of the messages the system sent this account.
   *
   * @param id - Message id.
   * @returns The message.
   * @throws Whatever `apiFetch` throws.
   */
  const fetchEmail = (id: string): Promise<ClientEmail> =>
    apiFetch<ClientEmail>(`/api/portal/client/emails/${id}`)

  /**
   * Reads one hosting service.
   *
   * @param id - Service id.
   * @returns The service.
   * @throws Whatever `apiFetch` throws.
   */
  const fetchService = (id: string): Promise<ServiceDetail> =>
    apiFetch<ServiceDetail>(`/api/portal/client/services/${id}`)

  /**
   * Reads the nameservers and certificate of one hosting service.
   *
   * @param id - Service id.
   * @returns The hosting info.
   * @throws Whatever `apiFetch` throws.
   */
  const fetchHostingInfo = (id: string): Promise<HostingInfo> =>
    apiFetch<HostingInfo>(`/api/portal/client/services/${id}/hosting-info`)

  /**
   * Reads whether a cancellation is pending on one hosting service.
   *
   * @param id - Service id.
   * @returns The cancellation status.
   * @throws Whatever `apiFetch` throws.
   */
  const fetchCancellationStatus = (id: string): Promise<CancellationStatus> =>
    apiFetch<CancellationStatus>(`/api/portal/client/services/${id}/cancellation-status`)

  /**
   * Reads the invoices raised for one hosting service.
   *
   * @param id - Service id.
   * @returns The service's invoices.
   * @throws Whatever `apiFetch` throws.
   */
  const fetchServiceInvoices = <T>(id: string): Promise<T> =>
    apiFetch<T>(`/api/portal/client/services/${id}/invoices`)

  /**
   * Changes the control-panel password on one hosting service.
   *
   * @param id - Service id.
   * @param password - The password to set.
   * @returns Whatever the endpoint answers with.
   * @throws Whatever `apiFetch` throws.
   */
  const changeServicePassword = (id: string, password: string): Promise<unknown> =>
    apiFetch(`/api/portal/client/services/${id}/change-password`, {
      method: 'POST',
      body: { password }
    })

  /**
   * Requests cancellation of one hosting service.
   *
   * @param id - Service id.
   * @param body - Reason, and `type` as a backend `CancellationType` member name. Typed rather
   * than left open so a display string can never reach the endpoint again: the handler parses
   * this field with `Enum.Parse`, so prose such as "End of Billing Period" is rejected.
   * @returns Whatever the endpoint answers with.
   * @throws Whatever `apiFetch` throws.
   */
  const cancelService = (
    id: string,
    body: { type: CancellationType; reason: string }
  ): Promise<unknown> =>
    apiFetch(`/api/portal/client/services/${id}/cancel`, { method: 'POST', body })

  /**
   * Mints a single-sign-on URL into the service's cPanel.
   *
   * @param id - Service id.
   * @returns The URL to open; it is short-lived, so it is fetched at click time rather than
   * rendered into the page.
   * @throws Whatever `apiFetch` throws — the caller falls back to the plain login page.
   */
  const fetchCpanelSsoUrl = (id: string): Promise<{ url: string }> =>
    apiFetch<{ url: string }>(`/api/portal/client/services/${id}/cpanel-sso`)

  /**
   * Completes first-time setup on a newly provisioned hosting service.
   *
   * @param id - Service id.
   * @param body - The setup answers.
   * @returns Whatever the endpoint answers with.
   * @throws Whatever `apiFetch` throws.
   */
  const setUpService = (id: string, body: Record<string, unknown>): Promise<unknown> =>
    apiFetch(`/api/portal/client/services/${id}/setup`, { method: 'POST', body })

  return {
    fetchMe,
    fetchMeOnce,
    updateMe,
    fetchServices,
    fetchInvoices,
    fetchDomains,
    fetchTickets,
    fetchUsers,
    inviteUser,
    removeUser,
    fetchContacts,
    createContact,
    updateContact,
    removeContact,
    fetchEmails,
    fetchEmail,
    fetchService,
    fetchHostingInfo,
    fetchCancellationStatus,
    fetchServiceInvoices,
    changeServicePassword,
    cancelService,
    fetchCpanelSsoUrl,
    setUpService
  }
}
