/**
 * Loads the clients a billing screen's client picker offers.
 *
 * Several screens (quotes, transactions, billable items) show the same picker; this keeps the
 * list, and the call that fills it, in one place instead of a copy per screen.
 */

import { ref } from 'vue'
import type { Ref } from 'vue'
import { useClientsStore } from '@/stores/clients'
import type { ClientOption } from '@/types/clientoption'

/** What {@link useClientOptions} hands back. */
export interface ClientOptions {
  /** The clients to offer; empty until {@link ClientOptions.load} resolves. */
  clients: Ref<ClientOption[]>
  /** Fetches the clients through the clients store. */
  load: () => Promise<void>
}

/**
 * Creates the list a client picker reads and the call that fills it.
 *
 * @returns The reactive client list and its loader.
 */
export const useClientOptions = (): ClientOptions => {
  const store = useClientsStore()
  const clients = ref<ClientOption[]>([])

  /** Fills `clients` from the store; an empty list when the call was refused. */
  const load = async (): Promise<void> => {
    clients.value = await store.fetchClientOptions()
  }

  return { clients, load }
}
